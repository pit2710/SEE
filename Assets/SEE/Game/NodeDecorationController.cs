﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decorates each block with an assigned texture
/// </summary>
public class NodeDecorationController : MonoBehaviour
{
    /// <summary>
    /// The gameNode to be decorated
    /// </summary>
    public GameObject nodeObject;

    /// <summary>
    /// The gameNode's bounds' size
    /// </summary>
    private Vector3 nodeSize;

    /// <summary>
    /// The gameNode's location
    /// </summary>
    private Vector3 nodeLocation;

    /// <summary>
    /// The Height-Percentage the bottom floor should have in
    /// contrast to the building height
    /// </summary>
    public float floorHightPercentage {
        get
        {
            return _floorHightPercentage;
        }
        set
        {
            _floorHightPercentage = Mathf.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    /// How far out the lobby should be from the building, percentage
    /// is in contrast to the building width
    /// </summary>
    public float lobbySpanPercentage
    {
        get
        {
            return _lobbySpanPercentage;
        }
        set
        {
            _lobbySpanPercentage = Mathf.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    /// The Height-Percentage the roof should have in contrast
    /// to the building height
    /// </summary>
    public float roofHeightPercentage
    {
        get
        {
            return _roofHeightPercentage;
        }
        set
        {
            _roofHeightPercentage = Mathf.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    /// How far out/in the roof should be in contast to the building, percentage
    /// is in contrast to building width
    /// </summary>
    public float roofSpanPercentage
    {
        get
        {
            return _roofSpanPercentage;
        }
        set
        {
            _roofSpanPercentage = Mathf.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    /// Contain the values of the above declared variables, limited to values between 0 and 1
    /// </summary>
    [SerializeField, Range(0f, 1f)]  private float _floorHightPercentage, _lobbySpanPercentage, _roofHeightPercentage, _roofSpanPercentage;

    /// <summary>
    /// Tile-texture used to decorate the block around it's sides
    /// </summary>
    public Texture2D blockTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Bottom floor texture
    /// </summary>
    public Texture2D bottomFloorTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Roof texture
    /// </summary>
    public Texture2D roofTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Get the gameNode's different properties
    /// </summary>
    private void fetchNodeDetails() {
        nodeSize = nodeObject.transform.localScale;
        nodeLocation = nodeObject.transform.position;
    }

    /// <summary>
    /// Renders the bottom floor of a building
    /// </summary>
    private void renderLobby()
    {
        // ========== TODO scale these when scaling gameobject ==========
        float lobbySizeX = nodeSize.x + nodeSize.x * _lobbySpanPercentage;
        float lobbySizeZ = nodeSize.z + nodeSize.z * _lobbySpanPercentage;
        float lobbyHeight = nodeSize.y * _floorHightPercentage;
        // Create lobby gameObject
        GameObject lobby = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lobby.transform.SetParent(nodeObject.transform);
        lobby.transform.localScale = new Vector3(lobbySizeX, lobbyHeight, lobbySizeZ);
        // Get the point on the Y axis at the bottom of the building
        float buildingGroundFloorHeight = nodeLocation.y - (nodeSize.y / 2);
        // Set the lobby to be at buildingGroundFloorHeight + half the height of the lobby (so its floor touches the building floor)
        float lobbyGroundFloorHeight = buildingGroundFloorHeight + (lobby.transform.localScale.y / 2);
        // Move the lobby object to te correct location
        lobby.transform.position = new Vector3(nodeLocation.x, lobbyGroundFloorHeight, nodeLocation.z);
        // TODO Render textures here


    }

    /// <summary>
    /// Renders the tetrahedron roof of a building
    /// *** Percentages are supplied as values between 0 and 1 ***
    /// </summary>
    private void renderRoof()
    {
        // ========== TODO scale these when scaling gameobject ==========
        float roofSizeX = nodeSize.x + nodeSize.x * _roofSpanPercentage;
        float roofSizeZ = nodeSize.z + nodeSize.z * _roofSpanPercentage;
        float roofHeight = nodeSize.y * _roofHeightPercentage;
        // Create roof GameObject
        GameObject tetrahedron = createFourFacedTetrahedron(roofSizeX, roofHeight, roofSizeZ);
        tetrahedron.transform.SetParent(nodeObject.transform);
        // Move tetrahedron to top of building, tetrahedron is moved with the bottom left corner
        tetrahedron.transform.position = new Vector3(nodeLocation.x - roofSizeX/2, nodeSize.y, nodeLocation.z - roofSizeZ/2);
        // TODO Render textures here

    }

    /// <summary>
    /// <author name="Leonard Haddad"/>
    /// Generates a 4-faced tetrahedron at the given coordinates
    /// Inspired by an article by <a href="https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/">Morten Nobel-Jørgensen</a>,
    /// </summary>
    public GameObject createFourFacedTetrahedron(float sizeX, float height, float sizeZ)
    {
        // TODO one of the tetra's bottom vertices renders opposite side up
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Tetrahedron";
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        // Tetrahedron floor nodes
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(sizeX, 0, 0);
        Vector3 p2 = new Vector3(sizeX, 0, sizeZ);
        Vector3 p3 = new Vector3(0, 0, sizeZ);
        // Tetrahedron top node
        Vector3 p4 = new Vector3(sizeX / 2, height, sizeZ / 2);
        // Create gameObject mesh
        Mesh mesh = new Mesh();
        mesh.Clear();
        // TODO THE VERTEX ISSUE IS CAUSED BY ONE OF THESE
        mesh.vertices = new Vector3[] {
            p0,p1,p3, // Bottom vertex #1
            p3,p2,p1, // Bottom vertex #2
            p1,p4,p2,
            p2,p4,p3,
            p3,p4,p0,
            p0,p4,p1
        };
        mesh.triangles = new int[]
        {
            0,1,2,
            3,4,5,
            6,7,8,
            9,10,11,
            12,13,14,
            15,16,17
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        meshFilter.mesh = mesh;
        return go;
    }

    /// <summary>
    /// Computes how many tiles fit the given side of the gameobject block
    /// <param name="side">Side of the block, which's tile-amount to calculate</param>
    /// </summary>
    // TODO side 0-3, clockwise
    private int GetTilesPerSide(int side)
    {
        return 0;
    }

    /// <summary>
    /// Decorates the block
    /// </summary>
    private void decorateBlock()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        fetchNodeDetails();
        renderLobby();
        renderRoof();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
