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

    public void MoveTile(Vector3 tileChords, Vector3 newPos)
    {
        Vector3 offsetChords;
        FindXZ(tileChords, out offsetChords);

        // if the chord gets outside of the grid, the grid grows
        Vector3 relativePos = offsetChords + newPos;
        if (relativePos.x < 0) IncreaseGrid(-1, 0);
        if (relativePos.x >= width - 1) IncreaseGrid(1, 0);
        if (relativePos.z < 0) IncreaseGrid(0, -1);
        if (relativePos.z >= length - 1) IncreaseGrid(0, 1);

        tileInformation[(int)offsetChords.x, (int)offsetChords.z].SetTileType(TileType.PLACEABLE_TILE);
        tileInformation[(int)offsetChords.x, (int)offsetChords.z].AddHeight(relativePos.y);

        UpdateNeighbours(tileChords, offsetChords);

        if (newPos.x != 0 || newPos.z != 0)
        {
            //tileInformation[(int)relativePos.x, (int)relativePos.z] = new TileInformation(tileInformation[(int)tileChords.x, (int)tileChords.z], relativePos);

            //tileInformation[(int)tileChords.x, (int)tileChords.z].SetType(TileType.NONE_TILE);
        }
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

        // from each opposite direction we set the neighbours new height
        north.SetSouth(-northDifference);
        east.SetWest(-eastDifference);
        south.SetNorth(-southDifference);
        west.SetEast(-westDifference);

        // if the neighbours type are of NONE, then set it to an edge tile. Edge tile will later become walls
        if (north.GetTileType() == TileType.NONE_TILE)
        {
            north.SetTileType(TileType.EDGE_TILE);
            current.SetNorth(0);
        }
        // and from our current tile the height have to be updated too, but not opposite direction
        else current.SetNorth(northDifference);
        if (east.GetTileType() == TileType.NONE_TILE)
        {
            east.SetTileType(TileType.EDGE_TILE);
            current.SetEast(0);
        }
        else current.SetEast(eastDifference);
        if (south.GetTileType() == TileType.NONE_TILE)
        {
            south.SetTileType(TileType.EDGE_TILE);
            current.SetSouth(0);
        }
        else current.SetSouth(southDifference);
        if (west.GetTileType() == TileType.NONE_TILE)
        {
            west.SetTileType(TileType.EDGE_TILE);
            current.SetWest(0);
        }
        else current.SetWest(westDifference);

        // and then we update everything
        north.UpdateMesh();
        east.UpdateMesh();
        south.UpdateMesh();
        west.UpdateMesh();
        current.UpdateMesh();
    }

    float HeightDifference(TileInformation current, TileInformation neighbour)
    {
        return current.GetHeight() - neighbour.GetHeight(); // if n > c --> x > 0, if n < c x < 0
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

        FlatTileWithEdgeMesh(materials[0]);
    }

    public TileInformation(TileInformation copyInformation, Vector3 myPosition)
    {
        north = 0;
        east = 0;
        south = 0;
        west = 0;
        myType = copyInformation.myType;
        myHeight = copyInformation.myHeight;
        myMaterials = copyInformation.myMaterials;
        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
        myCollider = myGameObject.GetComponent<MeshCollider>();

        UpdateMesh();
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void FlatFullTileMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }

    void CubeWithoutBottom(Material material)
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
        vertices[17] = new Vector3(1f, 1f * myHeight, 0f) + Vector3.down * myHeight + tileOffset; //[1, 1, 0]
        vertices[18] = new Vector3(1f, 1f * myHeight, 1f) + Vector3.down * myHeight + tileOffset; //[1, 1, 1]
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

        SetNewMesh(vertices, uv, triangles, material);
    }

    // single walls
    void SouthMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void NorthMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void WestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void EastMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }

    // double parallel walls
    void ParallelNorthSouth(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void ParallelEastWest(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }

    // corner walls
    void CornerSouthWestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void CornerSouthEastMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void CornerNorthWestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void CornerNorthEastMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }

    // tripple walls
    void TrippleSouthEastWestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthSouthWestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthSouthEastMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    void TrippleNorthEastWestMesh(Material material)
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

        SetNewMesh(vertices, uv, triangles, material);
    }
    #endregion

    void SetNewMesh(Vector3[] vertices, Vector2[] uv, int[] triangles, Material material)
    {
        myMesh = new Mesh();

        myMesh.vertices = vertices;
        myMesh.uv = uv;
        myMesh.triangles = triangles;
        myMesh.RecalculateNormals();

        myCollider.sharedMesh = myMesh;
        myGameObject.GetComponent<MeshFilter>().mesh = myMesh;

        myMeshRenderer.material = material;
    }

    public void SetTileType(TileType newType)
    {
        myType = newType;

        switch (myType)
        {
            case TileType.NONE_TILE:
                myCollider.enabled = false;
                break;

            case TileType.EDGE_TILE:
                myCollider.enabled = true;
                myCollider.convex = true;
                myCollider.isTrigger = true;
                break;

            case TileType.PLACEABLE_TILE:
                myCollider.enabled = true;
                myCollider.isTrigger = false;
                myCollider.convex = false;
                break;
        }
    }

    public TileType GetTileType()
    {
        return myType;
    }

    public void AddHeight(float newHeight)
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
                // set none verts and mats
                FlatTileWithEdgeMesh(myMaterials[0]);
                break;

            case TileType.EDGE_TILE:
                // set edge verts and mats
                FlatTileWithEdgeMesh(myMaterials[1]);
                // later walls on each side
                break;

            case TileType.PLACEABLE_TILE:
                // all neighbours on same height
                #region a so far failed experiment
                /*
                if (myHeight == north && myHeight == east && myHeight == south && myHeight == west)
                {
                    FlatFullTileMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight == east && myHeight == south && myHeight == west)
                {
                    NorthMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight > east && myHeight == south && myHeight == west)
                {
                    EastMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight == east && myHeight > south && myHeight == west)
                {
                    SouthMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight == east && myHeight == south && myHeight > west)
                {
                    WestMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight > east && myHeight == south && myHeight == west)
                {
                    CornerNorthEastMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight == east && myHeight > south && myHeight == west)
                {
                    ParallelNorthSouth(myMaterials[2]);
                }
                else if (myHeight > north && myHeight == east && myHeight == south && myHeight > west)
                {
                    CornerNorthWestMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight > east && myHeight > south && myHeight == west)
                {
                    CornerSouthEastMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight == east && myHeight > south && myHeight > west)
                {
                    CornerSouthWestMesh(myMaterials[2]);
                }
                else if (myHeight == north && myHeight > east && myHeight == south && myHeight > west)
                {
                    ParallelEastWest(myMaterials[2]);
                }
                else if (myHeight > north && myHeight > east && myHeight > south && myHeight == west)
                {
                    TrippleNorthSouthEastMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight == east && myHeight > south && myHeight > west)
                {
                    TrippleNorthSouthWestMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight > east && myHeight == south && myHeight > west)
                {
                    TrippleNorthEastWestMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight > east && myHeight == south && myHeight > west)
                {
                    TrippleNorthEastWestMesh(myMaterials[2]);
                }
                else if (myHeight > north && myHeight > east && myHeight > south && myHeight > west)
                {
                    CubeWithoutBottom(myMaterials[2]);
                }
                */
                #endregion
                CubeWithoutBottom(myMaterials[2]);
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

    public float GetHeight() { return myHeight; }

    public void SetNorth(float newNorth) { north = newNorth; }
    public void SetEast(float newEast) { east = newEast; }
    public void SetSouth(float newSouth) { south = newSouth; }
    public void SetWest(float newWest) { west = newWest; }
}

public enum TileType { NONE_TILE = 0, EDGE_TILE = 1, PLACEABLE_TILE = 2 }

