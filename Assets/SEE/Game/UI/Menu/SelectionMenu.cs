﻿using System.Linq;

namespace SEE.Game.UI
{
    /// <summary>
    /// A menu in which the user can choose one active selection out of a menu.
    /// It is assumed that only one selection can be active at a time.
    /// </summary>
    public partial class SelectionMenu: Menu<ToggleMenuEntry>
    {
        protected override void OnEntrySelected(ToggleMenuEntry entry)
        {
            // Disable all entries except the selected one, this will automatically call DoExitAction()
            foreach (ToggleMenuEntry listEntry in entries)
            {
                listEntry.Active = Equals(listEntry, entry);
            }
            // This will ensure that DoAction() is called on entry
            base.OnEntrySelected(entry);
        }

        /// <summary>
        /// Returns the first active entry in the <see cref="entries"/> list.
        /// If no entry is active, <c>null</c> will be returned.
        /// </summary>
        /// <returns>the first active entry in the <see cref="entries"/> list,
        /// or <c>null</c> if there is no such entry.</returns>
        /// <exception cref="InvalidOperationException">If more than one element is active</exception>
        public ToggleMenuEntry GetActiveEntry()
        {
            return Entries.SingleOrDefault(x => x.Active);
        }
    }
}
