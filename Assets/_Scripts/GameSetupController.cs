﻿using Photon.Pun;
using SEE.DataModel;
using SEE.Layout;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SEE
{

    public class GameSetupController : MonoBehaviour
    {
        void Start()
        {
            InitializePlayer();

            // TODO cities must be able to be generated outside of editor
#if false
            GraphSettings gs = new GraphSettings();
            gs.pathPrefix = Application.dataPath.Replace('/', '\\') + '\\';
            Graph g = SceneGraphs.Add(gs);
            if (g != null) // TODO fix .Add function
            {
                List<string> nm = new List<string>() {
                    gs.WidthMetric,
                    gs.HeightMetric,
                    gs.DepthMetric
                };
                ILayout l = new SEE.Layout.BalloonLayout(
                    gs.ShowEdges,
                    gs.WidthMetric, gs.HeightMetric, gs.DepthMetric,
                    gs.IssueMap(),
                    gs.InnerNodeMetrics,
                    new BuildingFactory(),
                    new ZScoreScale(g, gs.MinimalBlockLength, gs.MaximalBlockLength, nm),
                    gs.EdgeWidth,
                    gs.ShowErosions,
                    gs.EdgesAboveBlocks,
                    gs.ShowDonuts);
                l.Draw(g);
            }
#endif
            MenuBackdropGenerator mbg = GameObject.FindObjectOfType<MenuBackdropGenerator>();
            mbg.Initialize();

            SearchMenu sm = GameObject.FindObjectOfType<SearchMenu>();
            sm.Initialize();
            IngameMenu im = GameObject.FindObjectOfType<IngameMenu>();
            im.Initialize();

            GameStateController gsc = GameObject.FindObjectOfType<GameStateController>();
            gsc.Initialize();
        }

        private void InitializePlayer()
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player"), Vector3.zero, Quaternion.identity);
            GameObject playerHead = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "PlayerHead"), Vector3.zero, Quaternion.identity);
            GameObject playerHeadPrefab = PlayerData.GetPlayerHeadPrefab();

            playerHead.transform.parent = player.transform;
            PhotonView.Get(playerHead).RPC("InitializeMaterial", RpcTarget.All);
            PhotonView.Get(playerHead).RPC("SetTextureScaleX", RpcTarget.All, playerHeadPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTextureScale.x);
            PhotonView.Get(playerHead).RPC("SetTextureScaleY", RpcTarget.All, playerHeadPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTextureScale.y);
            PhotonView.Get(playerHead).RPC("SetTextureOffsetX", RpcTarget.All, playerHeadPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTextureOffset.x);
            PhotonView.Get(playerHead).RPC("SetTextureOffsetY", RpcTarget.All, playerHeadPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTextureOffset.y);

            //TODO NetworkController.OnPlayerConnected(PhotonView photonView)
        }
    }

}
