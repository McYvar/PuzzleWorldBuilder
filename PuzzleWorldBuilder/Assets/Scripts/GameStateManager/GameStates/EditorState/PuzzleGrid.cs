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
        FindXZ(tileChords, out tileChords);
        if (tileChords.x < negativeOffset.x || tileChords.x >= width - negativeOffset.x || tileChords.z < negativeOffset.z || tileChords.z >= length - negativeOffset.z)
        {
            return;
        }

        Vector3 relativePos = tileChords + newPos;
        if ((int)relativePos.x < negativeOffset.x || (int)relativePos.x >= width - negativeOffset.x || (int)relativePos.z < negativeOffset.z || (int)relativePos.z >= length - negativeOffset.z)
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
    Material[] myMaterials;

    Mesh myMesh;
    MeshRenderer myMeshRenderer;
    MeshCollider myCollider;

    Vector3 tileOffset = new Vector3(-0.5f, 0, -0.5f);

    public TileInformation(TileType myType, Vector3 myPosition, params Material[] materials) // materials 0 == none, 1 == edge, 2 == placeable
    {
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
        myType = copyInformation.myType;
        myHeight = copyInformation.myHeight;
        myMaterials = copyInformation.myMaterials;
        myGameObject = new GameObject("Tile(" + myPosition.x + ", " + myPosition.z + ")", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(GridObject));
        myGameObject.transform.position = myPosition;

        myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
        myCollider = myGameObject.GetComponent<MeshCollider>();

        UpdateMesh();
    }

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

    void FlatFullTileMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;
        vertices[1] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;
        vertices[2] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;
        vertices[3] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;

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

    void FullCubeOutsideMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[24];
        Vector2[] uv = new Vector2[24];
        int[] triangles = new int[36];

        // BACK
        vertices[0] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;           //[1, 0, 1]
        vertices[1] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;           //[0, 0, 1]
        vertices[2] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[3] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
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


        // FRONT
        vertices[4] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;           //[0, 0, 0]
        uv[4] = new Vector2(0, 1);
        uv[5] = new Vector2(1, 1);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        triangles[12] = 10;
        triangles[13] = 6;
        triangles[14] = 7;

        triangles[15] = 10;
        triangles[16] = 7;
        triangles[17] = 11;


        // TOP
        vertices[8] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[1, 1, 0]
        vertices[11] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[0, 1, 0]
        uv[8] = new Vector2(0, 0);
        uv[9] = new Vector2(1, 0);
        uv[10] = new Vector2(0, 0);
        uv[11] = new Vector2(1, 0);

        triangles[6] = 8;
        triangles[7] = 4;
        triangles[8] = 5;

        triangles[9] = 8;
        triangles[10] = 5;
        triangles[11] = 9;


        // BOTTOM
        vertices[12] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;          //[1, 0, 0]
        vertices[13] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;          //[1, 0, 1]
        vertices[14] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;          //[0, 0, 1]
        vertices[15] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;          //[0, 0, 0]
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


        // LEFT
        vertices[16] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;          //[0, 0, 1]
        vertices[17] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset; //[0, 1, 1]
        vertices[18] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[0, 1, 0]
        vertices[19] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;          //[0, 0, 0]
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


        // RIGHT
        vertices[20] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;          //[1, 0, 0]
        vertices[21] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[1, 1, 0]
        vertices[22] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset; //[1, 1, 1]
        vertices[23] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;          //[1, 0, 1]
        uv[20] = new Vector2(0, 0);
        uv[21] = new Vector2(0, 1);
        uv[22] = new Vector2(1, 1);
        uv[23] = new Vector2(1, 0);

        triangles[30] = 20;
        triangles[31] = 21;
        triangles[32] = 22;

        triangles[33] = 20;
        triangles[34] = 22;
        triangles[35] = 23;

        SetNewMesh(vertices, uv, triangles, material);
    }

    void CubeWithoutBottom(Material material, int height)
    {
        Vector3[] vertices = new Vector3[20];
        Vector2[] uv = new Vector2[20];
        int[] triangles = new int[30];

        // BACK
        vertices[0] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;           //[1, 0, 1]
        vertices[1] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;           //[0, 0, 1]
        vertices[2] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[3] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
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


        // FRONT
        vertices[4] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;           //[0, 0, 0]
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
        vertices[8] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[9] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[10] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[1, 1, 0]
        vertices[11] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[0, 1, 0]
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


        // LEFT
        vertices[12] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;          //[0, 0, 1]
        vertices[13] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset; //[0, 1, 1]
        vertices[14] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[0, 1, 0]
        vertices[15] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;          //[0, 0, 0]
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


        // RIGHT
        vertices[16] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;          //[1, 0, 0]
        vertices[17] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset; //[1, 1, 0]
        vertices[18] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset; //[1, 1, 1]
        vertices[19] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;          //[1, 0, 1]
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
    void FrontMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
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


        // FRONT
        vertices[4] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[5] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;           //[0, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;           //[1, 0, 0]
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
    void BackMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
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


        // BACK
        vertices[4] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[5] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;           //[1, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;           //[0, 0, 1]
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
    void LeftMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
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


        // LEFT
        vertices[4] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[5] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[6] = new Vector3(0f, 0f, 1f) + Vector3.down * height + tileOffset;           //[0, 0, 1]
        vertices[7] = new Vector3(0f, 0f, 0f) + Vector3.down * height + tileOffset;           //[0, 0, 0]
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
    void RightMesh(Material material, int height)
    {
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        // TOP
        vertices[0] = new Vector3(0f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[0, 1, 1]
        vertices[1] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[2] = new Vector3(0f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[0, 1, 0]
        vertices[3] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
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


        // RIGHT
        vertices[4] = new Vector3(1f, 1f * height, 0f) + Vector3.down * height + tileOffset;  //[1, 1, 0]
        vertices[5] = new Vector3(1f, 1f * height, 1f) + Vector3.down * height + tileOffset;  //[1, 1, 1]
        vertices[6] = new Vector3(1f, 0f, 0f) + Vector3.down * height + tileOffset;           //[1, 0, 0]
        vertices[7] = new Vector3(1f, 0f, 1f) + Vector3.down * height + tileOffset;           //[1, 0, 1]
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
        myGameObject.transform.position = new Vector3(myGameObject.transform.position.x, myHeight + tileOffset.y, myGameObject.transform.position.z);

        UpdateMesh();
    }

    void UpdateMesh()
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
                // set placeable verts and mats, three options: myHeight == 0, myHeight > 0, myHeight < 0 --> soon on basis of neighbors
                if (myHeight == 0)
                {
                    FlatFullTileMesh(myMaterials[2], myHeight);
                }
                else if (myHeight > 0)
                {
                    CubeWithoutBottom(myMaterials[2], myHeight);
                }
                else if (myHeight < 0)
                {
                    CubeWithoutBottom(myMaterials[2], myHeight);
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

