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


    private RenderTexture _target;
    private Camera _camera;
    private Texture[] assetTextures = null;
    private bool _initDone = false;
    ComputeBuffer lightFieldTextures;

    public Texture2D ToTexture2D(Texture self)
    {
        var sw = self.width;
        var sh = self.height;
        var format = TextureFormat.RGBA32;
        var result = new Texture2D(sw, sh, format, false);
        var currentRT = RenderTexture.active;
        var rt = new RenderTexture(sw, sh, 32);
        UnityEngine.Graphics.Blit(self, rt);
        RenderTexture.active = rt;
        var source = new Rect(0, 0, rt.width, rt.height);
        result.ReadPixels(source, 0, 0);
        result.Apply();
        RenderTexture.active = currentRT;
        return result;
    }


    Vector4[] texs;
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
        if (assetTextures == null ) {
            Graphics.Blit(source, destination);
            return;
        }

        if (!_initDone)
        {
            IEnumerable<Vector4> test = Enumerable.Empty<Vector4>();
            for (int i = 0; i < 1; i++)
            {
                
                Texture2D a = ToTexture2D(assetTextures[i]);
                
                
                for (int k = a.height - 1 ; k >= 0 ; k--)
                {
                    for (int j = 0; j < a.width; j++)
                    {
                        Vector4 vec = a.GetPixel(j, k);
                        test = test.Concat<Vector4>(new Vector4[] { vec });
                    }
                            
                }
                
            }
            texs = test.ToArray();
            _initDone = true;

        }

        SetShaderParameters();

        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        _renderShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        _renderShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        UnityEngine.Graphics.Blit(_target, destination);

        lightFieldTextures.Dispose();
        lightFieldTextures = null;
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
        
        lightFieldTextures = new ComputeBuffer(
            assetTextures[0].width * assetTextures[0].height * 1, //uvSideCount * uvSideCount
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector4))
            );
        lightFieldTextures.SetData(texs);

        float[] lightFieldSize = new float[] { assetTextures[0].width, assetTextures[0].height };

        _renderShader.SetBuffer(0, "LightFields", lightFieldTextures);
        _renderShader.SetFloats("LightFieldSize", lightFieldSize);
        _renderShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        _renderShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);

        
    }

}
