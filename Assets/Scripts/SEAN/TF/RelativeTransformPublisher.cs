// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace SEAN.TF
{
    public class RelativeTransformPublisher : BaseTransformPublisher
    {
        SEAN sean;
        public string FrameID;

        public float AdjustXRot = 0;
        public float AdjustYRot = 0;
        public float AdjustZRot = 0;

        private bool initialized = false;
        private RosMessageTypes.Std.MTime LastHeader = new RosMessageTypes.Std.MTime();
        GameObject BaseToFrame;

        RosMessageTypes.Geometry.MPoseStamped baseToFramePoseStamped = new RosMessageTypes.Geometry.MPoseStamped();

        private void Start()
        {
            sean = SEAN.instance;
            base.Start();
            BaseToFrame = new GameObject();
            BaseToFrame.name = "base_link_to_" + FrameID;
            baseToFramePoseStamped.header.frame_id = FrameID;
        }

        private void Update()
        {
            BaseToFrame.transform.position = transform.position - sean.robot.position;
            BaseToFrame.transform.rotation = transform.rotation * Quaternion.Inverse(sean.robot.rotation);
            if (AdjustXRot != 0 || AdjustYRot != 0 || AdjustZRot != 0) {
                Vector3 angles = BaseToFrame.transform.rotation.eulerAngles;
                angles[0] += AdjustXRot;
                angles[1] += AdjustYRot;
                angles[2] += AdjustZRot;
                BaseToFrame.transform.rotation = Quaternion.Euler(angles);
            }
            baseToFramePoseStamped.pose.position = Util.Geometry.GetGeometryPoint(BaseToFrame.transform.position.To<FLU>());
            baseToFramePoseStamped.pose.orientation = Util.Geometry.GetGeometryQuaternion(BaseToFrame.transform.rotation.To<FLU>());
            PublishIfNew(new NamedTransform(BaseToFrame.name, baseToFramePoseStamped));
        }
    }
}
