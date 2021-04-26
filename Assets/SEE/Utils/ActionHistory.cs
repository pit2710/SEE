using SEE.Net;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SEE.Utils
{
    public class ActionHistory
    {
        /// <summary>
        /// An enum which sets the type of an action in the history.
        /// </summary>
        public enum HistoryType
        {
            action,
            undoneAction,
        };

        /// <summary>
        /// If a user has done a undo
        /// </summary>
        private bool isRedo = false;

        /// <summary>
        /// The actionList it has an Tupel of a bool Isowner, The type of the Action (Undo Redo Action), the id of the ReversibleAction, the list with the ids of the manipulated GameObjects.
        /// A ringbuffer
        /// </summary>
        private List<Tuple<bool, HistoryType, string, List<string>>> allActionsList = new List<Tuple<bool, HistoryType, string, List<string>>>();

        /// <summary>
        /// Contains all actions executed by the player.
        /// </summary>
        private List<ReversibleAction> ownActions = new List<ReversibleAction>();

        /// <summary>
        /// Contains the Active Action from each Player needs to be updated with each undo/redo/action
        /// </summary>

        private ReversibleAction activeAction = null;

        /// <summary>
        /// Let C be the currently executed action (if there is any) in this action history. 
        /// Then <see cref="ReversibleAction.Stop"/> will be called for C. After that 
        /// <see cref="ReversibleAction.Awake()"/> and then <see cref="ReversibleAction.Start"/>
        /// will be called for <paramref name="action"/> and <paramref name="action"/> is added to 
        /// the action history and becomes the currently executed action for which 
        /// <see cref="ReversibleAction.Update"/> will be called whenever a client
        /// of this action history calls the action history's <see cref="Update"/> method.
        /// 
        /// No action previously undone can be redone anymore.
        /// 
        /// Precondition: <paramref name="action"/> is not already present in the action history.
        /// </summary>
        /// <param name="action">the action to be executed</param>
        /// <param name="ignoreRedoDeletion">Run with out deltetion of redos</param>
        public void Execute(ReversibleAction action, bool ignoreRedoDeletion = false)
        {
            activeAction?.Stop();
            Push(new Tuple<bool, HistoryType, string, List<string>>(true, HistoryType.action, action.GetId(), null));
            new GlobalActionHistoryNetwork(true, HistoryType.action, action.GetId(), null, true).Execute(null);
            ownActions.Add(action);
            activeAction = action;
            action.Awake();
            action.Start();

            // Whenever a new action is excuted, we consider the redo stack lost.
            if (!ignoreRedoDeletion && isRedo)
            {
                DeleteAllRedos();
            }
        }

        /// <summary>
        /// Calls the update method of each active action.
        /// </summary>
        public void Update()
        {
            foreach (ReversibleAction r in ownActions) { Debug.Log(r); }
            if (activeAction.Update() && activeAction.HadEffect())
            {
                Tuple<bool, HistoryType, string, List<string>> lastAction = FindLastActionOfPlayer(true, HistoryType.action);
                if (lastAction == null) return;
                DeleteItem(activeAction.GetId(), true);
                new GlobalActionHistoryNetwork(true, HistoryType.action, activeAction.GetId(), null, false).Execute(null);
                lastAction = new Tuple<bool, HistoryType, string, List<string>>(lastAction.Item1, lastAction.Item2, activeAction.GetId(), activeAction.GetChangedObjects());
                Push(lastAction);
                ownActions.Add(activeAction);
                new GlobalActionHistoryNetwork(lastAction.Item1, lastAction.Item2, lastAction.Item3, lastAction.Item4, true).Execute(null);
                Execute(activeAction.NewInstance());
            }
        }

        /// <summary>
        /// Pushes new actions to the <see cref="allActionsList"/>
        /// </summary>
        /// <param name="action">The action and all of its specific values which are needed for the history</param>
        public void Push(Tuple<bool, HistoryType, string, List<string>> action)
        {
            allActionsList.Add(action);
        }

        /// <summary>
        /// Finds a specific action by here id from the OwnActions
        /// </summary>
        /// <param name="id">thge id of the action</param>
        /// <returns>the action</returns>
        private ReversibleAction FindById(string id)
        {
            foreach (ReversibleAction it in ownActions) if (it.GetId().Equals(id)) return it;
            return null;
        }

        /// <summary>
        /// Finds the last executed action of a specific player.
        /// </summary>
        /// <param name="playerID">The player that wants to perform an undo/redo</param>
        /// <param name="type">the type of action he wants to perform</param>
        /// <returns>A tuple of the latest users action and if any later done action blocks the undo (True if some action is blocking || false if not)</returns>  
        /// Returns as second in the tuple that so each action could check it on its own >> List<ReversibleAction>> Returns Null if no action was found
        private Tuple<bool, HistoryType, string, List<string>> FindLastActionOfPlayer(bool isOwner, HistoryType type)
        {
            Tuple<bool, HistoryType, string, List<string>> result = null;

            for (int i = allActionsList.Count - 1; i >= 0; i--)
            {
                if (type == HistoryType.undoneAction && allActionsList[i].Item2 == HistoryType.undoneAction
                    || type == HistoryType.action && allActionsList[i].Item2 == HistoryType.action
                    && allActionsList[i].Item1 == true)
                {
                    result = allActionsList[i];
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks whether the action to be executed has conflicts to another action.
        /// </summary>
        /// <param name="affectedGameObjects">the gameObjects affected by the action to be undone/redone.</param>
        /// <returns>true, if there are conflicts, else false</returns>
        private bool ActionHasConflicts(List<string> affectedGameObjects)
        {
            int index = GetCountOfNewerAction(activeAction.GetId());
            for (int i = index; index < allActionsList.Count; i++)
            {
                foreach (string s in affectedGameObjects)
                {
                    if (allActionsList[i].Item4 != null)
                    {
                        if (allActionsList[i].Item4.Contains(s) && allActionsList[i].Item1 == false)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes all redos of a user
        /// </summary>
        private void DeleteAllRedos() //FIXME Muss auch die andere liste pflegen
        {
            for (int i = 0; i < allActionsList.Count; i++)
            {
                if (allActionsList[i].Item1.Equals(true) && allActionsList[i].Item2.Equals(HistoryType.undoneAction))
                {
                    ownActions.Remove(FindById(allActionsList[i].Item3)); //FIXME: is that uniqe and works?
                    allActionsList.RemoveAt(i);

                    i--;
                }
                isRedo = false;
            }
        }

        /// <summary>
        /// Deletes an item from the action list depending on its id.
        /// </summary>
        /// <param name="id">the id of the action which should be deleted</param>
        public void DeleteItem(string id, bool isOwner)
        {
            for (int i = 0; i < allActionsList.Count; i++)
            {
                if (allActionsList[i].Item3.Equals(id))
                {
                    allActionsList.RemoveAt(i);
                    if (isOwner) ownActions.Remove(FindById(id)); //FIXME: is that unique and works?
                    return;
                }
            }
        }

        /// <summary>
        /// Undoes the last action with an effect of a specific player.
        /// </summary>
        public void Undo()
        {
            Tuple<bool, HistoryType, string, List<string>> lastAction = FindLastActionOfPlayer(true, HistoryType.action);
            if (lastAction == null) return;
            while (!activeAction.HadEffect())
            {
                activeAction.Stop();
                DeleteItem(lastAction.Item3, lastAction.Item1);
                new GlobalActionHistoryNetwork(lastAction.Item1, HistoryType.action, lastAction.Item3, null, false).Execute(null);
                lastAction = FindLastActionOfPlayer(true, HistoryType.action);
                if (lastAction == null) return;
                activeAction = FindById(lastAction.Item3);
            }
            // Fixme: Right place ?
            if (ActionHasConflicts(activeAction.GetChangedObjects()))
            {
                // Fixme: Error
                Debug.Log("Undo");
                return;
            }
            else
            {
                activeAction?.Stop();
                activeAction?.Undo();
                DeleteItem(lastAction.Item3, lastAction.Item1);
                new GlobalActionHistoryNetwork(true, HistoryType.action, lastAction.Item3, null, false).Execute(null);
                Tuple<bool, HistoryType, string, List<string>> undoneAction = new Tuple<bool, HistoryType, string, List<string>>
                    (true, HistoryType.undoneAction, lastAction.Item3, lastAction.Item4);

                Push(undoneAction);

                ownActions.Add(activeAction);
                new GlobalActionHistoryNetwork(undoneAction.Item1, undoneAction.Item2, undoneAction.Item3, undoneAction.Item4, true).Execute(null);

                lastAction = FindLastActionOfPlayer(true, HistoryType.action);
                if (lastAction == null) return;
                activeAction = FindById(lastAction.Item3);
                
                Execute(activeAction.NewInstance(), true);
                //activeAction?.Start();
                isRedo = true;

            }
        }

        /// <summary>
        /// Redoes the last undone action of a specific player.
        /// </summary>
        public void Redo()
        {
            activeAction?.Stop();

            Tuple<bool, HistoryType, string, List<string>> lastUndoneAction = FindLastActionOfPlayer(true, HistoryType.undoneAction);
            if (lastUndoneAction == null) return;

            if (ActionHasConflicts(lastUndoneAction.Item4))
            {
                // Fixme: Error
                //NEED to delete the ownAction
                //Set the owner of the action to false, dont delete from allactions
                //notify the user
                Debug.Log("Redo");
                return;
            }
            ReversibleAction temp = FindById(lastUndoneAction.Item3);
            temp.Redo();
           
            Tuple<bool, HistoryType, string, List<string>> redoneAction = new Tuple<bool, HistoryType, string, List<string>>(true, HistoryType.action, lastUndoneAction.Item3, lastUndoneAction.Item4);
            DeleteItem(lastUndoneAction.Item3, lastUndoneAction.Item1);
            new GlobalActionHistoryNetwork(true, HistoryType.action, lastUndoneAction.Item3, null, false).Execute(null);
            Push(redoneAction);
            new GlobalActionHistoryNetwork(redoneAction.Item1, redoneAction.Item2, redoneAction.Item3, redoneAction.Item4, true).Execute(null);
            activeAction = temp;
            ownActions.Add(temp);
            Execute(activeAction.NewInstance(), true);

        }

        /// <summary>
        /// Returns the active action of a player
        /// </summary>
        /// <returns>The active action of a player</returns>
        public ReversibleAction GetActiveAction()
        {
            return activeAction;
        }

        /// <summary>
        /// Gets the number of all actions which are executed by other users after the action with the id <paramref name="idOfAction"/>.
        /// </summary>
        /// <returns>the number of newer actions than that with the id <paramref name="idOfAction"/>, which are not executed by the owner.</returns>
        private int GetCountOfNewerAction(string idOfAction)
        {

            for(int i = allActionsList.Count-1; i >= 0; i--)
            {
                if (allActionsList[i].Item3.Equals(idOfAction)) return allActionsList.Count - 1 - i;
            }
            return -1; //If not fouind the action
            //int neweractionscount = 0;
            //int sizeofactionslist = allactionslist.count - 1;
            //while (!allactionslist[sizeofactionslist].item3.equals(idofaction) && sizeofactionslist >= 0)
            //{
            //    neweractionscount++;
            //    sizeofactionslist--;
            //}

            //debug.log(neweractionscount);
            //return neweractionscount;
        }

        /// <summary>
        /// Returns wether a player has no Actions left to be undone
        /// </summary>
        /// <returns>True if no action left</returns>
        public bool NoActionsLeft()
        {
            return FindLastActionOfPlayer(true, HistoryType.action) == null;
        }

        /// <summary>
        /// Returns wether a player has some undone actions left
        /// </summary>
        /// <returns>True if none undone actions left</returns>
        public bool NoUndoneActionsLeft()
        {
            return FindLastActionOfPlayer(true, HistoryType.undoneAction) == null;
        }
    }
}