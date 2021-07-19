using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class DrawOnCamera : MonoBehaviour
{
    [SerializeField] private ComputeShader _renderShader;
    private const int uvSideCount = 17;
    private const int _lightFieldWidth = 1024;
    private const int _lightFieldHeight = 512;


    private RenderTexture _target;
    private Camera _camera;
    private Texture[] assetTextures = null;
    private bool _initDone = false;
    ComputeBuffer lightFieldTextures;
    float[] _lightFieldSize = new float[] { _lightFieldWidth, _lightFieldHeight };
    IEnumerable<Vector4> test;


    private void Awake()
    {
        _initDone = false;
        _camera = GetComponent<Camera>();


        StartCoroutine(
            GetAssetFromAssetBundle.GetAssetsAsync<Texture>(
                "AssetBundles/StandaloneWindows64/lightfield",
                (r) => assetTextures = r
            )
        );


    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (assetTextures == null && !_initDone) {
            Graphics.Blit(source, destination);
            return;
        }

        if (!_initDone)
        {
            test = Enumerable.Empty<Vector4>();
            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    Debug.Log("lightField_" + i.ToString("00") + "_" + j.ToString("00") );
                    _renderShader.SetTexture(0, "lightField_" + i.ToString("00") + "_" + j.ToString("00"), assetTextures[i * uvSideCount + j]);
                }
            } 
            _initDone = true;
            SetShaderParameters();
        }


        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        _renderShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        _renderShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        _renderShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 16.0f);
        _renderShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        UnityEngine.Graphics.Blit(_target, destination);

        //lightFieldTextures.Dispose();
        //lightFieldTextures = null;
        //assetTextures = null;
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            if(_target != null)
            {
                _target.Release();
            }

            _target = new RenderTexture(Screen.width, Screen.height, 0, 
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void SetShaderParameters()
    {
        /*
        lightFieldTextures = new ComputeBuffer(
            _lightFieldWidth * _lightFieldHeight * uvSideCount * 5,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector4))
            );
        lightFieldTextures.SetData(test.ToArray());*/

        // _renderShader.SetBuffer(0, "LightFields", lightFieldTextures);
        _renderShader.SetFloats("LightFieldSize", _lightFieldSize);
        
    }



    public Texture2D ToTexture2D(Texture input)
    {
        Texture2D result = new Texture2D(input.width, input.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture rt = new RenderTexture(input.width, input.height, 32);
        Graphics.Blit(input, rt);
        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        result.Apply();
        RenderTexture.active = currentRT;
        return result;
    }


}
