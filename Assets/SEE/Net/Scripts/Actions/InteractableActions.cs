﻿using SEE.Controls;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Net
{

    /// <summary>
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractAction"/> before modifying this class!
    /// </summary>
    public class SetHoverAction : AbstractAction
    {
        /// <summary>
        /// Every hovered object of the end point of every client. This is only used by
        /// the server.
        /// </summary>
        internal static readonly Dictionary<IPEndPoint, HashSet<InteractableObject>> HoveredObjects = new Dictionary<IPEndPoint, HashSet<InteractableObject>>();

        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public uint id;

        /// <summary>
        /// Whether the interactable should be hovered.
        /// </summary>
        public bool hover;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be (un)hovered.</param>
        /// <param name="hover">Whether the interactable should be hovered.</param>
        public SetHoverAction(InteractableObject interactable, bool hover)
        {
            Assert.IsNotNull(interactable);

            id = interactable.ID;
            this.hover = hover;
        }

        /// <summary>
        /// Adds/removes the interactable objects of given id to
        /// <see cref="HoveredObjects"/>.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            if (hover)
            {
                InteractableObject interactable = InteractableObject.Get(id);
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
                InteractableObject interactable = InteractableObject.Get(id);
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
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    interactable.SetHover(hover, false);
                }
            }
        }
    }

    /// <summary>
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractAction"/> before modifying this class!
    /// </summary>
    public class SetSelectAction : AbstractAction
    {
        /// <summary>
        /// Every selected object of the end point of every client. This is only used by
        /// the server.
        /// </summary>
        internal static readonly Dictionary<IPEndPoint, HashSet<InteractableObject>> SelectedObjects = new Dictionary<IPEndPoint, HashSet<InteractableObject>>();

        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public uint id;

        /// <summary>
        /// Whether the interactable should be selected.
        /// </summary>
        public bool select;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be (de)selected.</param>
        /// <param name="select">Whether the interactable should be selected.</param>
        public SetSelectAction(InteractableObject interactable, bool select)
        {
            Assert.IsNotNull(interactable);

            id = interactable.ID;
            this.select = select;
        }

        /// <summary>
        /// Adds/removes the interactable objects of given id to
        /// <see cref="SelectedObjects"/>.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            if (select)
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (!SelectedObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables = new HashSet<InteractableObject>();
                        SelectedObjects.Add(requester, interactables);
                    }
                    interactables.Add(interactable);
                }
            }
            else
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (SelectedObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables.Remove(interactable);
                        if (interactables.Count == 0)
                        {
                            SelectedObjects.Remove(requester);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the select value for the interactable object of given id.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    interactable.SetSelect(select, false);
                }
            }
        }
    }

    /// <summary>
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractAction"/> before modifying this class!
    /// </summary>
    public class SetGrabAction : AbstractAction
    {
        /// <summary>
        /// Every grabbed object of the end point of every client. This is only used by
        /// the server.
        /// </summary>
        internal static readonly Dictionary<IPEndPoint, HashSet<InteractableObject>> GrabbedObjects = new Dictionary<IPEndPoint, HashSet<InteractableObject>>();

        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public uint id;

        /// <summary>
        /// Whether the interactable should be grabbed.
        /// </summary>
        public bool grab;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be (un)grabbed.</param>
        /// <param name="grab">Whether the interactable should be grabbed.</param>
        public SetGrabAction(InteractableObject interactable, bool grab)
        {
            Assert.IsNotNull(interactable);

            id = interactable.ID;
            this.grab = grab;
        }

        /// <summary>
        /// Adds/removes the interactable objects of given id to
        /// <see cref="GrabbedObjects"/>.
        /// </summary>
        protected override void ExecuteOnServer()
        {
            if (grab)
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (!GrabbedObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables = new HashSet<InteractableObject>();
                        GrabbedObjects.Add(requester, interactables);
                    }
                    interactables.Add(interactable);
                }
            }
            else
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    IPEndPoint requester = GetRequester();
                    if (GrabbedObjects.TryGetValue(requester, out HashSet<InteractableObject> interactables))
                    {
                        interactables.Remove(interactable);
                        if (interactables.Count == 0)
                        {
                            GrabbedObjects.Remove(requester);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the grab value for the interactable object of given id.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    interactable.SetGrab(grab, false);
                }
            }
        }
    }

    /// <summary>
    /// !!! IMPORTANT !!!
    ///   See <see cref="AbstractAction"/> before modifying this class!
    /// </summary>
    public class SynchronizeInteractableAction : AbstractAction
    {
        /// <summary>
        /// The id of the interactable.
        /// </summary>
        public uint id;

        /// <summary>
        /// The position of the interactable.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation of the interactable.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// The local scale of the interactable or <see cref="Vector3.zero"/>, if the
        /// local scale is not to be synchronized.
        /// </summary>
        public Vector3 localScale;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interactable">The interactable to be synchronized.</param>
        /// <param name="syncLocalScale">Whether the local scale is to be synchronized.
        /// </param>
        public SynchronizeInteractableAction(InteractableObject interactable, bool syncLocalScale)
        {
            Assert.IsNotNull(interactable);

            id = interactable.ID;
            position = interactable.transform.position;
            rotation = interactable.transform.rotation;
            localScale = syncLocalScale ? interactable.transform.localScale : Vector3.zero;
        }

        protected override void ExecuteOnServer()
        {
        }

        /// <summary>
        /// Updates position, rotation and potentially local scale of the interactable
        /// object of given id.
        /// </summary>
        protected override void ExecuteOnClient()
        {
            if (!IsRequester())
            {
                InteractableObject interactable = InteractableObject.Get(id);
                if (interactable)
                {
                    interactable.InteractableSynchronizer?.NotifyJustReceivedUpdate();
                    interactable.transform.position = position;
                    interactable.transform.rotation = rotation;
                    if (localScale.sqrMagnitude > 0.0f)
                    {
                        interactable.transform.localScale = localScale;
                    }
                }
            }
        }
    }

}
