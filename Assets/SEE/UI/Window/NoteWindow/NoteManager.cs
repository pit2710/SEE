using SEE.DataModel.DG;
using SEE.GO;
using SEE.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.UI.Window.NoteWindow
{
    /// <summary>
    /// This class manages to save and load the notes.
    /// </summary>
    public class NoteManager : MonoBehaviour
    {
        private static NoteManager instance;

        /// <summary>
        /// The material to outline nodes and edges.
        /// </summary>
        private Material noteMaterial;

        /// <summary>
        /// Provides a instance of the <see cref="NoteManager"/> class.
        /// </summary>
        public static NoteManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("NoteManager");
                    instance = go.AddComponent<NoteManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        /// <summary>
        /// This Dictionary saves the content of the notes.
        /// </summary>
        public Dictionary<KeyValuePair<string, bool>, string> notesDictionary = new Dictionary<KeyValuePair<string, bool>, string>();

        /// <summary>
        /// List of GameObjects. It is used to check �f the GameObject is highlighted or not.
        /// </summary>
        public List<GameObject> objectList = new List<GameObject>();

        /// <summary>
        /// Finds the GameObject for the <see cref="graphElement"/>.
        /// And it marks it with an outliner.
        /// </summary>
        /// <param name="GraphID"></param>
        private void FindGameObjects(GraphElementRef graphElementRef)
        {
            GameObject gameObject = graphElementRef.gameObject;
            if (gameObject != null)
            {
                noteMaterial = Resources.Load<Material>(gameObject.IsNode() ? "Materials/Outliner_MAT" : "Materials/OutlinerEdge_MAT");
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer.materials[meshRenderer.materials.Length - 1].name != noteMaterial.name)
                {
                    objectList.Add(gameObject);
                    Material[] materialsArray = new Material[meshRenderer.materials.Length + 1];
                    Array.Copy(meshRenderer.materials, materialsArray, meshRenderer.materials.Length);
                    materialsArray[meshRenderer.materials.Length] = noteMaterial;
                    meshRenderer.materials = materialsArray;
                }
            }
        }

        /// <summary>
        /// Saves the note by putting it into <see cref="notesDictionary"/>.
        /// </summary>
        /// <param name="graphElementRef">the node/edge to save</param>
        /// <param name="isPublic">flag whether it should be saved public or private</param>
        /// <param name="content">the content to save</param>
        public void SaveNote(GraphElementRef graphElementRef, bool isPublic, string content)
        {
            GraphElement graphElement = graphElementRef.Elem;
            if (!string.IsNullOrEmpty(graphElement.ID))
            {
                KeyValuePair<string, bool> keyPair = new KeyValuePair<string, bool>(graphElement.ID, isPublic);
                notesDictionary[keyPair] = content;
                FindGameObjects(graphElementRef);
            }
            //Debug.Log("graphRef + isPublic + content " + graphElementRef + isPublic +content);
            //Debug.Log("notesDictionary.length: " + notesDictionary.Count);
        }

        /// <summary>
        /// Loads the note.
        /// </summary>
        /// <param name="title">the node/edge to load</param>
        /// <param name="isPublic">flag whether it should load the public or private note</param>
        /// <returns>the content for the note</returns>
        public string LoadNote(string title, bool isPublic)
        {
            KeyValuePair<string, bool> keyPair = new KeyValuePair<string, bool>(title, isPublic);
            if (notesDictionary.ContainsKey(keyPair))
            {
                return notesDictionary[keyPair];
            }
            return "";
        }
    }
}
