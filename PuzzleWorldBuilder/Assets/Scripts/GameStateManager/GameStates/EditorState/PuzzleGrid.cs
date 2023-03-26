using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

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
    Vector3 negativeOffset = Vector3Int.zero;
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
        x = (int)position.x - (int)negativeOffset.x;
        z = (int)position.z - (int)negativeOffset.z;
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

    public void OnSelectTile(Vector3 tileChords)
    {
        Vector3 offsetChords;
        FindXZ(tileChords, out offsetChords);
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].OnSelectTile();
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].SetTileType(TileType.PLACEABLE_TILE);
        UpdateNeighbours(tileChords, offsetChords);
    }

    public void OnDeselectTile(Vector3 tileChords)
    {
        Vector3 offsetChords;
        FindXZ(tileChords, out offsetChords);
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].OnDeselectTile();
    }

    public void OnDeleteTile(Vector3 tileChords)
    {
        Vector3 offsetChords;
        FindXZ(tileChords, out offsetChords);
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].Reset();
        UpdateNeighbours(tileChords, offsetChords);
        if (CheckPlaceableNeighbour(offsetChords))
        {
            tileInformation[(int)offsetChords.x, (int)offsetChords.z].SetTileType(TileType.EDGE_TILE);
            tileInformation[(int)offsetChords.x, (int)offsetChords.z].UpdateMesh();
        }

        // checks if the neighbours neighbours are placable tiles, if not, then they also get reset
        if (!CheckPlaceableNeighbour(offsetChords + new Vector3(0, 0, 1)) &&
            tileInformation[(int)(offsetChords + new Vector3(0, 0, 1)).x, (int)(offsetChords + new Vector3(0, 0, 1)).z].GetTileType() == TileType.EDGE_TILE)
            tileInformation[(int)(offsetChords + new Vector3(0, 0, 1)).x, (int)(offsetChords + new Vector3(0, 0, 1)).z].Reset();
        if (!CheckPlaceableNeighbour(offsetChords + new Vector3(1, 0, 0)) &&
            tileInformation[(int)(offsetChords + new Vector3(1, 0, 0)).x, (int)(offsetChords + new Vector3(1, 0, 0)).z].GetTileType() == TileType.EDGE_TILE)
            tileInformation[(int)(offsetChords + new Vector3(1, 0, 0)).x, (int)(offsetChords + new Vector3(1, 0, 0)).z].Reset();
        if (!CheckPlaceableNeighbour(offsetChords + new Vector3(0, 0, -1)) &&
            tileInformation[(int)(offsetChords + new Vector3(0, 0, -1)).x, (int)(offsetChords + new Vector3(0, 0, -1)).z].GetTileType() == TileType.EDGE_TILE)
            tileInformation[(int)(offsetChords + new Vector3(0, 0, -1)).x, (int)(offsetChords + new Vector3(0, 0, -1)).z].Reset();
        if (!CheckPlaceableNeighbour(offsetChords + new Vector3(-1, 0, 0)) &&
            tileInformation[(int)(offsetChords + new Vector3(-1, 0, 0)).x, (int)(offsetChords + new Vector3(-1, 0, 0)).z].GetTileType() == TileType.EDGE_TILE)
            tileInformation[(int)(offsetChords + new Vector3(-1, 0, 0)).x, (int)(offsetChords + new Vector3(-1, 0, 0)).z].Reset();
    }

    bool CheckPlaceableNeighbour(Vector3 tileChords)
    {
        if (tileInformation[(int)tileChords.x, (int)tileChords.z + 1].GetTileType() == TileType.PLACEABLE_TILE) return true;
        if (tileInformation[(int)tileChords.x + 1, (int)tileChords.z].GetTileType() == TileType.PLACEABLE_TILE) return true;
        if (tileInformation[(int)tileChords.x, (int)tileChords.z - 1].GetTileType() == TileType.PLACEABLE_TILE) return true;
        if (tileInformation[(int)tileChords.x - 1, (int)tileChords.z].GetTileType() == TileType.PLACEABLE_TILE) return true;
        return false;
    }

    public void MoveTile(Vector3 tileChords, Vector3 newPos)
    {
        Vector3 offsetChords;
        FindXZ(tileChords, out offsetChords);
        Vector3 relativePos = offsetChords + newPos;
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].SetHeight(relativePos.y);
        UpdateNeighbours(tileChords, offsetChords);
    }

    void UpdateNeighbours(Vector3 tileChords, Vector3 offsetChords)
    {
        /// <summary>
        /// The neighbours of this tile have to be updated. All TileInformation tiles keep track of the east of their
        /// neighbours height. When a height is changed, the neighbour of this tile it's height about our current tile has
        /// to be set to this height too. Then then if their height are up to date, we update the mesh around them.
        /// </summary>
        // to prevent chords out of bounds, make the grid bigger
        if (offsetChords.x - 2 < 0) IncreaseGrid(-2, 0);
        if (offsetChords.x + 2 >= width) IncreaseGrid(2, 0);
        if (offsetChords.z - 2 < 0) IncreaseGrid(0, -2);
        if (offsetChords.z + 2 >= length) IncreaseGrid(0, 2);
        FindXZ(tileChords, out offsetChords);

        TileInformation current = tileInformation[(int)offsetChords.x, (int)offsetChords.z];
        TileInformation north = tileInformation[(int)offsetChords.x, (int)offsetChords.z + 1]; // z+
        TileInformation east = tileInformation[(int)offsetChords.x + 1, (int)offsetChords.z];  // x+
        TileInformation south = tileInformation[(int)offsetChords.x, (int)offsetChords.z - 1]; // z-
        TileInformation west = tileInformation[(int)offsetChords.x - 1, (int)offsetChords.z];  // x-

        float northDifference = HeightDifference(current, north);
        float eastDifference = HeightDifference(current, east);
        float southDifference = HeightDifference(current, south);
        float westDifference = HeightDifference(current, west);

        // from each opposite direction we set the neighbours new height inverse
        north.SetSouth(-northDifference);
        east.SetWest(-eastDifference);
        south.SetNorth(-southDifference);
        west.SetEast(-westDifference);

        // if the neighbours type are not placeable, then set it to an edge tile. Edge tile will later become walls
        if (north.GetTileType() != TileType.PLACEABLE_TILE) north.SetTileType(TileType.EDGE_TILE);
        if (east.GetTileType() != TileType.PLACEABLE_TILE) east.SetTileType(TileType.EDGE_TILE);
        if (south.GetTileType() != TileType.PLACEABLE_TILE) south.SetTileType(TileType.EDGE_TILE);
        if (west.GetTileType() != TileType.PLACEABLE_TILE) west.SetTileType(TileType.EDGE_TILE);

        // and from our current tile the height have to be updated too, but not opposite direction
        current.SetNorth(northDifference);
        current.SetEast(eastDifference);
        current.SetSouth(southDifference);
        current.SetWest(westDifference);

        // and then we update everything
        north.UpdateMesh();
        east.UpdateMesh();
        south.UpdateMesh();
        west.UpdateMesh();
        current.UpdateMesh();
    }

    float HeightDifference(TileInformation current, TileInformation neighbour)
    {
        return neighbour.GetHeight() - current.GetHeight();
    }
}

