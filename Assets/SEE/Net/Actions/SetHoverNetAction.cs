﻿using System.Collections.Generic;
using System.Net;
using SEE.Controls;
using UnityEngine.Assertions;

namespace SEE.Net.Actions
{
    /// <summary>
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractNetAction"/> before modifying this class!
    /// </summary>
    public class SetHoverNetAction : AbstractNetAction
    {
        /// <summary>
        /// Every hovered object of the end point of every client. This is only used by
        /// the server.
        /// </summary>
        internal static readonly Dictionary<IPEndPoint, HashSet<InteractableObject>> HoveredObjects
            = new ();

        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public string ID;

        /// <summary>
        /// The hover flags of the interactable.
        /// </summary>
        public uint HoverFlags;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be (un)hovered.</param>
        /// <param name="hoverFlags">The hover flags of the interactable.</param>
        public SetHoverNetAction(InteractableObject interactable, uint hoverFlags)
        {
            Assert.IsNotNull(interactable);

            ID = interactable.name;
            this.HoverFlags = hoverFlags;
        }

        /// <summary>
        /// Adds/removes the interactable objects of given id to
        /// <see cref="HoveredObjects"/>.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            if (HoverFlags != 0)
            {
                InteractableObject interactable = InteractableObject.Get(ID);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (!HoveredObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables = new HashSet<InteractableObject>();
                        HoveredObjects.Add(requester, interactables);
                    }
                    interactables.Add(interactable);
                }
            }
            else
            {
                InteractableObject interactable = InteractableObject.Get(ID);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (HoveredObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables.Remove(interactable);
                        if (interactables.Count == 0)
                        {
                            HoveredObjects.Remove(requester);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the hover value for the interactable object of given id.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                InteractableObject interactable = InteractableObject.Get(ID);
                if (interactable)
                {
                    interactable.SetHoverFlags(HoverFlags, false);
                }
            }
        }
    }
}
