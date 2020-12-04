﻿#if UNITY_EDITOR

using System;
using System.Linq;
using SEE.Controls;
using SEE.Controls.Actions;
using SEE.DataModel;
using SEE.Game;
using SEE.Game.Charts.VR;
using SEE.Utils;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Plane = SEE.GO.Plane;
using PlayerSettings = SEE.Controls.PlayerSettings;

namespace SEEEditor
{
    /// <summary>
    /// An editor for the player settings class. Allows the user to set platform settings and create new code cities.
    /// </summary>
    [CustomEditor(typeof(PlayerSettings))]
    [CanEditMultipleObjects]
    public class PlayerSettingsEditor : Editor
    {
        /// <summary>
        /// An array of all types of code cities which the user should be able to create.
        /// </summary>
        private static readonly Type[] CityTypes = {
            // If there are SEECity types not listed in the menu, you can add them here.
            typeof(SEECity), typeof(SEECityEvolution), typeof(SEECityRandom), typeof(SEEDynCity), typeof(SEEJlgCity)
        };

        /// <summary>
        /// Names of the city types. This is automatically generated from <see cref="CityTypes"/> and shouldn't
        /// need to be changed.
        /// </summary>
        private static readonly string[] CityTypeNames = CityTypes.Select(x => x.Name).ToArray();
        
        // A few variables which help us keep track of the UI state
        private string cityName;
        private bool showCreation = true;
        private bool showPlatform = true;
        private int selectedType;
        
        public override void OnInspectorGUI()
        {
            // Platform settings which are defined in PlayerSettings class
            showPlatform = EditorGUILayout.Foldout(showPlatform, "Platform settings", true, EditorStyles.foldoutHeader);
            if (showPlatform)
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();  // additional space for improved readability
            }

            EditorGUILayout.Space();
                
            // Creation of new code city
            CodeCityGUI();
        }

        /// <summary>
        /// The GUI components responsible for configuring and creating a code city.
        /// </summary>
        private void CodeCityGUI()
        {
            showCreation = EditorGUILayout.Foldout(showCreation, "Create a new code city", true, EditorStyles.foldoutHeader);
            if (showCreation)
            {
                cityName = EditorGUILayout.TextField("Name of new city", cityName);
                EditorGUILayout.BeginHorizontal();
                // Dropdown of all code city types
                selectedType = EditorGUILayout.Popup("City type", selectedType, CityTypeNames);
                if (GUILayout.Button("Create City"))
                {
                    CreateCodeCity();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                GUILayout.Label("Setup new scene", EditorStyles.largeLabel);
                if (GUILayout.Button("Add required objects to scene"))
                {
                    SetupScene();
                }
            }
        }

        /// <summary>
        /// Creates a new code city out of the parameters set in this editor.
        /// </summary>
        private void CreateCodeCity()
        {
            GameObject codeCity = new GameObject {tag = Tags.CodeCity, name = cityName};
            codeCity.transform.localScale = new Vector3(1f, 0.0001f, 1f); // choose sensible y-scale

            // Add required components
            codeCity.AddComponent<MeshRenderer>();
            codeCity.AddComponent<BoxCollider>();
            // Attach portal plane to navigation action components
            Plane plane = codeCity.AddComponent<Plane>();
            codeCity.AddComponent<DesktopNavigationAction>().portalPlane = plane;
            codeCity.AddComponent<XRNavigationAction>().portalPlane = plane;
            codeCity.AddComponent(CityTypes[selectedType]);
        }

        /// <summary>
        /// Creates all required GameObjects for a scene to work, barring a code city.
        /// </summary>
        private void SetupScene()
        {
            //TODO: Check if objects are already there and only add as necessary
            //TODO: Make compatible with MRTK
            
            // Create light
            GameObject light = new GameObject{ name = "Light" };
            light.AddComponent<Light>().lightmapBakeType = LightmapBakeType.Mixed;
            
            // Create table from table prefab
            UnityEngine.Object tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Table.prefab");
            GameObject table = Instantiate(tablePrefab) as GameObject;
            UnityEngine.Assertions.Assert.IsNotNull(table);
            table.name = "Table";
            table.tag = Tags.CullingPlane;
            
            // Create VRPlayer from SteamVR prefab
            SetupVRPlayer(out GameObject vrCamera);

            // Create Desktop player from prefab
            UnityEngine.Object desktopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Players/DesktopPlayer.prefab");
            GameObject desktopPlayer = Instantiate(desktopPrefab) as GameObject;
            UnityEngine.Assertions.Assert.IsNotNull(desktopPlayer);
            desktopPlayer.name = "DesktopPlayer";
            desktopPlayer.tag = Tags.MainCamera;
            desktopPlayer.GetComponent<DesktopPlayerMovement>().focusedObject = table.GetComponent<Plane>();
            
            // Create InControl from prefab
            UnityEngine.Object inControlPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Players/InControl.prefab");
            GameObject inControl = Instantiate(inControlPrefab) as GameObject;
            UnityEngine.Assertions.Assert.IsNotNull(inControl);
            inControl.name = "InControl";
            
            // Create ChartManager from prefab
            UnityEngine.Object chartManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Charts/ChartManager.prefab");
            GameObject chartManager = Instantiate(chartManagerPrefab) as GameObject;
            UnityEngine.Assertions.Assert.IsNotNull(chartManager);
            chartManager.name = "Chart Manager";
            chartManager.transform.GetChild(0).GetComponent<ChartPositionVr>().cameraTransform = vrCamera.transform;
        }

        /// <summary>
        /// Sets up the VR player for use in a scene.
        /// </summary>
        /// <param name="vrCamera">This will be filled with the child camera object of the VRPlayer.</param>
        private static void SetupVRPlayer(out GameObject vrCamera)
        {
            UnityEngine.Object steamVrPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SteamVR/InteractionSystem/Core/Prefabs/Player.prefab");
            GameObject vrPlayer = Instantiate(steamVrPrefab) as GameObject;
            UnityEngine.Assertions.Assert.IsNotNull(vrPlayer);
            vrPlayer.name = "VRPlayer";
            // We need to find the right and left hand first to use them later
            Hand rightHand = GameObjectHierarchy.Descendants(vrPlayer)
                .First(x => x.name == "RightHand").GetComponent<Hand>();
            Hand leftHand = GameObjectHierarchy.Descendants(vrPlayer)
                .First(x => x.name == "LeftHand").GetComponent<Hand>();
            // We also need the camera later for the ChartManager
            vrCamera = GameObjectHierarchy.Descendants(vrPlayer, Tags.MainCamera).First();
            CharacterController vrController = vrPlayer.AddComponent<CharacterController>();
            vrPlayer.AddComponent<SteamVR_ActivateActionSetOnLoad>();
            vrPlayer.AddComponent<XRChartAction>();
            vrPlayer.AddComponent<XRRay>().PointingHand = rightHand;
            XRPlayerMovement playerMovement = vrPlayer.AddComponent<XRPlayerMovement>();
            playerMovement.characterController = vrController;
            playerMovement.DirectingHand = leftHand;
        }
    }
}

#endif