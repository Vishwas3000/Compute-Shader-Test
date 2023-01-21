using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCircle : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] MeshFilter meshFilter;

    [Header("Settings")]
    [Range(3, 20)]
    [SerializeField] int circleResolution;
    [SerializeField] float radius;

    [Header("IceCream Settings")]
    [SerializeField] int iceCreamResolution;
    [SerializeField] float iceCreamRadius;
    [SerializeField] float iceCreamHeight;
    [Range(1, 20)]
    [SerializeField] int loops;

    [Header("Data")]
    Mesh mesh ;

    List<Vector3> vertices = new List<Vector3>();   
    List<int> triangles  = new List<int>();

    List<Circle> circles = new List<Circle>();
    
    [Header("Animation Control")]
    [SerializeField] float speed = 1f;
    [SerializeField] bool rotate = false;

    // Start is called before the first frame update
    void Start()
    {
        GenerateCircles();
    }
    private void Update()
    {
        if(rotate)
            UpdateCircles();
    }
    void UpdateCircles()
    {
        UpdateVertices();
        mesh.vertices = vertices.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh= mesh;
    }
    void GenerateCircles()
    {
        mesh = new Mesh();

        vertices.Clear();
        triangles.Clear();  
        circles.Clear();

        CreateCircles();

        LinkCircles();

        AddData();
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh= mesh;
    }  
    void CreateCircles()
    {
        float angle = loops * 360f / iceCreamResolution;
        float yStep = iceCreamHeight / iceCreamResolution;

        for(int i=0; i<iceCreamResolution; i++)
        {
            Vector3 position = Vector3.zero;

            float currentCirclePercent = 1 - ((float)i / iceCreamResolution);
            float realIceCreamRadius = currentCirclePercent * iceCreamRadius;

            position.x = realIceCreamRadius * Mathf.Cos(angle*Mathf.Deg2Rad*i);
            position.y = i * yStep;
            position.z = realIceCreamRadius * Mathf.Sin(angle*Mathf.Deg2Rad*i);

            bool isCap = i == 0 || i == iceCreamResolution - 1;
            bool isFlipped = i== iceCreamResolution - 1;

            Quaternion rotation = Quaternion.Euler(0, -angle * i, 0);
            
            Circle circle = new Circle(radius, circleResolution, i, position, angle * i, rotation, isCap, isFlipped);
            circles.Add(circle);
        }
    }

    void UpdateVertices()
    {
        MoveVertices();
        vertices.Clear();
        for(int i=0; i<circles.Count; i++)
        {
            vertices.AddRange(circles[i].GetVertices());
        }
    }
    void MoveVertices()
    {
        for(int i=0; i<circles.Count; i++)
        {
            Vector3 position = circles[i].GetCenterPosition();
            float angle = circles[i].GetAngle();
            float iceCreamRadialDist = new Vector2(position.x - transform.position.x, position.z - transform.position.z).magnitude;
            
            angle += Time.deltaTime * speed;
            
            position.x = iceCreamRadialDist * Mathf.Cos(angle * Mathf.Deg2Rad);
            position.z = iceCreamRadialDist * Mathf.Sin(angle * Mathf.Deg2Rad);

            Quaternion rotation = Quaternion.Euler(0, -angle, 0);
            
            circles[i].UpdateCircle(position, angle, rotation);
        }
    }
    void AddData()
    {
        for(int i=0; i<circles.Count; i++)
        {
            vertices.AddRange(circles[i].GetVertices());
            triangles.AddRange(circles[i].GetTriangles());
        }
    }
    void LinkCircles()
    {
        for(int i=0; i<circles.Count-1; i++)
        {
            LinkCircles(circles[i], circles[i+1]);
        }
    }
    void LinkCircles(Circle c0, Circle c1)
    {
        int c0CenterIndex = c0.GetCenterIndex();
        int c1CenterIndex = c1.GetCenterIndex();

        for(int i=0; i<circleResolution-1; i++)
        {
            triangles.Add(c0CenterIndex + i + 1);
            triangles.Add(c1CenterIndex + i + 2);
            triangles.Add(c1CenterIndex + i + 1);

            triangles.Add(c0CenterIndex + i + 1);
            triangles.Add(c0CenterIndex + i + 2);
            triangles.Add(c1CenterIndex + i + 2);
        }

        triangles.Add(c0CenterIndex + circleResolution);
        triangles.Add(c1CenterIndex + 1);
        triangles.Add(c1CenterIndex + circleResolution);

        triangles.Add(c0CenterIndex + circleResolution);
        triangles.Add(c0CenterIndex + 1);
        triangles.Add(c1CenterIndex + 1);

    }
}
