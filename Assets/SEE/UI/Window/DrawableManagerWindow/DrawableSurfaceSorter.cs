﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SEE.UI.Window.DrawableManagerWindow
{
    public class DrawableSurfaceSorter
    {
        /// <summary>
        /// The attributes to sort by along with whether to sort descending, in the order of precedence.
        /// </summary>
        private readonly List<(string Name, Func<GameObject, object> GetKey, bool Descending)> sortAttributes = new();

        /// <summary>
        /// Add an attribute to sort by.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to sort by.</param>
        /// <param name="getAttribute">A function that returns the value to sort by for the given element.</param>
        /// <param name="descending">Whether to sort descending.</param>
        public void AddSortAttribute(string attributeName, Func<GameObject, object> getAttribute, bool descending)
        {
            sortAttributes.Add((attributeName, getAttribute, descending));
        }

        /// <summary>
        /// Removes the sort attribute with the given name.
        /// If there are multiple attributes with the given name, all of them are removed.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to remove.</param>
        public void RemoveSortAttribute(string attributeName)
        {
            sortAttributes.RemoveAll(a => a.Name == attributeName);
        }

        /// <summary>
        /// Applies the sort.
        /// </summary>
        /// <param name="list">The list to be sorted.</param>
        /// <returns>A sorted IEnumerable.</returns>
        private IEnumerable<GameObject> Apply(List<GameObject> list)
        {
            return sortAttributes.Count == 0 ? list :
                sortAttributes.Aggregate(list.OrderBy(_ => 0), 
                (current, sortAttribute) =>
                {
                    (_, Func<GameObject, object> getKey, bool descending) = sortAttribute;
                    return descending? current.ThenByDescending(x => getKey(x))
                        : current.ThenBy(x => getKey(x));
                });
        }

        /// <summary>
        /// Applies the sort and transform it to a list.
        /// </summary>
        /// <param name="list">The list to be sorted.</param>
        /// <returns>The sorted list.</returns>
        public List<GameObject> ApplySort(List<GameObject> list)
        {
            return new List<GameObject>(Apply(list));
        }

        /// <summary>
        /// Whether the given attribute is sorted descending.
        /// Note that this returns null if the attribute is not sorted at all.
        /// </summary>
        /// <param name="attributeName">The attribute to check.</param>
        /// <returns>Whether the attribute is sorted descending, or null if it is not sorted at all.</returns>
        /// <remarks>
        /// If there is more than one attribute with the given name, the first one is returned.
        /// </remarks>
        public bool? IsAttributeDescending(string attributeName)
        {
            (string, Func<GameObject, object>, bool Descending) result = sortAttributes.FirstOrDefault(a => a.Name == attributeName);
            if (result == default)
            {
                return null;
            }
            else
            {
                return result.Descending;
            }
        }

        /// <summary>
        /// Query if the sorter is active.
        /// </summary>
        /// <returns></returns>
        public bool IsActive() => sortAttributes.Count > 0;

        /// <summary>
        /// Resets the sorter to default.
        /// </summary>
        public void Reset() => sortAttributes.Clear();
    }
}