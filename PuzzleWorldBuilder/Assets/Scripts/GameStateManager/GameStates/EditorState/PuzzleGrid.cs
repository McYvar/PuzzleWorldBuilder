using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    TileInformation[,] tileInformation;
    TileInformation currentHighlightedTile;

    Material[] noneTileMaterials;

    public PuzzleGrid(int width, int length, Vector3 origin, params Material[] materials)
    {
        this.width = width;
        this.length = length;
        noneTileMaterials = materials;

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
        width += addWidth;
        length += addLength;
        TileInformation[,] temp = new TileInformation[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (x >= width - addWidth || z >= length - addLength)
                {
                    temp[x, z] = new TileInformation(TileType.NONE_TILE, new Vector3(x, 0, z), noneTileMaterials);
                    temp[x, z].AssignGridToObject(this);
                }
                else temp[x, z] = tileInformation[x, z];
            }
        }
        tileInformation = temp;
    }

    public void HighlightTile(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < length)
        {
            currentHighlightedTile?.Unhighlight();
            currentHighlightedTile = tileInformation[x, z];
            currentHighlightedTile.Highlight();
        }
    }
}

public class TileInformation
{
    TileType myType;
    GameObject myGameObject;

    Mesh myMesh;
    MeshRenderer myMeshRenderer;
    MeshCollider myCollider;

    Vector2 tileOffset = new Vector2(-0.5f, -0.5f);

    public TileInformation(TileType myType, Vector3 myPosition, params Material[] materials)
    {
        this.myType = myType;

        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0.05f + tileOffset.x, 0, 0.05f + tileOffset.y);
        vertices[1] = new Vector3(0.05f + tileOffset.x, 0, 0.95f + tileOffset.y);
        vertices[2] = new Vector3(0.95f + tileOffset.x, 0, 0.95f + tileOffset.y);
        vertices[3] = new Vector3(0.95f + tileOffset.x, 0, 0.05f + tileOffset.y);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        myMesh.vertices = vertices;
        myMesh.uv = uv;
        myMesh.triangles = triangles;

        myGameObject.GetComponent<MeshFilter>().mesh = myMesh;
        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();

        myMeshRenderer.materials = materials;

        myGameObject.GetComponent<MeshCollider>().sharedMesh = myMesh;
    }

    public void SetType(TileType newType)
    {
        myType = newType;
    }

    public void Highlight()
    {
        if (myType == TileType.NONE_TILE)
            myMeshRenderer.material.EnableKeyword("_EMISSION");
    }
    
    public void Unhighlight()
    {
        myMeshRenderer.material.DisableKeyword("_EMISSION");
    }

    public void AssignGridToObject(PuzzleGrid grid)
    {
        myGameObject.GetComponent<GridObject>().AssignGrid(grid);
    }
}

public enum TileType { NONE_TILE = 0, EDGE_TILE = 1, PLACEABLE_TILE = 2 }

