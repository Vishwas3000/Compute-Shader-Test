using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Circle 
{
    [Header("Settings")]
    float radius;
    int circleResolution;
    int index;
    int centerIndex;

    Vector3 position;
    Quaternion rotation;

    bool isCap;
    bool isFlipped;

    [Header("Data")]
    List<Vector3> vertices;
    List<int> triangles;

    public Circle(float _radius, int _resolution, int _index, Vector3 _position, Quaternion _rotation, bool _isCap, bool _isFlipped)
    {
        radius= _radius;
        circleResolution= _resolution;
        index= _index;

        centerIndex = index * (circleResolution + 1);

        position= _position;
        rotation= _rotation;
        
        isCap= _isCap;
        isFlipped= _isFlipped;

        vertices = new List<Vector3>();
        triangles = new List<int>();

        CreateVertices();
        if(isCap)
        {
            CreateTriangles();
        }
    }
    
    public Vector3[] GetVertices()
    {
        return vertices.ToArray();
    }
    public int[] GetTriangles()
    {
        return triangles.ToArray();
    }
    public int GetCenterIndex()
    {
        return centerIndex;
    }
    void CreateVertices()
    {
        Vector3 center = position;

        vertices.Add(center);
        float angleBetweenPoints = 360f / circleResolution;

        for (int i = 0; i < circleResolution; i++)
        {
            Vector3 vertex = center;
            vertex.x += radius * Mathf.Cos(angleBetweenPoints * i * Mathf.Deg2Rad);
            vertex.y += radius * Mathf.Sin(angleBetweenPoints * i * Mathf.Deg2Rad);
            
            vertex = rotation * (vertex - center) + center;

            vertices.Add(vertex);
        }
    }

    void CreateTriangles()
    {
        for (int i = centerIndex; i < circleResolution - 1 + centerIndex; i++)
        {
            triangles.Add(centerIndex);
            if(isFlipped)
            {
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }
            else
            {
                triangles.Add(i + 2);
                triangles.Add(i + 1);
            }
        }
        triangles.Add(centerIndex);
        if (isFlipped)
        {
            triangles.Add(circleResolution + centerIndex);
            triangles.Add(centerIndex + 1);
        }
        else
        {
            triangles.Add(centerIndex + 1);
            triangles.Add(circleResolution + centerIndex);
        }
    }
}
