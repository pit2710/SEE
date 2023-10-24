using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SEE.Tools.FaceCam
{
    /// <summary>
    /// The server code of <see cref="FaceCam"/>.
    /// </summary>
    internal partial class FaceCam
    {
        /// <summary>
        /// The network ids of all clients including the local one's.
        /// This list is maintained only on the server.
        /// </summary>
        private readonly List<ulong> clientsIdsList = new();

        /// <summary>
        /// The clients call this to add their ClientId to the list on the Server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void AddClientIdToListServerRPC(ulong clientId, ServerRpcParams serverRpcParams = default)
        {
#if DEBUG
            Debug.Log($"[RPC] Server received AddClientIdToListServerRPC from {serverRpcParams.Receive.SenderClientId} with clientId={clientId}\n");
#endif
            clientsIdsList.Add(clientId);
            // Create the RpcParams from the list to make the list usable as RpcParams.
            CreateClientRpcParams();
        }

        /// <summary>
        /// The clients call this to remove their ClientId from the list on the Server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RemoveClientFromListServerRPC(ulong clientId, ServerRpcParams serverRpcParams = default)
        {
#if DEBUG
            Debug.Log($"[RPC] Server received RemoveClientFromListServerRPC from {serverRpcParams.Receive.SenderClientId} with clientId={clientId}\n");
#endif
            clientsIdsList.Remove(clientId);
            // Create the RpcParams to make this list usable.
            CreateClientRpcParams();
        }

        /// <summary>
        /// This creates RpcParams from the list of ClientIds to make it usable.
        /// Only the server needs to work with this list.
        /// RpcParams is used to send RPC calls only to few Clients, and not to all.
        /// </summary>
        private void CreateClientRpcParams()
        {
            // Creates the needed array from the editable list.
            ulong[] allOtherClientIds = clientsIdsList.ToArray();

            // Creates the RpcParams with the array of ClientIds
            clientsIdsRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = allOtherClientIds
                }
            };
        }

        /// <summary>
        /// Tell the server to toggle the FaceCam on/off for all clients.
        /// </summary>
        /// <remarks>
        /// This call will be sent to the server. The default [ServerRpc] attribute
        /// setting allows only a client owner (client that owns the NetworkObject associated
        /// with the NetworkBehaviour containing the ServerRpc method) invocation rights.
        /// Any client that isn't the owner won't be allowed to invoke the ServerRpc.
        /// By setting the ServerRpc attribute's RequireOwnership parameter to false,
        /// any client has ServerRpc invocation rights.
        /// </remarks>
        [ServerRpc(RequireOwnership = false)]
        private void FaceCamOnOffServerRpc(bool networkFaceCamOn, ServerRpcParams serverRpcParams = default)
        {
            // A ServerRpc is a remote procedure call (RPC) that can be only invoked
            // by a client and will always be received and executed on the server/host.
#if DEBUG
            Debug.Log($"[RPC] Server received FaceCamOnOffServerRpc from {serverRpcParams.Receive.SenderClientId} with networkFaceCamOn={networkFaceCamOn}\n");
#endif
            FaceCamOnOffClientRpc(networkFaceCamOn);
        }

        /// <summary>
        /// Tell the server to toggle the FaceCam position of all clients.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void FaceCamOnFrontToggleServerRpc(bool networkFaceCamOnFront, ServerRpcParams serverRpcParams = default)
        {
#if DEBUG
            Debug.Log($"[RPC] Server received FaceCamOnFrontToggleServerRpc from {serverRpcParams.Receive.SenderClientId} with networkFaceCamOn={networkFaceCamOnFront}\n");
#endif
            FaceCamOnFrontToggleClientRpc(networkFaceCamOnFront);
        }

        /// <summary>
        /// Get the FaceCam status from the server to all clients.
        /// </summary
        [ServerRpc(RequireOwnership = false)]
        private void GetFaceCamStatusServerRpc(ServerRpcParams serverRpcParams = default)
        {
#if DEBUG
            Debug.Log($"[RPC] Server received GetFaceCamStatusServerRpc from {serverRpcParams.Receive.SenderClientId}.\n");
            Debug.Log($"[RPC] Server sends SetFaceCamStatusClientRpc(faceCamOn: {faceCamOn}, faceCamOnFront: {faceCamOnFront}) to all clients.\n");
#endif
            SetFaceCamStatusClientRpc(faceCamOn, faceCamOnFront);
        }

        /// <summary>
        /// The owner calls this to send his video to the server which sends it to all clients.
        /// Also the server and every client will render this video onto the FaceCam.
        /// </summary>
        //[ServerRpc(Delivery = RpcDelivery.Unreliable)]
        // Large files not supported by unreliable Rpc. (No documentation found regarding this limitation).
        [ServerRpc]
        private void RenderFaceServerRPC(byte[] videoFrame, ServerRpcParams serverRpcParams = default)
        {
#if DEBUG
            Debug.Log($"[RPC] Server received GetVideoFromClientAndSendItToClientsToRenderItServerRPC from {serverRpcParams.Receive.SenderClientId}\n");
#endif

            // The server will render this video onto his instance of the FaceCam.
            // RenderFace(videoFrame);

            // The server will send the video to all other clients (not the owner and server)
            // so they can render it.
            RenderFaceClientRPC(videoFrame);
        }
    }
}