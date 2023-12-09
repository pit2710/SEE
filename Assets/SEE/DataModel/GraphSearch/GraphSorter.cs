using System;
using System.Collections.Generic;
using System.Linq;
using SEE.DataModel.DG;

namespace SEE.DataModel.GraphSearch
{
    /// <summary>
    /// A configurable sorter for graph elements, mainly intended for use with <see cref="GraphSearch"/>.
    /// </summary>
    public class GraphSorter : IGraphModifier
    {
        /// <summary>
        /// The attributes to sort by along with whether to sort descending, in the order of precedence.
        /// </summary>
        private readonly List<(string Name, Func<GraphElement, object> GetKey, bool Descending)> SortAttributes = new();

        /// <summary>
        /// Add an attribute to sort by.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to sort by.</param>
        /// <param name="getAttribute">A function that returns the value to sort by for the given element.</param>
        /// <param name="descending">Whether to sort descending.</param>
        public void AddSortAttribute(string attributeName, Func<GraphElement, object> getAttribute, bool descending)
        {
            SortAttributes.Add((attributeName, getAttribute, descending));
        }

        /// <summary>
        /// Removes the sort attribute with the given name.
        /// If there are multiple attributes with the given name, all of them are removed.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to remove.</param>
        public void RemoveSortAttribute(string attributeName)
        {
            SortAttributes.RemoveAll(a => a.Name == attributeName);
        }

        public IEnumerable<T> Apply<T>(IEnumerable<T> elements) where T : GraphElement
        {
            return SortAttributes.Count == 0
                ? elements
                // The first `OrderBy` call is to get an IOrderedEnumerable<T> that we can repeatedly pass to `ThenBy`.
                // `OrderBy` is stable, so the order is preserved, as we are passing in a constant key.
                : SortAttributes.Aggregate(elements.OrderBy(_ => 0),
                                           (current, sortAttribute) =>
                                           {
                                               (_, Func<GraphElement, object> GetKey, bool Descending) = sortAttribute;
                                               return Descending
                                                   ? current.ThenByDescending(x => GetKey(x))
                                                   : current.ThenBy(x => GetKey(x));
                                           });
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
            (string, Func<GraphElement, object>, bool Descending) result = SortAttributes.FirstOrDefault(a => a.Name == attributeName);
            if (result == default)
            {
                return null;
            }
            else
            {
                return result.Descending;
            }
        }


        public bool IsActive() => SortAttributes.Count > 0;

        public void Reset() => SortAttributes.Clear();
    }
}
