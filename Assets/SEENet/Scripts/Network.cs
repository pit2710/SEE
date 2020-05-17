﻿using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using SEE.Command;
using SEE.Game;
using SEE.Net.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace SEE.Net
{

    public class Network : MonoBehaviour
    {
        private const Internal.Logger.Severity DEFAULT_SEVERITY = Internal.Logger.Severity.High;
        private static Network instance;

        [SerializeField] private bool useInOfflineMode = true;
        [SerializeField] private bool hostServer = false;
        [SerializeField] private string serverIPAddress = string.Empty;
        [SerializeField] private int localServerPort = 55555;
        [SerializeField] private int remoteServerPort = 0;
        [SerializeField] private bool loadCityOnStart = false;
        [SerializeField] private GameObject loadCityGameObject = null;

#if UNITY_EDITOR
        [SerializeField] private bool nativeLoggingEnabled = false;
        [SerializeField] private Internal.Logger.Severity minimalSeverity = DEFAULT_SEVERITY;
#endif



        public static bool UseInOfflineMode { get => instance ? instance.useInOfflineMode : true; }

        public static bool HostServer { get => instance ? instance.hostServer : false; }

        public static string ServerIPAddress { get => instance ? instance.serverIPAddress : ""; }

        public static int LocalServerPort { get => instance ? instance.localServerPort : -1; }

        public static int RemoteServerPort { get => instance ? instance.remoteServerPort : -1; }

        public static Thread MainThread { get; private set; } = Thread.CurrentThread;

        private static List<Connection> deadConnections = new List<Connection>();



        private void Awake()
        {
            if (instance)
            {
                Debug.LogError("There must not be more than one Network-script! This script will be destroyed!");
                Destroy(this);
                return;
            }

            instance = this;

            if (!useInOfflineMode)
            {
#if UNITY_EDITOR
                if (nativeLoggingEnabled)
                {
                    NetworkComms.EnableLogging(new Internal.Logger(minimalSeverity));
                }
                else
                {
                    NetworkComms.DisableLogging();
                }
#else
                NetworkComms.DisableLogging();
#endif

                try
                {
                    if (hostServer)
                    {
                        Server.Initialize();
                    }
                    Client.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogWarning("Some network-error happened! Continuing in offline mode...");
                    useInOfflineMode = true;
                }
            }

            InitializeGame();
        }

        private void InitializeGame()
        {
            if ((useInOfflineMode || hostServer) && loadCityOnStart && loadCityGameObject != null)
            {
                AbstractSEECity seeCity = loadCityGameObject.GetComponent<AbstractSEECity>();
                if (seeCity)
                {
                    new LoadCityCommand(seeCity).Execute();
                }
                else
                {
                    Debug.LogWarning("Attached GameObject does not contain an AbstractSEECity script! City will not be loaded!");
                }
            }

            new InstantiateCommand("Prefabs/SEENetPlayer").Execute();

            GameObject rig = GameObject.Find("Player Rig");
            if (rig)
            {
                ControlMode mode = rig.GetComponent<ControlMode>();
#if UNITY_EDITOR
                if (mode.ViveController && mode.LeapMotion)
                {
                    Debug.LogError("Only one mode should be enabled!");
                }
#endif
                if (mode.ViveController)
                {
                    new InstantiateCommand("Prefabs/SEENetViveControllerLeft").Execute();
                    new InstantiateCommand("Prefabs/SEENetViveControllerRight").Execute();
                    new InstantiateCommand("Prefabs/SEENetViveControllerRay").Execute();
                }
                else if (mode.LeapMotion)
                {
                    throw new NotImplementedException("Multiplayer does not support Leap Motion!");
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError("No mode selected!");
                }
#endif
            }
        }

        private void Update()
        {
            if (hostServer && !useInOfflineMode)
            {
                Server.Update();
            }
            Client.Update();
        }

        private void OnDestroy()
        {
            if (!useInOfflineMode)
            {
                if (hostServer)
                {
                    Server.Shutdown();
                }
                Client.Shutdown();
            }

            // TODO: there must be a better way to stop the logging spam!
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                FileInfo fileInfo = fileInfos[i];
                string fileName = fileInfo.Name;
                string[] prefixes = new string[] {
                    "CompleteIncomingItemTaskError",
                    "ConnectionKeepAlivePollError",
                    "Error",
                    "ManagedThreadPoolCallBackError",
                    "PacketHandlerErrorGlobal"
                };
                for (int j = 0; j < prefixes.Length; j++)
                {
                    if (fileName.Contains(prefixes[j]))
                    {
                        Debug.Log("Deleting file: '" + fileInfo.FullName + "'!");
                        fileInfo.Delete();
                        break;
                    }
                }
            }
        }



        internal static void SendPacket(Connection connection, AbstractPacket packet)
        {
            Assert.IsNotNull(connection);
            Assert.IsNotNull(packet);

            instance?.SendPacket(connection, PacketSerializer.Serialize(packet));
        }

        private void SendPacket(Connection connection, string serializedPacket)
        {
            string packetType = Client.Connection.Equals(connection) ? Server.PACKET_TYPE : Client.PACKET_TYPE;
            try
            {
                connection.SendObject(packetType, serializedPacket);
            }
            catch (Exception)
            {
                lock (deadConnections)
                {
                    if (!deadConnections.Contains(connection))
                    {
                        deadConnections.Add(connection);
                        Invoker.Invoke((Connection c) => { deadConnections.Remove(c); }, 1.0f, connection);
                        Debug.LogWarning(
                            "Packet could not be sent to '" +
                            connection.ConnectionInfo.RemoteEndPoint.ToString() +
                            "'! Destination may not be listening or connection timed out. Closing connection!"
                        );
                        // TODO: close connection. also, look at exception above
                    }
                }
            }
        }


        public static bool IsLocalIPAddress(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return false;
            }

            IPAddress[] localIPAddresses = LookupLocalIPAddresses();
            return localIPAddresses.Contains(ipAddress);
        }

        public static IPAddress[] LookupLocalIPAddresses()
        {
            string hostName = Dns.GetHostName(); ;
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            return hostEntry.AddressList;
        }


        internal static void ExecuteCommand(AbstractCommand command)
        {
            if (instance.useInOfflineMode)
            {
                command.ExecuteOnServer();
                KeyValuePair<GameObject[], GameObject[]> result = command.ExecuteOnClient();
                if (command.buffer)
                {
                    CommandHistory.OnExecute(null, result.Key, result.Value);
                }
            }
            else
            {
                ExecuteCommandPacket packet = new ExecuteCommandPacket(command);
                SendPacket(Client.Connection, packet);
            }
        }

        internal static void RedoCommand()
        {
            if (instance.useInOfflineMode)
            {
                CommandHistory.RedoOnClient();
            }
            else
            {
                RedoCommandPacket packet = new RedoCommandPacket();
                SendPacket(Client.Connection, packet);
            }
        }

        internal static void UndoCommand()
        {
            if (instance.useInOfflineMode)
            {
                CommandHistory.UndoOnClient();
            }
            else
            {
                UndoCommandPacket packet = new UndoCommandPacket();
                SendPacket(Client.Connection, packet);
            }
        }
    }

}
