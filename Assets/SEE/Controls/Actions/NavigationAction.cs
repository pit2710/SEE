﻿using SEE.GO;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Controls
{

    public class NavigationAction : CityAction
    {
        private enum NavigationMode
        {
            Move,
            Rotate
        }

        internal class ZoomCommand
        {
            public readonly int TargetZoomSteps;
            public readonly Vector2 ZoomCenter;
            private readonly float duration;
            private readonly float startTime;

            internal ZoomCommand(Vector2 zoomCenter, int targetZoomSteps, float duration)
            {
                TargetZoomSteps = targetZoomSteps;
                ZoomCenter = zoomCenter;
                this.duration = duration;
                startTime = Time.realtimeSinceStartup;
            }

            internal bool IsFinished()
            {
                bool result = Time.realtimeSinceStartup - startTime >= duration;
                return result;
            }

            internal float CurrentDeltaScale()
            {
                float x = Mathf.Min((Time.realtimeSinceStartup - startTime) / duration, 1.0f);
                float t = 0.5f - 0.5f * Mathf.Cos(x * Mathf.PI);
                float result = t * (float)TargetZoomSteps;
                return result;
            }
        }

        private struct MoveState
        {
            internal const float MaxVelocity = 10.0f;
            internal const float MaxSqrVelocity = MaxVelocity * MaxVelocity;
            internal const float SnapStepCount = 8;
            internal const float SnapStepAngle = 360.0f / SnapStepCount;
            internal const float DragFrictionFactor = 32.0f;

            internal UI3D.MoveGizmo moveGizmo;
            internal Bounds cityBounds;
            internal Vector3 dragStartTransformPosition;
            internal Vector3 dragStartOffset;
            internal Vector3 dragCanonicalOffset;
            internal Vector3 moveVelocity;
        }

        private struct RotateState
        {
            internal const float SnapStepCount = 8;
            internal const float SnapStepAngle = 360.0f / SnapStepCount;

            internal UI3D.RotateGizmo rotateGizmo;
            internal float originalEulerAngleY;
            internal Vector3 originalPosition;
            internal float startAngle;
        }

        private struct ZoomState
        {
            internal const float DefaultZoomDuration = 0.1f;
            internal const uint ZoomMaxSteps = 32;
            internal const float ZoomFactor = 0.5f;

            internal Vector3 originalScale;
            internal List<ZoomCommand> zoomCommands;
            internal uint currentTargetZoomSteps;
            internal float currentZoomFactor;
        }

        private struct ActionState
        {
            internal bool startDrag;         // drag entire city
            internal bool drag;              // drag entire city
            internal bool cancel;
            internal bool snap;
            internal bool reset;
            internal Vector3 mousePosition;  // TODO(torben): this needs to be abstracted for other modalities
            internal float zoomStepsDelta;
            internal bool zoomToggleToObject;
        }
        
        private Transform cityRootTransform;
        private UnityEngine.Plane raycastPlane;

        private NavigationMode mode;
        private bool movingOrRotating;
        private UI3D.Cursor cursor;

        private MoveState moveState;
        private RotateState rotateState;
        private ZoomState zoomState;
        private ActionState actionState;

        [Tooltip("The area in which to draw the code city")]
        public Plane cullingPlane;

        [Tooltip("The unique ID used for network synchronization. This must be set via inspector to ensure that every client will have the correct ID assigned to the appropriate NavigationAction!")]
        [SerializeField]
        private int id;
        public int ID { get => id; }

        private static readonly Dictionary<int, NavigationAction> navigationActionDict = new Dictionary<int, NavigationAction>(2);

        public static NavigationAction Get(int id)
        {
            bool result = navigationActionDict.TryGetValue(id, out NavigationAction value);
            if (result)
            {
                return value;
            }
            else
            {
                Debug.LogWarning("ID does not match any NavigationAction!");
                return null;
            }
        }

        private void Start()
        {
            UnityEngine.Assertions.Assert.IsNotNull(cullingPlane, "The culling plane must not be null!");
            UnityEngine.Assertions.Assert.IsTrue(!navigationActionDict.ContainsKey(id), "A unique ID must be assigned to every NavigationAction!");
            navigationActionDict.Add(id, this);

            cityRootTransform = GetCityRootNode(gameObject);
            if (cityRootTransform == null)
            {
                enabled = false; // deactivate this component as it cannot work
                throw new Exception("This NavigationAction is not attached to a code city.");
            }

            Debug.LogFormat("NavigationAction controls {0}.\n", cityRootTransform.name);

            zoomState.originalScale = cityRootTransform.localScale;
            Collider collider = cityRootTransform.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogErrorFormat("The city object {0} has no collider attached to it.\n", cityRootTransform.name);
            }
            else
            {
                moveState.cityBounds = collider.bounds;
            }
            raycastPlane = new UnityEngine.Plane(Vector3.up, cityRootTransform.position);
            
            moveState.dragStartTransformPosition = cityRootTransform.position;
            moveState.dragCanonicalOffset = Vector3.zero;
            moveState.moveVelocity = Vector3.zero;
            moveState.moveGizmo = UI3D.MoveGizmo.Create(0.008f * cullingPlane.MinLengthXZ);
            rotateState.rotateGizmo = UI3D.RotateGizmo.Create(cullingPlane, 1024);

            zoomState.zoomCommands = new List<ZoomCommand>((int)ZoomState.ZoomMaxSteps);
            zoomState.currentTargetZoomSteps = 0;
            zoomState.currentZoomFactor = 1.0f;

            cursor = UI3D.Cursor.Create();

        }

        private void Update()
        {
            // Note: Input MUST NOT be inquired in FixedUpdate() for the input to feel responsive!

            actionState.drag = Input.GetMouseButton(2);
            actionState.startDrag |= Input.GetMouseButtonDown(2);
            actionState.cancel |= Input.GetKeyDown(KeyCode.Escape);
            actionState.snap = Input.GetKey(KeyCode.LeftAlt);
            actionState.reset |= Input.GetKeyDown(KeyCode.R);
            actionState.mousePosition = Input.mousePosition;

            if (!actionState.drag)
            {
                actionState.zoomStepsDelta += Input.mouseScrollDelta.y;
                actionState.zoomToggleToObject |= Input.GetKeyDown(KeyCode.F);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                mode = NavigationMode.Move;
                movingOrRotating = false;
                rotateState.rotateGizmo.gameObject.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                mode = NavigationMode.Rotate;
                movingOrRotating = false;
                moveState.moveGizmo.gameObject.SetActive(false);
            }

            rotateState.rotateGizmo.Radius = 0.2f * (Camera.main.transform.position - rotateState.rotateGizmo.Center).magnitude;

            // selection with left mouse button
            if (!actionState.drag && Input.GetMouseButtonDown(0))
            {
                RaycastClippingPlane(out bool hitPlane, out bool insideClippingArea, out Vector3 planeHitPoint);
                if (insideClippingArea)
                {
                    foreach (RaycastHit hit in Raycasting.SortedHits())
                    {
                        if (hit.transform.gameObject.GetComponent<NodeRef>() != null)
                        {
                            Transform selectedTransform = hit.transform;
                            Transform parentTransform = selectedTransform;
                            do
                            {
                                if (parentTransform == cityRootTransform)
                                {
                                    Select(selectedTransform.gameObject);
                                    goto SelectionFinished;
                                }
                                else
                                {
                                    parentTransform = parentTransform.parent;
                                }
                            } while (parentTransform != null);
                        }
                    }
                }

                Select(null);
            }
        SelectionFinished:

            if (cursor.GetFocus())
            {
                rotateState.rotateGizmo.Center = cursor.GetFocus().position;
            }
        }

        // This logic is in FixedUpdate(), so that the behaviour is framerate-
        // 'independent'.
        private void FixedUpdate()
        {
            // TODO(torben): abstract mouse away!

            RaycastClippingPlane(out bool hitPlane, out bool insideClippingArea, out Vector3 planeHitPoint);

            if (actionState.cancel && !movingOrRotating)
            {
                Select(null);
            }

#region Move City

            else if (mode == NavigationMode.Move)
            {
                if (actionState.reset) // reset to center of table
                {
                    if ((insideClippingArea && !(actionState.drag ^ movingOrRotating)) || (actionState.drag && movingOrRotating))
                    {
                        movingOrRotating = false;
                        moveState.moveGizmo.gameObject.SetActive(false);

                        cityRootTransform.position = cullingPlane.CenterTop;
                    }
                }
                else if (actionState.cancel) // cancel movement
                {
                    if (movingOrRotating)
                    {
                        movingOrRotating = false;
                        moveState.moveGizmo.gameObject.SetActive(false);

                        moveState.moveVelocity = Vector3.zero;
                        cityRootTransform.position = moveState.dragStartTransformPosition + moveState.dragStartOffset - Vector3.Scale(moveState.dragCanonicalOffset, cityRootTransform.localScale);
                    }
                }
                else if (actionState.drag && hitPlane) // start or continue movement
                {
                    if (actionState.startDrag) // start movement
                    {
                        if (insideClippingArea)
                        {
                            movingOrRotating = true;
                            moveState.moveGizmo.gameObject.SetActive(true);

                            moveState.dragStartTransformPosition = cityRootTransform.position;
                            moveState.dragStartOffset = planeHitPoint - cityRootTransform.position;
                            moveState.dragCanonicalOffset = moveState.dragStartOffset.DividePairwise(cityRootTransform.localScale);
                            moveState.moveVelocity = Vector3.zero;
                        }
                    }

                    if (movingOrRotating) // continue movement
                    {
                        Vector3 totalDragOffsetFromStart = planeHitPoint - (moveState.dragStartTransformPosition + moveState.dragStartOffset);

                        if (actionState.snap)
                        {
                            totalDragOffsetFromStart = Project(totalDragOffsetFromStart);
                        }

                        Vector3 oldPosition = cityRootTransform.position;
                        Vector3 newPosition = moveState.dragStartTransformPosition + totalDragOffsetFromStart;

                        moveState.moveVelocity = (newPosition - oldPosition) / Time.fixedDeltaTime;
                        cityRootTransform.position = newPosition;
                        moveState.moveGizmo.SetPositions(moveState.dragStartTransformPosition + moveState.dragStartOffset, cityRootTransform.position + Vector3.Scale(moveState.dragCanonicalOffset, cityRootTransform.localScale));
                    }
                }
                else if (movingOrRotating) // finalize movement
                {
                    actionState.startDrag = false;
                    movingOrRotating = false;
                    moveState.moveGizmo.gameObject.SetActive(false);
                }
            }

#endregion

#region Rotate

            else // mode == NavigationMode.Rotate
            {
                if (actionState.reset) // reset rotation to identity();
                {
                    if ((insideClippingArea && !(actionState.drag ^ movingOrRotating)) || (actionState.drag && movingOrRotating))
                    {
                        movingOrRotating = false;
                        rotateState.rotateGizmo.gameObject.SetActive(false);

                        cityRootTransform.RotateAround(rotateState.rotateGizmo.Center, Vector3.up, -cityRootTransform.rotation.eulerAngles.y);
                    }
                }
                else if (actionState.cancel) // cancel rotation
                {
                    if (movingOrRotating)
                    {
                        movingOrRotating = false;
                        rotateState.rotateGizmo.gameObject.SetActive(false);

                        cityRootTransform.rotation = Quaternion.Euler(0.0f, rotateState.originalEulerAngleY, 0.0f);
                        cityRootTransform.position = rotateState.originalPosition;
                    }
                }
                else if (actionState.drag && hitPlane && cursor.GetFocus() != null) // start or continue rotation
                {
                    Vector2 toHit = planeHitPoint.XZ() - rotateState.rotateGizmo.Center.XZ();
                    float toHitAngle = toHit.Angle360();

                    if (actionState.startDrag) // start rotation
                    {
                        movingOrRotating = true;
                        rotateState.rotateGizmo.gameObject.SetActive(true);

                        rotateState.originalEulerAngleY = cityRootTransform.rotation.eulerAngles.y;
                        rotateState.originalPosition = cityRootTransform.position;
                        rotateState.startAngle = AngleMod(cityRootTransform.rotation.eulerAngles.y - toHitAngle);
                        rotateState.rotateGizmo.SetMinAngle(Mathf.Deg2Rad * toHitAngle);
                        rotateState.rotateGizmo.SetMaxAngle(Mathf.Deg2Rad * toHitAngle);
                    }

                    if (movingOrRotating) // continue rotation
                    {
                        float angle = AngleMod(rotateState.startAngle + toHitAngle);
                        if (actionState.snap)
                        {
                            angle = AngleMod(Mathf.Round(angle / RotateState.SnapStepAngle) * RotateState.SnapStepAngle);
                        }
                        cityRootTransform.RotateAround(cursor.GetFocus().position, Vector3.up, angle - cityRootTransform.rotation.eulerAngles.y);

                        float prevAngle = Mathf.Rad2Deg * rotateState.rotateGizmo.GetMaxAngle();
                        float currAngle = toHitAngle;

                        while (Mathf.Abs(currAngle + 360.0f - prevAngle) < Mathf.Abs(currAngle - prevAngle))
                        {
                            currAngle += 360.0f;
                        }
                        while (Mathf.Abs(currAngle - 360.0f - prevAngle) < Mathf.Abs(currAngle - prevAngle))
                        {
                            currAngle -= 360.0f;
                        }
                        if (actionState.snap)
                        {
                            currAngle = Mathf.Round((currAngle + rotateState.startAngle) / (RotateState.SnapStepAngle)) * (RotateState.SnapStepAngle) - rotateState.startAngle;
                        }

                        rotateState.rotateGizmo.SetMaxAngle(Mathf.Deg2Rad * currAngle);
                    }
                }
                else if (movingOrRotating) // finalize rotation
                {
                    movingOrRotating = false;
                    rotateState.rotateGizmo.gameObject.SetActive(false);
                }
            }

#endregion

#region Zoom

            if (actionState.zoomToggleToObject)
            {
                actionState.zoomToggleToObject = false;
                if (cursor.GetFocus() != null)
                {
                    float optimalTargetZoomFactor = cullingPlane.MinLengthXZ / (cursor.GetFocus().lossyScale.x / zoomState.currentZoomFactor);
                    float optimalTargetZoomSteps = ConvertZoomFactorToZoomSteps(optimalTargetZoomFactor);
                    int actualTargetZoomSteps = Mathf.FloorToInt(optimalTargetZoomSteps);
                    float actualTargetZoomFactor = ConvertZoomStepsToZoomFactor(actualTargetZoomSteps);

                    int zoomSteps = actualTargetZoomSteps - (int)zoomState.currentTargetZoomSteps;
                    if (zoomSteps == 0)
                    {
                        zoomSteps = -(int)zoomState.currentTargetZoomSteps;
                        actualTargetZoomFactor = 1.0f;
                    }

                    if (zoomSteps != 0)
                    {
                        float zoomFactor = ConvertZoomStepsToZoomFactor(zoomSteps);
                        Vector2 centerOfTableAfterZoom = zoomSteps == -(int)zoomState.currentTargetZoomSteps ? cityRootTransform.position.XZ() : cursor.GetFocus().position.XZ();
                        Vector2 toCenterOfTable = cullingPlane.CenterXZ - centerOfTableAfterZoom;
                        Vector2 zoomCenter = cullingPlane.CenterXZ - (toCenterOfTable * (zoomFactor / (zoomFactor - 1.0f)));
                        float duration = 2.0f * ZoomState.DefaultZoomDuration;
                        new Net.ZoomCommandAction(this, zoomCenter, zoomSteps, duration).Execute();
                    }
                }
            }

            if (Mathf.Abs(actionState.zoomStepsDelta) >= 1.0f && insideClippingArea)
            {
                float fZoomSteps = actionState.zoomStepsDelta >= 0.0f ? Mathf.Floor(actionState.zoomStepsDelta) : Mathf.Ceil(actionState.zoomStepsDelta);
                actionState.zoomStepsDelta -= fZoomSteps;
                int zoomSteps = Mathf.Clamp((int)fZoomSteps, -(int)zoomState.currentTargetZoomSteps, (int)ZoomState.ZoomMaxSteps - (int)zoomState.currentTargetZoomSteps);
                new Net.ZoomCommandAction(this, planeHitPoint.XZ(), zoomSteps, ZoomState.DefaultZoomDuration).Execute();
            }

            if (zoomState.zoomCommands.Count != 0)
            {
                float zoomSteps = (float)zoomState.currentTargetZoomSteps;
                int positionCount = 0;
                Vector2 positionSum = Vector3.zero;

                for (int i = 0; i < zoomState.zoomCommands.Count; i++)
                {
                    positionCount++;
                    positionSum += zoomState.zoomCommands[i].ZoomCenter;
                    if (zoomState.zoomCommands[i].IsFinished())
                    {
                        zoomState.zoomCommands.RemoveAt(i--);
                    }
                    else
                    {
                        zoomSteps -= zoomState.zoomCommands[i].TargetZoomSteps - zoomState.zoomCommands[i].CurrentDeltaScale();
                    }
                }
                Vector3 averagePosition = new Vector3(positionSum.x / positionCount, cityRootTransform.position.y, positionSum.y / positionCount);

                zoomState.currentZoomFactor = ConvertZoomStepsToZoomFactor(zoomSteps);
                Vector3 cityCenterToHitPoint = averagePosition - cityRootTransform.position;
                Vector3 cityCenterToHitPointUnscaled = cityCenterToHitPoint.DividePairwise(cityRootTransform.localScale);

                cityRootTransform.position += cityCenterToHitPoint;
                cityRootTransform.localScale = zoomState.currentZoomFactor * zoomState.originalScale;
                cityRootTransform.position -= Vector3.Scale(cityCenterToHitPointUnscaled, cityRootTransform.localScale);

                moveState.dragStartTransformPosition += moveState.dragStartOffset;
                moveState.dragStartOffset = Vector3.Scale(moveState.dragCanonicalOffset, cityRootTransform.localScale);
                moveState.dragStartTransformPosition -= moveState.dragStartOffset;
            }

#endregion

#region ApplyVelocityAndConstraints

            if (!movingOrRotating)
            {
                // Clamp velocity
                float sqrMag = moveState.moveVelocity.sqrMagnitude;
                if (sqrMag > MoveState.MaxSqrVelocity)
                {
                    moveState.moveVelocity = moveState.moveVelocity / Mathf.Sqrt(sqrMag) * MoveState.MaxVelocity;
                }

                // Apply velocity to city position
                cityRootTransform.position += moveState.moveVelocity * Time.fixedDeltaTime;

                // Apply friction to velocity
                Vector3 acceleration = MoveState.DragFrictionFactor * -moveState.moveVelocity;
                moveState.moveVelocity += acceleration * Time.fixedDeltaTime;
            }

            // Keep city constrained to table
            float radius = 0.5f * cityRootTransform.lossyScale.x;
            MathExtensions.TestCircleAABB(cityRootTransform.position.XZ(), 
                                          0.9f * radius,
                                          cullingPlane.LeftFrontCorner,
                                          cullingPlane.RightBackCorner,
                                          out float distance,
                                          out Vector2 normalizedFromCircleToSurfaceDirection);

            if (distance > 0.0f)
            {
                Vector2 toSurfaceDirection = distance * normalizedFromCircleToSurfaceDirection;
                cityRootTransform.position += new Vector3(toSurfaceDirection.x, 0.0f, toSurfaceDirection.y);
            }

            #endregion

            #region ResetActionState
            
            actionState.startDrag = false;
            actionState.cancel = false;
            actionState.reset = false;
            actionState.zoomToggleToObject = false;

            #endregion
        }

        private float ConvertZoomStepsToZoomFactor(float zoomSteps)
        {
            float result = Mathf.Pow(2, zoomSteps * ZoomState.ZoomFactor);
            return result;
        }

        private float ConvertZoomFactorToZoomSteps(float zoomFactor)
        {
            float result = Mathf.Log(zoomFactor, 2) / ZoomState.ZoomFactor;
            return result;
        }

        internal void PushZoomCommand(Vector2 zoomCenter, int zoomSteps, float duration)
        {
            zoomSteps = Mathf.Clamp(zoomSteps, -(int)zoomState.currentTargetZoomSteps, (int)ZoomState.ZoomMaxSteps - (int)zoomState.currentTargetZoomSteps);
            if (zoomSteps != 0)
            {
                uint newZoomStepsInProgress = (uint)((int)zoomState.currentTargetZoomSteps + zoomSteps);
                if (duration != 0)
                {
                    zoomState.zoomCommands.Add(new ZoomCommand(zoomCenter, zoomSteps, duration));
                }
                zoomState.currentTargetZoomSteps = newZoomStepsInProgress;
            }
        }

        private float AngleMod(float degrees)
        {
            float result = ((degrees % 360.0f) + 360.0f) % 360.0f;
            return result;
        }

        private Vector3 Project(Vector3 offset)
        {
            Vector2 point2 = new Vector2(offset.x, offset.z);
            float angleDeg = point2.Angle360();
            float snappedAngleDeg = Mathf.Round(angleDeg / MoveState.SnapStepAngle) * MoveState.SnapStepAngle;
            float snappedAngleRad = Mathf.Deg2Rad * snappedAngleDeg;
            Vector2 dir = new Vector2(Mathf.Cos(snappedAngleRad), Mathf.Sin(-snappedAngleRad));
            Vector2 proj = dir * Vector2.Dot(point2, dir);
            Vector3 result = new Vector3(proj.x, offset.y, proj.y);
            return result;
        }

        private void Select(GameObject go)
        {
            Transform oldT = cursor.GetFocus();
            Transform newT = go ? go.transform : null;
            bool updateServer = false;

            if (oldT != newT)
            {
                if (oldT)
                {
                    Outline outline = oldT.GetComponent<Outline>();
                    if (outline)
                    {
                        updateServer = true;
                        Destroy(outline);
                        cursor.SetFocus(null);
                    }
                }
                if (newT)
                {
                    if (newT.GetComponent<Outline>() == null)
                    {
                        updateServer = true;
                        Outline.Create(newT.gameObject);
                        cursor.SetFocus(newT);
                    }
                }
            }

            if (updateServer)
            {
                HoverableObject oldH = oldT ? oldT.GetComponent<HoverableObject>() : null;
                HoverableObject newH = newT ? newT.GetComponent<HoverableObject>() : null;
                new Net.SelectionAction(oldH, newH).Execute();
            }
        }

        private void RaycastClippingPlane(out bool hitPlane, out bool insideClippingArea, out Vector3 planeHitPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(actionState.mousePosition);

            hitPlane = raycastPlane.Raycast(ray, out float enter);
            if (hitPlane)
            {
                planeHitPoint = ray.GetPoint(enter);
                MathExtensions.TestPointAABB(
                    planeHitPoint.XZ(),
                    cullingPlane.LeftFrontCorner,
                    cullingPlane.RightBackCorner,
                    out float distanceFromPoint,
                    out Vector2 normalizedFromPointToSurfaceDirection
                );
                insideClippingArea = distanceFromPoint < 0.0f;
            }
            else
            {
                insideClippingArea = false;
                planeHitPoint = Vector3.zero;
            }
        }
    }
}
