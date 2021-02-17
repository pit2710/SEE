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
        private const ActionState.Type ThisActionState = ActionState.Type.ScaleNode;

        /// <summary>
        /// The old position of the top sphere
        /// </summary>
        private Vector3 topOldSpherePos;

        /// <summary>
        /// The old position of the first corner sphere
        /// </summary>
        private Vector3 firstCornerOldSpherePos;

        /// <summary>
        /// The old position of the second corner sphere
        /// </summary>
        private Vector3 secondCornerOldSpherePos;

        /// <summary>
        /// The old position of the third corner sphere
        /// </summary>
        private Vector3 thirdCornerOldSpherePos;

        /// <summary>
        /// The old position of the forth corner sphere
        /// </summary>
        private Vector3 forthCornerOldSpherePos;

        /// <summary>
        /// The old position of the first side sphere
        /// </summary>
        private Vector3 firstSideOldSpherePos;

        /// <summary>
        /// The old position of the second side sphere
        /// </summary>
        private Vector3 secondSideOldSpherePos;

        /// <summary>
        /// The old position of the third side sphere
        /// </summary>
        private Vector3 thirdSideOldSpherePos;

        /// <summary>
        /// The old position of the forth side sphere
        /// </summary>
        private Vector3 forthSideOldSpherePos;

        /// <summary>
        /// The scale at the start so the user can reset the changes made during scaling
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// The position at the start so the user can reset the changes made during scaling
        /// </summary>
        private Vector3 originalPosition;

        /// <summary>
        /// The sphere on top of the gameObject to scale
        /// </summary>
        private GameObject topSphere;

        /// <summary>
        /// The sphere on the first corner of the gameObject to scale
        /// </summary>
        private GameObject firstCornerSphere; //x0 y0

        /// <summary>
        /// The sphere on the second corner of the gameObject to scale
        /// </summary>
        private GameObject secondCornerSphere; //x1 y0

        /// <summary>
        /// The sphere on the third corner of the gameObject to scale
        /// </summary>
        private GameObject thirdCornerSphere; //x1 y1

        /// <summary>
        /// The sphere on the forth corner of the gameObject to scale
        /// </summary>
        private GameObject forthCornerSphere; //x0 y1

        /// <summary>
        /// The sphere on the first side of the gameObject to scale
        /// </summary>
        private GameObject firstSideSphere; //x0 y0

        /// <summary>
        /// The sphere on the second side of the gameObject to scale
        /// </summary>
        private GameObject secondSideSphere; //x1 y0

        /// <summary>
        /// The sphere on the third side of the gameObject to scale
        /// </summary>
        private GameObject thirdSideSphere; //x1 y1

        /// <summary>
        /// The sphere on the forth side of the gameObject to scale
        /// </summary>
        private GameObject forthSideSphere; //x0 y1

        /// <summary>
        /// The gameObject which will end the scaling and start the save process
        /// </summary>
        private GameObject endWithSave;

        /// <summary>
        /// The gameObject which will end the scaling process and start the discard changes process
        /// </summary>
        private GameObject endWithOutSave;

        /// <summary>
        /// The gameObject in which will be saved which sphere was dragged
        /// </summary>
        private GameObject draggedSphere = null;

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
                firstCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(firstCornerSphere);

                secondCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(secondCornerSphere);

                thirdCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(thirdCornerSphere);

                forthCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(forthCornerSphere);

                // Side spheres
                firstSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(firstSideSphere);

                secondSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(secondSideSphere);

                thirdSideSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereRadius(thirdSideSphere);

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
                    else if (hit.collider == firstCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = firstCornerSphere;
                    }
                    else if (hit.collider == secondCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = secondCornerSphere;
                    }
                    else if (hit.collider == thirdCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = thirdCornerSphere;
                    }
                    else if (hit.collider == forthCornerSphere.GetComponent<Collider>())
                    {
                        draggedSphere = forthCornerSphere;
                    }
                    // Sides
                    else if (hit.collider == firstSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = firstSideSphere;
                    }
                    else if (hit.collider == secondSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = secondSideSphere;
                    }
                    else if (hit.collider == thirdSideSphere.GetComponent<Collider>())
                    {
                        draggedSphere = thirdSideSphere;
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
                    GameNodeMover.MoveToLockAxes(draggedSphere, false, true, false);
                }
                else if (draggedSphere == firstCornerSphere || draggedSphere == secondCornerSphere 
                         || draggedSphere == thirdCornerSphere || draggedSphere == forthCornerSphere)
                {
                    GameNodeMover.MoveToLockAxes(draggedSphere, true, false, true);
                }
                else if (draggedSphere == firstSideSphere || draggedSphere == secondSideSphere)
                {
                    GameNodeMover.MoveToLockAxes(draggedSphere, true, false, false);
                }
                else if (draggedSphere == thirdSideSphere || draggedSphere == forthSideSphere)
                {
                    GameNodeMover.MoveToLockAxes(draggedSphere, false, false, true);
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
                    SphereRadius(firstSideSphere);
                    SphereRadius(secondSideSphere);
                    SphereRadius(thirdSideSphere);
                    SphereRadius(forthSideSphere);
                    SphereRadius(firstCornerSphere);
                    SphereRadius(secondCornerSphere);
                    SphereRadius(thirdCornerSphere);
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
            scale.y += topSphere.transform.position.y - topOldSpherePos.y;
            scale.x -= firstSideSphere.transform.position.x - firstSideOldSpherePos.x;
            scale.x += secondSideSphere.transform.position.x - secondSideOldSpherePos.x;
            scale.z -= thirdSideSphere.transform.position.z - thirdSideOldSpherePos.z;
            scale.z += forthSideSphere.transform.position.z - forthSideOldSpherePos.z;

            // Corner scaling
            float scaleCorner = 0;
            scaleCorner -= firstCornerSphere.transform.position.x - firstCornerOldSpherePos.x + (firstCornerSphere.transform.position.z - firstCornerOldSpherePos.z); //* 0.5f;
            scaleCorner += secondCornerSphere.transform.position.x - secondCornerOldSpherePos.x - (secondCornerSphere.transform.position.z - secondCornerOldSpherePos.z); //* 0.5f;
            scaleCorner += thirdCornerSphere.transform.position.x - thirdCornerOldSpherePos.x + (thirdCornerSphere.transform.position.z - thirdCornerOldSpherePos.z);// * 0.5f;
            scaleCorner -= forthCornerSphere.transform.position.x - forthCornerOldSpherePos.x - (forthCornerSphere.transform.position.z - forthCornerOldSpherePos.z);// * 0.5f;

            scale.x += scaleCorner;
            scale.z += scaleCorner;

            // Move the gameObject so the user thinks she/he scaled only in one direction
            Vector3 position = objectToScale.transform.position;
            position.y += scale.y * 0.5f;
           
            // Setting the old positions
            topOldSpherePos = topSphere.transform.position;
            firstCornerOldSpherePos = firstCornerSphere.transform.position;
            secondCornerOldSpherePos = secondCornerSphere.transform.position;
            thirdCornerOldSpherePos = thirdCornerSphere.transform.position;
            forthCornerOldSpherePos = forthCornerSphere.transform.position;
            firstSideOldSpherePos = firstSideSphere.transform.position;
            secondSideOldSpherePos = secondSideSphere.transform.position;
            thirdSideOldSpherePos = thirdSideSphere.transform.position;
            forthSideOldSpherePos = forthSideSphere.transform.position;

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

            topOldSpherePos = topSphere.transform.position;
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
            firstCornerSphere.transform.position = pos;
            firstCornerOldSpherePos = pos;

            // second corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.02f;
            pos.z -= trns.lossyScale.z / 2 + 0.02f;
            secondCornerSphere.transform.position = pos;
            secondCornerOldSpherePos = pos;

            // third corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.02f;
            pos.z += trns.lossyScale.z / 2 + 0.02f;
            thirdCornerSphere.transform.position = pos;
            thirdCornerOldSpherePos = pos;

            // forth corner
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x -= trns.lossyScale.x / 2 + 0.02f;
            pos.z += trns.lossyScale.z / 2 + 0.02f;
            forthCornerSphere.transform.position = pos;
            forthCornerOldSpherePos = pos;

            // first side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x -= trns.lossyScale.x / 2 + 0.01f;

            firstSideSphere.transform.position = pos;
            firstSideOldSpherePos = pos;

            // second side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();
            pos.x += trns.lossyScale.x / 2 + 0.01f;

            secondSideSphere.transform.position = pos;
            secondSideOldSpherePos = pos;

            // third side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();

            pos.z -= trns.lossyScale.z / 2 + 0.01f;
            thirdSideSphere.transform.position = pos;
            thirdSideOldSpherePos = pos;

            // forth side
            pos = objectToScale.transform.position;
            pos.y = objectToScale.GetRoof();

            pos.z += trns.lossyScale.z / 2 + 0.01f;
            forthSideSphere.transform.position = pos;
            forthSideOldSpherePos = pos;
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
            Destroy(firstCornerSphere);
            Destroy(secondCornerSphere);
            Destroy(thirdCornerSphere);
            Destroy(forthCornerSphere);
            Destroy(firstSideSphere);
            Destroy(secondSideSphere);
            Destroy(thirdSideSphere);
            Destroy(forthSideSphere);
            Destroy(endWithSave);
            Destroy(endWithOutSave);
            objectToScale = null;
            instantiated = false;
        }
    }
}