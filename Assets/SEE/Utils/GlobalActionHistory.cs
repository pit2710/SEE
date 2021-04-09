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
    public enum historyType
    {
        action,
        undo,
    };

    /// <summary>
    /// the start of the ringbuffer
    /// </summary>
    private int head = 0;

    /// <summary>
    /// The end of the Ringbuffer
    /// </summary>
    private int tail = 0;

    /// <summary>
    /// The size of the ActionList
    /// </summary>
    private int size = 0;


    /// <summary>
    /// Numbers of elements in the Buffer
    /// </summary>
    private int count = 0;

    /// <summary>
    /// indicates wether the tail should move or not
    /// </summary>
    private bool full = false;


    private bool isRedo = false;
    /// <summary>
    /// The actionList it has an Tupel of the time as it was performed, Player ID, The type of the Action (Undo Redo Action), the ReversibleAction, and the list with the ids of the GameObjects
    /// A ringbuffer
    /// </summary>
    private Tuple<DateTime, string, historyType, ReversibleAction, List<string>>[] actionList; //FIXME: GameObject ID muss eine LISTE sein

    /// <summary>
    /// Contains the Active Action from each Player needs to be updated with each undo/redo/action
    /// </summary>
    private Dictionary<String, ReversibleAction> activeAction = new Dictionary<string, ReversibleAction>();
    /// <summary>
    /// Constructs a new Global Action History and sets the Size of the Shared ActionList
    /// </summary>
    /// <param name="bufferSize">The Size of the shared ActionList Standard is 100</param>
    public GlobalActionHistory(int bufferSize = 100)
    {
        actionList = new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>[bufferSize];
        this.size = bufferSize;
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
    public void Execute(ReversibleAction action, string key)
    {
        getActiveAction(key)?.Stop();
        Push(new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>(DateTime.Now, key, historyType.action, action, null));       //UndoStack.Push(action);
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
                    Tuple<DateTime, string, historyType, ReversibleAction, List<string>> found = Find(activeAction.ElementAt(i).Key, historyType.action).Item1;
                    DeleteItem(activeAction.ElementAt(i).Key, found.Item1);
                    found = new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>(DateTime.Now, found.Item2, found.Item3, found.Item4, activeAction.ElementAt(i).Value.GetChangedObjects());
                    Push(found);
                }
                Execute(activeAction.ElementAt(i).Value.NewInstance(), activeAction.ElementAt(i).Key);
            }
        }
      /*  activeAction.ForEach(y =>
        {
            if (y.Value != null && y.Value.Update())
            {
                if (y.Value.HadEffect())
                {
                    Tuple<DateTime, string, historyType, ReversibleAction, List<string>> found = Find(y.Key, historyType.action).Item1;
                    DeleteItem(y.Key, found.Item1);
                    found = new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>(DateTime.Now, found.Item2, found.Item3, found.Item4, y.Value.GetChangedObjects());
                    Push(found);
                }
                Execute(y.Value.NewInstance(), y.Key);
            }
        }); */
    }


    /// <summary>
    /// Appends data to the Ringbuffer
    /// </summary>
    /// <param name="data"></param>
    private void Push(Tuple<DateTime, string, historyType, ReversibleAction, List<string>> data)
    {
        actionList[head] = data;
        count++;
        if (head < size - 1) head++;
        else
        {
            head = 0;
            full = true;
        }
        if (full && tail < size - 1) tail++;
        else tail = 0;
    }

    /// <summary>
    /// Finds the latest player action of the searched type an all relevant newer changes
    /// </summary>
    /// <param name="playerID">The player that wants to perform an undo/redo</param>
    /// <param name="type">the type of action he wants to perform</param>
    /// <returns>A tuple of the latest users action and if any later done action blocks the undo (True if some action is blocking || false if not)</returns>  // Returns as second in the tuple that so each action could check it on its own >> List<ReversibleAction>>
    private Tuple<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>, bool> Find(string playerID, historyType type)
    {
        int i = head - 1;

        //A list to persit all changes on the same GameObject
        //List<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>> results = new List<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>>();
        //List<ReversibleAction> results = new List<ReversibleAction>(); //Only Needed if you want to give all newer actions to the caller 
        Tuple<DateTime, string, historyType, ReversibleAction, List<string>> result = null;
        while (true)//FIXME: later difference whether it is an undo or redo search
        {

            if ((type == historyType.undo && actionList[i].Item3 == historyType.undo)
                || (type == historyType.action && actionList[i].Item3 == historyType.action)
                && actionList[i].Item2 == playerID)
            {
                result = actionList[i]; //FIXME: Somehow these data has to be deleted from the list, but not sure then to do it 
                break;
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
                UnityEngine.Debug.Log("WIHLE FOUND");
                //Checks if any item from list 1 is in list 2
                if ( result.Item5?.Where(it => actionList[i].Item5.Contains(it)) != null) //FIXME: Could make some trouble, not sure if it works
                {
                    //results.Add(actionList[i].Item4);
                    return new Tuple<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>, bool>(result, true); //Delete this return if you want to give all actions to the caller 
                }
                if (i == head - 1) break;
                if (i < size - 1) i++;
                else i = 0;
            }
        }

        return new Tuple<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>, bool>(result, false);

    }

    /// <summary>
    /// Deletes all redos of a user
    /// </summary>
    /// <param name="userid">the user that does the new action</param>
    private void DeleteRedo(string userid) //FIXME: maybe real delte (shifting of index?)?
    {

        for (int i = 0; i < size - 1; i++)
        {
            if (actionList[i] != null && actionList[i].Item2.Equals(userid) && actionList[i].Item3.Equals(historyType.undo))
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
                actionList[i] = null;
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
        UnityEngine.Debug.Log("UNDO Start");
        Tuple<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>, bool> find;
        find = Find(userid, historyType.action);    //Should be the same as getActiveAction     //With the result we need to calculate whether we can du undo or not and what changes the gameobject need
        UnityEngine.Debug.Log(" FOUND");
        while (!getActiveAction(userid).HadEffect())
        {
            UnityEngine.Debug.Log("WIHLE UNDO");
            getActiveAction(userid).Stop();
            //undo stack > 0 ? pop : return //muss ich die action wirklich l�schen

        }
        UnityEngine.Debug.Log(" after while");
        getActiveAction(userid).Stop();
        find.Item1.Item4.Undo();


        Push(new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>(DateTime.Now, userid, historyType.undo, find.Item1.Item4, find.Item1.Item5));

        //DO the POP
        DeleteItem(find.Item1.Item2, find.Item1.Item1);
        find = Find(userid, historyType.action);
        SetActiveAction(userid, find.Item1.Item4);
        isRedo = true;
        getActiveAction(userid)?.Start();

        //Eventuell noch im else was erledigen, was muss passieren wenn das undo nicht performed werden kann
    }

    /// <summary>
    /// REDO
    /// </summary>
    /// <param name="userid">The player that wants the redo </param>
    public void Redo(string userid)
    {
        getActiveAction(userid).Stop();

        Tuple<Tuple<DateTime, string, historyType, ReversibleAction, List<string>>, bool> find;
        find = Find(userid, historyType.undo);

        //With the result we need to calculate whether we can du undo or not and what changes the gameobject need
        find.Item1.Item4.Redo();
        Push(new Tuple<DateTime, string, historyType, ReversibleAction, List<string>>(DateTime.Now, userid, historyType.action, find.Item1.Item4, find.Item1.Item5));
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
    public ReversibleAction getActiveAction(string player)
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
