﻿using SEE.Game;
using SEE.GO;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// Action to scale an existing node.
    /// </summary>
    public class ScaleNodeAction : AbstractPlayerAction
    {
        /// <summary>
        /// Start() will register an anonymous delegate of type 
        /// <see cref="ActionState.OnStateChangedFn"/> on the event
        /// <see cref="ActionState.OnStateChanged"/> to be called upon every
        /// change of the action state, where the newly entered state will
        /// be passed as a parameter. The anonymous delegate will compare whether
        /// this state equals <see cref="ThisActionState"/> and if so, execute
        /// what needs to be done for this action here. If that parameter is
        /// different from <see cref="ThisActionState"/>, this action will
        /// put itself to sleep. 
        /// Thus, this action will be executed only if the new state is 
        /// <see cref="ThisActionState"/>.
        /// </summary>
        const ActionState.Type ThisActionState = ActionState.Type.ScaleNode;

        /// <summary>
        /// The old position of the top sphere
        /// </summary>
        Vector3 topOldSpherPos;

        /// <summary>
        /// The old position of the first corner sphere
        /// </summary>
        Vector3 fstCornerOldSpherPos;

        /// <summary>
        /// The old position of the second corner sphere
        /// </summary>
        Vector3 sndCornerOldSpherPos;

        /// <summary>
        /// The old position of the third corner sphere
        /// </summary>
        Vector3 thrdCornerOldSpherPos;

        /// <summary>
        /// The old position of the forth corner sphere
        /// </summary>
        Vector3 forthCornerOldSpherPos;

        /// <summary>
        /// The old position of the first side sphere
        /// </summary>
        Vector3 fstSideOldSpherPos;

        /// <summary>
        /// The old position of the second side sphere
        /// </summary>
        Vector3 sndSideOldSpherPos;

        /// <summary>
        /// The old position of the third side sphere
        /// </summary>
        Vector3 thrdSideOldSpherPos;

        /// <summary>
        /// The old position of the forth side sphere
        /// </summary>
        Vector3 forthSideOldSpherPos;

        /// <summary>
        /// The scale at the start so the user can reset the changes made during scaling
        /// </summary>
        Vector3 originalScale;

        /// <summary>
        /// The position at the start so the user can reset the changes made during scaling
        /// </summary>
        Vector3 originalPosition;

        /// <summary>
        /// The sphere on top of the gameObject to scale
        /// </summary>
        GameObject topSphere;

        /// <summary>
        /// The sphere on the first corner of the gameObject to scale
        /// </summary>
        GameObject fstCornerSphere; //x0 y0

        /// <summary>
        /// The sphere on the second corner of the gameObject to scale
        /// </summary>
        GameObject sndCornerSphere; //x1 y0

        /// <summary>
        /// The sphere on the third corner of the gameObject to scale
        /// </summary>
        GameObject thrdCornerSphere; //x1 y1

        /// <summary>
        /// The sphere on the forth corner of the gameObject to scale
        /// </summary>
        GameObject forthCornerSphere; //x0 y1

        /// <summary>
        /// The sphere on the first side of the gameObject to scale
        /// </summary>
        GameObject fstSideSphere; //x0 y0

        /// <summary>
        /// The sphere on the second side of the gameObject to scale
        /// </summary>
        GameObject sndSideSphere; //x1 y0

        /// <summary>
        /// The sphere on the third side of the gameObject to scale
        /// </summary>
        GameObject thrdSideSphere; //x1 y1

        /// <summary>
        /// The sphere on the forth side of the gameObject to scale
        /// </summary>
        GameObject forthSideSphere; //x0 y1

        /// <summary>
        /// The gameObject which will end the scaling and start the save process
        /// </summary>
        GameObject endWithSave;

        /// <summary>
        /// The gameObject which will end the scaling process and start the discard changes process
        /// </summary>
        GameObject endWithOutSave;

        /// <summary>
        /// The gameObject in which will be saved which sphere was dragged
        /// </summary>
        GameObject draggedSphere = null;

        /// <summary>
        /// The gameObject which should be scaled
        /// </summary>
        private GameObject objectToScale;

        public void Start()
        {
            ActionState.OnStateChanged += (ActionState.Type newState) =>
            {
                // Is this our action state where we need to do something?
                if (newState == ThisActionState)
                {
                    // The monobehaviour is enabled and Update() will be called by Unity.
                    enabled = true;
                    InteractableObject.LocalAnyHoverIn += LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut += LocalAnyHoverOut;
                }
                else
                {
                    // The monobehaviour is diabled and Update() no longer be called by Unity.
                    enabled = false;
                    InteractableObject.LocalAnyHoverIn -= LocalAnyHoverIn;
                    InteractableObject.LocalAnyHoverOut -= LocalAnyHoverOut;
                    hoveredObject = null;
                    instantiated = false;
                    RemoveSpheres();
                    objectToScale = null;
                }
            };
            enabled = ActionState.Is(ThisActionState);
        }

        private void Update()
        {
            if (objectToScale != null && instantiated == false)
            {
                originalScale = objectToScale.transform.lossyScale;
                originalPosition = objectToScale.transform.position;

                // Top sphere
                topSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(topSphere);

                // Corner spheres
                fstCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(fstCornerSphere);

                sndCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(sndCornerSphere);

                thrdCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(thrdCornerSphere);

                forthCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(forthCornerSphere);

                // Side spheres
                fstSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(fstSideSphere);

                sndSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(sndSideSphere);

                thrdSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(thrdSideSphere);

                forthSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(forthSideSphere);

                // End operations
                endWithSave = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                SphereRadius(endWithSave);
                endWithSave.GetComponent<Renderer>().material.color = Color.green;

                endWithOutSave = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                SphereRadius(endWithOutSave);
                endWithOutSave.GetComponent<Renderer>().material.color = Color.red;

                // Positioning
                SetOnRoof();
                SetOnSide();
                instantiated = true;
            }
            if (Input.GetMouseButtonDown(0) && objectToScale == null)
            {
                objectToScale = hoveredObject;
            }
            if (instantiated && Input.GetMouseButton(0))
            {
                if (draggedSphere == null)
                {
                    Ray ray = MainCamera.Camera.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hit;
                    // Casts the ray and get the first game object hit
                    Physics.Raycast(ray, out hit);

                    // Moves the sphere that was hit.
                    // Top
                    if (hit.collider == topSphere.GetComponent<Collider>())
                    {
                        draggedSphere = topSphere;
                    } // Corners
                    else if (hit.collider == fstCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = fstCornerSphere;
                    }
                    else if (hit.collider == sndCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = sndCornerSphere;
                    }
                    else if (hit.collider == thrdCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = thrdCornerSphere;
                    }
                    else if (hit.collider == forthCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = forthCornerSphere;
                    }
                    // Sides
                    else if (hit.collider == fstSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = fstSideSphere;
                    }
                    else if (hit.collider == sndSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = sndSideSphere;
                    }
                    else if (hit.collider == thrdSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = thrdSideSphere;
                    }
                    else if (hit.collider == forthSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = forthSideSphere;
                    }
                    //End Scalling
                    else if (hit.collider == endWithSave.GetComponent<Collider>())
                    {
                        EndScale(true);
                    }
                    else if (hit.collider == endWithOutSave.GetComponent<Collider>())
                    {
                        EndScale(false);
                    }
                }

                if (draggedSphere == topSphere)
                {
                   DesktopNavigationAction.MoveToLockAxes(draggedSphere, false, true, false);
                }
                else if (draggedSphere == fstCornerSphere || draggedSphere == sndCornerSphere 
                         || draggedSphere == thrdCornerSphere || draggedSphere == forthCornerSphere)
                {
                    DesktopNavigationAction.MoveToLockAxes(draggedSphere, true, false, true);
                }
                else if (draggedSphere == fstSideSphere || draggedSphere == sndSideSphere)
                {
                    DesktopNavigationAction.MoveToLockAxes(draggedSphere, true, false, false);
                }
                else if (draggedSphere == thrdSideSphere || draggedSphere == forthSideSphere)
                {
                    DesktopNavigationAction.MoveToLockAxes(draggedSphere, false, false, true);
                }
                else
                {
                    draggedSphere = null;
                }

                if(objectToScale != null)
                {
                    ScaleNode();
                    SetOnRoof();
                    SetOnSide();
                }                
            }
            else
            {
                if (objectToScale != null && instantiated)
                {
                    draggedSphere = null;
                    // Adjust the size of the scaling elements
                    SphereRadius(topSphere);
                    SphereRadius(fstSideSphere);
                    SphereRadius(sndSideSphere);
                    SphereRadius(thrdSideSphere);
                    SphereRadius(forthSideSphere);
                    SphereRadius(fstCornerSphere);
                    SphereRadius(sndCornerSphere);
                    SphereRadius(thrdCornerSphere);
                    SphereRadius(forthCornerSphere);

                    SphereRadius(endWithOutSave);
                    SphereRadius(endWithSave);
                }
            }
        }

        /// <summary>
        /// Sets the new scale of a node based on the sphere elements.
        /// </summary>
        private void ScaleNode()
        {

            Vector3 scale = Vector3.zero;
            scale.y += topSphere.transform.position.y - topOldSpherPos.y;
            scale.x -= fstSideSphere.transform.position.x - fstSideOldSpherPos.x;
            scale.x += sndSideSphere.transform.position.x - sndSideOldSpherPos.x;
            scale.z -= thrdSideSphere.transform.position.z - thrdSideOldSpherPos.z;
            scale.z += forthSideSphere.transform.position.z - forthSideOldSpherPos.z;

            // Corner scaling
            float scaleCorner = 0;
            scaleCorner -= fstCornerSphere.transform.position.x - fstCornerOldSpherPos.x + (fstCornerSphere.transform.position.z - fstCornerOldSpherPos.z); //* 0.5f;
            scaleCorner += sndCornerSphere.transform.position.x - sndCornerOldSpherPos.x - (sndCornerSphere.transform.position.z - sndCornerOldSpherPos.z); //* 0.5f;
            scaleCorner += thrdCornerSphere.transform.position.x - thrdCornerOldSpherPos.x + (thrdCornerSphere.transform.position.z - thrdCornerOldSpherPos.z);// * 0.5f;
            scaleCorner -= forthCornerSphere.transform.position.x - forthCornerOldSpherPos.x - (forthCornerSphere.transform.position.z - forthCornerOldSpherPos.z);// * 0.5f;

            scale.x += scaleCorner;
            scale.z += scaleCorner;

            // Move the gameObject so the user thinks she/he scaled only in one direction
            Vector3 position = objectToScale.transform.position;
            position.y += scale.y * 0.5f;
           
            // Setting the old positions
            topOldSpherPos = topSphere.transform.position;
            fstCornerOldSpherPos = fstCornerSphere.transform.position;
            sndCornerOldSpherPos = sndCornerSphere.transform.position;
            thrdCornerOldSpherPos = thrdCornerSphere.transform.position;
            forthCornerOldSpherPos = forthCornerSphere.transform.position;
            fstSideOldSpherPos = fstSideSphere.transform.position;
            sndSideOldSpherPos = sndSideSphere.transform.position;
            thrdSideOldSpherPos = thrdSideSphere.transform.position;
            forthSideOldSpherPos = forthSideSphere.transform.position;

            scale = objectToScale.transform.lossyScale + scale;

            // Fixes negative dimension
            if (scale.x <= 0)
            {
                scale.x = objectToScale.transform.lossyScale.x;
            }
            if (scale.y <= 0)
            {
                scale.y = objectToScale.transform.lossyScale.y;
                position.y = objectToScale.transform.position.y;
            }
            if (scale.z <= 0)
            {
                scale.z = objectToScale.transform.lossyScale.z;
            }

            // Transform the new position and scale
            objectToScale.transform.position = position;
            objectToScale.SetScale(scale);
            new ScaleNodeNetAction(objectToScale.name, scale, position).Execute(null);
        }

        /// <summary>
        /// Sets the top sphere at the top of <see cref="objectToScale"/> and
        /// the Save (<see cref="endWithSave"/>) and Discard (<see cref="endWithOutSave"/>)
        /// objects.
        /// </summary>
        private void SetOnRoof()
        {
            Vector3 pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof() + 0.01f;
            topSphere.transform.position = pos;

            topOldSpherPos = topSphere.transform.position;
            pos.y += 0.2f;
            pos.x += 0.1f;
            endWithSave.transform.position = pos;
            pos.x -= 0.2f;
            endWithOutSave.transform.position = pos;
        }

        /// <summary>
        /// Sets the side spheres.
        /// </summary>
        private void SetOnSide()
        {
            Transform trns = objectToScale.transform;

            // first corner
            Vector3 pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x -= trns.lossyScale.x / 2 + 0.02f;
            pos.z -= trns.lossyScale.z / 2 + 0.02f;
            fstCornerSphere.transform.position = pos;
            fstCornerOldSpherPos = pos;

            // second corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.02f;
            pos.z -= trns.lossyScale.z / 2 + 0.02f;
            sndCornerSphere.transform.position = pos;
            sndCornerOldSpherPos = pos;

            // third corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.02f;
            pos.z += trns.lossyScale.z / 2 + 0.02f;
            thrdCornerSphere.transform.position = pos;
            thrdCornerOldSpherPos = pos;

            // forth corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x -= trns.lossyScale.x / 2 + 0.02f;
            pos.z += trns.lossyScale.z / 2 + 0.02f;
            forthCornerSphere.transform.position = pos;
            forthCornerOldSpherPos = pos;

            // first side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x -= trns.lossyScale.x / 2 + 0.01f;

            fstSideSphere.transform.position = pos;
            fstSideOldSpherPos = pos;

            // second side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.01f;

            sndSideSphere.transform.position = pos;
            sndSideOldSpherPos = pos;

            // third side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();

            pos.z -= trns.lossyScale.z / 2 + 0.01f;
            thrdSideSphere.transform.position = pos;
            thrdSideOldSpherPos = pos;

            // forth side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();

            pos.z += trns.lossyScale.z / 2 + 0.01f;
            forthSideSphere.transform.position = pos;
            forthSideOldSpherPos = pos;
        }

        /// <summary>
        /// Sets the radius of a sphere dependent on the X and Z scale of <paramref name="sphere"/>
        ///  that is to be scaled.</summary>
        /// <param name="sphere">the sphere to be scaled</param>
        private void SphereRadius(GameObject sphere)
        {
            Vector3 goScale = objectToScale.transform.lossyScale;
            if (goScale.x > goScale.z && goScale.z > 0.1f)
            {
                sphere.transform.localScale = new Vector3(goScale.z, goScale.z, goScale.z) * 0.1f; ;
            }
            else if (goScale.x > 0.1f)
            {
                sphere.transform.localScale = new Vector3(goScale.x, goScale.x, goScale.x) * 0.1f;
            }
            else
            {
                sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
        }

        /// <summary>
        /// This will end the scaling action the user can choose between save and discard.
        /// </summary>
        /// <param name="save">Whether the changes should be saved</param>
        public void EndScale(bool save)
        {
            if (save)
            {
                // FIXME: Currently, the changes will not be saved after closing the game. 
                // SAVE THE CHANGES
                RemoveSpheres();
            }
            else
            {
                objectToScale.SetScale(originalScale);
                objectToScale.transform.position = originalPosition;
                new ScaleNodeNetAction(objectToScale.name, originalScale, originalPosition).Execute(null);
                RemoveSpheres();
            }
        }

        /// <summary>
        /// Resets all attributes from the gameObject.
        /// </summary>
        public void RemoveSpheres()
        {
            Destroy(topSphere);
            Destroy(fstCornerSphere);
            Destroy(sndCornerSphere);
            Destroy(thrdCornerSphere);
            Destroy(forthCornerSphere);
            Destroy(fstSideSphere);
            Destroy(sndSideSphere);
            Destroy(thrdSideSphere);
            Destroy(forthSideSphere);
            Destroy(endWithSave);
            Destroy(endWithOutSave);
            objectToScale = null;
            instantiated = false;
        }
    }
}