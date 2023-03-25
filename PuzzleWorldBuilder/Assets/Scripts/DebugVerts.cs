using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVerts : MonoBehaviour
{
    [SerializeField] MeshFilter mesh;
    Vector3 offset = Vector3.one / 2;

    private void Start()
    {
        for (int i = 0; i < mesh.mesh.vertexCount; i++)
        {
            Debug.Log(i + ": " + (mesh.mesh.vertices[i] + offset));
        }

        for (int i = 0; i < mesh.mesh.triangles.Length; i++)
        {
            Debug.Log(i + ": " + mesh.mesh.triangles[i]);
        }

        for (int i = 0; i < mesh.mesh.vertexCount; i++)
        {
            Debug.Log(i + ": " + mesh.mesh.uv[i]);
        }
    }
}
