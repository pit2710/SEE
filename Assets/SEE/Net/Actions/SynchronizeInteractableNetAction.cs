﻿using SEE.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Net.Actions
{

    /// <summary>
    /// Updates position, rotation and potentially local scale of an interactable
    /// object.
    ///
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractNetAction"/> before modifying this class!
    /// </summary>
    public class SynchronizeInteractableNetAction : AbstractNetAction
    {
        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public string id;

        /// <summary>
        /// The position of the interactable.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation of the interactable.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// The local scale of the interactable or <c>null</c>, if the
        /// local scale is not to be synchronized.
        /// </summary>
        public Vector3? localScale = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be synchronized.</param>
        /// <param name="syncLocalScale">Whether the local scale is to be synchronized.
        /// </param>
        public SynchronizeInteractableNetAction(InteractableObject interactable, bool syncLocalScale)
        {
            Assert.IsNotNull(interactable);

            id = interactable.name;
            position = interactable.transform.position;
            rotation = interactable.transform.rotation;
            if (syncLocalScale)
            {
                localScale = interactable.transform.localScale;
            }
        }

        public override void ExecuteOnServer()
        {
        }

        /// <summary>
        /// Updates position, rotation and potentially local scale of the interactable
        /// object of given id.
        /// </summary>
        public override void ExecuteOnClient()
        {
            InteractableObject interactable = InteractableObject.Get(id);
            if (interactable)
            {
                interactable.InteractableSynchronizer?.NotifyJustReceivedUpdate();
                interactable.transform.position = position;
                interactable.transform.rotation = rotation;
                if (localScale.HasValue)
                {
                    interactable.transform.localScale = localScale.Value;
                }
            }
        }
    }
}
