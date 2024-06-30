using SEE.GO;
using SEE.UI.Window;
using SEE.UI.Window.NoteWindow;
using SEE.Utils;
using SEE.Utils.History;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.Controls.Actions
{
    internal class NoteAction : AbstractPlayerAction
    {
        private NoteManager noteManager;

        private WindowSpace manager;

        private NoteButtonWindow noteButtonWindow;

        /// <summary>
        /// The material to outline the nodes and edges.
        /// </summary>
        Material noteMaterial = Resources.Load<Material>("Materials/Outliner_MAT");

        /// <summary>
        /// The material to outline the nodes and edges.
        /// </summary>
        Material noteMaterialEdge = Resources.Load<Material>("Materials/OutlinerEdge_MAT");

        /// <summary>
        /// Returns a new instance of <see cref="NoteAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="NoteAction"/></returns>
        internal static IReversibleAction CreateReversibleAction() => new NoteAction();

        /// <summary>
        /// Returns a new instance of <see cref="NoteAction"/>.
        /// </summary>
        /// <returns>new instance</returns>
        public override IReversibleAction NewInstance() => new NoteAction();

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.Notes"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.Notes;
        }

        public override void Start()
        {
            noteManager = NoteManager.Instance;
            //noteButtonWindow = new();
            //GameObject.Find("NoteManager").AddComponent<NoteButtonWindow>();

            manager = WindowSpaceManager.ManagerInstance[WindowSpaceManager.LocalPlayer];

        }

        public override void Stop()
        {
            base.Stop();
            noteButtonWindow.DestroyWindow();
        }

        public override bool Update()
        {
            //Hide all Outlines when 'P' is pressed
            if (Input.GetKeyDown(KeyCode.P))
            {
                HideAllNotes();
                return true;
            }
            //Hide or Show highlightes Nodes/Edges one by one
            if (Input.GetMouseButtonDown(0) && !Raycasting.IsMouseOverGUI())
            {
                HitGraphElement hitElement = Raycasting.RaycastGraphElement(out RaycastHit _, out GraphElementRef _);
                if (hitElement == HitGraphElement.Node || hitElement == HitGraphElement.Edge)
                {
                    Raycasting.RaycastGraphElement(out RaycastHit raycastHit, out GraphElementRef graphElementRef);
                    GameObject elemGameObject = graphElementRef.gameObject;
                    if (noteManager.objectList.Contains(elemGameObject))
                    {
                        HideOrHightlight(elemGameObject);
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
             }
             return false;
        }

        /// <summary>
        /// Hide or highlights every Node and Edge with a note.
        /// </summary>
        private void HideAllNotes()
        {
            foreach (GameObject gameObject in noteManager.objectList)
            {
                HideOrHightlight(gameObject);
            }
        }

        /// <summary>
        /// Hides or highlights the GameObject depending if they have a note.
        /// It hides them if they have an outline active.
        /// It highlights the GameObject if they have a note but no outline active.
        /// </summary>
        /// <param name="gameObject">gameObject that </param>
        private void HideOrHightlight(GameObject gameObject)
        {
            if (gameObject.IsNode())
            {
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

                string oldMaterialName = meshRenderer.materials[meshRenderer.materials.Length - 1].name;
                string newName = oldMaterialName.Replace(" (Instance)", "");
                //GameObject has no outline
                if (newName != noteMaterial.name)
                {
                    Material[] materialsArray = new Material[meshRenderer.materials.Length + 1];
                    Array.Copy(meshRenderer.materials, materialsArray, meshRenderer.materials.Length);
                    materialsArray[meshRenderer.materials.Length] = noteMaterial;
                    meshRenderer.materials = materialsArray;
                }
                //GameObject has outline
                else
                {
                    Material[] gameObjects = new Material[meshRenderer.materials.Length - 1];
                    Array.Copy(meshRenderer.materials, gameObjects, meshRenderer.materials.Length - 1);
                    meshRenderer.materials = gameObjects;
                }
            }
            else
            {
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

                string oldMaterialName = meshRenderer.materials[meshRenderer.materials.Length - 1].name;
                string newName = oldMaterialName.Replace(" (Instance)", "");
                //GameObject has no outline
                if (newName != noteMaterialEdge.name)
                {
                    Material[] materialsArray = new Material[meshRenderer.materials.Length + 1];
                    Array.Copy(meshRenderer.materials, materialsArray, meshRenderer.materials.Length);
                    materialsArray[meshRenderer.materials.Length] = noteMaterialEdge;
                    meshRenderer.materials = materialsArray;
                }
                //GameObject has outline
                else
                {
                    Material[] gameObjects = new Material[meshRenderer.materials.Length - 1];
                    Array.Copy(meshRenderer.materials, gameObjects, meshRenderer.materials.Length - 1);
                    meshRenderer.materials = gameObjects;
                }
            }
        }

        public void RemoveOutline(GameObject gameObject)
        {
            Material noteMaterial = Resources.Load<Material>("Materials/Outliner_MAT");
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            string oldMaterialName = meshRenderer.materials[meshRenderer.materials.Length - 1].name;
            string newName = oldMaterialName.Replace(" (Instance)", "");
            if (newName == noteMaterial.name)
            {
                Material[] gameObjects = new Material[meshRenderer.materials.Length - 1];
                Array.Copy(meshRenderer.materials, gameObjects, meshRenderer.materials.Length - 1);
                meshRenderer.materials = gameObjects;
            }
        }

        public override HashSet<string> GetChangedObjects()
        {
            return new HashSet<string>();
        }
    }
}
