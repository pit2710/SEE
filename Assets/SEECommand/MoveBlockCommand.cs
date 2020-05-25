﻿using UnityEngine;

namespace SEE.Command
{

    public class MoveBlockCommand : AbstractCommand
    {
        public int id;
        public Vector3 originalPosition;
        public Vector3 newPosition;

        public MoveBlockCommand(GameObject block, Vector3 originalPosition, Vector3 newPosition, bool buffer) : base(buffer)
        {
            id = block.GetComponent<Interactable>().id;
            this.originalPosition = originalPosition;
            this.newPosition = newPosition;
        }

        protected override bool ExecuteOnServer()
        {
            return true;
        }

        protected override bool ExecuteOnClient()
        {
            foreach (Interactable interactable in Object.FindObjectsOfType<Interactable>())
            {
                if (interactable.id == id)
                {
                    interactable.transform.position = newPosition;
                    return true;
                }
            }

            return false;
        }

        protected override bool UndoOnServer()
        {
            return true;
        }

        protected override bool UndoOnClient()
        {
            foreach (Interactable interactable in Object.FindObjectsOfType<Interactable>())
            {
                if (interactable.id == id)
                {
                    if (interactable.transform.position == newPosition)
                    {
                        interactable.transform.position = originalPosition;
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        protected override bool RedoOnServer()
        {
            return true;
        }

        protected override bool RedoOnClient()
        {
            bool result = ExecuteOnClient();
            return result;
        }
    }

}
