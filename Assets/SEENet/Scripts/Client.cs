﻿using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace SEE.Net.Internal
{

    public static class Client
    {
        public static readonly string PACKET_PREFIX = "Client.";
        public static Connection Connection { get; private set; } = null;
        public static ClientPacketHandler PacketHandler { get; private set; } = new ClientPacketHandler(PACKET_PREFIX);
        public static IPEndPoint LocalEndPoint { get => Connection != null ? (IPEndPoint)Connection.ConnectionInfo.LocalEndPoint : null; }
        public static IPEndPoint RemoteEndPoint { get => Connection != null ? (IPEndPoint)Connection.ConnectionInfo.RemoteEndPoint : null; }

        public static void Initialize()
        {
            void OnIncomingPacket(PacketHeader packetHeader, Connection connection, string data) => PacketHandler.Push(packetHeader, connection, data);

            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PACKET_PREFIX + GXLPacketData.PACKET_NAME, OnIncomingPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PACKET_PREFIX + InstantiatePacketData.PACKET_NAME, OnIncomingPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PACKET_PREFIX + TransformViewPositionPacketData.PACKET_NAME, OnIncomingPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PACKET_PREFIX + TransformViewRotationPacketData.PACKET_NAME, OnIncomingPacket);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>(PACKET_PREFIX + TransformViewScalePacketData.PACKET_NAME, OnIncomingPacket);

            List<IPEndPoint> endPoints = Network.HostServer
                ? (from connectionListener in Server.ConnectionListeners select connectionListener.LocalListenEndPoint as IPEndPoint).ToList()
                : new List<IPEndPoint>() { new IPEndPoint(IPAddress.Parse(Network.ServerIPAddress), Network.RemoteServerPort) };

            bool success = false;
            foreach (ConnectionInfo connectionInfo in from endPoint in endPoints select new ConnectionInfo(endPoint))
            {
                try
                {
                    Connection = TCPConnection.GetConnection(connectionInfo);
                    success = true;
                    break;
                }
                catch (ConnectionSetupException) { }
            }
            if (!success)
            {
                throw new ConnectionSetupException();
            }
        }
        public static void Update()
        {
            PacketHandler.HandlePendingPackets();
        }
        public static void Shutdown() // TODO: send message to server
        {
            Connection?.CloseConnection(false);
            Connection = null;
        }
    }

}
