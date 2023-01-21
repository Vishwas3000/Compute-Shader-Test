using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream : MonoBehaviour
{
    public Mesh mesh;
    public float Omega;
    public float lagPhase, amount, point, pourRate, yOffset, restTime, val;
    Vector3[] vertices;
    Vector3[] IniOffset;
    // Start is called before the first frame update
    void Start()
    {
        mesh= GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        IniOffset = new Vector3[vertices.Length];
        for(int i=0; i<vertices.Length; i++)
        {
            IniOffset[i] = vertices[i];
        }
    }

    float Amplitude(float val)
    {
        return val * val;
    }

    // Update is called once per frame

    void MoveCream(int i)
    {
        float dist = vertices[i].y - point;
        float hInf = dist * lagPhase;
        float x = Mathf.Cos(Time.time * Omega + hInf) * Amplitude(dist * amount) + IniOffset[i].x;
        float z = Mathf.Sin(Time.time * Omega + hInf) * Amplitude(dist * amount) + IniOffset[i].z;
        float y = pourRate * Time.deltaTime +  vertices[i].y;
        Vector3 newVertx = new Vector3(x, y, z);
        vertices[i] = newVertx;
    }
    void FixedUpdate()
    {
        
        for(int i=0; i<vertices.Length; i++) 
        {
            if (Time.time - IniOffset[i].y * val - restTime<0)
            {
                MoveCream(i);
            }
        }
        mesh.vertices = vertices;
    }
}
