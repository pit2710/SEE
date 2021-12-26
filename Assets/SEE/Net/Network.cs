﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using SEE.Game.City;
using SEE.GO;
using SEE.Net.Util;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace SEE.Net
{
    /// <summary>
    /// Handles the most general parts of networking.
    /// </summary>
    public class Network : MonoBehaviour
    {
        /// <summary>
        /// The default severity of the native logger of <see cref="NetworkCommsDotNet"/>.
        /// </summary>
        private const NetworkCommsLogger.Severity DefaultSeverity = NetworkCommsLogger.Severity.High;

        /// <summary>
        /// The single unique instance of the network.
        /// </summary>
        public static Network Instance { get; set; }

        /// <summary>
        /// The maximal port number.
        /// </summary>
        private const int MaxServerPort = 65535;

        /// <summary>
        /// The port of the server where the server listens to SEE action requests.
        /// Note: This field is accessed in NetworkEditor, hence, the name must not change.
        /// </summary>
        public int ServerActionPort = 55555;

        /// <summary>
        /// The port where the server listens to NetCode and Dissonance traffic.
        /// Valid range is [0, 65535].
        /// </summary>
        public int ServerPort
        {
            set
            {
                if (value < 0 ||value > MaxServerPort)
                {
                    throw new ArgumentOutOfRangeException($"A port must be in [0..{MaxServerPort}. Received: {value}.");
                }
                Unity.Netcode.Transports.UNET.UNetTransport netTransport
                    = NetworkManager.Singleton.NetworkConfig.NetworkTransport
                    as Unity.Netcode.Transports.UNET.UNetTransport;
                netTransport.ConnectPort = value;
                netTransport.ServerListenPort = value;

            }
            get
            {
                Unity.Netcode.Transports.UNET.UNetTransport netTransport
                    = NetworkManager.Singleton.NetworkConfig.NetworkTransport
                      as Unity.Netcode.Transports.UNET.UNetTransport;
                return netTransport.ServerListenPort;
            }
        }

        /// <summary>
        /// The IP4 address of the server.
        /// </summary>
        public string ServerIP4Address
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException($"Invalid server IP address: {value}.");
                }
                Unity.Netcode.Transports.UNET.UNetTransport netTransport
                    = NetworkManager.Singleton.NetworkConfig.NetworkTransport
                    as Unity.Netcode.Transports.UNET.UNetTransport;
                netTransport.ConnectAddress = value;
            }

            get
            {
                Unity.Netcode.Transports.UNET.UNetTransport netTransport
                   = NetworkManager.Singleton.NetworkConfig.NetworkTransport
                     as Unity.Netcode.Transports.UNET.UNetTransport;
                return netTransport.ConnectAddress;
            }
        }

        /// <summary>
        /// Whether the city should be loaded on start up. Is ignored, if this client
        /// does not host the server.
        /// </summary>
        [SerializeField] private bool loadCityOnStart = false;

#if UNITY_EDITOR
        /// <summary>
        /// Whether the logging of NetworkComms should be enabled.
        /// </summary>
        [SerializeField] private bool networkCommsLoggingEnabled = false;

        /// <summary>
        /// Whether the internal logging should be enabled.
        /// </summary>
        [SerializeField] private bool internalLoggingEnabled = true;

        /// <summary>
        /// The minimal logged severity.
        /// </summary>
        [SerializeField] private NetworkCommsLogger.Severity minimalSeverity = DefaultSeverity;
#endif

        /// <summary>
        /// Submitted packets, that will be sent in the next <see cref="LateUpdate"/>.
        /// </summary>
        private readonly Dictionary<Connection, List<string>> submittedSerializedPackets = new Dictionary<Connection, List<string>>();

        /// <summary>
        /// True if we are running a host or server.
        /// </summary>
        public static bool HostServer => NetworkManager.Singleton != null
            && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);

        /// <summary>
        /// The IP address of the host or server, respectively; the empty string
        /// if none is set.
        /// </summary>
        public static string RemoteServerIPAddress => NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress;

        /// <summary>
        /// <see cref="loadCityOnStart"/>
        /// </summary>
        public static bool LoadCityOnStart => Instance && Instance.loadCityOnStart;

