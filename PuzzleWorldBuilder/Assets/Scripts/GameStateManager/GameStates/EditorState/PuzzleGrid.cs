using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PuzzleGrid
{
    /// <summary>
    /// Date: 03/22/23, By: Yvar
    /// Okay, this is what I have in mind: A 2d grid that you can extend using arrows. The most outer layer will always have arrows pointing outwards.
    /// The outer layer consists of "NONE tiles". They do not have a visible tile on them, no objects should be placed on them.
    /// When you press such arrow, that tile will extend, creating a visible tile on the grid where objects can be placed down.
    /// Around the newly created tile, a detection system will determine if there should grow a new NONE tile.
    /// This class also tracks the total grid size, which should be able to grow.
    /// </summary>
    
    int width;
    int length;
    Vector3Int negativeOffset = Vector3Int.zero;
    TileInformation[,] tileInformation;
    TileInformation currentHighlightedTile;

    Material[] noneTileMaterials;

    public PuzzleGrid(int width, int length, Vector3 origin, params Material[] materials)
    {
        this.width = width;
        this.length = length;
        noneTileMaterials = materials;
        negativeOffset.x = 0;
        negativeOffset.z = 0;

        tileInformation = new TileInformation[width, length];

        for (int x = 0; x < tileInformation.GetLength(0); x++)
        {
            for (int z = 0; z < tileInformation.GetLength(1); z++)
            {
                tileInformation[x, z] = new TileInformation(TileType.NONE_TILE, new Vector3(x, 0, z), materials);
                tileInformation[x, z].AssignGridToObject(this);
            }
        }
    }

    public void IncreaseGrid(int addWidth, int addLength)
    {
        width += Mathf.Abs(addWidth);
        length += Mathf.Abs(addLength);
        TileInformation[,] temp = new TileInformation[width, length];

        if (addWidth < 0) negativeOffset.x += addWidth;
        if (addLength < 0) negativeOffset.z += addLength;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if ((addWidth < 0 && x < 0 - addWidth) || (addWidth > 0 && x >= width - addWidth) ||
                    (addLength < 0 && z < 0 - addLength) || (addLength > 0 && z >= length - addLength))
                {
                    temp[x, z] = new TileInformation(TileType.NONE_TILE, new Vector3(x, 0, z) + negativeOffset, noneTileMaterials);
                    temp[x, z].AssignGridToObject(this);
                }
                else
                {
                    int offsetX = x;
                    int offsetZ = z;
                    if (addWidth < 0) offsetX += addWidth;
                    if (addLength < 0) offsetZ += addLength;
                    temp[x, z] = tileInformation[offsetX, offsetZ];
                }
            }
        }
        tileInformation = temp;
    }

    void FindXZ(Vector3 position, out int x, out int z)
    {
        x = (int)position.x - negativeOffset.x;
        z = (int)position.z - negativeOffset.z;
    }

    void FindXZ(Vector3 current, out Vector3 offset)
    {
        offset = current - negativeOffset;
    }

    public void HighlightTile(int x, int z)
    {
        FindXZ(new Vector3(x, 0, z), out x, out z);
        if (x >= 0 && z >= 0 && x < width && z < length)
        {
            currentHighlightedTile?.Unhighlight();
            currentHighlightedTile = tileInformation[x, z];
            currentHighlightedTile.Highlight();
        }
    }

    public void MoveTile(Vector3 tileChords, Vector3 newPos)
    {
        FindXZ(tileChords, out tileChords);
        if (tileChords.x < negativeOffset.x || tileChords.x >= width - negativeOffset.x || tileChords.z < negativeOffset.z || tileChords.z >= length - negativeOffset.z)
        {
            return;
        }

        Vector3 relativePos = tileChords + newPos;
        if (relativePos.x < negativeOffset.x || relativePos.x >= width - negativeOffset.x || relativePos.z < negativeOffset.z || relativePos.z >= length - negativeOffset.z)
        {
            // here we add to the grid instead soon
            return;
        }
        tileInformation[(int)tileChords.x, (int)tileChords.z].SetType(TileType.PLACEABLE_TILE);
        tileInformation[(int)tileChords.x, (int)tileChords.z].AddHeight((int)relativePos.y);
        
        if (newPos.x != 0 || newPos.z != 0)
        {
            //tileInformation[(int)relativePos.x, (int)relativePos.z] = new TileInformation(tileInformation[(int)tileChords.x, (int)tileChords.z], relativePos);

            //tileInformation[(int)tileChords.x, (int)tileChords.z].SetType(TileType.NONE_TILE);
        }
    }
}

public class TileInformation
{
    TileType myType;
    GameObject myGameObject;
    int myHeight;
    int startHeight;
    Material[] myMaterials;

    Mesh myMesh;
    MeshRenderer myMeshRenderer;
    MeshCollider myCollider;

    Vector3 tileOffset = new Vector3(-0.5f, -0.5f, -0.5f);

    public TileInformation(TileType myType, Vector3 myPosition, params Material[] materials) // materials 0 == none, 1 == edge, 2 == placeable
    {
        this.myType = myType;
        myHeight = 0;
        myMaterials = materials;

        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
        myCollider = myGameObject.GetComponent<MeshCollider>();
        
        FlatTileMesh(materials[0]);
    }

