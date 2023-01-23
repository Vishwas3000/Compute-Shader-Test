using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Sphere
{
    public float radius;
    public Vector3 position;
    public Vector3 albedo;
    public Vector3 specular;

};

public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture skyboxTexture;

    private RenderTexture _target;
    private Camera _camera;

    private uint _currentSample = 0;
    private Material _addMaterial;

    public Light DirectionalLight;

    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;
    private ComputeBuffer _sphereBuffer;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }
    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }



    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();
            // Radius and radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Debug.Log(sphere.radius);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }

            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            // Add the sphere to the list
            spheres.Add(sphere);
            SkipSphere:
                continue;
        }
        // Assign to compute buffer
        _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        _sphereBuffer.SetData(spheres);
    }

    private void SetShaderParameter()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameter();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        int kernelHandle = RayTracingShader.FindKernel("CSMain"); // Decide on the kernel

        RayTracingShader.SetTexture(kernelHandle, "Result", _target);

        int threadGroupX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(Screen.height / 8.0f);

        RayTracingShader.Dispatch(kernelHandle, threadGroupX, threadGroupY, 1);


        if (_addMaterial==null)
        {
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        }
        _addMaterial.SetFloat("_sample", _currentSample);

        //Graphics.Blit(_target, destination);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;

    }

    private void InitRenderTexture()
    {
        if(_target==null || _target.width!=Screen.width || _target.height!=Screen.height)
        {
            if(_target!=null)
            {
                _target.Release();
            }

            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;

            _target.Create();
        }
    }

    private void Update()
    {
        if(transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
            Debug.Log("has changed");
        }
    }
}
