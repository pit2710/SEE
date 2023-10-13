using SEE.Utils;
using System.Collections.Generic;
using UnityEngine;
using SEE.Net.Actions.Drawable;
using SEE.Game.Drawable.Configurations;
using Assets.SEE.Game;
using Assets.SEE.Game.Drawable;

namespace SEE.Controls.Actions.Drawable
{
    /// <summary>
    /// This class is responsible for changing the layer order of a <see cref="DrawableType"/>
    /// </summary>
    class LayerChangerAction : AbstractPlayerAction
    {
        /// <summary>
        /// Saves all the information needed to revert or repeat this action.
        /// </summary>
        private Memento memento;

        /// <summary>
        /// Bool to identifiy if the action is running.
        /// </summary>
        private bool isInAction = false;

        /// <summary>
        /// This method manages the player's interaction with the mode <see cref="ActionStateType.LayerChanger"/>.
        /// </summary>
        /// <returns>Whether this Action is finished</returns>
        public override bool Update()
        { 
            if (!Raycasting.IsMouseOverGUI())
            {
                /// Increse block - it increses the order in layer of a game object on a drawable with a <see cref="OrderInLayerValueHolder"/> component.
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !isInAction &&
                    Raycasting.RaycastAnythingBackface(out RaycastHit raycastHit) &&
                    GameDrawableFinder.hasDrawable(raycastHit.collider.gameObject) &&
                    raycastHit.collider.gameObject.GetComponent<OrderInLayerValueHolder>() != null)
                {
                    isInAction = true;
                    GameObject hittedObject = raycastHit.collider.gameObject;
                    GameObject drawable = GameDrawableFinder.FindDrawable(hittedObject);

                    int oldOrder = hittedObject.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer();
                    int newOrder = oldOrder + 1;
                    GameLayerChanger.Increase(hittedObject, newOrder);
                    memento = new Memento(drawable, GameLayerChanger.LayerChangerStates.Increase,
                                    hittedObject, hittedObject.name, oldOrder, newOrder);
                    new LayerChangerNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable),
                        memento.obj.name, memento.state, memento.newOrder).Execute();
                    currentState = ReversibleAction.Progress.InProgress;
                }

