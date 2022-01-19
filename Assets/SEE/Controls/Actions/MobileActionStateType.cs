using System;
using System.Collections.Generic;
using System.Linq;
using SEE.Utils;
using UnityEngine;

namespace SEE.Controls.Actions
{
    /// <summary>
    /// The type of a state-based action.
    /// Implemented using the "Enumeration" (not enum) or "type safe enum" pattern.
    /// The following two pages have been used for reference:
    /// <ul>
    /// <li>https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types#implement-an-enumeration-base-class</li>
    /// <li>https://ardalis.com/enum-alternatives-in-c/</li>
    /// </ul>
    /// </summary>
    public class MobileActionStateType
    {
        /// <summary>
        /// A list of all available ActionStateTypes.
        /// </summary>
        public static List<MobileActionStateType> AllTypes { get; } = new List<MobileActionStateType>();

        #region Static Types

        //Vertical Buttons
        public static MobileActionStateType Select { get; } =
            new MobileActionStateType(0, "Select", "Mode to select objects",
                Color.white.Darker(), "Materials/Charts/MoveIcon.png", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Delete { get; } =
            new MobileActionStateType(1, "Delete", "Delete a node on touch",
                Color.white.Darker(), "Materials/ModernUIPack/Trash", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType DeleteMulti { get; } =
            new MobileActionStateType(2, "Delete Multi", "Delete Multi Mode",
                Color.white.Darker(), "Materials/ModernUIPack/Minus", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Rotate { get; } =
            new MobileActionStateType(3, "Rotate", "Rotation Mode",
                Color.white.Darker(), "Materials/ModernUIPack/Refresh", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Move { get; } =
            new MobileActionStateType(4, "Move", "Move Mode",
                Color.white.Darker(), "Materials/ModernUIPack/Horizontal Selector", DeleteAction.CreateReversibleAction);

        // Horizontal Groups
        //select
        public static MobileActionStateType Deselect { get; } =
            new MobileActionStateType(5, "Deselect", "Deselect object",
                Color.white.Darker(), "Materials/ModernUIPack/Cancel Bold", DeleteAction.CreateReversibleAction);
        //delete multi
        public static MobileActionStateType CancelDeletion { get; } =
            new MobileActionStateType(6, "Cancel Deletion", "Cancel the deletion of the selected objects",
                Color.white.Darker(), "Materials/ModernUIPack/CancelBold", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType AcceptDeletion { get; } =
            new MobileActionStateType(7, "Accept Deletion", "Accept the deletion of the selected objects",
                Color.white.Darker(), "Materials/ModernUIPack/CheckBold", DeleteAction.CreateReversibleAction);
        //rotate
        public static MobileActionStateType RotateCity { get; } =
            new MobileActionStateType(8, "Rotate City", "Rotate the City",
                Color.white.Darker(), "placeholderN", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType RotateObject { get; } =
            new MobileActionStateType(9, "Rotate Object", "Rotate an Object",
                Color.white.Darker(), "placeholder1", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType LockedRotate { get; } =
            new MobileActionStateType(10, "Locked Rotation Mode", "Locked Rotation Mode",
                Color.white.Darker(), "placeholder", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType LockedCenter { get; } =
            new MobileActionStateType(11, "Locked Around Center Mode", "Locked Around Center Mode",
                Color.white.Darker(), "placeholder", DeleteAction.CreateReversibleAction);
        //move
        public static MobileActionStateType MoveCity { get; } =
            new MobileActionStateType(12, "Move City", "Move hole City",
                Color.white.Darker(), "PlaceholderN", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType MoveObject { get; } =
            new MobileActionStateType(13, "Move Object", "Move Object Mode",
                Color.white.Darker(), "Placeholder 8", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType EightDirections { get; } =
            new MobileActionStateType(14, "8-Directions Mode", "8-Directions Mode",
                Color.white.Darker(), "Materials/ModernUIPack/Plus", DeleteAction.CreateReversibleAction);

        // Quick Menu Group
        public static MobileActionStateType Redo { get; } =
            new MobileActionStateType(15, "Redo Action", "Redo Action",
                Color.white.Darker(), "placeholderRedo", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Undo { get; } =
            new MobileActionStateType(16, "Undo", "Undo Action",
                Color.white.Darker(), "placeholder Undo", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType CameraLock { get; } =
            new MobileActionStateType(17, "Camera Lock Mode", "Camera Lock Mode",
                Color.white.Darker(), "Materials/ModernUIPack/Lock Open", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Rerotate { get; } =
            new MobileActionStateType(18, "Rerotate", "Set Rotation back to standart",
                Color.white.Darker(), "Materials/ModernUIPack/Refresh", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Recenter { get; } =
            new MobileActionStateType(19, "Recenter", "Recenter the City",
                Color.white.Darker(), "Materials/ModernUIPack/Button", DeleteAction.CreateReversibleAction);
        public static MobileActionStateType Collapse { get; } =
            new MobileActionStateType(20, "Collapse", "Collapse the Quick Menu",
                Color.white.Darker(), "Materials/ModernUIPack/Arrow Bold", DeleteAction.CreateReversibleAction);
        
        #endregion

        /// <summary>
        /// The name of this action.
        /// Must be unique across all types.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description for this action.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Color for this action.
        /// Will be used in the <see cref="DesktopMenu"/> and <see cref="ActionStateIndicator"/>.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Path to the material of the icon for this action.
        /// The icon itself should be a visual representation of the action.
        /// Will be used in the <see cref="DesktopMenu"/>.
        /// </summary>
        public string IconPath { get; }

        /// <summary>
        /// Numeric value of this action.
        /// Must be unique across all types.
        /// Must increase by one for each new instantiation of an <see cref="ActionStateType"/>.
        /// </summary>
        public int MobileValue { get; }

        /// <summary>
        /// Delegate to be called to create a new instance of this kind of action.
        /// May be null if none needs to be created (in which case this delegate will not be called).
        /// </summary>
        public CreateReversibleAction CreateReversible { get; }

        /// <summary>
        /// Constructor allowing to set <see cref="CreateReversible"/>.
        /// 
        /// This constructor is needed for the test cases which implement
        /// their own variants of <see cref="ReversibleAction"/> and 
        /// which need to provide an <see cref="ActionStateType"/> of
        /// their own.
        /// </summary>
        /// <param name="createReversible">value for <see cref="CreateReversible"/></param>
        protected MobileActionStateType(CreateReversibleAction createReversible)
        {
            CreateReversible = createReversible;
        }

        /// <summary>
        /// Constructor for ActionStateType.
        /// Because this class replaces an enum, values of this class may only be created inside of it,
        /// hence the visibility modifier is set to private.
        /// </summary>
        /// <param name="value">The ID of this ActionStateType. Must increase by one for each new instantiation.</param>
        /// <param name="name">The Name of this ActionStateType. Must be unique.</param>
        /// <param name="description">Description for this ActionStateType.</param>
        /// <param name="color">Color for this ActionStateType.</param>
        /// <param name="iconPath">Path to the material of the icon for this ActionStateType.</param>
        /// <exception cref="ArgumentException">When the given <paramref name="name"/> or <paramref name="value"/>
        /// is not unique, or when the <paramref name="value"/> doesn't fulfill the "must increase by one" criterion.
        /// </exception>
        private MobileActionStateType(int value, string name, string description, Color color, string iconPath, CreateReversibleAction createReversible)
        {
            MobileValue = value;
            Name = name;
            Description = description;
            Color = color;
            IconPath = iconPath;
            CreateReversible = createReversible;

            // Check for duplicates
            if (AllTypes.Any(x => x.MobileValue == value || x.Name == name))
            {
                throw new ArgumentException("Duplicate ActionStateTypes may not exist!\n");
            }

            // Check whether the ID is always increased by 1. For this, it suffices to check
            // the most recently added element, as all added elements go through this check.
            if (value != AllTypes.Select(x => x.MobileValue + 1).DefaultIfEmpty(0).Last())
            {
                throw new ArgumentException("ActionStateType IDs must be increasing by one!\n");
            }

            // Add new value to list of all types
            AllTypes.Add(this);
        }

        /// <summary>
        /// Returns the ActionStateType whose ID matches the given parameter.
        /// </summary>
        /// <param name="ID">The ID of the ActionStateType which shall be returned</param>
        /// <returns>the ActionStateType whose ID matches the given parameter</returns>
        /// <exception cref="InvalidOperationException">If no such ActionStateType exists.</exception>
        public static MobileActionStateType FromID(int ID)
        {
            return AllTypes.Single(x => x.MobileValue == ID);
        }

        #region Equality & Comparators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && ((ActionStateType)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        #endregion
    }
}
