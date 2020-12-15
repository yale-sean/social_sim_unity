using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PixelCube {
    public int x;
    public int z;
    public GameObject cube;
}

public class MapCreator : MonoBehaviour
{
    // The main scene object over which collisions will be checked
    public GameObject SceneObject;
    // height of the top of the map for the "tall" objects
    public float MapHeight = 1.0f;
    // resolution in meters per pixel
    public float Resolution = 0.1f;
    private int steps_per_meter;
    // only show the origin
    public bool TransformOnly = false;
    // publishers
    private RosSharp.RosBridgeClient.TexturePublisher short_map_publisher;
    private RosSharp.RosBridgeClient.TexturePublisher tall_map_publisher;
    // published image quality level
    [Range(0, 100)]
    public int qualityLevel = 50;
    // ros:(x,y,yaw), unity:(x,z,yaw) of bottom left pixel in the map
    private Vector3 pose;
    // current location
    private int x_min, x_max, x_i, z_min, z_max, z_i = 0;
    // map bounds
    private GameObject bounds;
    // current pixel
    private List<PixelCube> short_cubes;
    private List<PixelCube> tall_cubes;
    // map building in coroutine
    private IEnumerator coroutine;
    // images
    private Texture2D short_map;
    private Texture2D tall_map;

    // has the map been created
    private bool created = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Building map. Disable this script in production. Enable to build a new map.");
        // disable bound objects
        bounds = GameObject.Find("CreateMap/LowerMap");
        bounds.SetActive(false);

        // create cube list
        short_cubes = new List<PixelCube>();
        tall_cubes = new List<PixelCube>();

        // set map publishers from same object
        foreach (RosSharp.RosBridgeClient.TexturePublisher tp in this.GetComponents<RosSharp.RosBridgeClient.TexturePublisher>()) {
            if (tp.Topic.StartsWith("/short_map")) {
                short_map_publisher = tp;
            }
            else if (tp.Topic.StartsWith("/tall_map")) {
                tall_map_publisher = tp;
            }
            else {
                Debug.LogError("Texture Publisher Topic names must start with [short_map|tall_map]");
            }
        }

        // compute bottom left corner
        x_min = (int)(-bounds.transform.localScale.x/2+bounds.transform.localPosition.x);
        x_max = (int)(bounds.transform.localScale.x/2+bounds.transform.localPosition.x);
        z_min = (int)(-bounds.transform.localScale.z/2+bounds.transform.localPosition.z);
        z_max = (int)(bounds.transform.localScale.z/2+bounds.transform.localPosition.z);
        // See TransformExtensions.Unity2Ros
        pose = new Vector3(z_min, -x_max, 0f);
        steps_per_meter = Mathf.RoundToInt(1/Resolution);
        x_i = x_min * steps_per_meter;
        z_i = z_min * steps_per_meter;
        // scale image to correct (pixel) dimensions
        int w = (x_max-x_min) * steps_per_meter;
        int h = (z_max-z_min) * steps_per_meter;
        Debug.Log("map (w,h): (" + w * Resolution + ", " + h * Resolution + ")");
        Debug.Log("origin (x,y,yaw): (" + pose[0] + ", " + pose[1] + ", 0)");
        short_map = new Texture2D(w, h, TextureFormat.RGB24, false);
        tall_map = new Texture2D(w, h, TextureFormat.RGB24, false);

        // create the map
        if (!TransformOnly) {
            CreateCubes();
        }
    }


    // Update is called once per frame
    void Update() {
        if (TransformOnly) {
            return;
        }
        if (!created) {
            created = true;
            CheckIntersections();
            RotateTextures();
            UpdateMessages();
        }
    }

    void CheckIntersections() {
        Debug.Log("Checking Intersections");
        System.DateTime start = System.DateTime.Now;
        // set the current pixel black if there are any intersections
        foreach (PixelCube cube in short_cubes) {
            if (IntersectsMap(cube.cube)) {
                short_map.SetPixel(cube.x, cube.z, Color.black);
            } else {
                short_map.SetPixel(cube.x, cube.z, Color.white);
            }
        }
        foreach (PixelCube cube in tall_cubes) {
            if (IntersectsMap(cube.cube)) {
                tall_map.SetPixel(cube.x, cube.z, Color.black);
            } else {
                tall_map.SetPixel(cube.x, cube.z, Color.white);
            }
        }
        Debug.Log("Done Checking Intersections in " + (System.DateTime.Now - start).TotalSeconds + " seconds.");
    }

    void RotateTextures() {
        for (int i = 3; i > 0; i--) {
            short_map = RotateTexture90(short_map);
            tall_map = RotateTexture90(tall_map);
        }
    }

    Texture2D RotateTexture90(Texture2D texture) {
        Color32[] px = texture.GetPixels32();
        int w = texture.width;
        int h = texture.height;
        Color32[] rot = new Color32[w * h];
        // 90 degress counter clockwise
        for (int i = 0; i < w; ++i) {
            for (int j = 0; j < h; ++j) {
                rot[i*h+j] = px[(h-j-1)*w+i];
            }
        }
        Texture2D rotated = new Texture2D(h, w);
        rotated.SetPixels32(rot);
        rotated.Apply();
        return rotated;
    }

    bool IntersectsMap(GameObject cube) {
        foreach (Transform transform in SceneObject.GetComponentsInChildren<Transform>()) {
            if (transform.gameObject.activeSelf) {
                foreach (Collider collider in transform.gameObject.GetComponents<Collider>()) {
                    if (collider == null) { continue; }
                    if (!Intersects(cube.GetComponent<Collider>(), collider)) { continue; }
                    return true;
                }
            }
        }
        return false;
    }

    void UpdateMessages() {
        short_map_publisher.message.data = short_map.EncodeToJPG(qualityLevel);
        tall_map_publisher.message.data = tall_map.EncodeToJPG(qualityLevel);
    }

    void CreateCubes() {
        for (; x_i < x_max*steps_per_meter; x_i++) {
            for (z_i = z_min * steps_per_meter; z_i < z_max*steps_per_meter; z_i++) {
                int x = x_i - (x_min * steps_per_meter);
                int z = z_i - (z_min * steps_per_meter);

                PixelCube short_cube = new PixelCube();
                short_cube.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                short_cube.cube.GetComponent<Renderer>().material.color = new Color(0f,0.5f,0f,0.1f);
                short_cube.cube.transform.position = new Vector3(Resolution*x_i, bounds.transform.localPosition.y, Resolution*z_i);
                short_cube.cube.transform.localScale = new Vector3(Resolution, bounds.transform.localScale.y, Resolution);
                short_cube.x = x;
                short_cube.z = z;
                short_cubes.Add(short_cube);

                PixelCube tall_cube = new PixelCube();
                tall_cube.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tall_cube.cube.GetComponent<Renderer>().material.color = new Color(0.5f,0f,0.5f,0.1f);
                tall_cube.cube.transform.position = new Vector3(Resolution*x_i, bounds.transform.localPosition.y+bounds.transform.localScale.y, Resolution*z_i);
                tall_cube.cube.transform.localScale = new Vector3(Resolution, MapHeight - bounds.transform.localScale.y, Resolution);
                tall_cube.x = x;
                tall_cube.z = z;
                tall_cubes.Add(tall_cube);
            }
        }
    }

    bool Intersects(Collider a, Collider b){
        bool overlapping = a.bounds.Intersects(b.bounds);
        return overlapping;
    } 

}