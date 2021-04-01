﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using UnityEngine;

namespace SEE.Net
{

    /// <summary>
    /// The server of the game. Various clients can connect to this server in case they
    /// know the IP-address.
    /// </summary>
    public static class Server
    {
        /// <summary>
        /// The identifier for packets designated to the server.
        /// </summary>
        public const string PacketType = "Server";

        /// <summary>
        /// The list of all active connections.
        /// </summary>
        public static List<Connection> Connections { get; private set; }

        /// <summary>
        /// The list of all connection listeners of the server.
        /// </summary>
        public static List<ConnectionListenerBase> ConnectionListeners { get; private set; }

        /// <summary>
        /// The packet handler processes incoming packets.
        /// </summary>
        private static PacketHandler packetHandler;

        /// <summary>
        /// All new connections that have yet to be processed. New connections are
        /// announced via a different thread and are buffered and processed later in the
        /// main thread. This is necessary, as most of the Unity features only work in
        /// the main thread.
        /// </summary>
        private static Stack<Connection> pendingEstablishedConnections;

        /// <summary>
        /// All closed connecting that have yet to be processed.
        /// </summary>
        private static Stack<Connection> pendingClosedConnections;

        /// <summary>
        /// For each connection, the next to be processed packet id is saved to ensue the
        /// correct execution order of incoming packets. The first id for every
        /// connection is always zero and is increased after each incoming packet.
        /// </summary>
        public static Dictionary<Connection, ulong> incomingPacketSequenceIDs;

        /// <summary>
        /// The next id per connecting for outgoing packets.
        /// </summary>
        public static Dictionary<Connection, ulong> outgoingPacketSequenceIDs;

        /// <summary>
        /// Whether the server is currently initialized.
        /// </summary>
        private static bool initialized = false;


        /// <summary>
        /// Initializes the server for listening for connections and receiving packets.
        /// </summary>
        public static void Initialize()
        {
            if (!initialized)
            {
                Connections = new List<Connection>();
                ConnectionListeners = new List<ConnectionListenerBase>();
                packetHandler = new PacketHandler(true);
                pendingEstablishedConnections = new Stack<Connection>();
                pendingClosedConnections = new Stack<Connection>();
                incomingPacketSequenceIDs = new Dictionary<Connection, ulong>();
                outgoingPacketSequenceIDs = new Dictionary<Connection, ulong>();

                NetworkComms.AppendGlobalConnectionEstablishHandler((Connection c) => pendingEstablishedConnections.Push(c));
                NetworkComms.AppendGlobalConnectionCloseHandler((Connection c) => pendingClosedConnections.Push(c));

                void OnIncomingPacket(PacketHeader packetHeader, Connection connection, string data) => packetHandler.Push(packetHeader, connection, data);
                NetworkComms.AppendGlobalIncomingPacketHandler<string>(PacketType, OnIncomingPacket);

                try
                {
                    ConnectionListeners.AddRange(Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, Network.LocalServerPort), false));
#if UNITY_EDITOR
                    string message = "Server listening on end-points:";
                    foreach (ConnectionListenerBase connectionListener in ConnectionListeners)
                    {
                        message += "\n" + connectionListener.LocalListenEndPoint;
                    }
                    Logger.Log(message);
#endif
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }

