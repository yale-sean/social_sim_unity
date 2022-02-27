using System.Collections;
using UnityEditor;
using UnityEngine;

public class RGBCameraPublisher : BaseCameraPublisher
{
    public int FPS = 15;
    public bool EnableOffscreenRendering = true;

    public int width = 640;
    public int height = 480;

    private RenderTexture renderTexture;
    private Texture2D texturePixels;
    private Camera offscreenCamera;

    void Start()
    {
        base.Start();
        //print("SystemInfo.supportsAsyncGPUReadback: " + SystemInfo.supportsAsyncGPUReadback);
        renderTexture = new RenderTexture(width, height, 24);
        texturePixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Can't have 2 cameras on a single gameobject, so make a child
        GameObject offscreenCameraGO = new GameObject("OffscreenCamera");
        offscreenCameraGO.transform.parent = transform;
        offscreenCameraGO.transform.localPosition = Vector3.zero;
        offscreenCameraGO.transform.localRotation = Quaternion.identity;
        // Add the offscreen camera (a copy of the onscreenCamera)
        offscreenCamera = SEAN.Util.Unity.CopyComponent(camera, offscreenCameraGO);
        offscreenCamera.rect = new Rect(0f, 0f, 1f, 1f);
        offscreenCamera.targetTexture = renderTexture;
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
            offscreenCamera.Render();

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
                // read offscreen texture contents into the CPU readable texture
                texturePixels.ReadPixels(new Rect(0, 0, texturePixels.width, texturePixels.height), 0, 0);
                texturePixels.Apply();
                byteArray = texturePixels.EncodeToPNG();
            }

            // restore
            RenderTexture.active = prevActiveRT;

            RosMessageTypes.Sensor.MCompressedImage imageMessage = new RosMessageTypes.Sensor.MCompressedImage(new RosMessageTypes.Std.MHeader(), "png", byteArray);
            PublishWithCameraInfo(imageMessage);

            yield return new WaitForSeconds(1.0f / FPS);
        }
    }
}