    public TileInformation(TileInformation copyInformation, Vector3 myPosition)
    {
        myType = copyInformation.myType;
        myHeight = copyInformation.myHeight;
        myMaterials = copyInformation.myMaterials; 
        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
        myCollider = myGameObject.GetComponent<MeshCollider>();

        UpdateMesh();
    }

    void FlatTileMesh(Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0.05f, 0, 0.05f) + tileOffset;
        vertices[1] = new Vector3(0.05f, 0, 0.95f) + tileOffset;
        vertices[2] = new Vector3(0.95f, 0, 0.95f) + tileOffset;
        vertices[3] = new Vector3(0.95f, 0, 0.05f) + tileOffset;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        SetNewMesh(vertices, uv, triangles, material);
    }

    void CubicOutsideMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[30];

        vertices[0] = new Vector3(0f + tileOffset.x, 0f, 0f + tileOffset.z) + Vector3.down * height;
        vertices[1] = new Vector3(0f + tileOffset.x, 0f, 1f + tileOffset.z) + Vector3.down * height;
        vertices[2] = new Vector3(1f + tileOffset.x, 0f, 1f + tileOffset.z) + Vector3.down * height;
        vertices[3] = new Vector3(1f + tileOffset.x, 0f, 0f + tileOffset.z) + Vector3.down * height;
        vertices[4] = new Vector3(0f + tileOffset.x, 1f * height, 0f + tileOffset.z) + Vector3.down * height;
        vertices[5] = new Vector3(0f + tileOffset.x, 1f * height, 1f + tileOffset.z) + Vector3.down * height;
        vertices[6] = new Vector3(1f + tileOffset.x, 1f * height, 1f + tileOffset.z) + Vector3.down * height;
        vertices[7] = new Vector3(1f + tileOffset.x, 1f * height, 0f + tileOffset.z) + Vector3.down * height;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(0, 1);
        uv[6] = new Vector2(1, 1);
        uv[7] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 4;
        triangles[2] = 7;

        triangles[3] = 0;
        triangles[4] = 1;
        triangles[5] = 4;

        triangles[6] = 0;
        triangles[7] = 7;
        triangles[8] = 3;

        triangles[9] = 1;
        triangles[10] = 5;
        triangles[11] = 4;

        triangles[12] = 1;
        triangles[13] = 2;
        triangles[14] = 5;

        triangles[15] = 2;
        triangles[16] = 6;
        triangles[17] = 5;

        triangles[18] = 2;
        triangles[19] = 3;
        triangles[20] = 7;

        triangles[21] = 2;
        triangles[22] = 7;
        triangles[23] = 6;

        triangles[24] = 4;
        triangles[25] = 5;
        triangles[26] = 6;

        triangles[27] = 4;
        triangles[28] = 6;
        triangles[29] = 7;

        SetNewMesh(vertices, uv, triangles, material);
    }

    void SetNewMesh(Vector3[] vertices, Vector2[] uv, int[] triangles, Material material)
    {
        myMesh = new Mesh();

        myMesh.vertices = vertices;
        myMesh.uv = uv;
        myMesh.triangles = triangles;

        myCollider.sharedMesh = myMesh;
        myGameObject.GetComponent<MeshFilter>().mesh = myMesh;

        myMeshRenderer.material = material;
    }

    public void SetType(TileType newType)
    {
        myType = newType;

        switch (myType)
        {
            case TileType.NONE_TILE:
                myCollider.enabled = false;
                break;

            case TileType.EDGE_TILE:
                myCollider.enabled = true;
                break;

            case TileType.PLACEABLE_TILE:
                myCollider.enabled = true;
                break;
        }
    }

    public void AddHeight(int newHeight)
    {
        myHeight = newHeight;
        myGameObject.transform.position = new Vector3(myGameObject.transform.position.x, newHeight, myGameObject.transform.position.z);

        UpdateMesh();
    }

    void UpdateMesh()
    {
        // update the tile
        switch (myType)
        {
            case TileType.NONE_TILE:
                // set none verts and mats
                FlatTileMesh(myMaterials[0]);
                break;

            case TileType.EDGE_TILE:
                // set edge verts and mats
                FlatTileMesh(myMaterials[1]);
                // later walls on each side
                break;

            case TileType.PLACEABLE_TILE:
                // set placeable verts and mats, three options: myHeight == 0, myHeight > 0, myHeight < 0
                if (myHeight == 0)
                {
                    FlatTileMesh(myMaterials[2]);
                }
                else if (myHeight > 0)
                {
                    CubicOutsideMesh(myMaterials[2], myHeight);
                }
                else if (myHeight < 0)
                {

                }
                break;
        }
    }

    public void Highlight()
    {
        myMeshRenderer.material.EnableKeyword("_EMISSION");
    }

    public void Unhighlight()
    {
        myMeshRenderer.material.DisableKeyword("_EMISSION");
    }

    public void AssignGridToObject(PuzzleGrid grid)
    {
        myGameObject.GetComponent<GridObject>().Initialize(grid);
    }
}

public enum TileType { NONE_TILE = 0, EDGE_TILE = 1, PLACEABLE_TILE = 2 }

