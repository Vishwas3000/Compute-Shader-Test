using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShadeCaller : MonoBehaviour
{

    public ComputeShader TextureShader;
    private RenderTexture _rTexture;

    [Range (0,1)]
    public float variable;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_rTexture == null)
        {
            _rTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _rTexture.enableRandomWrite = true;
            _rTexture.Create();

            int kernel = TextureShader.FindKernel("CSMain");

            TextureShader.SetFloat("width", Screen.width);
            TextureShader.SetFloat("height", Screen.height);
            TextureShader.SetFloat("variable", variable);
            TextureShader.SetTexture(kernel, "Result", _rTexture);
            int workGroupX = Mathf.CeilToInt(Screen.width/8.0f);
            int workGroupY = Mathf.CeilToInt(Screen.height/8.0f);

            TextureShader.Dispatch(kernel, workGroupX, workGroupY, 1);

            Graphics.Blit(_rTexture, destination);
        }
    }
}
