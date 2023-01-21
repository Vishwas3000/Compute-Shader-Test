using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamMaster : MonoBehaviour
{
    ComputeBuffer buffer;
    [SerializeField] Material material;
    Mesh mesh;
    int vertexCount;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Material>();
        mesh = GetComponent<MeshFilter>().mesh;
        vertexCount = mesh.vertexCount;
        buffer = new ComputeBuffer(vertexCount, VertexData.GetSize());

        VertexData[] vertexDatas= new VertexData[vertexCount];
 
        for(int i=0; i<vertexCount; i++)
        {
            VertexData data = new VertexData()
            {
                restTime = 0.5f
            };
            vertexDatas[i] = data;  
        }

        buffer.SetData(vertexDatas);
        material.SetBuffer("buffer", buffer);
    }

    private void OnDisable()
    {
        buffer.Dispose();
    }
    private void OnDestroy()
    {
        buffer.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public struct VertexData
{
    public float restTime;

    public static int GetSize()
    {
        return sizeof(float) ;
    }
};