﻿using Photon.Pun;
using UnityEngine;

namespace SEE
{

    public class Head : MonoBehaviour
    {
        private static readonly float DEFAULT_TEXTURE_SCALE_X = 3.0f;
        private static readonly float DEFAULT_TEXTURE_SCALE_Y = 1.1f;
        private static readonly float DEFAULT_TEXTURE_OFFSET_X = -1.0f;
        private static readonly float DEFAULT_TEXTURE_OFFSET_Y = -0.19f;

        void Awake()
        {
            InitializeMaterial();
        }

        [PunRPC]
        public void InitializeMaterial()
        {
            Material prefab = (Material)Resources.Load("Materials/FaceMaterial/FaceMaterial", typeof(Material));
            Material material = new Material(prefab);
            GetComponentInChildren<MeshRenderer>().material = material;
            material.mainTextureScale = new Vector2(DEFAULT_TEXTURE_SCALE_X, DEFAULT_TEXTURE_SCALE_Y);
            material.mainTextureOffset = new Vector2(DEFAULT_TEXTURE_OFFSET_X, DEFAULT_TEXTURE_OFFSET_Y);
        }

        [PunRPC]
        public void SetTextureScaleX(float xScale)
        {
            Material material = GetComponentInChildren<MeshRenderer>().material;
            material.mainTextureScale = new Vector2(xScale, material.mainTextureScale.y);
        }

        [PunRPC]
        public void SetTextureScaleY(float yScale)
        {
            Material material = GetComponentInChildren<MeshRenderer>().material;
            GetComponentInChildren<MeshRenderer>().material.mainTextureScale = new Vector2(material.mainTextureScale.x, yScale);
        }

        [PunRPC]
        public void SetTextureOffsetX(float xOffset)
        {
            Material material = GetComponentInChildren<MeshRenderer>().material;
            GetComponentInChildren<MeshRenderer>().material.mainTextureOffset = new Vector2(xOffset, material.mainTextureOffset.y);
        }

        [PunRPC]
        public void SetTextureOffsetY(float yOffset)
        {
            Material material = GetComponentInChildren<MeshRenderer>().material;
            GetComponentInChildren<MeshRenderer>().material.mainTextureOffset = new Vector2(material.mainTextureOffset.x, yOffset);
        }
    }

}