                initialized = true;
            }
        }

        /// <summary>
        /// Handles pending packets and established and closed connections.
        /// </summary>
        public static void Update()
        {
            if (initialized)
            {
                while (pendingEstablishedConnections.Count != 0)
                {
                    OnConnectionEstablished(pendingEstablishedConnections.Pop());
                }
                while (pendingClosedConnections.Count != 0)
                {
                    OnConnectionClosed(pendingClosedConnections.Pop());
                }
                packetHandler.HandlePendingPackets();
            }
        }

        /// <summary>
        /// Shuts the server down. Stops listening for connections and closes all valid
        /// connections.
        /// </summary>
        public static void Shutdown()
        {
            if (initialized)
            {
                lock (Connections)
                {
                    initialized = false;

                    outgoingPacketSequenceIDs = null;
                    incomingPacketSequenceIDs = null;
                    pendingClosedConnections = null;
                    pendingEstablishedConnections = null;
                    packetHandler = null;

                    Connection.StopListening(ConnectionListeners);
                    ConnectionListeners = null;

                    foreach (Connection t in Connections)
                    {
                        t.CloseConnection(false);
                    }
                    Connections = null;
                }
            }
        }

        /// <summary>
        /// Handles established connection. Sends game state and buffered packets to new
        /// client.
        /// </summary>
        /// <param name="connection">The established connection.</param>
        private static void OnConnectionEstablished(Connection connection)
        {
            bool connectionListenerInitialized = false;
            foreach (ConnectionListenerBase connectionListener in ConnectionListeners)
            {
                if (connectionListener.LocalListenEndPoint.Equals(connection.ConnectionInfo.LocalEndPoint))
                {
                    connectionListenerInitialized = true;
                    break;
                }
            }

            if (connectionListenerInitialized)
            {
                if (!Connections.Contains(connection))
                {
                    Logger.Log("Connection with client established: " + connection);

                    // synchronize current state with new client
                    if (!connection.ConnectionInfo.RemoteEndPoint.Equals(Client.LocalEndPoint))
                    {
                        IPEndPoint[] recipient = new IPEndPoint[] { (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint };
                        List<InstantiatePrefabAction> actions = PrefabAction.GetAllActions();
                        foreach (InstantiatePrefabAction action in actions)
                        {
                            action.Execute(recipient);
                        }
                        if (Network.LoadCityOnStart)
                        {
                            foreach (Game.AbstractSEECity city in UnityEngine.Object.FindObjectsOfType<Game.AbstractSEECity>())
                            {
                                new LoadCityAction(city).Execute(recipient);
                            }
                        }
                        foreach (Controls.Actions.NavigationAction navigationAction in UnityEngine.Object.FindObjectsOfType<Controls.Actions.NavigationAction>())
                        {
                            new SyncCitiesAction(navigationAction).Execute(recipient);
                        }
                        foreach (Controls.InteractableObject interactableObject in Controls.InteractableObject.GrabbedObjects)
                        {
                            new SetGrabAction(interactableObject, true).Execute(recipient);
                        }
                        foreach (Controls.InteractableObject interactableObject in Controls.InteractableObject.SelectedObjects)
                        {
                            new SetSelectAction(interactableObject, true).Execute(recipient);
                        }
                        foreach (Controls.InteractableObject interactableObject in Controls.InteractableObject.HoveredObjects)
                        {
                            new SetHoverAction(interactableObject, interactableObject.HoverFlags).Execute(recipient);
                        }
                    }

                    // recognize client
                    Connections.Add(connection);
                    incomingPacketSequenceIDs.Add(connection, 0);
                    outgoingPacketSequenceIDs.Add(connection, 0);

                    // create new player head for every client
                    new InstantiatePrefabAction(
                        (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint,
                        "PlayerHead",
                        Vector3.zero,
                        Quaternion.identity,
                        new Vector3(0.02f, 0.015f, 0.015f)
                    ).Execute();
                }
                else
                {
                    connection.CloseConnection(true);
                }
            }
        }

        /// <summary>
        /// Handles closing of given connection. Destroys the player prefab of given
        /// connection.
        /// </summary>
        /// <param name="connection"></param>
        private static void OnConnectionClosed(Connection connection)
        {
            bool connectionListenerInitialized = ConnectionListeners.Any(listener => listener.LocalListenEndPoint.Equals(connection.ConnectionInfo.LocalEndPoint));

            if (connectionListenerInitialized)
            {
                lock (Connections)
                {
                    Connections.Remove(connection);
                    incomingPacketSequenceIDs.Remove(connection);
                    outgoingPacketSequenceIDs.Remove(connection);

                    IPEndPoint remoteEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

                    ViewContainer[] viewContainers = ViewContainer.GetViewContainersByOwner(remoteEndPoint);
                    foreach (ViewContainer viewContainer in viewContainers)
                    {
                        new DestroyPrefabAction(viewContainer).Execute();
                    }

                    if (SetGrabAction.GrabbedObjects.TryGetValue(remoteEndPoint, out HashSet<Controls.InteractableObject> grabbedInteractables))
                    {
                        foreach (Controls.InteractableObject grabbedInteractable in grabbedInteractables)
                        {
                            SetGrabAction action = new SetGrabAction(grabbedInteractable, false);
                            action.SetRequester(remoteEndPoint);
                            action.Execute();
                        }
                    }
                    if (SetSelectAction.SelectedObjects.TryGetValue(remoteEndPoint, out HashSet<Controls.InteractableObject> selectedInteractables))
                    {
                        foreach (Controls.InteractableObject selectedInteractable in selectedInteractables)
                        {
                            SetSelectAction action = new SetSelectAction(selectedInteractable, false);
                            action.SetRequester(remoteEndPoint);
                            action.Execute();
                        }
                    }
                    if (SetHoverAction.HoveredObjects.TryGetValue(remoteEndPoint, out HashSet<Controls.InteractableObject> hoveredInteractables))
                    {
                        foreach (Controls.InteractableObject hoveredInteractable in hoveredInteractables)
                        {
                            SetHoverAction action = new SetHoverAction(hoveredInteractable, 0); // TODO(torben): is the '0' correct here?
                            action.SetRequester(remoteEndPoint);
                            action.Execute();
                        }
                    }

                    Logger.Log("Connection closed: " + connection);
                }
            }
        }
    }

}
