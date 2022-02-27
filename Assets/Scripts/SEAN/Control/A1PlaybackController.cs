// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SEAN.Control
{
    class MocapFrame
    {
        public static string[] JOINTS = {
            "FR_hip",
            "FR_thigh",
            "FR_calf",
            "FL_hip",
            "FL_thigh",
            "FL_calf",
            "RR_hip",
            "RR_thigh",
            "RR_calf",
            "RL_hip",
            "RL_thigh",
            "RL_calf",
        };

        public int frame;
        public int t;
        public float[] joints;

        public MocapFrame(string csvLine)
        {
            var tokens = csvLine.Split(',');
            frame = int.Parse(tokens[0]);
            t = int.Parse(tokens[1]);
            joints = new float[JOINTS.Length];
            for (int i = 0; i < JOINTS.Length; i++)
            {
                joints[i] = float.Parse(tokens[i + 2]) * (180 / Mathf.PI);
            }
        }
    }

    public class A1PlaybackController : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        private List<MocapFrame> frames;
        //public float stiffness;
        //public float damping;
        public float forceLimit = 10;
        //public float speed = 5f; // Units: degree/s
        //public float torque = 100f; // Units: Nm or N
        //public float acceleration = 5f;// Units: m/s^2 / degree/s^2

        public int updateFrequency = 60;
        private int updateCount = 0;
        private int currentFrame = 0;

        void Awake()
        {
            var sr = new StreamReader(Application.dataPath + @"/Resources/a1mocap.csv");
            string line;
            frames = new List<MocapFrame>();
            while ((line = sr.ReadLine()) != null)
            {
                //print(line);
                frames.Add(new MocapFrame(line));
            }
        }

        void Start()
        {
            // Add Joint Control
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();
            foreach (ArticulationBody joint in articulationChain)
            {
                joint.gameObject.AddComponent<JointControl>();
                //joint.jointFriction = defDyanmicVal;
                //joint.angularDamping = defDyanmicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
            }
        }

        private void Update()
        {
            updateCount++;
            if (updateFrequency % updateCount != 0)
            {
                return;
            }
            currentFrame %= frames.Count;
            int i = 0;
            //print("joint count: " + articulationChain.Length);
            foreach (ArticulationBody joint in articulationChain)
            {
                if (!MocapFrame.JOINTS.Contains(joint.name))
                {
                    continue;
                }
                RotateTo(joint, frames[currentFrame].joints[i++]);
            }
            currentFrame++;
        }

        void RotateTo(ArticulationBody articulation, float primaryAxisRotation)
        {
            print("rotating " + articulation.name + " to " + primaryAxisRotation);
            var drive = articulation.xDrive;
            drive.target = primaryAxisRotation;
            articulation.xDrive = drive;
        }
    }
}
