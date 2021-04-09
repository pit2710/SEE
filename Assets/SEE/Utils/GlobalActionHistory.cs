using OdinSerializer.Utilities;
using SEE.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

public class GlobalActionHistory
{
    /// <summary>
    /// An enum which sets the type of an Action in the History
    /// </summary>
    public enum HistoryType
    {
        action,
        undo,
    };

    /// <summary>
    /// The size of the ActionList
    /// </summary>
    private readonly int size = 100;

    /// <summary>
    /// Numbers of elements in the Buffer
    /// </summary>
    private int count = 0;

    /// <summary>
    /// 
    /// </summary>
    private bool isRedo = false;

    /// <summary>
    /// The actionList it has an Tupel of the time as it was performed, Player ID, The type of the Action (Undo Redo Action), the ReversibleAction, and the list with the ids of the GameObjects
    /// A ringbuffer
    /// </summary>
    private List<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>> actionList; //FIXME: GameObject ID muss eine LISTE sein

    /// <summary>
    /// Contains the Active Action from each Player needs to be updated with each undo/redo/action
    /// </summary>
    private Dictionary<string, ReversibleAction> activeAction = new Dictionary<string, ReversibleAction>();

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
    public void Execute(ReversibleAction action, string key)
    {
        GetActiveAction(key)?.Stop();
        Push(new Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>(DateTime.Now, key, HistoryType.action, action, null));       //UndoStack.Push(action);
        SetActiveAction(key, action);
        action.Awake();
        action.Start();
        // Whenever a new action is excuted, we consider the redo stack lost.
        if (isRedo) DeleteRedo(key);    //RedoStack.Clear();
    }

    /// <summary>
    /// Calls the Update method of each Active Action
    /// </summary>
    public void Update() //FIXME: in der Action history wird das etwas anders gemacht
    {
        for(int i = 0; i < activeAction.Count; i++)
        {
            if (activeAction.ElementAt(i).Value != null && activeAction.ElementAt(i).Value.Update())
            {
                if (activeAction.ElementAt(i).Value.HadEffect())
                {
                    Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>> found = Find(activeAction.ElementAt(i).Key, HistoryType.action).Item1;
                    DeleteItem(activeAction.ElementAt(i).Key, found.Item1);
                    found = new Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>(DateTime.Now, found.Item2, found.Item3, activeAction.ElementAt(i).Value, activeAction.ElementAt(i).Value.GetChangedObjects());
                    Push(found);
                }
                Execute(activeAction.ElementAt(i).Value.NewInstance(), activeAction.ElementAt(i).Key);
            }
        }
    }


    /// <summary>
    /// Appends data to the Ringbuffer
    /// </summary>
    /// <param name="data"></param>
    private void Push(Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>> data)
    {
        actionList.Add(data);
        count++;
        // Fixme: Max size..?
    }

    /// <summary>
    /// Finds the latest player action of the searched type an all relevant newer changes
    /// </summary>
    /// <param name="playerID">The player that wants to perform an undo/redo</param>
    /// <param name="type">the type of action he wants to perform</param>
    /// <returns>A tuple of the latest users action and if any later done action blocks the undo (True if some action is blocking || false if not)</returns>  // Returns as second in the tuple that so each action could check it on its own >> List<ReversibleAction>>
    private Tuple<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>, bool> Find(string playerID, HistoryType type)
    {
        int i = head - 1;

        //A list to persit all changes on the same GameObject
        //List<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>> results = new List<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>>();
        //List<ReversibleAction> results = new List<ReversibleAction>(); //Only Needed if you want to give all newer actions to the caller 
        Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>> result = null;
        while (true)
        {
            if (actionList[i] != null)
            {
                if ((type == HistoryType.undo && actionList[i].Item3 == HistoryType.undo)
                    || (type == HistoryType.action && actionList[i].Item3 == HistoryType.action)
                    && actionList[i].Item2 == playerID)
                {
                    result = actionList[i]; //FIXME: Somehow these data has to be deleted from the list, but not sure then to do it 
                    break;
                }
            }

            if (i == tail) break;
            if (i > 0) i--;
            else i = size - 1;
        }
        //Find all newer changes that could be a problem to the undo
        if (count > 1 && result != null ) //IF Result == NULL no undo could be performed
        {
            while (true)
            {
                //Checks if any item from list 1 is in list 2
                if ( result.Item5?.Where(it => actionList[i].Item5.Contains(it)) != null) //FIXME: Could make some trouble, not sure if it works
                {
                    //results.Add(actionList[i].Item4);
                    return new Tuple<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>, bool>(result, true); //Delete this return if you want to give all actions to the caller 
                }
                if (i == head - 1) break;
                if (i < size - 1) i++;
                else i = 0;
            }
        }

        return new Tuple<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>, bool>(result, false);

    }

