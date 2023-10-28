using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using SEE.UI.Window.CodeWindow;
using UnityEngine.Events;
using static SEE.Utils.CRDT;

namespace SEE.Utils
{
    /// <summary>
    /// The static interface to use and maintain the actual <see cref="CRDT"/>
    /// </summary>
    public static class ICRDT
    {
        /// <summary>
        /// A dictionary for the CRDTs with the filename as key.
        /// </summary>
        private static readonly Dictionary<string, CRDT> crdts = new();

        /// <summary>
        /// Manages the player ids.
        /// </summary>
        private static readonly PlayerIdManager playerIdManager = new();

        /// <summary>
        /// Finds or creates a CRDT instance by the name of the file.
        /// </summary>
        /// <param name="file">The name of the file</param>
        /// <returns>A crdt instance</returns>
        private static CRDT GetInstanceByName(string file)
        {
            if (crdts != null && crdts.Count > 0 && crdts.ContainsKey(file))
            {
                return crdts[file];
            }
            else
            {
                crdts.Add(file, new CRDT(playerIdManager.GetClientID().ToString(), file));
                return crdts[file];
            }
        }

        /// <summary>
        /// Synchronizes all CodeWindows to a new client.
        /// </summary>
        public static void SyncCodeWindows(IPEndPoint[] recipient)
        {
            foreach (KeyValuePair<string, CRDT> elm in crdts)
            {
                elm.Value.SyncCodeWindows(recipient);
            }
        }

        /// <summary>
        /// ATTENTION: ONLY FOR THE USE IN EXECUTE ON THE SERVER.
        /// Requests an id from the player ID manager started from the server.
        /// </summary>
        /// <returns>The requested id</returns>
        public static int RequestID()
        {
            return playerIdManager.RequestID();
        }

        /// <summary>
        /// Gets the id of the local player.
        /// </summary>
        /// <returns>The local player id.</returns>
        public static int GetLocalID()
        {
            return playerIdManager.GetClientID();
        }

        /// <summary>
        /// ONLY FOR SERVER USAGE.
        /// Sets the id of the local player.
        /// </summary>
        /// <param name="id">The id to set</param>
        public static void SetLocalID(int id)
        {
            playerIdManager.SetClientID(id);
        }

        /// <summary>
        /// Adds <paramref name="c"/> to the CRDT remotely.
        /// </summary>
        /// <param name="c">The character to add.</param>
        /// <param name="position">The position of the character.</param>
        /// <param name="file">The name of the file in which the character should be inserted.</param>
        /// <exception cref="RemoteAddCharNotPossibleException">Thrown if no fitting
        /// index for the change could be found or the position is null.</exception>
        public static void RemoteAddChar(char c, Identifier[] position, string file)
        {
            GetInstanceByName(file).RemoteAddChar(c, position);
        }

        /// <summary>
        /// Synchronizes the content from the existing CodeWindows into the new client.
        /// Used when a new client joins an existing session.
        /// </summary>
        /// <param name="c">The character to add.</param>
        /// <param name="position">The position of the character.</param>
        /// <param name="file">The name of the file in which the character should be inserted.</param>
        public static void SingleRemoteAddChar(char c, Identifier[] position, string file)
        {
            GetInstanceByName(file).SingleRemoteAddChar(c, position);
        }

        /// <summary>
        /// Deletes a character from the crdt remotely.
        /// </summary>
        /// <param name="position">The position at which a character should be deleted.</param>
        /// <param name="file">The name of the file in which a character should be deleted.</param>
        /// <exception cref="RemoteDeleteNotPossibleException">Thrown when the
        /// requested position is not contained by the <see cref="crdt"/></exception>
        public static void RemoteDeleteChar(Identifier[] position, string file)
        {
            GetInstanceByName(file).RemoteDeleteChar(position);
        }

        /// <summary>
        /// Adds <paramref name="s"/> to the crdt.
        /// </summary>
        /// <param name="s">The string which should be added.</param>
        /// <param name="startIdx">The start index of the string or position, respectively, in the code window.</param>
        /// <param name="file">The name of the file in which the string should be added.</param>
        public static void AddString(string s, int startIdx, string file)
        {
            CRDT crdt = GetInstanceByName(file);
            if (GetLocalID() > 0  && crdt.GetId().Equals("0"))
            {
                crdt.SetId(GetLocalID().ToString());
            }
            crdt.AddString(s, startIdx);
        }

        /// <summary>
        /// Asynchronously adds <paramref name="s"/> to the crdt, formerly used for transmitting huge datastreams,
        /// e.g., during the start-up of a code window.
        /// During the adding process no changes should be made in the crdt or code window.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="startIdx">The start position in the code window.</param>
        /// <param name="file">The name of the file in which the string should be added.</param>
        /// <param name="startUp">If true, no undo/redo is activated for the added characters.</param>
        public static async UniTask AsyncAddString(string s, int startIdx, string file, bool startUp = false)
        {
            await GetInstanceByName(file).AsyncAddString(s, startIdx, startUp);
        }

