using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class DepthImageSynthesis : BaseCameraPublisher
{
    public int FPS = 15;
    public bool EnableOffscreenRendering = true;

    public int width = 640;
    public int height = 480;
    public int depth = 32;
    public bool antialiasing = true;

    private RenderTexture renderTexture;
    private Texture2D renderToTexturePixels;
    private Texture2D texturePixels;

    private bool started = false;

    private Shader _shader;
    public Shader uberReplacementShader
    {
        get
        {
            if (_shader == null)
            {
                return _shader = Shader.Find("Hidden/UberReplacement");
            }
            return _shader;
        }
        set
        {
            _shader = value;
        }
    }

    new protected void Start()
    {
        base.Start();

        int antiAliasing = antialiasing ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;
        renderTexture = new RenderTexture(width, height, depth);
        renderTexture.antiAliasing = antiAliasing;
        renderToTexturePixels = new Texture2D(height, width, TextureFormat.RGBA32, false);
        texturePixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
        camera.targetTexture = renderTexture;
        StartCoroutine("OffscreenRendering");
    }

    IEnumerator OffscreenRendering()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            // render
            RenderTexture prevActiveRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            camera.Render();

            if (SystemInfo.supportsAsyncGPUReadback && EnableOffscreenRendering)
            {
                AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(renderTexture, 0);
                while (!request.done)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (request.hasError)
                {
                    Debug.Log("GPU readback error detected.");
                    yield return new WaitForSeconds(1.0f / FPS);
                    continue;
                }
                renderToTexturePixels.LoadRawTextureData(request.GetData<uint>());
                renderToTexturePixels.Apply();
                texturePixels.SetPixels(renderToTexturePixels.GetPixels());
            }
            else
            {
                // read offscreen texture contents into the CPU readable texture
                texturePixels.ReadPixels(new Rect(0, 0, texturePixels.width, texturePixels.height), 0, 0);
                texturePixels.Apply();
            }

            RenderTexture.active = prevActiveRT;

            byte[] pngBytes = texturePixels.EncodeToPNG();
            RosMessageTypes.Sensor.MCompressedImage imageMessage = new RosMessageTypes.Sensor.MCompressedImage(new RosMessageTypes.Std.MHeader(), "png", pngBytes);
            PublishWithCameraInfo(imageMessage);

            yield return new WaitForSeconds(1.0f / FPS);
        }
    }

    void LateUpdate()
    {
        // Start later after starting all other game objects
        if (!started)
        {
            started = true;
            OnCameraChange();
        }
    }

    static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacementMode mode, Color clearColor)
    {
        var cb = new CommandBuffer();
        cb.SetGlobalFloat("_OutputMode", (int)mode); // @TODO: CommandBuffer is missing SetGlobalInt() method
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
        cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
        cam.SetReplacementShader(shader, "");
        cam.backgroundColor = clearColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.allowHDR = false;
        cam.allowMSAA = false;
    }

    static private void SetupCameraWithPostShader(Camera cam, Material material, DepthTextureMode depthTextureMode = DepthTextureMode.None)
    {
        var cb = new CommandBuffer();
        cb.Blit(null, BuiltinRenderTextureType.CurrentActive, material);
        cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);
        cam.depthTextureMode = depthTextureMode;
    }

    public enum ReplacementMode
    {
        ObjectId = 0,
        CatergoryId = 1,
        DepthCompressed = 2,
        DepthMultichannel = 3,
        Normals = 4
    };

    public void OnCameraChange()
    {
        SetupCameraWithReplacementShader(camera, uberReplacementShader, ReplacementMode.DepthMultichannel, Color.white);
    }

}