    /// <summary>
    /// Deletes all redos of a user
    /// </summary>
    /// <param name="userid">the user that does the new action</param>
    private void DeleteRedo(string userid) //FIXME: maybe real delte (shifting of index?)?
    {

        for (int i = 0; i < size - 1; i++)
        {
            if (actionList[i] != null && actionList[i].Item2.Equals(userid) && actionList[i].Item3.Equals(HistoryType.undo))
            {
                actionList[i] = null; //FIXME changed to undo because  i think we need to delete this and not the undos which are actualy actions?}
                count--;
            }
            isRedo = false;
        }
    }


    /// <summary>
    /// Deletes a Item from the Action list depending on Time and Userid
    /// </summary>
    /// <param name="userid">the user for the action that should be deleted</param>
    /// <param name="time">the time of the action which should be deleted</param>
    private void DeleteItem(string userid, DateTime time)
    {
        for (int i = 0; i < size - 1; i++)
        {
            if (actionList[i]!= null && actionList[i].Item1.Equals(time) && actionList[i].Item2.Equals(userid))
            {
                actionList.RemoveAt(i);
                count--;
                return;
            }
        }
    }


    /// <summary>
    /// Undo
    /// </summary>
    /// <param name="userid"></param>
    public void Undo(string userid) //FIXME: UNDO AND REDO NEEDS TO UPDATE ALSO THE ACTIVEACTION
    {
<<<<<<< HEAD
        Tuple<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>, bool> find;
        find = Find(userid, HistoryType.action);    //Should be the same as getActiveAction     //With the result we need to calculate whether we can du undo or not and what changes the gameobject need
        SetActiveAction(userid, find.Item1.Item4);
        while (!GetActiveAction(userid).HadEffect())
        {
            GetActiveAction(userid).Stop();
            if (count > 1)
            {
                //POP
                DeleteItem(find.Item1.Item2, find.Item1.Item1);
                find = Find(userid, HistoryType.action);
                SetActiveAction(userid, find.Item1.Item4);
            }
            else
            {
                // Fixme: Eventuell noch im else was erledigen, was muss passieren wenn das undo nicht performed werden kann
                return;
            }
        }
        GetActiveAction(userid).Stop();
        find.Item1.Item4.Undo();

        Push(new Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>(DateTime.Now, userid, HistoryType.undo, find.Item1.Item4, find.Item1.Item5));

        //DO the POP
        DeleteItem(find.Item1.Item2, find.Item1.Item1);
        find = Find(userid, HistoryType.action);
        SetActiveAction(userid, find.Item1.Item4);
        
        isRedo = true;
        GetActiveAction(userid)?.Start();
    }

    /// <summary>
    /// REDO
    /// </summary>
    /// <param name="userid">The player that wants the redo </param>
    public void Redo(string userid)
    {
        GetActiveAction(userid)?.Stop();

        Tuple<Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>, bool> find = Find(userid, HistoryType.undo);

        // With the result we need to calculate whether we can du undo or not and what changes the gameobject need
        find.Item1.Item4.Redo();
        Push(new Tuple<DateTime, string, HistoryType, ReversibleAction, List<string>>(DateTime.Now, userid, HistoryType.action, find.Item1.Item4, find.Item1.Item5));
        find.Item1.Item4.Start();

        SetActiveAction(userid, find.Item1.Item4); //FIXME: IST die action hier richtig
        DeleteItem(find.Item1.Item2, find.Item1.Item1);
        //FIXME Was passiert wenn das redo nicht erfolgreich wird
    }

    /// <summary>
    /// Returns the Active Action for the Player
    /// </summary>
    /// <param name="player">The Player that performs an Action</param>
    /// <returns>The active action || null if key not in dictionary</returns>
    public ReversibleAction GetActiveAction(string player)
    {
        try
        {
            return activeAction[player];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }


    /// <summary>
    /// Sets the Active Action of a Player 
    /// </summary>
    /// <param name="player">The player to set the active action</param>
    /// <param name="action">the new active action</param>
    private void SetActiveAction(string player, ReversibleAction action)
    {
        try
        {
            activeAction[player] = action;
        }
        catch (KeyNotFoundException)
        {
            activeAction.Add(player, action);
        }
    }

}
