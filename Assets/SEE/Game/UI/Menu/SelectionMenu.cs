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
            foreach (ToggleMenuEntry listEntry in Entries)
            {
                listEntry.Active = listEntry == entry;
            }
            // This will ensure that DoAction() is called on entry
            base.OnEntrySelected(entry);
        }

        /// <summary>
        /// Returns the first active entry in the <see cref="entries"/> list.
        /// </summary>
        /// <returns>the first active entry in the <see cref="entries"/> list.</returns>
        private ToggleMenuEntry GetActiveEntry()
        {
            return Entries.First(x => x.Active);
        }
    }
}