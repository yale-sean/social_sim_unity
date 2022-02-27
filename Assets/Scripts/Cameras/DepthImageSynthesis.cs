using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class DepthImageSynthesis : BaseCameraPublisher
{
    public int FPS = 15;
    public bool EnableOffscreenRendering = true;

    public int width = 640;
    public int height = 480;
    public int depth = 24;
    public bool antialiasing = true;

    private RenderTexture renderTexture;
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

    void Start()
    {
        base.Start();
        int antiAliasing = antialiasing ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;
        renderTexture = new RenderTexture(width, height, depth);
        renderTexture.antiAliasing = antiAliasing;
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

            byte[] byteArray;
            if (SystemInfo.supportsAsyncGPUReadback && EnableOffscreenRendering)
            {
                UnityEngine.Rendering.AsyncGPUReadbackRequest request = UnityEngine.Rendering.AsyncGPUReadback.Request(texturePixels, 0);
                while (!request.done)
                {
                    yield return new WaitForEndOfFrame();
                }
                byteArray = request.GetData<byte>().ToArray();
            }
            else
            {
                // read offsreen texture contents into the CPU readable texture
                texturePixels.ReadPixels(new Rect(0, 0, texturePixels.width, texturePixels.height), 0, 0);
                texturePixels.Apply();
                byteArray = texturePixels.EncodeToPNG();
            }

            RenderTexture.active = prevActiveRT;

            RosMessageTypes.Sensor.MCompressedImage imageMessage = new RosMessageTypes.Sensor.MCompressedImage(new RosMessageTypes.Std.MHeader(), "png", byteArray);
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

    static private void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacementMode mode)
    {
        SetupCameraWithReplacementShader(cam, shader, mode, Color.black);
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
        ////int targetDisplay = 2;
        ////var camera = GetComponent<Camera>();
        //foreach (var pass in capturePasses)
        //{
        //    if (pass.camera == GetComponent<Camera>())
        //        continue;

        //    // cleanup capturing camera
        //    pass.camera.RemoveAllCommandBuffers();

        //    // copy all "main" camera parameters into capturing camera
        //    pass.camera.CopyFrom(GetComponent<Camera>());

        //    // set targetDisplay here since it gets overriden by CopyFrom()
        //    //pass.camera.targetDisplay = targetDisplay++;
        //}

        // setup command buffers and replacement shaders
        SetupCameraWithReplacementShader(camera, uberReplacementShader, ReplacementMode.DepthMultichannel, Color.white);
    }

    // #if UNITY_EDITOR
    //     private GameObject lastSelectedGO;
    //     private int lastSelectedGOLayer = -1;
    //     private string lastSelectedGOTag = "unknown";
    //     private bool DetectPotentialSceneChangeInEditor()
    //     {
    //         bool change = false;
    //         // there is no callback in Unity Editor to automatically detect changes in scene objects
    //         // as a workaround lets track selected objects and check, if properties that are 
    //         // interesting for us (layer or tag) did not change since the last frame
    //         if (UnityEditor.Selection.transforms.Length > 1)
    //         {
    //             // multiple objects are selected, all bets are off!
    //             // we have to assume these objects are being edited
    //             change = true;
    //             lastSelectedGO = null;
    //         }
    //         else if (UnityEditor.Selection.activeGameObject)
    //         {
    //             var go = UnityEditor.Selection.activeGameObject;
    //             // check if layer or tag of a selected object have changed since the last frame
    //             var potentialChangeHappened = lastSelectedGOLayer != go.layer || lastSelectedGOTag != go.tag;
    //             if (go == lastSelectedGO && potentialChangeHappened)
    //                 change = true;

    //             lastSelectedGO = go;
    //             lastSelectedGOLayer = go.layer;
    //             lastSelectedGOTag = go.tag;
    //         }

    //         return change;
    //     }
    // #endif // UNITY_EDITOR
}