public class TileInformation
{
    TileType myType;
    GameObject myGameObject;
    float myHeight;
    Material[] myMaterials;

    Mesh myMesh;
    MeshRenderer myMeshRenderer;
    MeshCollider myCollider;

    Vector3 tileOffset = new Vector3(-0.5f, 0, -0.5f);

    // neighbourtiles height difference
    float north;
    float east;
    float south;
    float west;

    bool isSelected;

    public TileInformation(TileType myType, Vector3 myPosition, params Material[] materials) // materials 0 == none, 1 == edge, 2 == placeable
    {
        north = 0;
        east = 0;
        south = 0;
        west = 0;
        this.myType = myType;
        myHeight = 0;
        myMaterials = materials;

        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
        myCollider = myGameObject.GetComponent<MeshCollider>();

        myMesh = new Mesh();
        myCollider.sharedMesh = myMesh;
        myGameObject.GetComponent<MeshFilter>().mesh = myMesh;

        CubeWithoutBottomSmaller(materials[0], 0, 0, 0, 0);
    }


    #region meshes
    void FlatTileWithEdgeMesh(Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0.05f, 0f, 0.05f) + tileOffset;
        vertices[1] = new Vector3(0.05f, 0f, 0.95f) + tileOffset;
        vertices[2] = new Vector3(0.95f, 0f, 0.95f) + tileOffset;
        vertices[3] = new Vector3(0.95f, 0f, 0.05f) + tileOffset;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        AssignMesh(vertices, uv, triangles, material);
    }
    void FlatFullTileMeshOnZero(Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0f, 0f, 0f) + Vector3.down * myHeight + tileOffset;
        vertices[1] = new Vector3(0f, 0f, 1f) + Vector3.down * myHeight + tileOffset;
        vertices[2] = new Vector3(1f, 0f, 1f) + Vector3.down * myHeight + tileOffset;
        vertices[3] = new Vector3(1f, 0f, 0f) + Vector3.down * myHeight + tileOffset;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        AssignMesh(vertices, uv, triangles, material);
    }

    void FlatFullTileMesh(Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0f, 0f, 0f) + tileOffset;
        vertices[1] = new Vector3(0f, 0f, 1f) + tileOffset;
        vertices[2] = new Vector3(1f, 0f, 1f) + tileOffset;
        vertices[3] = new Vector3(1f, 0f, 0f) + tileOffset;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        AssignMesh(vertices, uv, triangles, material);
    }

    void CubeWithoutBottomSmaller(Material material, float north, float south, float east, float west)
    {
        Vector3[] vertices = new Vector3[20];
        Vector2[] uv = new Vector2[20];
        int[] triangles = new int[30];

        // NORTH
        vertices[0] = new Vector3(0.95f, 0f, 0.95f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[1] = new Vector3(0.05f, 0f, 0.95f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        vertices[2] = new Vector3(0.95f, 1f * north, 0.95f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[3] = new Vector3(0.05f, 1f * north, 0.95f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 3;

        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 1;


        // SOUTH
        vertices[4] = new Vector3(0.95f, 1f * south, 0.05f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(0.05f, 1f * south, 0.05f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(0.95f, 0f, 0.05f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(0.05f, 0f, 0.05f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 10;
        triangles[7] = 6;
        triangles[8] = 7;

        triangles[9] = 10;
        triangles[10] = 7;
        triangles[11] = 11;


        // TOP
        vertices[8] = new Vector3(0.95f, 1f * myHeight, 0.95f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0.05f, 1f * myHeight, 0.95f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(0.95f, 1f * myHeight, 0.05f) + Vector3.down * myHeight + tileOffset; //[1, 1, 0]
        vertices[11] = new Vector3(0.05f, 1f * myHeight, 0.05f) + Vector3.down * myHeight + tileOffset; //[0, 1, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(1, 0);
        uv[10] = new Vector2(0, 0);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 4;
        triangles[14] = 5;

        triangles[15] = 8;
        triangles[16] = 5;
        triangles[17] = 9;


        // WEST
        vertices[12] = new Vector3(0.05f, 0f, 0.95f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[13] = new Vector3(0.05f, 1f * west, 0.95f) + Vector3.down * west + tileOffset; //[0, 1, 1]
        vertices[14] = new Vector3(0.05f, 1f * west, 0.05f) + Vector3.down * west + tileOffset; //[0, 1, 0]
        vertices[15] = new Vector3(0.05f, 0f, 0.05f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(0, 1);
        uv[14] = new Vector2(1, 1);
        uv[15] = new Vector2(1, 0);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 12;
        triangles[22] = 14;
        triangles[23] = 15;


        // EAST
        vertices[16] = new Vector3(0.95f, 0f, 0.05f) + Vector3.down * east + tileOffset;          //[1, 0, 0]
        vertices[17] = new Vector3(0.95f, 1f * east, 0.05f) + Vector3.down * east + tileOffset; //[1, 1, 0]
        vertices[18] = new Vector3(0.95f, 1f * east, 0.95f) + Vector3.down * east + tileOffset; //[1, 1, 1]
        vertices[19] = new Vector3(0.95f, 0f, 0.95f) + Vector3.down * east + tileOffset;          //[1, 0, 1]
        uv[16] = new Vector2(0, 0);
        uv[17] = new Vector2(0, 1);
        uv[18] = new Vector2(1, 1);
        uv[19] = new Vector2(1, 0);

        triangles[24] = 16;
        triangles[25] = 17;
        triangles[26] = 18;

        triangles[27] = 16;
        triangles[28] = 18;
        triangles[29] = 19;

        AssignMesh(vertices, uv, triangles, material);
    }

    void CubeWithoutBottom(Material material, float north, float south, float east, float west)
    {
        Vector3[] vertices = new Vector3[20];
        Vector2[] uv = new Vector2[20];
        int[] triangles = new int[30];

        // NORTH
        vertices[0] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[1] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        vertices[2] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[3] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 3;

        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 1;


        // SOUTH
        vertices[4] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 10;
        triangles[7] = 6;
        triangles[8] = 7;

        triangles[9] = 10;
        triangles[10] = 7;
        triangles[11] = 11;


        // TOP
        vertices[8] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset; //[1, 1, 0]
        vertices[11] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset; //[0, 1, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(1, 0);
        uv[10] = new Vector2(0, 0);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 4;
        triangles[14] = 5;

        triangles[15] = 8;
        triangles[16] = 5;
        triangles[17] = 9;


        // WEST
        vertices[12] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[13] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset; //[0, 1, 1]
        vertices[14] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset; //[0, 1, 0]
        vertices[15] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(0, 1);
        uv[14] = new Vector2(1, 1);
        uv[15] = new Vector2(1, 0);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 12;
        triangles[22] = 14;
        triangles[23] = 15;


        // EAST
        vertices[16] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;          //[1, 0, 0]
        vertices[17] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset; //[1, 1, 0]
        vertices[18] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset; //[1, 1, 1]
        vertices[19] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;          //[1, 0, 1]
        uv[16] = new Vector2(0, 0);
        uv[17] = new Vector2(0, 1);
        uv[18] = new Vector2(1, 1);
        uv[19] = new Vector2(1, 0);

        triangles[24] = 16;
        triangles[25] = 17;
        triangles[26] = 18;

        triangles[27] = 16;
        triangles[28] = 18;
        triangles[29] = 19;

        AssignMesh(vertices, uv, triangles, material);
    }

    // single walls
    void SouthMesh(Material material, float south)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        AssignMesh(vertices, uv, triangles, material);
    }
    void NorthMesh(Material material, float north)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // NORTH
        vertices[4] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[5] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        AssignMesh(vertices, uv, triangles, material);
    }
    void WestMesh(Material material, float west)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // WEST
        vertices[4] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[5] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;           //[0, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;           //[0, 0, 0]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(0, 1);
        uv[6] = new Vector2(1, 1);
        uv[7] = new Vector2(1, 0);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        AssignMesh(vertices, uv, triangles, material);
    }
    void EastMesh(Material material, float east)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // EAST
        vertices[4] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;  //[1, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;           //[1, 0, 1]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        AssignMesh(vertices, uv, triangles, material);
    }

    // double parallel walls
    void ParallelNorthSouth(Material material, float north, float south)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // NORTH
        vertices[8] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;          //[1, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;          //[0, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }
    void ParallelEastWest(Material material, float east, float west)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // WEST
        vertices[4] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[5] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(0, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // EAST
        vertices[8] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;   //[1, 1, 0]
        vertices[9] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;   //[1, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;            //[1, 0, 0]
        vertices[11] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;            //[1, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(1, 0);
        uv[10] = new Vector2(0, 1);
        uv[11] = new Vector2(1, 1);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }

    // corner walls
    void CornerSouthWestMesh(Material material, float south, float west)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // WEST
        vertices[8] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[9] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[10] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }
    void CornerSouthEastMesh(Material material, float south, float east)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // EAST
        vertices[8] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;   //[1, 1, 0]
        vertices[9] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;   //[1, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;           //[1, 0, 0]
        vertices[11] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;           //[1, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }
    void CornerNorthWestMesh(Material material, float north, float west)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // NORTH
        vertices[4] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[5] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // WEST
        vertices[8] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[9] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[10] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }
    void CornerNorthEastMesh(Material material, float north, float east)
    {
        Vector3[] vertices = new Vector3[12];
        Vector2[] uv = new Vector2[12];
        int[] triangles = new int[18];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // NORTH
        vertices[4] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[5] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // EAST
        vertices[8] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;   //[1, 1, 0]
        vertices[9] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;   //[1, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;           //[1, 0, 0]
        vertices[11] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;           //[1, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 1);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        AssignMesh(vertices, uv, triangles, material);
    }

    // tripple walls
    void TrippleSouthEastWestMesh(Material material, float south, float east, float west)
    {
        Vector3[] vertices = new Vector3[16];
        Vector2[] uv = new Vector2[16];
        int[] triangles = new int[24];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // WEST
        vertices[8] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[9] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[10] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        // EAST
        vertices[12] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;   //[1, 1, 0]
        vertices[13] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;   //[1, 1, 1]
        vertices[14] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;            //[1, 0, 0]
        vertices[15] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;            //[1, 0, 1]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(0, 1);
        uv[14] = new Vector2(1, 1);
        uv[15] = new Vector2(1, 0);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 13;
        triangles[22] = 15;
        triangles[23] = 14;

        AssignMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthSouthWestMesh(Material material, float north, float south, float west)
    {
        Vector3[] vertices = new Vector3[16];
        Vector2[] uv = new Vector2[16];
        int[] triangles = new int[24];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // NORTH
        vertices[8] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;          //[1, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;          //[0, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        // WEST
        vertices[12] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset; //[0, 1, 1]
        vertices[13] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset; //[0, 1, 0]
        vertices[14] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[15] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(0, 1);
        uv[14] = new Vector2(1, 1);
        uv[15] = new Vector2(1, 0);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 13;
        triangles[22] = 15;
        triangles[23] = 14;

        AssignMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthSouthEastMesh(Material material, float north, float south, float east)
    {
        Vector3[] vertices = new Vector3[16];
        Vector2[] uv = new Vector2[16];
        int[] triangles = new int[24];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        // SOUTH
        vertices[4] = new Vector3(0f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * south, 0f) + Vector3.down * south + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * south + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * south + tileOffset;           //[1, 0, 0]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // NORTH
        vertices[8] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;          //[1, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;          //[0, 0, 1]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(1, 1);
        uv[11] = new Vector2(1, 0);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        // EAST
        vertices[12] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;  //[1, 1, 0]
        vertices[13] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;  //[1, 1, 1]
        vertices[14] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;           //[1, 0, 0]
        vertices[15] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;           //[1, 0, 1]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(0, 1);
        uv[14] = new Vector2(1, 1);
        uv[15] = new Vector2(1, 0);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 13;
        triangles[22] = 15;
        triangles[23] = 14;

        AssignMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthEastWestMesh(Material material, float north, float east, float west)
    {
        Vector3[] vertices = new Vector3[16];
        Vector2[] uv = new Vector2[16];
        int[] triangles = new int[24];

        // TOP
        vertices[0] = new Vector3(0f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset;  //[1, 1, 0]
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;


        // NORTH
        vertices[4] = new Vector3(1f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[1, 1, 1]
        vertices[5] = new Vector3(0f, 1f * north, 1f) + Vector3.down * north + tileOffset;  //[0, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 1f) + Vector3.down * north + tileOffset;           //[1, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 1f) + Vector3.down * north + tileOffset;           //[0, 0, 1]
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;

        triangles[9] = 5;
        triangles[10] = 7;
        triangles[11] = 6;

        // WEST
        vertices[8] = new Vector3(0f, 1f * west, 1f) + Vector3.down * west + tileOffset;  //[0, 1, 1]
        vertices[9] = new Vector3(0f, 1f * west, 0f) + Vector3.down * west + tileOffset;  //[0, 1, 0]
        vertices[10] = new Vector3(0f, 0f, 1f) + Vector3.down * west + tileOffset;          //[0, 0, 1]
        vertices[11] = new Vector3(0f, 0f, 0f) + Vector3.down * west + tileOffset;          //[0, 0, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(0, 1);
        uv[10] = new Vector2(0, 1);
        uv[11] = new Vector2(1, 1);

        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;

        triangles[15] = 9;
        triangles[16] = 11;
        triangles[17] = 10;

        // EAST
        vertices[12] = new Vector3(1f, 1f * east, 0f) + Vector3.down * east + tileOffset;   //[1, 1, 0]
        vertices[13] = new Vector3(1f, 1f * east, 1f) + Vector3.down * east + tileOffset;   //[1, 1, 1]
        vertices[14] = new Vector3(1f, 0f, 0f) + Vector3.down * east + tileOffset;            //[1, 0, 0]
        vertices[15] = new Vector3(1f, 0f, 1f) + Vector3.down * east + tileOffset;            //[1, 0, 1]
        uv[12] = new Vector2(0, 0);
        uv[13] = new Vector2(1, 0);
        uv[14] = new Vector2(0, 1);
        uv[15] = new Vector2(1, 1);

        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;

        triangles[21] = 13;
        triangles[22] = 15;
        triangles[23] = 14;

        AssignMesh(vertices, uv, triangles, material);
    }
    #endregion

    void AssignMesh(Vector3[] vertices, Vector2[] uv, int[] triangles, Material material)
    {
        myMesh.vertices = vertices;
        myMesh.uv = uv;
        myMesh.triangles = triangles;

        myMesh.RecalculateNormals();

        myMeshRenderer.material = material;
        myCollider.sharedMesh = myMesh;
    }

    public void SetTileType(TileType newType)
    {
        myType = newType;

        switch (myType)
        {
            case TileType.NONE_TILE:
                myCollider.convex = true;
                myCollider.isTrigger = true;
                break;

            case TileType.EDGE_TILE:
                myCollider.convex = true;
                myCollider.isTrigger = true;
                break;

            case TileType.PLACEABLE_TILE:
                myCollider.isTrigger = false;
                myCollider.convex = false;
                break;
        }
    }

    public TileType GetTileType()
    {
        return myType;
    }

    public void SetHeight(float newHeight)
    {
        myHeight = newHeight;
        myGameObject.transform.position = new Vector3(myGameObject.transform.position.x, myHeight + tileOffset.y, myGameObject.transform.position.z);
    }

    public void UpdateMesh()
    {
        // update the tile
        switch (myType)
        {
            case TileType.NONE_TILE:
                CubeWithoutBottomSmaller(myMaterials[0], 0, 0, 0, 0);
                break;

            case TileType.EDGE_TILE:
                SetMesh2(myMaterials[1], north, east, south, west);
                break;

            case TileType.PLACEABLE_TILE:
                if (isSelected) SetMesh2(myMaterials[3], north, east, south, west);
                else SetMesh2(myMaterials[2], north, east, south, west);
                break;
        }
    }

    void SetMesh2(Material material, float north, float east, float south, float west)
    {
        float newNorth = north;
        if (newNorth > 0) newNorth = 0;
        float newEast = east;
        if (newEast > 0) newEast = 0;
        float newSouth = south;
        if (newSouth > 0) newSouth = 0;
        float newWest = west;
        if (newWest > 0) newWest = 0;

        CubeWithoutBottom(material, -newNorth, -newSouth, -newEast, -newWest);
    }

    public void OnSelectTile()
    {
        isSelected = true;
    }

    public void OnDeselectTile()
    {
        isSelected = false;
        switch (myType)
        {
            case TileType.NONE_TILE:
                myMeshRenderer.material = myMaterials[0];
                break;

            case TileType.EDGE_TILE:
                myMeshRenderer.material = myMaterials[1];
                break;

            case TileType.PLACEABLE_TILE:
                myMeshRenderer.material = myMaterials[2];
                break;
        }
    }
    
    // This seemed to never work out, even tough everything is checked 10x, when editing alot of meshes at the same time things just broke...
    void SetMesh(Material material)
    {
        Debug.Log(north + " : " + east + " : " + south + " : " + west);
        if (myHeight == north && myHeight == east && myHeight == south && myHeight == west)
        {
            FlatFullTileMeshOnZero(material);
        }
        else if (myHeight > north && myHeight == east && myHeight == south && myHeight == west)
        {
            NorthMesh(material, - north);
        }
        else if (myHeight == north && myHeight > east && myHeight == south && myHeight == west)
        {
            EastMesh(material, -east);
        }
        else if (myHeight == north && myHeight == east && myHeight > south && myHeight == west)
        {
            SouthMesh(material, -south);
        }
        else if (myHeight == north && myHeight == east && myHeight == south && myHeight > west)
        {
            WestMesh(material, -west);
        }
        else if (myHeight > north && myHeight > east && myHeight == south && myHeight == west)
        {
            CornerNorthEastMesh(material, -north, -east);
        }
        else if (myHeight > north && myHeight == east && myHeight > south && myHeight == west)
        {
            ParallelNorthSouth(material, -north, -south);
        }
        else if (myHeight > north && myHeight == east && myHeight == south && myHeight > west)
        {
            CornerNorthWestMesh(material, -north, -west);
        }
        else if (myHeight == north && myHeight > east && myHeight > south && myHeight == west)
        {
            CornerSouthEastMesh(material, -south, -east);
        }
        else if (myHeight == north && myHeight == east && myHeight > south && myHeight > west)
        {
            CornerSouthWestMesh(material, -south, -west);
        }
        else if (myHeight == north && myHeight > east && myHeight == south && myHeight > west)
        {
            ParallelEastWest(material, -east, -west);
        }
        else if (myHeight > north && myHeight > east && myHeight > south && myHeight == west)
        {
            TrippleNorthSouthEastMesh(material, -north, -south, -east);
        }
        else if (myHeight > north && myHeight == east && myHeight > south && myHeight > west)
        {
            TrippleNorthSouthWestMesh(material, -north, -south, -west);
        }
        else if (myHeight > north && myHeight > east && myHeight == south && myHeight > west)
        {
            TrippleNorthEastWestMesh(material, -north, -east, west);
        }
        else if (myHeight == north && myHeight > east && myHeight > south && myHeight > west)
        {
            TrippleSouthEastWestMesh(material, -south, -east, -west);
        }
        else if (myHeight > north && myHeight > east && myHeight > south && myHeight > west)
        {
            CubeWithoutBottom(material, -north, -south, -east, -west);
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

    public float GetHeight() { return myHeight; }

    public void SetNorth(float newNorth) { north = newNorth; }
    public void SetEast(float newEast) { east = newEast; }
    public void SetSouth(float newSouth) { south = newSouth; }
    public void SetWest(float newWest) { west = newWest; }

    public void Reset()
    {
        SetTileType(TileType.NONE_TILE);
        SetHeight(0);
        UpdateMesh();
    }
}

public enum TileType { NONE_TILE = 0, EDGE_TILE = 1, PLACEABLE_TILE = 2 }

