// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace SEAN.Util
{
    public class Geometry
    {
        public static RosMessageTypes.Geometry.MVector3 GetGeometryVector3(Vector3<FLU> vector3)
        {
            RosMessageTypes.Geometry.MVector3 geometryVector3 = new RosMessageTypes.Geometry.MVector3();
            geometryVector3.x = vector3.x;
            geometryVector3.y = vector3.y;
            geometryVector3.z = vector3.z;
            return geometryVector3;
        }

        public static RosMessageTypes.Geometry.MPoint GetGeometryPoint(Vector3<FLU> position)
        {
            RosMessageTypes.Geometry.MPoint geometryPoint = new RosMessageTypes.Geometry.MPoint();
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
            return geometryPoint;
        }

        public static RosMessageTypes.Geometry.MQuaternion GetGeometryQuaternion(Quaternion<FLU> quaternion)
        {
            RosMessageTypes.Geometry.MQuaternion geometryQuaternion = new RosMessageTypes.Geometry.MQuaternion();
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
            return geometryQuaternion;
        }

        public static RosMessageTypes.Geometry.MPose GetMPose(GameObject gameObject)
        {
            return GetMPose(new Pose(gameObject.transform.position, gameObject.transform.rotation));
        }

        public static RosMessageTypes.Geometry.MPose GetMPose(Transform transform)
        {
            return GetMPose(new Pose(transform.position, transform.rotation));
        }

        public static RosMessageTypes.Geometry.MPose GetMPose(Pose pose)
        {
            RosMessageTypes.Geometry.MPose mpose = new RosMessageTypes.Geometry.MPose();
            mpose.position = GetGeometryPoint(pose.position.To<FLU>());
            mpose.orientation = GetGeometryQuaternion(pose.rotation.To<FLU>());
            return mpose;
        }

        public static RosMessageTypes.Geometry.MTwist GetMTwist(Scenario.Trajectory.TrackedTrajectory trajectory)
        {
            RosMessageTypes.Geometry.MTwist mtwist = new RosMessageTypes.Geometry.MTwist();
            // LHS ground plane are: x, z but this conversion already occured in the LinearTrajectory class
            mtwist.linear = new RosMessageTypes.Geometry.MVector3(trajectory.vector.x, trajectory.vector.y, 0);
            mtwist.angular = new RosMessageTypes.Geometry.MVector3(0, 0, 0);
            return mtwist;
        }

        public static float GroundPlaneDist(Vector3 a, Vector3 b)
        {
            Vector3 diff = a - b;
            diff.y = 0;
            return diff.magnitude;
        }

        public static Vector3 Tangent(Vector3 a)
        {
            return new Vector3(-a.z, 0, a.x);
        }
    }
}