                /// Decrease block - it decreases the order in layer of a game object on a drawable with a <see cref="OrderInLayerValueHolder"/> component.
                if ((Input.GetMouseButtonDown(1) || Input.GetMouseButton(1)) && !isInAction &&
                    Raycasting.RaycastAnything(out RaycastHit raycastHit2) && GameDrawableFinder.hasDrawable(raycastHit2.collider.gameObject))
                {
                    isInAction = true;
                    GameObject hittedObject = raycastHit2.collider.gameObject;
                    GameObject drawable = GameDrawableFinder.FindDrawable(hittedObject);

                    int oldOrder = hittedObject.GetComponent<OrderInLayerValueHolder>().GetOrderInLayer();
                    int newOrder = oldOrder - 1;
                    newOrder = newOrder < 0 ? 0 : newOrder;
                    GameLayerChanger.Decrease(hittedObject, newOrder);
                    memento = new Memento(drawable, GameLayerChanger.LayerChangerStates.Decrease,
                                hittedObject, hittedObject.name, oldOrder, newOrder);
                    new LayerChangerNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable),
                        memento.obj.name, memento.state, memento.newOrder).Execute();
                    currentState = ReversibleAction.Progress.InProgress;
                }

                /// The completes block. 
                /// It completes the action after a layer changing, if the user releases the pressed mouse button.
                if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) ) && isInAction)
                {
                    isInAction = false;
                    currentState = ReversibleAction.Progress.Completed;
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// This struct can store all the information needed to revert or repeat a <see cref="LayerChangerAction"/>.
        /// </summary>
        private struct Memento
        {
            /// <summary>
            /// The drawable on which the object to be layer changed is located.
            /// </summary>
            public readonly GameObject drawable;
            /// <summary>
            /// Is the state of the layer change.
            /// </summary>
            public readonly GameLayerChanger.LayerChangerStates state;
            /// <summary>
            /// The object that should be changed his order in layer.
            /// </summary>
            public GameObject obj;
            /// <summary>
            /// The id of the object to be changed.
            /// </summary>
            public readonly string id;
            /// <summary>
            /// The old order in layer value.
            /// </summary>
            public readonly int oldOrder;
            /// <summary>
            /// The new order in layer value.
            /// </summary>
            public readonly int newOrder;

            /// <summary>
            /// The constructor, whcih simply assigns its only parameter to a field in this struct.
            /// </summary>
            /// <param name="drawable">The drawable to save into this Memento</param>
            /// <param name="state">The state to save into this Memento</param>
            /// <param name="obj">The object to save into this Memento</param>
            /// <param name="id">The object id to save into this Memento</param>
            /// <param name="oldOrder">The old order in layer value to save into this Memento</param>
            /// <param name="newOrder">The new order in layer value to save into this Memento</param>
            public Memento(GameObject drawable, GameLayerChanger.LayerChangerStates state, GameObject obj, string id, int oldOrder, int newOrder)
            {
                this.drawable = drawable;
                this.state = state;
                this.obj = obj;
                this.id = id;
                this.oldOrder = oldOrder;
                this.newOrder = newOrder;
            }
        }

        /// <summary>
        /// Reverts this action, i.e., changed the order in layer back to it's old value.
        /// </summary>
        public override void Undo()
        {
            base.Undo();
            if (memento.obj == null && memento.id != null)
            {
                memento.obj = GameDrawableFinder.FindChild(memento.drawable, memento.id);
            }
            switch (memento.state)
            {
                case GameLayerChanger.LayerChangerStates.Increase:
                    GameLayerChanger.Decrease(memento.obj, memento.oldOrder);
                    new LayerChangerNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable), 
                        memento.obj.name, GameLayerChanger.LayerChangerStates.Decrease, memento.oldOrder).Execute();
                    break;
                case GameLayerChanger.LayerChangerStates.Decrease:
                    GameLayerChanger.Increase(memento.obj, memento.oldOrder);
                    new LayerChangerNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable), 
                        memento.obj.name, GameLayerChanger.LayerChangerStates.Increase, memento.oldOrder).Execute();
                    break;
            }
        }

        /// <summary>
        /// Repeats this action, i.e., changed the order in layer back to it's new value.
        /// </summary>
        public override void Redo()
        {
            base.Redo();
            if (memento.obj == null && memento.id != null)
            {
                memento.obj = GameDrawableFinder.FindChild(memento.drawable, memento.id);
            }
            switch (memento.state)
            {
                case GameLayerChanger.LayerChangerStates.Increase:
                    GameLayerChanger.Increase(memento.obj, memento.newOrder);
                    break;
                case GameLayerChanger.LayerChangerStates.Decrease:
                    GameLayerChanger.Decrease(memento.obj, memento.newOrder);
                    break;
            }
            new LayerChangerNetAction(memento.drawable.name, GameDrawableFinder.GetDrawableParentName(memento.drawable), 
                memento.obj.name, memento.state, memento.newOrder).Execute();
        }

        /// <summary>
        /// A new instance of <see cref="LayerChangerAction"/>.
        /// See <see cref="ReversibleAction.CreateReversibleAction"/>.
        /// </summary>
        /// <returns>new instance of <see cref="LayerChangerAction"/></returns>
        public static ReversibleAction CreateReversibleAction()
        {
            return new LayerChangerAction();
        }

        /// <summary>
        /// A new instance of <see cref="LayerChangerAction"/>.
        /// See <see cref="ReversibleAction.NewInstance"/>.
        /// </summary>
        /// <returns>new instance of <see cref="LayerChangerAction"/></returns>
        public override ReversibleAction NewInstance()
        {
            return CreateReversibleAction();
        }

        /// <summary>
        /// Returns the <see cref="ActionStateType"/> of this action.
        /// </summary>
        /// <returns><see cref="ActionStateType.LayerChanger"/></returns>
        public override ActionStateType GetActionStateType()
        {
            return ActionStateTypes.LayerChanger;
        }

        /// <summary>
        /// The set of IDs of all gameObjects changed by this action.
        /// <see cref="ReversibleAction.GetActionStateType"/>
        /// Because this action does not actually change any game object, 
        /// an empty set is always returned.
        /// </summary>
        /// <returns>an empty set</returns>
        public override HashSet<string> GetChangedObjects()
        {
            if (memento.obj == null)
            {
                return new HashSet<string>();
            }
            else
            {
                return new HashSet<string>
            {
                memento.obj.name
            };
            }
        }
    }
}
