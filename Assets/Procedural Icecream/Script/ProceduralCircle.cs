using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static Circle;

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

    [Header("System Settings")]
    [SerializeField] IceCreamState iceCreamState;

    [Header("Animation Control")]
    [SerializeField] float speed = 1f;
    [SerializeField] bool rotate = false;
    [SerializeField] MovementType movementType;
    [SerializeField] Vector3 InitialOffset;
    [SerializeField] float flowRate = 1f;
    [SerializeField] float tollerence = 2f;
    [SerializeField] float releaseRate = 10f;


    float startTime = - 1;
    bool isReset = false;
    bool isEmitting;
    float checkTime =0;

    enum MovementType
    {
        RotateAlongTheAxis,
        FollowHelix
    }
    enum IceCreamState
    {
        Create,
        Simulate
    }

    // Start is called before the first frame update
    void Start()
    {
    }
    private void Update()
    {
        switch(iceCreamState)
        {
            case IceCreamState.Create:                
                GenerateCircles();
                break;

            case IceCreamState.Simulate:
                if(rotate)
                {
                    UpdateCircles();
                }
                break;

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

            circle.SetActive(false);
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
        if (!circles[index].GetMotionData().isActive)
        {
            circles[index].SetActive(true);
        }

    }
    void MoveVertices()
    {
        switch(movementType)
        {
            case MovementType.RotateAlongTheAxis:
                // For Debugging
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

                if (isReset)
                    isReset = false;

                break;


            case MovementType.FollowHelix:


                float incAngle = loops * 360f / iceCreamResolution;
                float yStep = (iceCreamHeight + InitialOffset.y) / iceCreamResolution;

                if(!isReset)
                {
                    ResetPosition();
                    startTime= -1;
                }

                for(int i=0; i<circles.Count; i++)
                {
s
                    if(isEmitting)
                    {
                        checkTime += Time.deltaTime / releaseRate;
                    }

                    if (checkTime * flowRate < i)
                        break;
                    else
                    {
                        Vector3 distanceCheck = circles[i].GetCenterPosition() - circles[i].GetFinalOrentation().position;

                        if (!circles[i].GetMotionData().isActive && distanceCheck.magnitude>tollerence*10)
                        {
                            circles[i].SetActive(true);
                            circles[i].SetStartTime(Time.time);
                        }

                        Vector3 position = InitialOffset;

                        float currentCirclePercent = 1 - ((float)i / iceCreamResolution);
                        float realIceCreamRadius = currentCirclePercent * iceCreamRadius;
                        
                        MotionData motionData = circles[i].GetMotionData();
                        float time = Time.time - motionData.startTime;

                        position.x = realIceCreamRadius * Mathf.Cos(incAngle * Mathf.Deg2Rad * i);
                        //position.y -= (simulationTime * simulationTime * flowRate - i) * yStep;
                        position.y -= (time * time * flowRate) * yStep;
                        position.z = realIceCreamRadius * Mathf.Sin(incAngle * Mathf.Deg2Rad * i);

                        Quaternion rotation = Quaternion.Euler(0, -incAngle * i, 0);

                        if (Mathf.Abs(position.y - circles[i].GetFinalOrentation().position.y) <= tollerence)
                        {
                            position.y = i * yStep;
                            circles[i].SetActive(false);
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
            circles[i].SetActive(true);
            circles[i].UpdateCircle(InitialOffset, 0, Quaternion.identity);
            circles[i].SetActive(false);
        }
        isReset = true;
        checkTime= 0;
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

    public void FlipEmit()
    {
        isEmitting ^= true;
    }

}
