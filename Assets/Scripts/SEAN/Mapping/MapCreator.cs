// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

namespace SEAN.Mapping
{
    public class PixelCube
    {
        public int x;
        public int z;
        public GameObject cube;
    }

    public class QuadCube
    {
        public float x_min;
        public float x_max;
        public float z_min;
        public float z_max;
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
        // published image quality level
        [Range(0, 100)]
        public int qualityLevel = 50;
        // ros:(x,y,yaw), unity:(x,z,yaw) of bottom left pixel in the map
        private Vector3 pose;
        // current location
        private int x_min, x_max, x_i, z_min, z_max, z_i = 0;

        // unchecked area [[x_min, x_max, z_min, z_max], ...]
        private List<float[]> toCheck = new List<float[]>();

        // map bounds
        private GameObject bounds;
        // current pixel
        private List<PixelCube> short_cubes;
        private List<PixelCube> tall_cubes;
        private List<QuadCube> quad_cubes;
        // map building in coroutine
        private IEnumerator coroutine;
        // images
        private Texture2D short_map;
        private Texture2D tall_map;

        // has the map been created
        private bool created = false;
        private bool built = false;
        private bool running = false;
        private float time_elap;

        // Ros connection and Ros Messages
        ROSConnection ros;
        RosMessageTypes.Sensor.MCompressedImage short_map_msg = new RosMessageTypes.Sensor.MCompressedImage();
        RosMessageTypes.Sensor.MCompressedImage tall_map_msg = new RosMessageTypes.Sensor.MCompressedImage();

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
            quad_cubes = new List<QuadCube>();

            time_elap = Time.time;

            // Use ROSTCP connector
            ros = ROSConnection.instance;

            // compute bottom left corner
            x_min = (int)(-bounds.transform.localScale.x / 2 + bounds.transform.localPosition.x);
            x_max = (int)(bounds.transform.localScale.x / 2 + bounds.transform.localPosition.x);
            z_min = (int)(-bounds.transform.localScale.z / 2 + bounds.transform.localPosition.z);
            z_max = (int)(bounds.transform.localScale.z / 2 + bounds.transform.localPosition.z);

            // See TransformExtensions.Unity2Ros
            pose = new Vector3(z_min, -x_max, 0f);
            steps_per_meter = Mathf.RoundToInt(1 / Resolution);
            x_i = x_min * steps_per_meter;
            z_i = z_min * steps_per_meter;
            // scale image to correct (pixel) dimensions
            int w = (x_max - x_min) * steps_per_meter;
            int h = (z_max - z_min) * steps_per_meter;
            Debug.Log("map (w,h): (" + w * Resolution + ", " + h * Resolution + ")");
            Debug.Log("origin (x,y,yaw): (" + pose[0] + ", " + pose[1] + ", 0)");
            short_map = new Texture2D(w, h, TextureFormat.RGB24, false);
            tall_map = new Texture2D(w, h, TextureFormat.RGB24, false);

            float[] map_dimensions = new float[] { x_min, x_max, z_min, z_max };
            toCheck.Add(map_dimensions);
        }


        // Update is called once per frame
        void Update()
        {
            if (TransformOnly)
            {
                return;
            }

            if (!running && !built)
            {
                running = true;
                CheckQuadrants();
                CheckQuadIntersections();
                running = false;
            }

            if (Time.time - time_elap > 1)
            {
                built = true;
            }

            if (built && !created)
            {
                created = true;
                RotateTextures();
                UpdateMessages();
                ros.Send("/short_map/compressed", short_map_msg);
                ros.Send("/tall_map/compressed", tall_map_msg);
                Debug.Log("Done!");
            }

        }

        void CheckQuadIntersections()
        {
            foreach (QuadCube cube in quad_cubes)
            {
                if (IntersectsMap(cube.cube))
                {
                    float[] map_dimensions = new float[] { cube.x_min, cube.x_max, cube.z_min, cube.z_max };
                    // Debug.Log(cube.cube.transform.position);
                    // Debug.Break();
                    toCheck.Add(map_dimensions);
                    Destroy(cube.cube);
                    time_elap = Time.time;
                }
                else
                {
                    int x_min_meter = (int)(cube.x_min * steps_per_meter);
                    int x_max_meter = (int)(cube.x_max * steps_per_meter);
                    int z_min_meter = (int)(cube.z_min * steps_per_meter);
                    int z_max_meter = (int)(cube.z_max * steps_per_meter);

                    int e_temp = 0;
                    int j_temp = 0;
                    for (int e = x_min_meter; e < x_max_meter; e++)
                    {
                        for (int j = z_min_meter; j < z_max_meter; j++)
                        {
                            e_temp = e - (x_min * steps_per_meter);
                            j_temp = j - (z_min * steps_per_meter);
                            short_map.SetPixel(e_temp, j_temp, Color.white);
                        }
                    }
                    // Don't destroy cube if you want to visualize the cubes
                    // but leftover cubes will cause problems
                    Destroy(cube.cube);
                }
            }

            quad_cubes = new List<QuadCube>();
        }


