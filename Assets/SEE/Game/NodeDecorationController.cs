﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decorates each block with an assigned texture
/// </summary>
public class NodeDecorationController : MonoBehaviour
{
    /// <summary>
    /// Whether or not the block contains other nodes 
    /// </summary>
    public bool foldedBlock = false;

    /// <summary>
    /// Enables debug and runs tests
    /// </summary>
    public bool debug = false;

    /// <summary>
    /// Number of rows for test block
    /// </summary>
    public int testBlockRowCount;

    /// <summary>
    /// Number of columns for test block
    /// </summary>
    public int testBlockColumnCount;

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
    /// Roof type dropdown menu items
    /// </summary>
    public enum RoofType {
        Rectangular,
        Tetrahedron,
        Dome
    }

    /// <summary>
    /// Roof dropdown menu selector (inspector)
    /// </summary>
    public RoofType selectedRoofType;

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
    [SerializeField, Range(0f, 1f)]  
    private float _floorHightPercentage, _lobbySpanPercentage, _roofHeightPercentage, _roofSpanPercentage;

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
        float lobbySizeX = nodeSize.x + nodeSize.x * _lobbySpanPercentage;
        float lobbySizeZ = nodeSize.z + nodeSize.z * _lobbySpanPercentage;
        float lobbyHeight = nodeSize.y * _floorHightPercentage;
        // Create lobby gameObject
        GameObject lobby = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lobby.name = "Lobby";
        lobby.transform.localScale = new Vector3(lobbySizeX, lobbyHeight, lobbySizeZ);
        // *** Note: setting the lobby as a child object needs to be done after the transform, otherwise the size
        //     is relative to the parent object ***
        lobby.transform.SetParent(nodeObject.transform);
        // Get the point on the Y axis at the bottom of the building
        float buildingGroundFloorHeight = nodeLocation.y - (nodeSize.y / 2);
        // Set the lobby to be at buildingGroundFloorHeight + half the height of the lobby (so its floor touches the building floor)
        float lobbyGroundFloorHeight = buildingGroundFloorHeight + (lobby.transform.localScale.y / 2);
        // Move the lobby object to te correct location
        lobby.transform.position = new Vector3(nodeLocation.x, lobbyGroundFloorHeight, nodeLocation.z);
    }

    /// <summary>
    /// Renders the tetrahedron roof of a building
    /// *** Percentages are supplied as values between 0 and 1 ***
    /// </summary>
    private void renderRoof()
    {
        float roofSizeX = nodeSize.x + nodeSize.x * _roofSpanPercentage;
        float roofSizeZ = nodeSize.z + nodeSize.z * _roofSpanPercentage;
        float roofHeight = nodeSize.y * _roofHeightPercentage;
        // Create roof GameObject
        switch (selectedRoofType)
        {
            case RoofType.Tetrahedron:
                GameObject tetrahedron = createPyramid(roofSizeX, roofHeight, roofSizeZ);
                tetrahedron.transform.SetParent(nodeObject.transform);
                // Move tetrahedron to top of building, tetrahedron is moved with the bottom left corner
                tetrahedron.transform.position = new Vector3(nodeLocation.x - roofSizeX / 2, nodeSize.y / 2 + nodeLocation.y, nodeLocation.z - roofSizeZ / 2);
                break;
            case RoofType.Rectangular:
                renderRectangularRoof(roofSizeX,roofHeight,roofSizeZ);
                break;
            case RoofType.Dome:
                renderDomeRoof(roofSizeX, roofHeight, roofSizeZ);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Renders a rectangular roof for the node object
    /// <param name="roofSizeX">The roof's size on the X-axis</param>
    /// <param name="roofHeight">The roof's height on the Y-axis</param>
    /// <param name="roofSizeZ">The roof's size on the Z-axis</param>
    /// </summary>
    private void renderRectangularRoof(float roofSizeX, float roofHeight, float roofSizeZ)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "RectangularRoof";
        go.transform.localScale = new Vector3(roofSizeX, roofHeight, roofSizeZ);
        float nodeRoofHeight = nodeObject.transform.position.y + nodeSize.y / 2;
        go.transform.position = new Vector3(nodeLocation.x, nodeRoofHeight, nodeLocation.z);
        go.transform.SetParent(nodeObject.transform);
    }

    /// <summary>
    /// Renders a dome roof for the node object
    /// <param name="roofSizeX">The roof's size on the X-axis</param>
    /// <param name="roofHeight">The roof's height on the Y-axis</param>
    /// <param name="roofSizeZ">The roof's size on the Z-axis</param>
    /// </summary>
    private void renderDomeRoof(float roofSizeX, float roofHeight, float roofSizeZ)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SphericalRoof";
        go.transform.localScale = new Vector3(roofSizeX, roofHeight, roofSizeZ);
        float nodeRoofHeight = nodeObject.transform.position.y + nodeSize.y / 2;
        go.transform.position = new Vector3(nodeLocation.x, nodeRoofHeight, nodeLocation.z);
        go.transform.SetParent(nodeObject.transform);
    }

    /// <summary>
    /// <author name="Leonard Haddad"/>
    /// Generates a 4-faced pyramid at the given coordinates
    /// Inspired by an article by <a href="https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/">Morten Nobel-Jørgensen</a>,
    /// </summary>
    public GameObject createPyramid(float sizeX, float height, float sizeZ)
    {
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
        mesh.vertices = new Vector3[] {
            p0,p1,p3, // Bottom vertex #1
            p2,p3,p1, // Bottom vertex #2
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
    /// Decorates the block
    /// </summary>
    private void decoratePackedBlock(List<GameObject> hiddenObjects, GameObject parent)
    {
        for (int i = 0; i < hiddenObjects.Count; i++)
        {
            GameObject clone = new GameObject(); // TODO this gameobject has no rendered mesh. Use GameObject.CreatePrimitive instead
            clone.transform.position.Set(hiddenObjects[i].transform.position.x, parent.transform.localScale.y, hiddenObjects[i].transform.position.z); // TODO the height is wong as it doesn't take the gameobject's y position into account
            clone.transform.localScale.Set(hiddenObjects[i].transform.localScale.x, 0.00000001f, hiddenObjects[i].transform.localScale.z);
        }
        decoratePackedBlockWall(hiddenObjects,parent);
    }

    /// <summary>
    /// Decorates the walls of the packed block
    /// <param name="hiddenObjects">The list of gamenodes that are hidden inside the packed block</param>
    /// <param name="packedBlock">The packed block</param>
    /// </summary>
    private void decoratePackedBlockWall(List<GameObject> hiddenObjects, GameObject packedBlock)
    {
        // Get packed block dimensions and corners, North - Positive X, West - Positive Z
        Vector3 packedBlockDimensions = packedBlock.transform.localScale;
        Vector3 packedBlockLocation = packedBlock.transform.position;
        float NECornerX = packedBlockLocation.x + packedBlockDimensions.x / 2;
        float NECornerZ = packedBlockLocation.z - packedBlockDimensions.z / 2;
        float SECornerX = packedBlockLocation.x - packedBlockDimensions.x / 2;
        float SECornerZ = packedBlockLocation.z - packedBlockDimensions.z / 2;
        float NWCornerX = packedBlockLocation.x + packedBlockDimensions.x / 2;
        float NWCornerZ = packedBlockLocation.z + packedBlockDimensions.z / 2;
        float SWCornerX = packedBlockLocation.x - packedBlockDimensions.x / 2;
        float SWCornerZ = packedBlockLocation.z + packedBlockDimensions.z / 2;
        float packedBlockFloorY = packedBlockLocation.y - packedBlockDimensions.y / 2;

        // Compute sum of block heights
        float totalBlocksHeight = 0f;
        foreach (GameObject o in hiddenObjects)
        {
            totalBlocksHeight += packedBlockDimensions.y;
        }
        
        // How much empty space to show between the block decorations, tweak the 1% at will
        float freeSpaceX = 0.01f * packedBlock.transform.localScale.x;
        float freeSpaceY = 0.01f * packedBlock.transform.localScale.y;
        float freeSpaceZ = 0.01f * packedBlock.transform.localScale.z;
        
        // Compute block grid
        float roundedRoot = Mathf.Ceil(Mathf.Sqrt(hiddenObjects.Count));
        float blocksHorizontalAxis = roundedRoot;
        float blocksVerticalAxis = roundedRoot;
        // Check for an empty row (too many rows)
        // Math: Assuming 4x4 grid, if the last row is empty and we have 11 gameObjects, we calculate
        // 16 - 11 >=? 4 -> if yes then the last row is empty and can be removed. Note that 9 blocks will create a 3x3 grid
        if (blocksHorizontalAxis * blocksVerticalAxis - hiddenObjects.Count >= blocksVerticalAxis)
        {
            blocksVerticalAxis -= 1;
        }

        // Add empty gameobject children to identify different clones
        GameObject northClones = new GameObject();
        GameObject southClones = new GameObject();
        GameObject westClones = new GameObject();
        GameObject eastClones = new GameObject();
        northClones.transform.name = "northClones";
        southClones.transform.name = "southClones";
        westClones.transform.name = "westClones";
        eastClones.transform.name = "eastClones";
        northClones.transform.SetParent(packedBlock.transform);
        southClones.transform.SetParent(packedBlock.transform);
        westClones.transform.SetParent(packedBlock.transform);
        eastClones.transform.SetParent(packedBlock.transform);

        // Location on each surface, used to align clones on block surface
        float northPosZ = NECornerZ + freeSpaceZ;
        float northPosY = packedBlockFloorY + freeSpaceY;
        float southPosZ = SECornerZ + freeSpaceZ;
        float southPosY = packedBlockFloorY + freeSpaceY;
        float eastPosX = SECornerX + freeSpaceX;
        float eastPosY = packedBlockFloorY + freeSpaceY;
        float westPosX = SWCornerX + freeSpaceX;
        float westPosY = packedBlockFloorY + freeSpaceY;

        // Create gameobject clones and set them on the walls of the packed block
        List <GameObject> clones = new List<GameObject>();
        float maxCloneWidthZ = (packedBlockDimensions.z - freeSpaceZ * (blocksHorizontalAxis + 1)) / blocksHorizontalAxis; 
        float maxCloneWidthX = (packedBlockDimensions.x - freeSpaceX * (blocksHorizontalAxis + 1)) / blocksHorizontalAxis;
        float maxCloneHeightY = (packedBlockDimensions.y - freeSpaceY * (blocksVerticalAxis + 1)) / blocksVerticalAxis;
        foreach (GameObject o in hiddenObjects)
        {
            // Block height percentage - in contrast to total height, used for scaling the clone
            float blockHeightPercentage = o.transform.localScale.y / totalBlocksHeight;

            // Create positive north clone
            GameObject cloneN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloneN.name = o.transform.name + "NorthDecorator";
            

            // Create North/South clones (Positive x - north, negative x - south)

            GameObject cloneS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            cloneS.name = "PackedBlockSouthWallDecoration";
            // Compute dimensions of clones
            float blockOccupancyHorizontal = blockHeightPercentage * packedBlockDimensions.z;
            float blockOccupancyVertical = blockHeightPercentage * packedBlockDimensions.y;
            Vector3 size = new Vector3(0.00000001f,blockOccupancyVertical,blockOccupancyHorizontal); // TODO remove magic number
            cloneN.transform.localScale = size;
            cloneS.transform.localScale = size;

            // Create West/East clones (Positive z - west, negative z - east)
            GameObject cloneW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject cloneE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloneW.name = "PackedBlockWesthWallDecoration";
            cloneE.name = "PackedBlockEastWallDecoration";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        fetchNodeDetails();
        if (!foldedBlock)
        {
            renderLobby();
            renderRoof();
        }
        else
        {
            if (debug)
            {
                testImplementation(testBlockColumnCount, testBlockRowCount);
            }
            else {
                // TODO add decoration call here
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (nodeObject.transform.localScale != nodeSize)
        {
            // TODO remove this after testing
            Debug.Log("Node size changed, reloading data...");
            for (int i=0; i<nodeObject.transform.childCount; ++i)
            {
                Destroy(nodeObject.transform.GetChild(i).gameObject);
            }
            fetchNodeDetails();
            if (!foldedBlock)
            {
                renderLobby();
                renderRoof();
            }
            else
            {
                if (debug)
                {
                    testImplementation(testBlockColumnCount, testBlockRowCount);
                }
                else
                {
                    // TODO add decoration call
                }
            }
        }
    }

    /// <summary>
    /// Test the block decoration implementation both visually and mathematically
    /// </summary>
    private void testImplementation(int columns, int rows)
    {
        List<GameObject> childNodes = new List<GameObject>();
        // Free space inbetween child nodes
        float freeSpaceX = 0.01f * nodeSize.x;
        float freeSpaceZ = 0.01f * nodeSize.z;
        // Create a few child nodes
        for (int i=0; i<12; i++)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Gamenodes will be laid out as 4x3 graph while testing
            Vector3 childNodeDimensions = new Vector3((nodeSize.x - 5f * freeSpaceX) / rows, Random.Range(0.01f * nodeSize.y, nodeSize.y), (nodeSize.z - 5f * freeSpaceZ) / columns);
            o.transform.localScale = childNodeDimensions;
            o.name = i.ToString();
            o.GetComponent<Renderer>().material.color = Color.red;
            o.transform.SetParent(nodeObject.transform);
            childNodes.Add(o);
        }
        // Find corners of parent node, parent node is moved using it's 3d center
        float parentNodeLowX = nodeLocation.x - nodeSize.x / 2;
        float parentNodeHighX = nodeLocation.x + nodeSize.x / 2;
        float parentNodeLowZ = nodeLocation.z - nodeSize.z / 2;
        float parentNodeHighZ = nodeLocation.z + nodeSize.z / 2;
        float parentNodeFloorY = nodeLocation.y - nodeSize.y / 2;
        // Lay nodes out as 4x3 grid inside parent node
        int currentListIndex = 0;
        float currentLocationX = parentNodeLowX + freeSpaceX;
        float currentLocationZ = parentNodeLowZ + freeSpaceZ;
        float childWidthX = nodeSize.x / 4 - 5f * freeSpaceX;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject currentChild = childNodes[currentListIndex];
                // Location for current child
                float childLocX = currentLocationX + currentChild.transform.localScale.x / 2;
                float childLocZ = currentLocationZ + currentChild.transform.localScale.z / 2;
                float childLocY = parentNodeFloorY + currentChild.transform.localScale.y / 2;
                // Move child to new location
                currentChild.transform.position = new Vector3(childLocX, childLocY, childLocZ);
                currentListIndex += 1;
                currentLocationZ += freeSpaceZ + currentChild.transform.localScale.z; 
            }
            currentLocationZ = parentNodeLowZ + freeSpaceZ;
            currentLocationX = freeSpaceX + childWidthX;
        }
    }
}
