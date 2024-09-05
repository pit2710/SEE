using FuzzySharp;
using Michsky.UI.ModernUIPack;
using MoreLinq;
using SEE.Controls;
using SEE.DataModel.DG;
using SEE.Game.Drawable;
using SEE.GO;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace SEE.UI.Window
{
    /// <summary>
    /// Represents a movable, scrollable window containing metrics of a <see cref="GraphElement"/>.
    /// It consists of a search field and a list of properties, where each property is represented by a row
    /// holding the attribute name and its value.
    /// </summary>
    public class PropertyWindow : BaseWindow
    {
        /// <summary>
        /// GraphElement whose metrics are to be shown.
        /// </summary>
        public GraphElement GraphElement;

        /// <summary>
        /// Prefab for the <see cref="PropertyWindow"/>.
        /// </summary>
        private static string WindowPrefab => UIPrefabFolder + "PropertyWindow";

        /// <summary>
        /// Prefab for the <see cref="PropertyRowLine"/>.
        /// </summary>
        private static string ItemPrefab => UIPrefabFolder + "PropertyRowLine";

        /// <summary>
        /// The alpha keys for the gradient of a menu item (fully opaque).
        /// </summary>
        private static readonly GradientAlphaKey[] alphaKeys = { new(1, 0), new(1, 1) };

        /// <summary>
        /// The amount by which the text of an item is indented per level.
        /// </summary>
        private const int indentShift = 22;

        protected override void StartDesktop()
        {
            base.StartDesktop();
            CreateUIInstance();
        }

        /// <summary>
        /// Activates all <paramref name="propertyRows"/> if they match the <paramref name="searchQuery"/>.
        /// All others are deactivated. In other words, the <paramref name="searchQuery"/> is applied as
        /// a filter.
        /// </summary>
        /// <param name="searchQuery"> attribute name to search for </param>
        /// <param name="propertyRows">mapping of attribute names onto gameObjects representing
        /// the corresponding metric row</param>
        private static void ActivateMatches(string searchQuery, Dictionary<string, (string value, GameObject gameObject)> propertyRows)
        {
            // Remove whitespace.
            searchQuery = searchQuery.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                // There is no search query, so activate all metric rows.
                SetActive(propertyRows, true);
            }
            else
            {
                // First, deactivate all metric rows and then activate only those that match the
                // search results.
                SetActive(propertyRows, false);
                foreach (string attributeName in Search(searchQuery, propertyRows))
                {
                    if (propertyRows.TryGetValue(attributeName, out (string v, GameObject activeObject) t))
                    {
                        t.activeObject.SetActive(true);
                    }
                }
            }

            return;

            static void SetActive(Dictionary<string, (string, GameObject)> searchableObjects, bool activate)
            {
                foreach ((_, GameObject go) in searchableObjects.Values)
                {
                    go.SetActive(activate);
                }
            }
        }

        /// <summary>
        /// Returns the attribute names of all <paramref name="propertyRows"/> whose attribute name or value matches the
        /// <paramref name="query"/>.
        /// </summary>
        /// <param name="query"> the search query (part of an attribute name / value)</param>
        /// <param name="propertyRows"> the dictionary representing property rows to search through</param>
        /// <returns> the attribute names / values matching the <paramref name="query"/> </returns>
        private static IEnumerable<string> Search(string query, Dictionary<string, (string value, GameObject gameObject)> propertyRows)
        {
            List<string> results = new();
            foreach(string key in propertyRows.Keys)
            {
                if (key.ToLower().Contains(query.ToLower()) || propertyRows[key].value.ToLower().Contains(query.ToLower()))
                {
                    results.Add(key);
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the name of a node attribute stored in the first child of the <paramref name="propertyRow"/>.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the metric window providing
        /// the name and value of a node attribute (metric).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>name of the node attribute</returns>
        private static string AttributeName(GameObject propertyRow)
        {
            return Attribute(propertyRow).text;
        }

        /// <summary>
        /// Returns the TMP container of <paramref name="propertyRow"/> holding the attribute name.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the property window providing
        /// the name and value of a node attribute (metric).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>the TMP holding the attribute name</returns>
        /// <remarks>Assumes that the attribute name is stored in the first child of the metric row.</remarks>
        private static TextMeshProUGUI Attribute(GameObject propertyRow)
        {
            return GameFinder.FindChild(propertyRow, "AttributeLine").MustGetComponent<TextMeshProUGUI>();
        }

        private static  string AttributeValue(GameObject propertyRow)
        {
            return Value(propertyRow).text;
        }

        /// <summary>
        /// Returns the TMP container of <paramref name="propertyRow"/> holding the attribute value.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the metric window providing
        /// the name and value of a node attribute (metric).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>the TMP holding the attribute value</returns>
        /// <remarks>Assumes that the attribute name is stored in the second child of the metric row.</remarks>
        private static TextMeshProUGUI Value(GameObject propertyRow)
        {
            return GameFinder.FindChild(propertyRow, "ValueLine").MustGetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Clears all children items of <paramref name="scrollViewContent"/>.
        /// </summary>
        /// <param name="scrollViewContent">The item holder.</param>
        private static void ClearItems(Transform scrollViewContent)
        {
            foreach(Transform item in scrollViewContent)
            {
                DestroyImmediate(item.gameObject);
            }
        }

        /// <summary>
        /// Creates the property window.
        /// </summary>
        public void CreateUIInstance()
        {
            // Instantiate PropertyWindow
            GameObject propertyWindow = PrefabInstantiator.InstantiatePrefab(WindowPrefab, Window.transform.Find("Content"), false);
            propertyWindow.name = "Property Window";

            Transform scrollViewContent = propertyWindow.transform.Find("Content/Items").transform;
            ClearItems(scrollViewContent);
            TMP_InputField searchField = propertyWindow.transform.Find("Search/SearchField").gameObject.MustGetComponent<TMP_InputField>();

            searchField.onSelect.AddListener(_ => SEEInput.KeyboardShortcutsEnabled = false);
            searchField.onDeselect.AddListener(_ => SEEInput.KeyboardShortcutsEnabled = true);

            Dictionary<string, string> data = new()
            {
                { "Kind", GraphElement is Node ? "Node" : "Edge" },
            };
            if (GraphElement is Edge edge)
            {
                data.Add("ID", edge.ID);
                data.Add("Source", edge.Source.ID);
                data.Add("Target", edge.Target.ID);
            }
            data.Add("Type", GraphElement.Type);

            /// Data Attributes
            DisplayAttributes(data, propertyWindow);

            /// Toggle Attributes
            DisplayAttributes(GraphElement.ToggleAttributes.ToDictionary(item => item, item => true), propertyWindow);

            /// String Attributes
            DisplayAttributes(GraphElement.StringAttributes, propertyWindow);

            /// Int Attributes
            DisplayAttributes(GraphElement.IntAttributes, propertyWindow);

            /// Float Attributes
            DisplayAttributes(GraphElement.FloatAttributes, propertyWindow);

            /// Create mapping of attribute names onto gameObjects representing the corresponding property row.
            Dictionary<string, (string, GameObject)> activeElements = new();
            foreach (Transform child in scrollViewContent)
            {
                activeElements.Add(AttributeName(child.gameObject), (AttributeValue(child.gameObject), child.gameObject));
            }

            searchField.onValueChanged.AddListener(searchQuery => ActivateMatches(searchQuery, activeElements));
        }

        /// <summary>
        /// Displays the attributes and their corresponding values.
        /// </summary>
        /// <typeparam name="T">The type of the attribute values.</typeparam>
        /// <param name="attributes">A dictionary containing attribute names (keys) and their corresponding values (values).</param>
        /// <param name="propertyWindowObject">The GameObject representing the property window.</param>
        private static void DisplayAttributes<T>(Dictionary<string, T> attributes, GameObject propertyWindowObject, Transform parent = null, int level = 0)
        {
            parent ??= propertyWindowObject.transform.Find("Content/Items").transform;
            foreach ((string name, T value) in attributes)
            {
                /// Create GameObject
                GameObject propertyRow = PrefabInstantiator.InstantiatePrefab(ItemPrefab, parent, false);
                /// Attribute Name
                Attribute(propertyRow).text = name;
                /// Value Name
                Value(propertyRow).text = value.ToString();
                /// Colors and orders the item
                ColorOrderItem();
                continue;

                void ColorOrderItem()
                {
                    Color[] gradient = new[] { Color.white, Color.white.Darker() };
                    RectTransform background = (RectTransform)propertyRow.transform.Find("Background");
                    background.GetComponent<UIGradient>().EffectGradient.SetKeys(gradient.ToGradientColorKeys().ToArray(), alphaKeys);
                    background.offsetMin = background.offsetMin.WithXY(x: indentShift * level);
                    RectTransform foreground = (RectTransform)propertyRow.transform.Find("Foreground");
                    foreground.offsetMin = foreground.offsetMin.WithXY(x: indentShift * level);
                }
            }
        }

        public override void RebuildLayout()
        {
            // Nothing needs to be done.
        }

        protected override void InitializeFromValueObject(WindowValues valueObject)
        {
            // TODO (#732): Should metric windows be sent over the network?
            throw new NotImplementedException();
        }

        public override void UpdateFromNetworkValueObject(WindowValues valueObject)
        {
            // TODO (#732): Should metric windows be sent over the network?
            throw new NotImplementedException();
        }

        public override WindowValues ToValueObject()
        {
            // TODO (#732): Should metric windows be sent over the network?
            throw new NotImplementedException();
        }
    }
}
