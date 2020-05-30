﻿using SEE.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Net
{

    public class SelectionAction : AbstractAction
    {
        public uint oldID;
        public uint newID;



        public SelectionAction(HoverableObject oldHoverableObject, HoverableObject newHoverableObject) : base(true)
        {
            Assert.IsTrue(oldHoverableObject != newHoverableObject);
            Assert.IsTrue(oldHoverableObject != null || newHoverableObject != null);

            oldID = oldHoverableObject ? oldHoverableObject.id : uint.MaxValue;
            newID = newHoverableObject ? newHoverableObject.id : uint.MaxValue;
        }



        protected override bool ExecuteOnServer()
        {
            return true;
        }

        protected override bool ExecuteOnClient()
        {
            HoverableObject oldHoverableObject = (HoverableObject)InteractableObject.Get(oldID);
            HoverableObject newHoverableObject = (HoverableObject)InteractableObject.Get(newID);

            Assert.IsTrue(oldHoverableObject != null || newHoverableObject != null);

            if (oldHoverableObject)
            {
                oldHoverableObject.Unhovered();
            }

            if (newHoverableObject)
            {
                newHoverableObject.Hovered(IsRequester());
            }

            return true;
        }

        protected override bool UndoOnServer()
        {
            throw new System.NotImplementedException();
        }

        protected override bool UndoOnClient()
        {
            throw new System.NotImplementedException();
        }

        protected override bool RedoOnServer()
        {
            throw new System.NotImplementedException();
        }

        protected override bool RedoOnClient()
        {
            throw new System.NotImplementedException();
        }
    }

}
