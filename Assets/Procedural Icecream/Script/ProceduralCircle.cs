using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static Circle;

public enum IceCreamState
{
    Create,
    Simulate
}
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
    [SerializeField] public IceCreamState iceCreamState;

    [Header("Animation Control")]
    [SerializeField] float speed = 1f;
    [SerializeField] bool rotate = false;
    [SerializeField] MovementType movementType;
    [SerializeField] Vector3 InitialOffset;
    [SerializeField] float flowRate = 1f;
    [SerializeField] float tollerence = 2f;
    [SerializeField] float renderTollerence = 2f;
    [SerializeField] float releaseRate = 10f;


    bool isReset = false;
    bool isEmitting;
    float checkTime =0;
    int duplicateCircles = 0;
    int startActiveCircle;
    int lastActiveCircle=0;

    enum MovementType
    {
        RotateAlongTheAxis,
        FollowHelix
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Update()
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
                }
                for(int i=0; i<circles.Count; i++)
                {
                    if(isEmitting)
                    {
                        checkTime += Time.deltaTime / releaseRate;
                        //Debug.Log(gameObject.name + " is emmiting " + checkTime);
                    }
                    //Debug.Log(gameObject.name + " : " +  startActiveCircle + " : " + (i - duplicateCircles));
                    //
                    if ((checkTime * flowRate <= (i - duplicateCircles)))
                        break;
                    else if (((i - duplicateCircles) < startActiveCircle) && !circles[i - duplicateCircles].GetMotionData().isActive)
                        continue;
                    else
                    {
                        Vector3 distanceCheck = circles[i].GetCenterPosition() - circles[i].GetFinalOrentation().position;

                        if (!circles[i].GetMotionData().isActive && Mathf.Abs(distanceCheck.magnitude) > tollerence * 10)
                        {
                            circles[i].SetActive(true);
                            circles[i].SetStartTime(Time.time);

                            if (i > 0)
                            {
                                //Debug.Log("Lineking circles");

                                if (Mathf.Abs(circles[i - 1].GetCenterPosition().y - circles[i].GetCenterPosition().y) > renderTollerence)
                                {
                                    //Debug.Log("creating circle");
                                    Circle dupCircle = new Circle(circles[i - 1]);
                                    dupCircle.SetActive(true);
                                    dupCircle.SetStartTime(Time.time);
                                    dupCircle.UpdateCircle(InitialOffset, 0, Quaternion.identity);

                                    circles.Insert(i, dupCircle);
                                    //LinkCircles(dupCircle, circles[i+1]);
                                    //i++;
                                    duplicateCircles++;
                                }
                                else
                                {
                                    LinkCircles(circles[i - 1], circles[i]);

                                }

                            }

                            mesh.triangles = triangles.ToArray();

                            mesh.RecalculateBounds();
                            mesh.RecalculateNormals();

                            meshFilter.sharedMesh = mesh;
                        }

                        Vector3 position = InitialOffset;

                        float currentCirclePercent = 1 - ((float)i / iceCreamResolution);
                        float realIceCreamRadius = currentCirclePercent * iceCreamRadius;

                        MotionData motionData = circles[i].GetMotionData();
                        float time = Time.time - motionData.startTime;

                        position.x = realIceCreamRadius * Mathf.Cos(incAngle * Mathf.Deg2Rad * (i - duplicateCircles));
                        position.y -= (time * time * flowRate) * yStep;
                        position.z = realIceCreamRadius * Mathf.Sin(incAngle * Mathf.Deg2Rad * (i - duplicateCircles));

                        Quaternion rotation = Quaternion.Euler(0, -incAngle * (i - duplicateCircles), 0);

                        if (circles[i].GetMotionData().isActive && Mathf.Abs(position.y - circles[i].GetFinalOrentation().position.y) <= tollerence)
                        {
                            position.y = (i - duplicateCircles) * yStep;
                            circles[i].SetActive(false);
                            //Debug.Log("set inactive " + i);
                        }
                        circles[i].UpdateCircle(position, incAngle * i, rotation);

                        lastActiveCircle = Mathf.Max(lastActiveCircle, i - duplicateCircles);
                    }

                }

                break;


        }
    }
    public void ResetPosition()
    {
        triangles.Clear();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;

        for (int i=0; i<circles.Count; i++)
        {
            circles[i].SetActive(true);
            circles[i].UpdateCircle(InitialOffset, 0, Quaternion.identity);
            circles[i].SetActive(false);
        }
        isReset = true;
        checkTime= 0;
        lastActiveCircle= 0;
    }
    void AddData()
    {
        for(int i=0; i<circles.Count; i++)
        {
            vertices.AddRange(circles[i].GetVertices());
            triangles.AddRange(circles[i].GetTriangles());
        }
    }
    void AddTriangles()
    {
        for (int i = 0; i < circles.Count; i++)
        {
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
    public bool IsEmitting()
    {
        return isEmitting;
    }
    public int GetLastActiveCircle()
    {
        return lastActiveCircle;
    }
    public void SetStartActiveCircle(int val)
    {
        startActiveCircle = val;
    }
}
