using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture skyboxTexture;

    private RenderTexture _target;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void SetShaderParameter()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameter();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        //int kernelHandle = RayTracingShader.FindKernel("some"); // Decide on the kernel

        RayTracingShader.SetTexture(0, "Result", _target);

        int threadGroupX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(Screen.height / 8.0f);

        RayTracingShader.Dispatch(0, threadGroupX, threadGroupY, 1);

        Graphics.Blit(_target, destination);

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
}