        /// <summary>
        /// Deletes a string from the crdt.
        /// </summary>
        /// <param name="startIdx">the start index in the code window of the string that should be deleted.</param>
        /// <param name="endIdx">The end position of the string in the code window.</param>
        /// <param name="file">The name of the file in which the string should be deleted.</param>
        public static void DeleteString(int startIdx, int endIdx, string file)
        {
            CRDT crdt = GetInstanceByName(file);
            if (GetLocalID() > 0 && crdt.GetId().Equals("0"))
            {
                crdt.SetId(GetLocalID().ToString());
            }
            crdt.DeleteString(startIdx, endIdx);
        }

        /// <summary>
        /// Prints the crdt as a string.
        /// </summary>
        /// <param name="file">The filename of the crdt that should be printed.</param>
        /// <returns>The content from the crdt as a string.</returns>
        public static string PrintString(string file)
        {
            return GetInstanceByName(file).PrintString();
        }


        /// <summary>
        /// Converts a string into a position.
        /// TODO: Probably obsolete because we do not change anything inside the crdt;
        /// maybe we should move the code to the icrdt?
        /// </summary>
        /// <param name="s">The string that should be converted.</param>
        /// <param name="file">The filename which crdt should be used.</param>
        /// <returns>the position</returns>
        public static Identifier[] StringToPosition(string s, string file)
        {
            return CRDT.StringToPosition(s);
        }

        /// <summary>
        /// Converts a position into a string.
        /// TODO: same as above.
        /// </summary>
        /// <param name="position">The position that should be converted.</param>
        /// <param name="file">The name of the file that should be used for conversion.</param>
        /// <returns>the position as a string</returns>
        public static string PositionToString(Identifier[] position, string file)
        {
            return CRDT.PositionToString(position);
        }

        /// <summary>
        /// Tests if a crdt is empty.
        /// </summary>
        /// <param name="file">The name of the file that should be tested.</param>
        /// <returns>True if the file in the crdt is empty, false if not.</returns>
        public static bool IsEmpty(string file)
        {
            return GetInstanceByName(file).IsEmpty();
        }

        /// <summary>
        /// Gets the change event from the crdt.
        /// </summary>
        /// <param name="file">The filename in the crdt</param>
        /// <returns>A change event that notifies the user when the content of the crdt changes.</returns>
        public static UnityEvent<char, int, CodeWindow.OperationType> GetChangeEvent(string file)
        {
            return GetInstanceByName(file).ChangeEvent;
        }

        /// <summary>
        /// Perfomes an undo.
        /// </summary>
        /// <param name="file">The name of the file in which an undo should be performed.</param>
        /// <exception cref="UndoNotPossibleExcpetion">Thrown when the undo could not be performed,
        /// e.g., the undo stack is empty or the operation cannot be undone</exception>
        public static void Undo(string file)
        {
            GetInstanceByName(file).Undo();
        }

        /// <summary>
        /// Performs a redo.
        /// </summary>
        /// <param name="file">The name of the file in which the redo should be perfomed</param>
        /// <exception cref="RedoNotPossibleException">Thrown when the redo is impossible
        /// because of an empty redo stack or the action could not be redone.</exception>
        public static void Redo(string file)
        {
            GetInstanceByName(file).Redo();
        }

        /// <summary>
        /// Remotely adds <paramref name="text"/> to the crdt.
        /// </summary>
        /// <param name="text">The string that should be added.</param>
        /// <param name="file">The filename in the crdt.</param>
        public static void RemoteAddString(string text, string file)
        {
            GetInstanceByName(file).RemoteAddString(text);
        }
    }

    /// <summary>
    /// The PlayerIdMangager manages the site IDs of the users. The instance of the server will
    /// increase the playerIDCounter and the clients will save their own ids inside the playerIdManager.
    /// </summary>
    internal class  PlayerIdManager
    {
        /// <summary>
        /// The number of all players using a crdt in the session.
        /// Only maintained on the server instance.
        /// </summary>
        private int playerIDcounter = 0;

        /// <summary>
        /// The id of the local client.
        /// </summary>
        private int clientID = 0;

        /// <summary>
        /// Generates the new Player ID.
        /// </summary>
        /// <returns>The Id for the user</returns>
        public int RequestID()
        {
            return ++playerIDcounter;
        }

        /// <summary>
        /// Sets the local ID of the local player.
        /// </summary>
        /// <param name="id">The id to set.</param>
        public void SetClientID(int id)
        {
            clientID = id;
        }

        /// <summary>
        /// Gets the local id of the local player.
        /// </summary>
        /// <returns>local id of local player</returns>
        public int GetClientID()
        {
            return clientID;
        }
    }
}
