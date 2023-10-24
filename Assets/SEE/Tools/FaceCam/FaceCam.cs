#if !PLATFORM_LUMIN || UNITY_EDITOR // This Line of code is from the WebCamTextureToMatHelperExample.
using OpenCVForUnity.UnityUtils.Helper;
using SEE.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using SEE.Controls;
using SEE.GO;
using UnityEngine.Assertions;

namespace SEE.Tools.FaceCam
{
    /// <summary>
    /// This component can be used to display a WebCam image of the tracked
    /// face of the user over the network.
    ///
    /// This component is attached to a FaceCam.prefab, which in turn is an
    /// immediate child of an avatar game object. The avatar can be local
    /// or remote, in other words, every avatar his this child FaceCam prefab
    /// as a child.
    ///
    /// The avatar is assumed to have a <see cref="NetworkObject"/> component
    /// attached to it.
    ///
    /// Note that a <see cref="NetworkBehaviour"/> cannot be instantiated at
    /// run-time programmatically, it needs to be instantiated along with a
    /// game object having a <see cref="NetworkObject"/> component.
    ///
    /// The face cam can be switched off and off, and it can toggle the position between being
    /// above the player always facing the camera and the front of the player's face.
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    internal partial class FaceCam : NetworkBehaviour
    {
        /// <summary>
        /// The object with the position where the face/nose of the player is at.
        /// </summary>
        private Transform playersFace;

        /// <summary>
        /// All Network Ids, but not the owner (where the video is recorded) or the server.
        /// This attribute is assigned in <see cref="CreateClientRpcParams()"/>, but never
        /// read.
        /// TODO: Is it really needed? Maybe the assigned value is kept in this field
        /// such that it will not be cleaned up by the garbage collector.
        /// </summary>
        private ClientRpcParams clientsIdsRpcParams;

        /// <summary>
        /// The WebGL coroutine to get the dlib shape predictor file path.
        /// </summary>
#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        /// <summary>
        /// The on/off state of the FaceCam.
        /// </summary>
        private bool faceCamOn;

        /// <summary>
        /// The state of the position of the FaceCam.
        /// Can be on front of the face or above the face, tilted to the observer.
        /// </summary>
        private bool faceCamOnFront = true;

        /// <summary>
        /// The mesh renderer of the FaceCam, used to hide it.
        /// </summary>
        private MeshRenderer meshRenderer;

        /// <summary>
        /// The material of the FaceCam, its texture displaying a default picture, or
        /// the face of the user.
        /// </summary>
        private Material mainMaterial;

        /// <summary>
        /// A timer used to ensure the frame rate of the video transmitted over the network.
        /// It counts the seconds until the video is transmitted. Then it resets.
        /// </summary>
        private float networkVideoTimer;

        /// <summary>
        /// A delay used to ensure the frame rate of the video transmitted over the network.
        /// Seconds until the video is transmitted.
        /// </summary>
        private float networkVideoDelay;

        /// <summary>
        /// Set the frame rate of video network transmission.
        /// </summary>
        [SerializeField, FormerlySerializedAs("Network FPS"),
            Tooltip("Set the frame rate of the video which will be transmitted over the Network.")]
        private float networkFPS;
        public float NetworkFPS
        {
            get => networkFPS;
            set => networkFPS = Mathf.Clamp(value, 1, float.MaxValue);
        }

        /// <summary>
        /// Adds the own client id to the server's list of clients to which the video should
        /// be broadcasted if and only
        ///
        /// </summary>
        /// <remarks>Called on network spawn before Start through NetCode.</remarks>
        public override void OnNetworkSpawn()
        {
            Initialize();

            // IsOwner is true if the local client is the owner of this NetworkObject.
            // IsServer is true if this code runs on the server. Note: a host can be
            // a server and a local client at the same time, in which case IsServer
            // would also be true.
            // The default NetworkObject.Spawn method assumes server-side ownership,
            // but the ownership can be transferred to a client (and also returned to
            // the server again). We do not do that. That is, our server always owns
            // all network objects.

            // Add own ClientId to list of Clients, to which the video should be broadcasted.
            AddClientIdToListServerRPC(NetworkManager.Singleton.LocalClientId);
            // Always invoke the base.
            base.OnNetworkSpawn();
        }

        /// <summary>
        /// The web camera where the face of the local player is extracted from.
        /// </summary>
        private WebCam webCam;

        /// <summary>
        /// Creates <see cref="webCam"/>.
        /// </summary>
        private void Awake()
        {
            webCam = new();
        }

        #region WebCamTextureToMatHelper event handler
        /// <summary>
        /// Notifies the underlying <see cref="webCam"/> that <see cref="WebCamTextureToMatHelper"/>
        /// has been initialized.
        /// </summary>
        /// <remarks>This method is registered at the <see cref="WebCamTextureToMatHelper"/> component
        /// attached to the same game object this <see cref="FaceCam"/> is attached to.
        /// The setting is done in the FaceCam prefab.</remarks>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            webCam.OnWebCamTextureToMatHelperInitialized();
        }

