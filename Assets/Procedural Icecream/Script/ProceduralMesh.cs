using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMesh : MonoBehaviour
{

    [Header("Element")]
    [SerializeField] private MeshFilter meshFilter;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh(); 

        List<Vector3> vertices = new List<Vector3>();

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = Vector3.right;
        Vector3 p2 = Vector3.up;
        Vector3 p3 = Vector3.up + Vector3.right;

        vertices.Add(p0);
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);

        List<int> triangles = new List<int>();

        triangles.Add(0);
        triangles.Add(3);
        triangles.Add(1);

        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        meshFilter.sharedMesh = mesh;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