        void RotateTextures()
        {
            for (int i = 3; i > 0; i--)
            {
                short_map = RotateTexture90(short_map);
                tall_map = RotateTexture90(tall_map);
            }
        }

        Texture2D RotateTexture90(Texture2D texture)
        {
            Color32[] px = texture.GetPixels32();
            int w = texture.width;
            int h = texture.height;
            Color32[] rot = new Color32[w * h];
            // 90 degress counter clockwise
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    rot[i * h + j] = px[(h - j - 1) * w + i];
                }
            }
            Texture2D rotated = new Texture2D(h, w);
            rotated.SetPixels32(rot);
            rotated.Apply();
            return rotated;
        }

        // bool IntersectsMap(GameObject cube) {
        //     foreach (Transform transform in SceneObject.GetComponentsInChildren<Transform>()) {
        //         if (transform.gameObject.activeSelf) {
        //             foreach (Collider collider in transform.gameObject.GetComponents<Collider>()) {
        //                 if (collider == null) { continue; }
        //                 //if (!Intersects(cube.GetComponent<Collider>(), collider)) { continue; }
        //                 if (!Intersects(cube.GetComponent<Renderer>(), collider)) { continue; }
        //                 //Debug.Log(collider);
        //                 return true;
        //             }
        //         }
        //     }
        //     return false;
        // }
        bool IntersectsMap(GameObject cube)
        {
            Collider[] overlaps = Physics.OverlapBox(cube.transform.position, cube.GetComponent<Renderer>().bounds.extents);
            return (overlaps.Length > 0);
        }

        void UpdateMessages()
        {
            short_map_msg.header = new RosMessageTypes.Std.MHeader(); // blank header for now
            // short_map_msg.format = "png";
            // short_map_msg.data = short_map.EncodeToPNG();
            short_map_msg.format = "jpeg";
            short_map_msg.data = short_map.EncodeToJPG(qualityLevel);

            tall_map_msg.header = new RosMessageTypes.Std.MHeader(); // blank header for now
            tall_map_msg.format = "jpeg";
            tall_map_msg.data = tall_map.EncodeToJPG(qualityLevel);
        }

        void CheckQuadrants()
        {
            while (toCheck.Count != 0)
            {

                float[] area_checking = toCheck[0];
                float x_min_t = area_checking[0];
                float x_max_t = area_checking[1];
                float z_min_t = area_checking[2];
                float z_max_t = area_checking[3];

                float parent_scale = x_max_t - x_min_t;
                if (parent_scale <= Resolution)
                {
                    int x = (int)((x_min_t + (parent_scale / 2)) * steps_per_meter);
                    int z = (int)((z_min_t + (parent_scale / 2)) * steps_per_meter);
                    x = x - (x_min * steps_per_meter);
                    z = z - (z_min * steps_per_meter);
                    short_map.SetPixel(x, z, Color.black);
                    // Debug.Log(parent_scale);
                    // Debug.Break();

                }
                else
                {

                    for (int i = 0; i < 4; i++)
                    {
                        float scale = (x_max_t - x_min_t) / 2;
                        float x_pos = 0;
                        float z_pos = 0;
                        if (i == 0)
                        {
                            x_pos = x_min_t + (scale / 2);
                            z_pos = z_min_t + (scale / 2);
                        }
                        if (i == 1)
                        {
                            x_pos = x_min_t + (3 * (scale / 2));
                            z_pos = z_min_t + (scale / 2);
                        }
                        if (i == 2)
                        {
                            x_pos = x_min_t + (scale / 2);
                            z_pos = z_min_t + (3 * (scale / 2));
                        }
                        if (i == 3)
                        {
                            x_pos = x_min_t + (3 * (scale / 2));
                            z_pos = z_min_t + (3 * (scale / 2));
                        }

                        QuadCube quad_cube = new QuadCube();
                        quad_cube.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        quad_cube.cube.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0.5f, 0.1f);
                        quad_cube.cube.transform.position = new Vector3(x_pos, bounds.transform.localPosition.y + bounds.transform.localScale.y, z_pos);
                        quad_cube.cube.transform.localScale = new Vector3(scale, MapHeight - bounds.transform.localScale.y, scale);
                        quad_cube.x_min = x_pos - (scale / 2);
                        quad_cube.x_max = x_pos + (scale / 2);
                        quad_cube.z_min = z_pos - (scale / 2);
                        quad_cube.z_max = z_pos + (scale / 2);
                        quad_cubes.Add(quad_cube);

                    }

                }

                toCheck.Remove(area_checking);

            }
        }

        bool Intersects(Renderer a, Collider b)
        {

            bool overlapping = a.bounds.Intersects(b.bounds);
            return overlapping;
        }

    }
}