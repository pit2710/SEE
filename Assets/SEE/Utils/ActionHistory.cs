using SEE.Net;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.SEE.Utils
{
    public class ActionHistory
    {

        public class MoreThanOneActionWithNoEffectException : System.Exception
        {

        }

        public class EmptyUndoHistoryException : System.Exception
        {

        }

        // Implementation note: This ActionHistory is a bit more complicated than
        // action histories in other contexts because we do not have atomic actions
        // that are either executed or not. Our <see cref="ReversibleAction"/> have
        // a life cycle described by <see cref="ReversibleAction.Progress"/>.
        // They start and receive Update calls until they are completely done.
        // If an action was completely done, the execution continues with a new instance
        // of the same action kind. If an action is undone while in progress state
        // <see cref="ReversibleAction.Progress.InProgress"/>, it has already had
        // some effect that needs to be undone.
        //
        // See also the test cases in <seealso cref="SEETests.TestActionHistory"/> for 
        // additional information.

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
        private Stack<ReversibleAction> ownActions1 = new Stack<ReversibleAction>();

        /// <summary>
        /// Contains all undone actions executed by the player.
        /// </summary>
        private Stack<ReversibleAction> ownUndoneActions = new Stack<ReversibleAction>();

        /// <summary>
        /// Contains the Active Action from each Player needs to be updated with each undo/redo/action
        /// </summary>
        private ReversibleAction activeAction = null;

        /// <summary>
        /// The maximal size of the action history.
        /// </summary>
        private const int historySize = 100;

        /// <summary>
        /// Checks the invariant that the <see cref="UndoStack"/> has at most
        /// one action with progress state <see cref="ReversibleAction.Progress.NoEffect"/>
        /// and this action is at the top of <see cref="UndoStack"/>.
        /// </summary>
        private void AssertAtMostOneActionWithNoEffect()
        {
            UnityEngine.Assertions.Assert.IsTrue
               (ownActions1.Skip(1).All(action => action.CurrentProgress() != ReversibleAction.Progress.NoEffect));
        }

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
            ownActions1.Push(action);
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

            if (activeAction.Update() && activeAction.CurrentProgress() == ReversibleAction.Progress.Completed)
            {
                Push(new Tuple<bool, HistoryType, string, List<string>>(true, HistoryType.action, activeAction.GetId(), activeAction.GetChangedObjects()));
                new GlobalActionHistoryNetwork().Push(HistoryType.action, activeAction.GetId(), ListToString(activeAction.GetChangedObjects()));
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
        /// 
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        public void Replace(Tuple<bool, HistoryType, string, List<string>> oldItem, 
            Tuple<bool, HistoryType, string, List<string>> newItem, bool isNetwork)
        {
            int index = GetIndexOfAction(oldItem.Item3);  //FIXME: OwnAction muss evtl auch geloescht werden
            allActionsList[index] = newItem;
            if (!isNetwork) new GlobalActionHistoryNetwork().Replace(oldItem.Item2, oldItem.Item3, ListToString(oldItem.Item4), newItem.Item2, ListToString(newItem.Item4));
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
            int index = GetIndexOfAction(activeAction.GetId());
            if (index == -1)
            {
                Debug.Log("ERROR IN GETCOUNT");
                return false;
            }
            index++;
            for (int i = index; i < allActionsList.Count; i++)
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
        private void DeleteAllRedos()
        {
            for (int i = 0; i < allActionsList.Count; i++)
            {
                if (allActionsList[i].Item1.Equals(true) && allActionsList[i].Item2.Equals(HistoryType.undoneAction))
                {
                    new GlobalActionHistoryNetwork().Delete(allActionsList[i].Item3);
                    allActionsList.RemoveAt(i);
                    i--;
                }
                isRedo = false;
            }
            ownUndoneActions.Clear();
        }

        /// <summary>
        /// Deletes an item from the action list depending on its id.
        /// </summary>
        /// <param name="id">the id of the action which should be deleted</param>
        public void DeleteItem(string id)
        {
            for (int i = 0; i < allActionsList.Count; i++)
            {
                if (allActionsList[i].Item3.Equals(id))
                {
                    // Fixme: Pop an jeder Stelle ausf�hren, wo DeleteItem() aufgerufen wird.
                    allActionsList.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Undoes the last action with an effect of a specific player.
        /// </summary>
        public void Undo()
        {
            ownActions1.Peek().Stop();

            ReversibleAction current = LastActionWithEffect();

            if (current == null) return;

            Tuple<bool, HistoryType, string, List<string>> lastAction = allActionsList[GetIndexOfAction(current.GetId())];

            if (ActionHasConflicts(current.GetChangedObjects()))
            {
                Debug.LogWarning("Undo not possible, someone else had made a change on the same object!");
                Replace(lastAction, new Tuple<bool, HistoryType, string, List<string>>(false, HistoryType.undoneAction, lastAction.Item3, lastAction.Item4), false);

                ownActions1.Pop();
                ownActions1.Peek().Start();
                activeAction = LastActionWithEffect();
                return;
            }
            else
            {
                current.Undo();
                ownUndoneActions.Push(current);
                ownActions1.Pop();
                DeleteItem(lastAction.Item3);
                new GlobalActionHistoryNetwork().Delete(lastAction.Item3);

                Tuple<bool, HistoryType, string, List<string>> undoneAction = new Tuple<bool, HistoryType, string, List<string>>
                    (true, HistoryType.undoneAction, lastAction.Item3, lastAction.Item4);

                Push(undoneAction);
                new GlobalActionHistoryNetwork().Push(undoneAction.Item2, undoneAction.Item3, ListToString(undoneAction.Item4));

                // lastAction = FindLastActionOfPlayer(true, HistoryType.action);
                activeAction = LastActionWithEffect();
                if (activeAction == null) return;
                isRedo = true;

                Resume(activeAction);
                
            }
        }

        /// <summary>
        /// Redoes the last undone action of a specific player.
        /// </summary>
        public void Redo()
        {
            Tuple<bool, HistoryType, string, List<string>> lastUndoneAction = FindLastActionOfPlayer(true, HistoryType.undoneAction);
            if (ownUndoneActions.Count == 0)
            {
                throw new EmptyUndoHistoryException();
            }
            else
            {
                ownActions1?.Peek().Stop();
                if (ActionHasConflicts(lastUndoneAction.Item4))
                {
                    ownUndoneActions.Pop();
                    Replace(lastUndoneAction, new Tuple<bool, HistoryType, string, List<string>>(false, HistoryType.undoneAction, lastUndoneAction.Item3, lastUndoneAction.Item4), false);
                    Debug.LogWarning("Redo not possible, someone else had made a change on the same object!");
                    ownActions1.Peek().Start();
                    return;
                }

                ReversibleAction redoAction = ownUndoneActions.Pop();
                LastActionWithEffect();
                ownActions1.Push(redoAction);
                redoAction.Redo();
                Resume(redoAction);

                UnityEngine.Assertions.Assert.IsTrue(ownUndoneActions.Count == 0
                                 || ownUndoneActions.Peek().CurrentProgress() != ReversibleAction.Progress.NoEffect);

                Tuple<bool, HistoryType, string, List<string>> redoneAction = new Tuple<bool, HistoryType, string, List<string>>(true, HistoryType.action, lastUndoneAction.Item3, lastUndoneAction.Item4);
                DeleteItem(lastUndoneAction.Item3);
                new GlobalActionHistoryNetwork().Delete(lastUndoneAction.Item3);
                Push(redoneAction);
                new GlobalActionHistoryNetwork().Push(redoneAction.Item2, redoneAction.Item3, ListToString(redoneAction.Item4));

            }
        }

        /// <summary>
        /// Resumes the execution with a fresh instance of the given <paramref name="action"/> 
        /// if its current progress is <see cref="ReversibleAction.Progress.Completed"/>
        /// or otherwise with <paramref name="action"/> if the current progress
        /// is <see cref="ReversibleAction.Progress.InProgress"/>.
        /// 
        /// Precondition: the current progress of <paramref name="action"/>
        /// is different from <see cref="ReversibleAction.Progress.NoEffect"/>.
        /// </summary>
        /// <param name="action">action to be resumed</param>
        private void Resume(ReversibleAction action)
        {
            UnityEngine.Assertions.Assert.IsTrue(action.CurrentProgress() != ReversibleAction.Progress.NoEffect);
            if (action.CurrentProgress() == ReversibleAction.Progress.Completed)
            {
                // We will resume with a fresh instance of the current action as
                // the (now) current has already been completed.
                action = action.NewInstance();
                ownActions1.Push(action);
                action.Awake();
            }
            action.Start();
        }

        /// <summary>
        /// Returns the last action on the <see cref="UndoStack"/> that
        /// has had any effect (preliminary or complete) or null if there is
        /// no such action. 
        /// 
        /// Side effect: All actions at the top of the <see cref="UndoStack"/> 
        /// are popped off.
        /// </summary>
        /// <returns>the last action on the <see cref="UndoStack"/> that
        /// has had any effect (preliminary or complete) or null<</returns>
        private ReversibleAction LastActionWithEffect()
        {
            while(ownActions1.Count > 0)
            {
                ReversibleAction action = ownActions1.Peek();
                if (action.CurrentProgress() != ReversibleAction.Progress.NoEffect)
                {
                    return action;
                }
                else
                {
                    ownActions1.Pop();
                }
            }
            return null;
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
        private int GetIndexOfAction(string idOfAction)
        {
            for (int i = allActionsList.Count - 1; i >= 0; i--)
            {
                if (allActionsList[i].Item3.Equals(idOfAction)) return i;
            }
            return -1;
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

        /// <summary>
        /// Converts a List of gameObjectIds to a single comma seperated string for sending it to other clients.
        /// </summary>
        /// <param name="gameObjectIds">the gameObjectIds</param>
        /// <returns>a single comma seperated string of all gameObjectIds.</returns>
        private string ListToString(List<string> gameObjectIds)
        {
            string result = "";
            if (gameObjectIds == null) return null;
            foreach (string s in gameObjectIds)
            {
                result += s + ",";
            }
            if (result != "" && result != null)
            {
                return result.Substring(0, result.Length - 1);
            }
            else return null;
        }
    }
}