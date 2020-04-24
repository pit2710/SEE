﻿using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using SEE.Command;
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

            if (useInOfflineMode)
            {
                return;
            }

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

            if (hostServer)
            {
                Server.Initialize();
            }
            Client.Initialize();

            Initialize();
        }

        private void Initialize()
        {
            Instantiate("Prefabs/SEENetPlayer");

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
                    Instantiate("Prefabs/SEENetViveControllerLeft");
                    Instantiate("Prefabs/SEENetViveControllerRight");
                    Instantiate("Prefabs/SEENetViveControllerRay");
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


        public static void Instantiate(string prefabName)
        {
            Instantiate(prefabName, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void Instantiate(string prefabName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            InstantiatePacket p = new InstantiatePacket(prefabName, Client.LocalEndPoint, position, rotation, scale);
            if (instance.useInOfflineMode)
            {
                Client.PacketHandler.Push(
                    new PacketHeader(Client.PACKET_PREFIX + InstantiatePacket.PACKET_TYPE, 0),
                    null,
                    p.Serialize()
                );
            }
            else
            {
                Send(Client.Connection, p);
            }
        }



        internal static void Send(Connection connection, Internal.Packet packet, SendReceiveOptions options = null)
        {
            Send(connection, packet.packetType, packet.Serialize(), options);
        }

        internal static void Send(Connection connection, string packetType, string packetData, SendReceiveOptions options = null)
        {
            Assert.IsNotNull(connection);
            Assert.IsNotNull(packetData);
            Assert.IsNotNull(packetType);
            Assert.IsNotNull(Client.Connection);

            string packetTargetPrefix = Client.Connection.Equals(connection) ? Server.PACKET_PREFIX : Client.PACKET_PREFIX;
            string fullPacketType = packetTargetPrefix + packetType;

            if (instance.useInOfflineMode)
            {
                Debug.LogWarning("Packets can not be sent in offline mode!");
                return;
            }

            new Thread(new ThreadStart(() =>
            {
                try
                {
                    connection.SendObject(fullPacketType, packetData, options ?? NetworkComms.DefaultSendReceiveOptions);
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
            })).Start();
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
                command.ExecuteLocally();
            }
            else
            {
                CommandPacket commandPacket = new CommandPacket(command);
                Send(Client.Connection, commandPacket);
            }
        }
    }

}
