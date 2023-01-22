using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

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
    [SerializeField] MovementType movementType;
    [SerializeField] Vector3 InitialOffset;
    [SerializeField] float flowRate = 1f;
    [SerializeField] float tollerence = 2f;


    float startTime = 0;
    bool isReset = false;

    enum MovementType
    {
        RotateAlongTheAxis,
        FollowHelix
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateCircles();
    }
    private void Update()
    {
        if(rotate)
        {
            UpdateCircles();
        }
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
        float incAngle = loops * 360f / iceCreamResolution;
        float yStep = iceCreamHeight / iceCreamResolution;

        for(int i=0; i<iceCreamResolution; i++)
        {
            Vector3 position = Vector3.zero;

            float currentCirclePercent = 1 - ((float)i / iceCreamResolution);
            float realIceCreamRadius = currentCirclePercent * iceCreamRadius;

            position.x = realIceCreamRadius * Mathf.Cos(incAngle*Mathf.Deg2Rad*i);
            position.y = i * yStep;
            position.z = realIceCreamRadius * Mathf.Sin(incAngle*Mathf.Deg2Rad*i);

            bool isCap = i == 0 || i == iceCreamResolution - 1;
            bool isFlipped = i== iceCreamResolution - 1;

            Quaternion rotation = Quaternion.Euler(0, -incAngle * i, 0);
            
            Circle circle = new Circle(radius, circleResolution, i, position, incAngle * i, rotation, isCap, isFlipped);
            circle.SetRested(true);
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

    void CheckCircle(int index)
    {
        if (circles[index].GetRested())
        {
            circles[index].SetRested(false);
        }

    }
    void MoveVertices()
    {
        switch(movementType)
        {
            case MovementType.RotateAlongTheAxis:

                for(int i=0; i<circles.Count; i++)
                {
                    CheckCircle(i);

                    Vector3 position = circles[i].GetCenterPosition();
                    float angle = circles[i].GetAngle();
                    float iceCreamRadialDist = new Vector2(position.x - transform.position.x, position.z - transform.position.z).magnitude;
            
                    angle += Time.deltaTime * speed;
            
                    position.x = iceCreamRadialDist * Mathf.Cos(angle * Mathf.Deg2Rad);
                    position.z = iceCreamRadialDist * Mathf.Sin(angle * Mathf.Deg2Rad);

                    Quaternion rotation = Quaternion.Euler(0, -angle, 0);
            
                    circles[i].UpdateCircle(position, angle, rotation);
                }

                if(isReset)
                    isReset = false;


                break;
            case MovementType.FollowHelix:


                float incAngle = loops * 360f / iceCreamResolution;
                float yStep = (iceCreamHeight + InitialOffset.y) / iceCreamResolution;

                if(!isReset)
                {
                    ResetPosition();
                    startTime= Time.time;
                }
                
                for(int i=0; i<circles.Count; i++)
                {
                    float time = (Time.time - startTime);

                    if (time * time * time * flowRate < i)
                        break;
                    else
                    {
                        Vector3 position = InitialOffset;

                        float currentCirclePercent = 1 - ((float)i / iceCreamResolution);
                        float realIceCreamRadius = currentCirclePercent * iceCreamRadius;

                        position.x = realIceCreamRadius * Mathf.Cos(incAngle * Mathf.Deg2Rad * i);
                        position.y -= (time * time * time * flowRate - i) * yStep;
                        position.z = realIceCreamRadius * Mathf.Sin(incAngle * Mathf.Deg2Rad * i);

                        Quaternion rotation = Quaternion.Euler(0, -incAngle * i, 0);
                        if (Mathf.Abs(position.y - circles[i].GetFinalOrentation().position.y) <= tollerence)
                        {
                            position.y = i * yStep;
                            circles[i].SetRested(true);
                        }
                        circles[i].UpdateCircle(position, incAngle * i, rotation);
                    }

                }

                break;
        }
    }
    void ResetPosition()
    {
        for(int i=0; i<circles.Count; i++)
        {
            circles[i].UpdateCircle(InitialOffset, 0, Quaternion.identity);
        }
        isReset= true;
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