        /// <summary>
        /// Notifies the underlying <see cref="webCam"/> that <see cref="WebCamTextureToMatHelper"/>
        /// has been disposed.
        /// </summary>
        /// <remarks>This method is registered at the <see cref="WebCamTextureToMatHelper"/> component
        /// attached to the same game object this <see cref="FaceCam"/> is attached to.
        /// The setting is done in the FaceCam prefab.</remarks>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            webCam.OnWebCamTextureToMatHelperDisposed();
        }

        /// <summary>
        /// Notifies the underlying <see cref="webCam"/> that an error has occurred for the
        /// <see cref="WebCamTextureToMatHelper"/>
        /// </summary>
        /// <remarks>This method is registered at the <see cref="WebCamTextureToMatHelper"/> component
        /// attached to the same game object this <see cref="FaceCam"/> is attached to.
        /// The setting is done in the FaceCam prefab.</remarks>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            WebCam.OnWebCamTextureToMatHelperErrorOccurred(errorCode);
        }

        #endregion

        /// <summary>
        /// Initializes <see cref="webCamTextureToMatHelper"/> if not already set.
        /// Sets <see cref="meshRenderer"/>.
        ///
        /// If this game object does not have a <see cref="WebCamTextureToMatHelper"/> component
        /// attached, this behaviour is disabled.
        ///
        /// If this game object does not have a <see cref="MeshRenderer"/> component
        /// attached, the game object is set inactive.
        /// </summary>
        private void Initialize()
        {
            // For dynamically spawned NetworkObjects (instantiating a network Prefab
            // during runtime) the OnNetworkSpawn method is invoked before the Start
            // method is invoked. So, it's important to be aware of this because finding
            // and assigning components to a local property within the Start method exclusively
            // will result in that property not being set in a NetworkBehaviour component's
            // OnNetworkSpawn method when the NetworkObject is dynamically spawned. To
            // circumvent this issue, you can have a common method that initializes the
            // components and is invoked both during the Start method and the
            // OnNetworkSpawned method. That's the purpose of this method.

            if (webCam.webCamTextureToMatHelper == null)
            {
                if (!gameObject.TryGetComponentOrLog(out webCam.webCamTextureToMatHelper))
                {
                    enabled = false;
                }
            }

            if (!gameObject.TryGetComponentOrLog(out meshRenderer))
            {
                gameObject.SetActive(false);
                return;
            }

            // New texture for the cropped texture only displaying the face, resp. the final texture.
            face ??= new Texture2D(0, 0, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// The 'Start' code, called before the first frame update.
        /// The network FPS, size of the FaceCam, and speed of the face tracking is set.
        /// The location of the players face is saved in a variable.
        /// A cropped texture is created.
        /// The startup code from the WebCamTextureToMatHelperExample is executed.
        /// The status of the FaceCam is received it this is not the owner.
        /// </summary>
        private void Start()
        {
            // The network FPS is used to calculate everything needed to send the video
            // at the specified frame rate.
            networkVideoDelay = 1f / NetworkFPS;

            // This is the size of the FaceCam at the start
            transform.localScale = new Vector3(0.2f, 0.2f, -1); // z = -1 to face away from the player.

            // For the location of the face of the player we use his nose. This makes
            // the FaceCam also aprox. centered to his face.
            playersFace = transform.parent.Find("Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head/NoseBase");

            Initialize();

            webCam.Initialize();

            // Receive the status of the FaceCam if this is not the owner.
            if (!IsOwner)
            {
                GetFaceCamStatusServerRpc();
            }

            // Cache the material of the FaceCam to change its texture later. (Display a default
            // picture or the face of the user).
            mainMaterial = meshRenderer.material;

            if (NetworkManager.LocalClient == null)
            {
                Debug.LogError("[FaceCam.Start] No local client.\n");
            }
            else if (NetworkManager.LocalClient.PlayerObject == null)
            {
                Debug.LogError("[FaceCam.Start] No player object for local client.\n");
            }
            else
            {
                Debug.Log($"[FaceCam.Start] Owner of player {NetworkManager.LocalClient.PlayerObject.name} is server: {NetworkManager.LocalClient.PlayerObject.IsOwnedByServer} or is local client: {NetworkManager.LocalClient.PlayerObject.IsOwner}\n");
            }
        }

        /// <summary>
        /// Once per frame, the local video is displayed.
        /// Switches the FaceCam on and off, if requested by the user.
        /// It also checks whether the video should be sent to the clients in this frame - based
        /// on the specified network FPS - and transmits it.
        /// </summary>
        private void Update()
        {
            // If the NetworkObject is not yet spawned, exit early.
            if (!IsSpawned)
            {
                return;
            }
            // Netcode specific logic executed when spawned.

            // Display/render the video from the Webcam if this is the owner.
            // The local client owns the player object the NetworkObject is attached to.
            // The FaceCam is attached to a child of the local player.
            // Hence, the local player (client) is the owner of the local FaceCam.
            // NetworkBehaviour.IsOwner is true if the local client is the owner of this NetworkObject.
            if (IsOwner)
            {
                // Switch the FaceCam on or off.
                if (SEEInput.ToggleFaceCam())
                {
                    FaceCamOnOffServerRpc(faceCamOn);
                }

                if (faceCamOn)
                {
                    // The local video is displayed.
                    // Renders the cutout texture onto the FaceCam.
                    webCam.GetFace(out Texture2D texture, out Vector3? localScale);
                    RenderLocalFace(texture, localScale);
                    // Used to send video only at specified frame rate.
                    networkVideoTimer += Time.deltaTime;
                    // Check if this is a Frame in which the video should be transmitted
                    if (networkVideoTimer >= networkVideoDelay)
                    {
                        // Transmit and display the frame on all other clients.
                        RenderFaceRemotely();
                        networkVideoTimer = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the position of the FaceCam if it is turned on.
        /// </summary>
        /// <remarks>Called by Unity each frame after the Update() function.</remarks>
        ///
        private void LateUpdate()
        {
            if (faceCamOn)
            {
                RefreshFaceCamPosition();
            }
        }

        /// <summary>
        /// Refresh the position of the FaceCam.
        /// The position can be toggled with the Key 'O'.
        /// This means switching the position between above the avatars face and in front of it.
        /// </summary>
        private void RefreshFaceCamPosition()
        {
            // Switch the position of the FaceCam.
            if (SEEInput.ToggleFaceCamPosition())
            {
                FaceCamOnFrontToggleServerRpc(faceCamOnFront);
            }

            // Calculate the position of the FaceCam
            if (playersFace != null) // Sometimes the playersFace seems to be null, i can't find out why.
                                     // Seems to have nothing to do with this class.
            {
                // Put it where the players face is.
                transform.SetPositionAndRotation(playersFace.position, playersFace.rotation);
                if (faceCamOnFront)
                {
                    // Rotate and move it a bit up and a bit forward.
                    transform.Rotate(0, -90, -90); // To face away from the avatars face.
                    transform.position += transform.forward * 0.025f;
                    transform.position += transform.up * 0.03f;
                }
                else
                {
                    // Rotate and move it a up and a bit forward.
                    transform.Rotate(0, -90, -90); // To face away from the avatars face.
                    transform.position -= transform.forward * 0.08f;
                    transform.position += transform.up * 0.3f;
                    if (!IsOwner) // If this is the owner the FaceCam should just face forward and
                                  // not down to the own camera.
                    {
                        // check if there is any main camera, and face towards it.
                        if (MainCamera.Camera != null)
                        {
                            transform.LookAt(MainCamera.Camera.transform);
                        }
                    }
                }
            }
        }

        #region Client

        /// <summary>
        /// Toggle the FaceCam on/off for all clients.
        /// (Can only be used by the server).
        /// </summary>
        /// <remarks>This call is sent from the server to all its clients.</remarks>
        [ClientRpc]
        private void FaceCamOnOffClientRpc(bool networkFaceCamOn)
        {
#if DEBUG
            Debug.Log($"[RPC] Client {NetworkManager.Singleton.LocalClientId} received FaceCamOnOffClientRpc from server with networkFaceCamOn={networkFaceCamOn}\n");
#endif

            // Note: The host is both a client and a server. If a host invokes a client RPC,
            // it triggers the call on all clients, including the host.
            //
            // When running as a host, Netcode for GameObjects invokes RPCs immediately within the
            // same stack as the method invoking the RPC. Since a host is both considered a server
            // and a client, you should avoid design patterns where a ClientRpc invokes a ServerRpc
            // that invokes the same ClientRpc as this can end up in a stack overflow (infinite
            // recursion).

            // NetworkFaceCamOn, resp. FaceCamOn has the value which should be inverted.
            if (faceCamOn == networkFaceCamOn)
            {
                faceCamOn = !faceCamOn;
                FaceCamOnOffToggle();
            }
        }

        /// <summary>
        /// Toggle the FaceCam position of all clients.
        /// (Can only be used by the server).
        /// </summary
        [ClientRpc]
        private void FaceCamOnFrontToggleClientRpc(bool networkFaceCamOnFront)
        {
#if DEBUG
            Debug.Log($"[RPC] Client {NetworkManager.Singleton.LocalClientId} received FaceCamOnFrontToggleClientRpc from server with networkFaceCamOnFront={networkFaceCamOnFront}\n");
#endif

            if (faceCamOnFront == networkFaceCamOnFront)
            {
                faceCamOnFront = !faceCamOnFront;
            }
        }

        /// <summary>
        /// Set the FaceCam status on all clients.
        /// (Can only be used by the server).
        /// </summary
        [ClientRpc]
        private void SetFaceCamStatusClientRpc(bool faceCamOn, bool faceCamOnFront)
        {
#if DEBUG
            Debug.Log($"[RPC] Client {NetworkManager.Singleton.LocalClientId} received SetFaceCamStatusClientRpc from server with faceCamOn={faceCamOn} and faceCamOnFront={faceCamOnFront}\n");
#endif
            this.faceCamOn = faceCamOn;
            this.faceCamOnFront = faceCamOnFront;
            // Make the FaceCam visible/invisible and/or start/stop it.
            FaceCamOnOffToggle();
        }

        /// <summary>
        /// The Server calls this, to send his video to all clients.
        /// Also every client will render this video onto the FaceCam.
        /// </summary>
        /// <param name="videoFrame">the encoded face texture</param>
        //[ClientRpc(Delivery = RpcDelivery.Unreliable)]
        // Large files not supported by unreliable Rpc. (No documentation found regarding this limitation).
        [ClientRpc]
        private void RenderFaceClientRPC(byte[] videoFrame)
        {
#if DEBUG
            Debug.Log($"[RPC] Client {NetworkManager.Singleton.LocalClientId} received SendVideoToClientsToRenderItClientRPC from server\n");
#endif
            RenderRemoteFace(videoFrame);
        }

        #endregion

        /// <summary>
        /// Toggle the FaceCam on off state.
        /// </summary>
        private void FaceCamOnOffToggle()
        {
            if (faceCamOn)
            {
                Debug.Log("FaceCam is playing.\n");
                webCam.webCamTextureToMatHelper.Play();
            }
            else
            {
                Debug.Log("FaceCam is stopped.\n");
                webCam.webCamTextureToMatHelper.Stop();
            }
            // Hide the FaceCam if it's deactivated.
            meshRenderer.enabled = faceCamOn;
        }

        /// <summary>
        /// Displays the video on any client, but not where the video is recorded.
        /// </summary>
        private void RenderFaceRemotely()
        {
            // A frame of the video, created from the source video already displayed on
            // this owners client.
            byte[] videoFrame = EncodeFace();

            // videoframe is null if the file size is too big.
            if (videoFrame == null)
            {
                Debug.LogWarning("Video frame is too big. Not being sent.\n");
                return;
            }

            RenderFaceServerRPC(videoFrame);
#if false
            // Send the frame to the server, unless this is the server.
            if (!IsServer)
            {
                RenderFaceServerRPC(videoFrame);
            }
            else // If this is the owner (creator of video) and also the server.
            {
                // Send the frame to all clients. (But not the server and owner, which in
                // this case, is the server.)
                RenderFaceClientRPC(videoFrame);
            }
#endif
        }

        /// <summary>
        /// This seems to be the maximum size for files in bytes to be sent over the network.
        /// (No documentation found regarding this limitation).
        /// </summary>
        private const int maximumNetworkByteSize = 32768;

        /// <summary>
        /// Returns a JPG encoding of the <see cref="face"/> texture.
        /// The frame can be sent over the network and is compressed.
        /// </summary>
        /// <returns>face data encoded as JPG; may be <c>null</c> if <see cref="face"/>
        /// could not be encoded or the encoding is too large to be sent</returns>
        private byte[] EncodeFace()
        {
            // Converts the texture to an byte array containing an JPG.
            byte[] networkTexture = face.EncodeToJPG();
            // Only return the array if it's not too big.
            if (networkTexture != null && networkTexture.Length <= maximumNetworkByteSize)
            {
                return networkTexture;
            }
            return null;
        }

        /// <summary>
        /// Texture2D of the cropped webcam frame, containing the face.
        /// </summary>
        public Texture2D face;

        /// <summary>
        /// Renders <paramref name="faceTexture"/>, more precisely, sets <see cref="face"/>
        /// and <see cref="mainMaterial.mainTexture"/> if this <see cref="FaceCam"/>
        /// is attached to a remote avatar. If the avatar represents the local player,
        /// nothing happens.
        /// </summary>
        /// <param name="faceTexture">encoded face data to be rendered</param>
        /// <remarks>
        /// This method is assumed to be called from the server for an avatar representing
        /// a remote player. The <paramref name="faceTexture"/> represents the face of
        /// that remote player. It was captured by a different client.</remarks>
        public void RenderRemoteFace(byte[] faceTexture)
        {
            if (!IsOwner)
            {
                if (faceTexture == null || faceTexture.Length == 0)
                {
                    Debug.LogError("Received empty face texture.\n");
                }
                else if (face is null)
                {
                    Debug.LogError("Face texture is null.\n");
                }
                else if (face.LoadImage(faceTexture))
                {
                    mainMaterial.mainTexture = face;
                }
                else
                {
                    Debug.LogError("Could not decode face texture.\n");
                }
            }
        }

        /// <summary>
        /// Renders <paramref name="faceTexture"/>.
        /// </summary>
        /// <param name="faceTexture">the texture representing a face</param>
        /// <param name="localScale">the local scale the video tile for rendering the face texture should have;
        /// ignored if undefined</param>
        /// <remarks>This method must be called only by the owner. It is intended to
        /// render the face of the local player for the local player him/herself,
        /// which is required if the local player looks into the mirror.</remarks>
        private void RenderLocalFace(Texture2D faceTexture, Vector3? localScale)
        {
            Assert.IsTrue(IsOwner);
            if (faceTexture != null)
            {
                face = faceTexture;
                mainMaterial.mainTexture = faceTexture;
            }
            if (localScale.HasValue)
            {
                transform.localScale = localScale.Value;
            }
        }

        /// <summary>
        /// If the FaceCam should be destroyed (player disconnects), clean everything up.
        /// </summary>
        public override void OnDestroy()
        {
            // Remove own ClientId from the list of connected ClientIds
            if (!IsServer && !IsOwner) // Owner and server is not in the list.
            {
                RemoveClientFromListServerRPC(NetworkManager.Singleton.LocalClientId);
            }

            // Code from the WebCamTextureToMatHelperExample.
            if (webCam.webCamTextureToMatHelper != null)
            {
                webCam.webCamTextureToMatHelper.Dispose();
            }

            // Code from the WebCamTextureToMatHelperExample.
            webCam.faceLandmarkDetector?.Dispose();

            // Code from the WebCamTextureToMatHelperExample.
#if UNITY_WEBGL
            if (getFilePath_Coroutine != null)
            {
                StopCoroutine(getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose();
            }
#endif
            // Code from the WebCamTextureToMatHelperExample.
            // Always invoke the base.
            base.OnDestroy();
        }
    }
}

#endif // This Line of code is from the WebCamTextureToMatHelperExample.

// Author of WebCamTextureToMatHelperExample.cs: Enox Software,
// enoxsoftware.com/, enox.software@gmail.com
