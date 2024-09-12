using DG.Tweening;
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
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SEE.UI.Window.PropertyWindow
{
    /// <summary>
    /// Represents a movable, scrollable window containing properties of a <see cref="GraphElement"/>.
    /// It consists of a search field and a list of properties, where each property is represented by a row
    /// holding the attribute name and its value.
    /// </summary>
    public class PropertyWindow : BaseWindow
    {
        /// <summary>
        /// GraphElement whose properties are to be shown.
        /// </summary>
        public GraphElement GraphElement;

        /// <summary>
        /// Prefab for the <see cref="PropertyWindow"/>.
        /// </summary>
        private readonly string WindowPrefab = UIPrefabFolder + "PropertyWindow";

        /// <summary>
        /// Prefab for the <see cref="PropertyRowLine"/>.
        /// </summary>
        private readonly string ItemPrefab = UIPrefabFolder + "PropertyRowLine";

        /// <summary>
        /// Prefab for the groups.
        /// </summary>
        private readonly string GroupPrefab = UIPrefabFolder + "PropertyGroupItem";

        /// <summary>
        /// The alpha keys for the gradient of a menu item (fully opaque).
        /// </summary>
        private readonly GradientAlphaKey[] alphaKeys = { new(1, 0), new(1, 1) };

        /// <summary>
        /// The amount by which the text of an item is indented per level.
        /// </summary>
        private const int indentShift = 22;

        /// <summary>
        /// The context menu that is displayed when the user uses the filter, gorup or sort buttons.
        /// </summary>
        private PropertyWindowContextMenu contextMenu;

        /// <summary>
        /// Transform of the object containing the items of the property window.
        /// </summary>
        private RectTransform items;

        /// <summary>
        /// The input field in which the user can enter a search term.
        /// </summary>
        private TMP_InputField searchField;

        /// <summary>
        /// The dictionary that holds the items for a group.
        /// </summary>
        private readonly Dictionary<string, List<GameObject>> groupHolder = new();

        /// <summary>
        /// A set of all items that have been expanded.
        /// Note that this may contain items that are not currently visible due to collapsed parents.
        /// Such items will be expanded when they become visible again.
        /// </summary>
        private readonly ISet<string> expandedItems = new HashSet<string>();

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
        /// the corresponding property row</param>
        private void ActivateMatches(string searchQuery)
        {
            Dictionary<string, (string, GameObject)> propertyRows = new();
            /// Create mapping of attribute names onto gameObjects representing the corresponding property row.
            foreach (Transform child in items)
            {
                if (!propertyRows.ContainsKey(AttributeName(child.gameObject)))
                {
                    propertyRows.Add(AttributeName(child.gameObject), (AttributeValue(child.gameObject), child.gameObject));
                }
                else
                {
                    /// If the key already in use.
                    Debug.Log("Key bereits in Benutzung ("+AttributeName(child.gameObject)+")");
                    string name = AttributeName(child.gameObject) + RandomStrings.GetRandomString(10);
                    while(propertyRows.ContainsKey(name))
                    {
                        name = AttributeName(child.gameObject) + RandomStrings.GetRandomString(10);
                    }
                    Debug.Log("Verwende stattdessen: " + name);
                    propertyRows.Add(name, (AttributeValue(child.gameObject), child.gameObject));
                }
            }

            /// Remove whitespace.
            searchQuery = searchQuery.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                /// There is no search query, so activate the main property rows.
                ActivateMainProperties();
            }
            else
            {
                /// First, deactivate all property rows and then activate only those that match the
                /// search results.
                SetActive(propertyRows, false);
                foreach (string attributeName in Search(searchQuery, propertyRows))
                {
                    if (propertyRows.TryGetValue(attributeName, out (string v, GameObject activeObject) t))
                    {
                        t.activeObject.SetActive(true);
                        foreach (KeyValuePair<string, List<GameObject>> pair in groupHolder)
                        {
                            if (pair.Value.Contains(t.activeObject))
                            {
                                GameObject group = pair.Value.ToList().Find(go => go.name == pair.Key);
                                if (group != null)
                                {
                                    group.SetActive(true);
                                    RotateExpandIcon(group, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates the main property rows.
        /// </summary>
        private void ActivateMainProperties()
        {
            foreach (KeyValuePair<string, List<GameObject>> pair in groupHolder)
            {
                GameObject group = pair.Value.ToList().Find(go => go.name == pair.Key);
                if (group != null)
                {
                    bool expanded = expandedItems.Contains(pair.Key);
                    foreach (GameObject go in pair.Value)
                    {
                        go.SetActive(expanded);
                    }
                    group.SetActive(true);
                    RotateExpandIcon(group, expanded);
                }
                else
                {
                    foreach (GameObject go in pair.Value)
                    {
                        go.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the activity of the game objects of the given dictionary (<paramref name="objects"/>)
        /// </summary>
        /// <param name="objects">The objects to set their activity.</param>
        /// <param name="activate">The activity</param>
        private void SetActive(Dictionary<string, (string, GameObject)> objects, bool activate)
        {
            foreach ((_, GameObject go) in objects.Values)
            {
                go.SetActive(activate);

                if (activate)
                {
                    AnimateIn(go);
                }
                else
                {
                    if (GameFinder.FindChild(go, "Expand Icon") != null)
                    {
                        RotateExpandIcon(go, false);
                    }
                }

                if (expandedItems.Contains(go.name))
                {
                    SetActive(GetDictOfGroup(go.name), activate);
                    RotateExpandIcon(go, activate);
                }
            }
            return;

            /// Expands the game object by animating its scale.
            void AnimateIn(GameObject go)
            {
                go.transform.localScale = new Vector3(1, 0, 1);
                go.transform.DOScaleY(1, duration: 0.5f);
            }
        }

        /// <summary>
        /// Returns the attribute names of all <paramref name="propertyRows"/> whose attribute name or value matches the
        /// <paramref name="query"/>.
        /// </summary>
        /// <param name="query"> the search query (part of an attribute name / value)</param>
        /// <param name="propertyRows"> the dictionary representing property rows to search through</param>
        /// <returns> the attribute names / values matching the <paramref name="query"/> </returns>
        private IEnumerable<string> Search(string query, Dictionary<string, (string value, GameObject gameObject)> propertyRows)
        {
            List<string> results = new();
            foreach (string key in propertyRows.Keys)
            {
                /// AttributeName instead of key checking, as the key may contain a random string if two identical keys would otherwise exist.
                if (AttributeName(propertyRows[key].gameObject).ToLower().Contains(query.ToLower())
                    || propertyRows[key].value.ToLower().Contains(query.ToLower()))
                {
                    results.Add(key);
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the name of a node attribute stored in the first child of the <paramref name="propertyRow"/>.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the property window providing
        /// the name and value of a node attribute (property).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>name of the node attribute</returns>
        private string AttributeName(GameObject propertyRow)
        {
            return Attribute(propertyRow).text;
        }

        /// <summary>
        /// Returns the TMP container of <paramref name="propertyRow"/> holding the attribute name.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the property window providing
        /// the name and value of a node attribute (property).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>the TMP holding the attribute name</returns>
        private TextMeshProUGUI Attribute(GameObject propertyRow)
        {
            return GameFinder.FindChild(propertyRow, "AttributeLine").MustGetComponent<TextMeshProUGUI>();
        }

        private string AttributeValue(GameObject propertyRow)
        {
            return Value(propertyRow) != null ? Value(propertyRow).text : AttributeName(propertyRow);
        }

        /// <summary>
        /// Returns the TMP container of <paramref name="propertyRow"/> holding the attribute value.
        /// Parameter <paramref name="propertyRow"/> is assumed to represent a row in the property window providing
        /// the name and value of a node attribute (property).
        /// </summary>
        /// <param name="propertyRow">a game object representing a pair of an attribute name and an attribute value</param>
        /// <returns>the TMP holding the attribute value</returns>
        private TextMeshProUGUI Value(GameObject propertyRow)
        {
            return GameFinder.FindChild(propertyRow, "ValueLine")?.MustGetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Creates the property window.
        /// </summary>
        public void CreateUIInstance()
        {
            // Instantiate PropertyWindow
            GameObject propertyWindow = PrefabInstantiator.InstantiatePrefab(WindowPrefab, Window.transform.Find("Content"), false);
            propertyWindow.name = "Property Window";

            items = (RectTransform)propertyWindow.transform.Find("Content/Items");
            searchField = propertyWindow.transform.Find("Search/SearchField").gameObject.MustGetComponent<TMP_InputField>();

            searchField.onSelect.AddListener(_ => SEEInput.KeyboardShortcutsEnabled = false);
            searchField.onDeselect.AddListener(_ => SEEInput.KeyboardShortcutsEnabled = true);

            ButtonManagerBasic filterButton = propertyWindow.transform.Find("Search/Filter").gameObject.MustGetComponent<ButtonManagerBasic>();
            ButtonManagerBasic sortButton = propertyWindow.transform.Find("Search/Sort").gameObject.MustGetComponent<ButtonManagerBasic>();
            ButtonManagerBasic groupButton = propertyWindow.transform.Find("Search/Group").gameObject.MustGetComponent<ButtonManagerBasic>();
            PopupMenu.PopupMenu popupMenu = gameObject.AddComponent<PopupMenu.PopupMenu>();
            UnityEvent rebuild = new();
            rebuild.AddListener(() => Rebuild());
            contextMenu = new PropertyWindowContextMenu(popupMenu, rebuild, filterButton, sortButton, groupButton);

            CreateItems();
            searchField.onValueChanged.AddListener(searchQuery => ActivateMatches(searchQuery));
        }

        private void Rebuild()
        {
            ClearItems();
            CreateItems();
        }

        /// <summary>
        /// Clears the property window of all items.
        /// </summary>
        private void ClearItems()
        {
            for (int i = 0; i < items.childCount;)
            {
                DestroyImmediate(items.GetChild(i).gameObject);
            }
            groupHolder.Clear();
        }

        /// <summary>
        /// Creates the items (rows) for the attributes.
        /// It populates the window.
        /// </summary>
        private void CreateItems()
        {
            if (contextMenu.Filter.IncludeHeader)
            {
                Dictionary<string, string> header = new()
                {
                    { "Kind", GraphElement is Node ? "Node" : "Edge" },
                };
                if (GraphElement is Edge edge)
                {
                    header.Add("ID", edge.ID);
                    header.Add("Source", edge.Source.ID);
                    header.Add("Target", edge.Target.ID);
                }
                header.Add("Type", GraphElement.Type);

                /// Data Attributes
                Dictionary<string, (string, GameObject gameObject)> headerItems = DisplayAttributes(header);
                groupHolder.Add("Header", headerItems.Values.Select(x => x.gameObject).ToList());
                expandedItems.Add("Header");
            }
            /// Toggle Attributes
            if (GraphElement.ToggleAttributes.Count > 0 && contextMenu.Filter.IncludeToggleAttributes)
            {
                DisplayGroup("Toggle Attributes", GraphElement.ToggleAttributes.ToDictionary(item => item, item => true));
            }
            if (!contextMenu.Grouper)
            {
                /// String Attributes
                if (GraphElement.StringAttributes.Count > 0 && contextMenu.Filter.IncludeStringAttributes)
                {
                    DisplayGroup("String Attributes", GraphElement.StringAttributes);
                }

                /// Int Attributes
                if (GraphElement.IntAttributes.Count > 0 && contextMenu.Filter.IncludeIntAttributes)
                {
                    DisplayGroup("Int Attributes", GraphElement.IntAttributes);
                }

                /// Float Attributes
                if (GraphElement.FloatAttributes.Count > 0 && contextMenu.Filter.IncludeFloatAttributes)
                {
                    DisplayGroup("Float Attributes", GraphElement.FloatAttributes);
                }
            }
            else
            {
                Dictionary<string, object> attributes = new();
                if (GraphElement.StringAttributes.Count > 0 & contextMenu.Filter.IncludeStringAttributes)
                {
                    foreach (KeyValuePair<string, string> pair in GraphElement.StringAttributes)
                    {
                        attributes.Add(pair.Key, pair.Value);
                    }
                }
                if (GraphElement.IntAttributes.Count > 0 & contextMenu.Filter.IncludeIntAttributes)
                {
                    foreach (KeyValuePair<string, int> pair in GraphElement.IntAttributes)
                    {
                        attributes.Add(pair.Key, pair.Value);
                    }
                }
                if (GraphElement.FloatAttributes.Count > 0 & contextMenu.Filter.IncludeFloatAttributes)
                {
                    foreach (KeyValuePair<string, float> pair in GraphElement.FloatAttributes)
                    {
                        attributes.Add(pair.Key, pair.Value);
                    }
                }
                SplitInAttributeGroup(attributes);
            }

            /// Sorts the properties
            Sort();

            /// Applies the search
            if (!string.IsNullOrEmpty(searchField.text) && !string.IsNullOrWhiteSpace(searchField.text))
            {
                ActivateMatches(searchField.text);
            }
        }

        /// <summary>
        /// Divides the attributes into groups and subgroups.
        /// </summary>
        /// <param name="attributes">The attributes to divide.</param>
        private void SplitInAttributeGroup(Dictionary<string, object> attributes)
        {
            Dictionary<string, object> nestedDict = new();
            foreach (KeyValuePair<string, object> pair in attributes)
            {
                AddNestedAttribute(nestedDict, pair.Key.Split('.'), pair.Value);
            }

            CreateNestedGroups(nestedDict);
        }

        /// <summary>
        /// Inserts an attribute to the nested dictionary.
        /// </summary>
        /// <param name="dict">The nested dictionary.</param>
        /// <param name="keys">The keys to add.</param>
        /// <param name="value">The value of the attribute.</param>
        private void AddNestedAttribute(Dictionary<string, object> dict, string[] keys, object value)
        {
            Dictionary<string, object> currentDict = dict;

            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (!currentDict.ContainsKey(keys[i]))
                {
                    currentDict[keys[i]] = new Dictionary<string, object>();
                }
                currentDict = (Dictionary<string, object>)currentDict[keys[i]];
            }
            currentDict[keys[keys.Length - 1]] = value;
        }

        /// <summary>
        /// Creates the nested groups and attributes.
        /// </summary>
        /// <param name="dict">The nested dicitonary.</param>
        /// <param name="groupName">The group name in which the object should be added.</param>
        /// <param name="level">The hierarchy level.</param>
        private void CreateNestedGroups(Dictionary<string, object> dict, string groupName = null, int level = 0)
        {
            foreach (KeyValuePair<string, object> pair in dict)
            {
                if (pair.Value is Dictionary<string, object> nestedDict)
                {
                    DisplayGroup(pair.Key, new Dictionary<string, string>(), level, groupName);
                    CreateNestedGroups(nestedDict, pair.Key, level + 1);
                }
                else
                {
                    groupName ??= "Header";
                    DisplayAttributes(new Dictionary<string, string>() { { pair.Key, pair.Value.ToString() } },
                        level, expandedItems.Contains(groupName), groupName);
                }
            }
        }

        /// <summary>
        /// Sorts the properties within the group.
        /// </summary>
        private void Sort()
        {
            if (contextMenu.Sorter.IsActive())
            {
                foreach (IEnumerable<GameObject> values in groupHolder.Values)
                {
                    List<GameObject> list = values.ToList();
                    list.RemoveAll(x => !x.name.Contains("RowLine"));
                    ChangeOrder(list);
                }
            }
        }

        /// <summary>
        /// Applies the new order.
        /// </summary>
        /// <param name="listToOrder">The list to be sorted.</param>
        private void ChangeOrder(List<GameObject> listToOrder)
        {
            int lowestSilbing = listToOrder.Min(x => x.transform.GetSiblingIndex());
            List<GameObject> sortedHeader = contextMenu.Sorter.ApplySort(listToOrder);
            for (int i = 0; i < sortedHeader.Count; i++)
            {
                sortedHeader[i].transform.SetSiblingIndex(lowestSilbing);
                lowestSilbing += 1;
            }
        }

        /// <summary>
        /// Displays a attribute group and their corresponding attributes with their values.
        /// </summary>
        /// <typeparam name="T">The type of the attribute values.</typeparam>
        /// <param name="name">The group name</param>
        /// <param name="attributes">A dictionary containing attribute names (keys) and their corresponding values (values).</param>
        /// <param name="level">The level for the group.</param>
        private void DisplayGroup<T>(string name, Dictionary<string, T> attributes, int level = 0, string parentGroup = null)
        {
            GameObject group = PrefabInstantiator.InstantiatePrefab(GroupPrefab, items, false);
            group.name = name;
            GameFinder.FindChild(group, "AttributeLine").MustGetComponent<TextMeshProUGUI>().text = name;
            Dictionary<string, (string, GameObject gameObject)> dict = DisplayAttributes(attributes, level + 1, expandedItems.Contains(group.name));
            OrderGroup();
            RotateExpandIcon(group, expandedItems.Contains(group.name), 0.01f);
            RegisterClickHandler();
            groupHolder.Add(name, dict.Values.Select(x => x.gameObject).Append(group).ToList());
            return;

            void RegisterClickHandler()
            {
                if (group.TryGetComponentOrLog(out PointerHelper pointerHelper))
                {
                    /// expands/collapses the group item
                    pointerHelper.ClickEvent.AddListener(e =>
                    {
                        Dictionary<string, (string, GameObject gameObject)> newDict = GetDictOfGroup(group.name);
                        if (newDict.First().Value.gameObject.activeInHierarchy)
                        {
                            expandedItems.Remove(group.name);
                            SetActive(newDict, false);
                            RotateExpandIcon(group, false);
                        }
                        else
                        {
                            expandedItems.Add(group.name);
                            SetActive(newDict, true);
                            RotateExpandIcon(group, true);
                        }
                    });
                }
            }

            void OrderGroup()
            {
                if (level > 0)
                {
                    RectTransform background = (RectTransform)group.transform.Find("Background");
                    background.offsetMin = background.offsetMin.WithXY(x: indentShift * level);
                    RectTransform foreground = (RectTransform)group.transform.Find("Foreground");
                    foreground.offsetMin = foreground.offsetMin.WithXY(x: indentShift * level);
                    if (parentGroup != null && groupHolder.TryGetValue(parentGroup, out List<GameObject> value))
                    {
                        value.Add(group);
                        group.SetActive(expandedItems.Contains(parentGroup));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve the dictionary of items from a group, excluding the group itself.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <returns>A created dictionary of the group.</returns>
        private Dictionary<string, (string, GameObject)> GetDictOfGroup(string groupName)
        {
            List<GameObject> rowLines = groupHolder.GetValueOrDefault(groupName);
            rowLines.RemoveAll(x => x.name == groupName);
            Dictionary<string, (string, GameObject)> newDict = new();

            foreach (GameObject go in rowLines)
            {
                newDict.Add(AttributeName(go), (AttributeValue(go), go));
            }
            return newDict;
        }

        /// <summary>
        /// Roates the expand icon of a group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="expanded">Whether the group should be expanded or not.</param>
        private void RotateExpandIcon(GameObject group, bool expanded, float duration = 0.5f)
        {
            if (GameFinder.FindChild(group, "Expand Icon").TryGetComponentOrLog(out RectTransform transform))
            {
                Vector3 endValue = expanded ? new Vector3(0, 0, -180) : new Vector3(0, 0, -90);
                transform.DORotate(endValue, duration: duration);
            }
        }

        /// <summary>
        /// Displays the attributes and their corresponding values.
        /// </summary>
        /// <typeparam name="T">The type of the attribute values.</typeparam>
        /// <param name="attributes">A dictionary containing attribute names (keys) and their corresponding values (values).</param>
        /// <param name="level">The level for the property row.</param>
        /// <param name="active">Whether the attributes should be active.</param>
        /// <param name="group">The group to which this attribute is to be assigned.
        /// Used only if the attribute is to be added to the group later.</param>
        private Dictionary<string, (string, GameObject)> DisplayAttributes<T>(Dictionary<string, T> attributes,
            int level = 0, bool active = true, string group = null)
        {
            Dictionary<string, (string, GameObject)> dict = new();
            foreach ((string name, T value) in attributes)
            {
                /// Create GameObject
                GameObject propertyRow = PrefabInstantiator.InstantiatePrefab(ItemPrefab, items, false);
                /// Attribute Name
                Attribute(propertyRow).text = name;
                /// Value Name
                Value(propertyRow).text = value.ToString();
                /// Colors and orders the item
                ColorOrderItem();
                dict.Add(name, (AttributeValue(propertyRow), propertyRow));
                if (group != null && groupHolder.TryGetValue(group, out List<GameObject> groupList))
                {
                    groupList.Add(propertyRow);
                }
                continue;

                void ColorOrderItem()
                {
                    Color[] gradient = new[] { Color.white, Color.white.Darker() };
                    RectTransform background = (RectTransform)propertyRow.transform.Find("Background");
                    background.GetComponent<UIGradient>().EffectGradient.SetKeys(gradient.ToGradientColorKeys().ToArray(), alphaKeys);
                    background.offsetMin = background.offsetMin.WithXY(x: indentShift * level);
                    RectTransform foreground = (RectTransform)propertyRow.transform.Find("Foreground");
                    foreground.offsetMin = foreground.offsetMin.WithXY(x: indentShift * level);
                    propertyRow.SetActive(active);
                }
            }
            return dict;
        }

        public override void RebuildLayout()
        {
            // Nothing needs to be done.
        }

        protected override void InitializeFromValueObject(WindowValues valueObject)
        {
            throw new NotImplementedException();
        }

        public override void UpdateFromNetworkValueObject(WindowValues valueObject)
        {
            throw new NotImplementedException();
        }

        public override WindowValues ToValueObject()
        {
            throw new NotImplementedException();
        }
    }
}