#if UNITY_EDITOR
        /// <summary>
        /// <see cref="internalLoggingEnabled"/>
        /// </summary>
        public static bool InternalLoggingEnabled => Instance && Instance.internalLoggingEnabled;
#endif

        /// <summary>
        /// The Unity main thread. Note that we cannot initialize its value here
        /// because the elaboration code initializing static attributes may be
        /// executed by a thread different from Unity's main thread. This attribute
        /// will be initialized in <see cref="Awake"/> for this reason.
        /// </summary>
        private static Thread mainThread = null;
        /// <summary>
        /// Contains the Unity main thread of the application.
        /// </summary>
        public static Thread MainThread
        {
            get
            {
                Assert.IsNotNull(mainThread, "The main Unity thread must not have been determined as of now!");
                return mainThread;
            }
            private set
            {
                Assert.IsNull(mainThread, "The main Unity thread has already been determined!");
                Assert.IsNotNull(value, "The main Unity thread must not be null!");
                mainThread = value;
            }
        }

        /// <summary>
        /// List of dead connections. If packets can not be sent, this list is searched
        /// to reduce the frequency of warning messages.
        /// </summary>
        private static readonly List<Connection> deadConnections = new List<Connection>();

        /// <summary>
        /// Makes sure that we have only one <see cref="Instance"/>.
        /// </summary>
        private void Start()
        {
            if (Instance)
            {
                if (Instance != this)
                {
                    Util.Logger.LogError("There must not be more than one Network component! "
                        + $"This component in {gameObject.GetFullName()} will be destroyed!\n");
                    Destroy(this);
                    return;
                }
            }
            else
            {
                Instance = this;
            }

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }

        /// <summary>
        /// Initializes the server, client and game.
        /// </summary>
        private void StartUp()
        {
            /// The field <see cref="MainThread"/> is supposed to denote Unity's main thread.
            /// The <see cref="Awake"/> function is guaranteed to be executed by Unity's main
            /// thread, that is, <see cref="Thread.CurrentThread"/> represents Unity's
            /// main thread here.
            MainThread = Thread.CurrentThread;

#if UNITY_EDITOR
            if (networkCommsLoggingEnabled)
            {
                NetworkComms.EnableLogging(new NetworkCommsLogger(minimalSeverity));
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
                if (HostServer)
                {
                    Server.Initialize();
                }
                Client.Initialize();
                StartVoiceChat();
            }
            catch (Exception e)
            {
                Util.Logger.LogError("Some network error happened! Exception: " + e);
            }

            InitializeGame();
        }

        /// <summary>
        /// Starts the selected voice chat system according to <see cref="VoiceChat"/>.
        /// </summary>
        private void StartVoiceChat()
        {
            switch (VoiceChat)
            {
                case VoiceChatSystems.Vivox:
                    VivoxInitialize();
                    break;
                case VoiceChatSystems.Dissonance:
                    DissonanceInitialize();
                    break;
                case VoiceChatSystems.None:
                    // nothing to be done
                    break;
                default:
                    throw new NotImplementedException($"Unhanded voice chat option {VoiceChat}.");
            }
        }

        /// <summary>
        /// Initalizes Dissonance as the voice chat system.
        /// </summary>
        private void DissonanceInitialize()
        {
            // FIXME: Needs to be implemented.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the game.
        /// </summary>
        private void InitializeGame()
        {
            if (HostServer && loadCityOnStart)
            {
                foreach (AbstractSEECity city in FindObjectsOfType<AbstractSEECity>())
                {
                    if (city is SEECity seeCity)
                    {
                        seeCity.LoadAndDrawGraph();
                    }
                    else
                    {
                        Util.Logger.LogError("Unsupported city type!");
                    }
                }
            }

            GameObject rig = GameObject.Find("Player Rig");
            if (rig)
            {
                // FIXME this has to adapted once VR-hardware is available. Also, this is now initialized in Server.cs
#if false
                ControlMode mode = rig.GetComponent<ControlMode>();
#if UNITY_EDITOR
                if (mode.ViveController && mode.LeapMotion)
                {
                    Logger.LogError("Only one mode should be enabled!");
                }
#endif
                if (mode.ViveController)
                {
                    new InstantiateAction("SEENetViveControllerLeft").Execute();
                    new InstantiateAction("SEENetViveControllerRight").Execute();
                    new InstantiateAction("SEENetViveControllerRay").Execute();
                }
                else if (mode.LeapMotion)
                {
                    throw new NotImplementedException("Multiplayer does not support Leap Motion!");
                }
#if UNITY_EDITOR
                else
                {
                    Logger.LogError("No mode selected!");
                }
#endif
#endif
            }
        }

        /// <summary>
        /// Sends all pending packets.
        /// </summary>
        private void LateUpdate()
        {
            if (HostServer)
            {
                Server.Update();
            }
            Client.Update();

            if (submittedSerializedPackets.Count != 0)
            {
                foreach (Connection connection in submittedSerializedPackets.Keys)
                {
                    List<string> serializedObjects = submittedSerializedPackets[connection];

                    if (serializedObjects.Count != 0)
                    {
                        ulong id = ulong.MaxValue;
                        if (HostServer && Server.Connections.Contains(connection))
                        {
                            id = Server.outgoingPacketSequenceIDs[connection]++;
                        }
                        else if (Client.Connection.Equals(connection))
                        {
                            id = Client.outgoingPacketID++;
                        }
                        Assert.IsTrue(id != ulong.MaxValue);

                        PacketSequencePacket packet = new PacketSequencePacket(id, serializedObjects.ToArray());
                        Send(connection, PacketSerializer.Serialize(packet));
                        serializedObjects.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Shuts down the server and the client.
        /// </summary>
        private void OnDestroy()
        {
            Server.Shutdown();
            Client.Shutdown();

            // FIXME there must be a better way to stop the logging spam!
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                string fileName = fileInfo.Name;
                string[] prefixes = {
                    "CompleteIncomingItemTaskError",
                    "ConnectionKeepAlivePollError",
                    "Error",
                    "ManagedThreadPoolCallBackError",
                    "PacketHandlerErrorGlobal"
                };
                if (prefixes.Any(t => fileName.Contains(t)))
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (IOException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Submits a packet for dispatch.
        /// </summary>
        /// <param name="connection">The connecting, the packet should be sent through.
        /// </param>
        /// <param name="packet">The packet to be sent.</param>
        internal static void SubmitPacket(Connection connection, AbstractPacket packet)
        {
            Assert.IsNotNull(connection);
            Assert.IsNotNull(packet);

            SubmitPacket(connection, PacketSerializer.Serialize(packet));
        }

        /// <summary>
        /// Submits a packet for dispatch.
        /// </summary>
        /// <param name="connection">The connecting, the packet should be sent through.
        /// </param>
        /// <param name="packet">The serialized packet to be sent.</param>
        internal static void SubmitPacket(Connection connection, string serializedPacket)
        {
            bool result = Instance.submittedSerializedPackets.TryGetValue(connection, out List<string> serializedPackets);
            if (!result)
            {
                serializedPackets = new List<string>();
                Instance.submittedSerializedPackets.Add(connection, serializedPackets);
            }
            serializedPackets.Add(serializedPacket);
        }

        /// <summary>
        /// Sends a serialized packet via given connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="serializedPacket">The serialized packet to be sent.</param>
        private void Send(Connection connection, string serializedPacket)
        {
            string packetType = Client.Connection.Equals(connection) ? Server.PacketType : Client.PacketType;

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
                        Util.Logger.LogWarning(
                            "Packet could not be sent to '" +
                            connection.ConnectionInfo.RemoteEndPoint.ToString() +
                            "'! Destination may not be listening or connection timed out. Closing connection!"
                        );
                        if (HostServer)
                        {
                            connection.CloseConnection(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an array of all local IP-Addresses.
        /// </summary>
        /// <returns>An array of all local IP-Addresses.</returns>
        public static IPAddress[] LookupLocalIPAddresses()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            return hostEntry.AddressList;
        }

        /// <summary>
        /// The name of the scene to be loaded when the game starts.
        /// </summary>
        [Tooltip("The name of the game scene.")]
        public string GameScene = "SEEWorld";

        /// <summary>
        /// Loads the <see cref="GameScene"/>. Will be called when the server was started.
        /// </summary>
        private void OnServerStarted()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
        }

        /// <summary>
        /// Starts a host process, i.e., a server and a local client.
        /// </summary>
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            StartUp();
        }

        /// <summary>
        /// Starts a client.
        /// </summary>
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            StartUp();
        }

        /// <summary>
        /// Starts a dedicated server without client.
        /// </summary>
        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            StartUp();
        }

        /// <summary>
        /// The kinds of voice-chats system we support. None means no voice
        /// chat whatsoever.
        /// </summary>
        private enum VoiceChatSystems
        {
            None = 0,       // no voice chat
            Dissonance = 1, // Dissonance voice chat
            Vivox = 2       // Vivox voice chat
        }

        /// <summary>
        /// The voice chat system as selected by the user. Note: This attribute
        /// can be changed in the editor via <see cref="NetworkEditor"/> as well
        /// as at the start up in the <see cref="OpeningDialog"/>.
        /// </summary>
        [SerializeField]
        private VoiceChatSystems VoiceChat = VoiceChatSystems.None;

        /// <summary>
        /// Shuts down the voice-chat system.
        /// </summary>
        private void OnApplicationQuit()
        {
            switch (VoiceChat)
            {
                case VoiceChatSystems.None:
                    // nothing to be done
                    break;
                case VoiceChatSystems.Dissonance:
                    break;
                case VoiceChatSystems.Vivox:
                    VivoxClient?.Uninitialize();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #region Vivox

        public const string VivoxIssuer = "torben9605-se19-dev";
        public const string VivoxDomain = "vdx5.vivox.com";
        public const string VivoxSecretKey = "kick271";
        public static readonly TimeSpan VivoxExpirationDuration = new TimeSpan(365, 0, 0, 0);

        public static VivoxUnity.Client VivoxClient { get; private set; } = null;
        public static VivoxUnity.AccountId VivoxAccountID { get; private set; } = null;
        public static VivoxUnity.ILoginSession VivoxLoginSession { get; private set; } = null;
        public static VivoxUnity.IChannelSession VivoxChannelSession { get; private set; } = null;

        [SerializeField] private string vivoxChannelName = string.Empty;
        public static string VivoxChannelName { get => Instance ? Instance.vivoxChannelName : string.Empty; }

        private static void VivoxInitialize()
        {
            VivoxUnity.VivoxConfig config = new VivoxUnity.VivoxConfig { InitialLogLevel = vx_log_level.log_debug };
            VivoxClient = new VivoxUnity.Client();
            VivoxClient.Initialize(config);

            string userName = "u-" + Client.LocalEndPoint.Address.ToString().Replace(':', '.') + '-' + Client.LocalEndPoint.Port;
            VivoxAccountID = new VivoxUnity.AccountId(VivoxIssuer, userName, VivoxDomain);
            VivoxLoginSession = VivoxClient.GetLoginSession(VivoxAccountID);
            VivoxLoginSession.PropertyChanged += VivoxOnLoginSessionPropertyChanged;
            VivoxLoginSession.BeginLogin(new Uri("https://vdx5.www.vivox.com/api2"), VivoxLoginSession.GetLoginToken(VivoxSecretKey, VivoxExpirationDuration), ar0 =>
            {
                VivoxLoginSession.EndLogin(ar0);

                string channelName = channelName = "c-" + VivoxChannelName;
                VivoxUnity.ChannelId channelID = new VivoxUnity.ChannelId(VivoxIssuer, channelName, VivoxDomain, VivoxUnity.ChannelType.NonPositional);

                // NOTE(torben): GetChannelSession() creates a new channel, if it does
                // not exist yet. Thus, a client, that is not the server could
                // potentially create the voice channel. To make sure this does not
                // happen, VivoxInitialize() must always be called AFTER the server
                // and client were initialized, because if this client tries to connect
                // to a server and can not connect, it will go to offline mode and not
                // initialize Vivox.
                VivoxChannelSession = VivoxLoginSession.GetChannelSession(channelID);
                VivoxChannelSession.PropertyChanged += VivoxOnChannelPropertyChanged;
                VivoxChannelSession.MessageLog.AfterItemAdded += VivoxOnChannelMessageReceived;
                VivoxChannelSession.BeginConnect(true, true, true, VivoxChannelSession.GetConnectToken(VivoxSecretKey, VivoxExpirationDuration), ar1 =>
                {
                    VivoxChannelSession.EndConnect(ar1);
                    if (HostServer && VivoxChannelSession.Participants.Count != 0)
                    {
                        // TODO: this channel already exists and the name is unavailable!
                        Util.Logger.Log("Channel with given name already exists. Select a differend name!");
                        VivoxChannelSession.Disconnect();
                        VivoxLoginSession.DeleteChannelSession(channelID);
                    }
                });
            });
        }

        private static void SendGroupMessage()
        {
            string channelName = VivoxChannelSession.Channel.Name;
            string senderName = VivoxAccountID.Name;
            string message = "Hello World!";

            VivoxChannelSession.BeginSendText(message, ar =>
            {
                try
                {
                    VivoxChannelSession.EndSendText(ar);
                }
                catch (Exception e)
                {
                    Util.Logger.LogException(e);
                }
            });
        }

        private static void VivoxOnLoginSessionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "State")
            {
                switch ((sender as VivoxUnity.ILoginSession).State)
                {
                    case VivoxUnity.LoginState.LoggingIn:
                        break;

                    case VivoxUnity.LoginState.LoggedIn:
                        break;

                    case VivoxUnity.LoginState.LoggedOut:
                        break;
                    default:
                        break;
                }
            }
        }

        private static void VivoxOnChannelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            VivoxUnity.IChannelSession channelSession = (VivoxUnity.IChannelSession)sender;

            if (propertyChangedEventArgs.PropertyName == "AudioState")
            {
                switch (channelSession.AudioState)
                {
                    case VivoxUnity.ConnectionState.Connected: Util.Logger.Log("Audio chat connected in " + channelSession.Key.Name + " channel."); break;
                    case VivoxUnity.ConnectionState.Disconnected: Util.Logger.Log("Audio chat disconnected in " + channelSession.Key.Name + " channel."); break;
                }
            }
            else if (propertyChangedEventArgs.PropertyName == "TextState")
            {
                switch (channelSession.TextState)
                {
                    case VivoxUnity.ConnectionState.Connected:
                        Util.Logger.Log("Text chat connected in " + channelSession.Key.Name + " channel.");
                        SendGroupMessage();
                        break;
                    case VivoxUnity.ConnectionState.Disconnected:
                        Util.Logger.Log("Text chat disconnected in " + channelSession.Key.Name + " channel.");
                        break;
                }
            }
        }

        private static void VivoxOnChannelMessageReceived(object sender, VivoxUnity.QueueItemAddedEventArgs<VivoxUnity.IChannelTextMessage> queueItemAddedEventArgs)
        {
            string channelName = queueItemAddedEventArgs.Value.ChannelSession.Channel.Name;
            string senderName = queueItemAddedEventArgs.Value.Sender.Name;
            string message = queueItemAddedEventArgs.Value.Message;

            Util.Logger.Log(channelName + ": " + senderName + ": " + message + "\n");
        }

        #endregion
    }

}
