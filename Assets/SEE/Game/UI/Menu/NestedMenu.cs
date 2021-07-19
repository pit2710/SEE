﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace SEE.Game.UI.Menu
{
    /// <summary>
    /// A menu similar to the <see cref="SimpleMenu"/> with buttons which can open other menus.
    /// </summary>
    public class NestedMenu : SimpleMenu<MenuEntry>
    {
        /// <summary>
        /// The menu levels we have ascended through.
        /// </summary>
        private readonly Stack<MenuLevel> Levels = new Stack<MenuLevel>();

        /// <summary>
        /// Path to the NestedMenu prefab.
        /// </summary>
        protected override string MENU_PREFAB => "Prefabs/UI/NestedMenu";

        /// <summary>
        /// The keyword to be used to step back in the menu verbally.
        /// </summary>
        private const string BackMenuCommand = "go back";

        /// <summary>
        /// Whether to reset the level of the menu when clicking on the close button.
        /// Can only be changed before this component has been started.
        /// </summary>
        public bool ResetLevelOnClose = true;

        protected override void OnEntrySelected(MenuEntry entry)
        {
            if (entry is NestedMenuEntry nestedEntry)
            {
                // If this contains another menu level, repopulate list with new level after saving the current one
                AscendLevel(nestedEntry);
            }
            else
            {
                // Otherwise, we do the same we'd do normally
                base.OnEntrySelected(entry);
            }
        }

        /// <summary>
        /// Returns the titles of all <see cref="entries"/> plus
        /// <see cref="CloseMenuCommand"/> appended at the end.
        /// </summary>
        /// <returns>titles of all <see cref="entries"/> appended by 
        /// <see cref="CloseMenuCommand"/></returns>
        protected override string[] GetMenuEntryTitles()
        {
            return entries.Select(x => x.Title).Append(CloseMenuCommand).Append(BackMenuCommand).ToArray();
        }

        /// <summary>
        /// Callback registered in <see cref="Listen(bool)"/> to be called when
        /// one of the menu entry titles was recognized (spoken by the user).
        /// Triggers the corresponding action of the selected entry if the 
        /// corresponding entry title was recognized and then closes the menu 
        /// again. If only <see cref="CloseMenuCommand"/> was recognized, no 
        /// action will be triggered, yet the menu will be closed, too.
        /// </summary>
        /// <param name="args">the phrase recognized</param>
        protected override void OnMenuEntryTitleRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log(args.text);
            Debug.Log("overwritten");
            int i = 0;
            foreach (string keyword in GetMenuEntryTitles())
            {
                if (args.text == keyword)
                {
                    if (args.text == CloseMenuCommand)
                    {
                        ToggleMenu();
                    }
                    if (args.text == BackMenuCommand)
                    {
                        DescendLevel();
                    }
                    else
                    {
                        SelectEntry(i);
                    }
                    break;
                }
                i++;
            }
        }

        /// <summary>
        /// Ascends up in the menu hierarchy by creating a new level from the given <paramref name="nestedEntry"/>.
        /// </summary>
        /// <param name="nestedEntry">The entry from which to construct the new level</param>
        private void AscendLevel(NestedMenuEntry nestedEntry)
        {
            Levels.Push(new MenuLevel(Title, Description, Icon, entries));
            while (entries.Count != 0)
            {
                RemoveEntry(entries[0]); // Remove all entries
            }
            Title = nestedEntry.Title;
            //TODO: Instead of abusing the description for this, use a proper individual text object
            // (Maybe displaying it above the title in a different color or something would work,
            // as the title is technically the last element in the breadcrumb)
            string breadcrumb = GetBreadcrumb();
            Description = nestedEntry.Description + (breadcrumb.Length > 0 ? $"\nHierarchy: {GetBreadcrumb()}" : "");
            Icon = nestedEntry.Icon;
            nestedEntry.InnerEntries.ForEach(AddEntry);
            keywordInput.Unregister(OnMenuEntryTitleRecognized);
            keywordInput.Register(OnMenuEntryTitleRecognized);
            Tooltip.Hide();
        }

        /// <summary>
        /// Returns a "breadcrumb" for the current level, displaying our current position in the menu hierarchy.
        /// </summary>
        /// <returns>breadcrumb for the current level</returns>
        private string GetBreadcrumb() => string.Join(" / ", Levels.Reverse().Select(x => x.Title));
        
        /// <summary>
        /// Descends down a level in the menu hierarchy and removes the entry from the <see cref="Levels"/>.
        /// </summary>
        private void DescendLevel()
        {
            if (Levels.Count != 0)
            {
                MenuLevel level = Levels.Pop();
                Title = level.Title;
                Description = level.Description;
                Icon = level.Icon;
                while (entries.Count != 0)
                {
                    RemoveEntry(entries[0]); // Remove all entries
                }
                level.Entries.ForEach(AddEntry);
            }
            else
            {
                ShowMenu(false);
            }
            Tooltip.Hide();
        }

        /// <summary>
        /// Resets to the lowest level, i.e. resets the menu to the state it was in before any
        /// <see cref="NestedMenuEntry"/> was clicked.
        /// </summary>
        public void ResetToBase()
        {
            while (Levels.Count > 0)
            {
                DescendLevel();
            }
        }

        protected override void StartDesktop()
        {
            base.StartDesktop();
            Manager.onCancel.AddListener(DescendLevel); // Go one level higher when clicking "back"
            if (ResetLevelOnClose)
            {
                Manager.onConfirm.AddListener(ResetToBase); // When closing the menu, its level will be reset to the top
            }
        }

        /// <summary>
        /// A state of this menu representing a nesting level established by selecting a <see cref="NestedMenuEntry"/>.
        /// Contains everything necessary to restore the menu to this state.
        /// </summary>
        private class MenuLevel
        {
            public readonly string Title;
            public readonly string Description;
            public readonly Sprite Icon;
            public readonly List<MenuEntry> Entries;

            public MenuLevel(string title, string description, Sprite icon, IList<MenuEntry> entries)
            {
                Title = title ?? throw new ArgumentNullException(nameof(title));
                Description = description ?? throw new ArgumentNullException(nameof(description));
                Icon = icon;
                Entries = entries != null ? new List<MenuEntry>(entries) : throw new ArgumentNullException(nameof(entries));
            }
        }
    }
}