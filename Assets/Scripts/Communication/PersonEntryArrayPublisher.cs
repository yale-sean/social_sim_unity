using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.MessageTypes.SocialSimRos;

namespace RosSharp.RosBridgeClient
{
    public class PersonEntryArrayPublisher : UnityPublisher<MessageTypes.SocialSimRos.PersonEntryArray>
    {
        public string FrameId = "Unity";

        private MessageTypes.SocialSimRos.PersonEntryArray message;
        private ulong numPersons;
        private GameObject[] actors;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new PersonEntryArray
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        private void InitializePeopleArray(ulong n)
        {
            message.people = new PersonEntry[n];
            for (ulong i = 0; i < n; i++)
            {
                message.people[i] = new PersonEntry
                {
                    header = new MessageTypes.Std.Header()
                    {
                        frame_id = FrameId
                    }
                };
            }
        }

        private void UpdateMessage()
        {
            actors = GameObject.FindGameObjectsWithTag("Actor");
            numPersons = (ulong) actors.Length;
            //Debug.Log(actors.Length);
            InitializePeopleArray(numPersons);
            message.header.Update();
            for (ulong i = 0; i < numPersons; i++)
            {
                message.people[i].header.Update();
                message.people[i].track_id = ulong.Parse(actors[i].name);
                message.people[i].pose.position = GetGeometryPoint(actors[i].transform.position.Unity2Ros());
                message.people[i].pose.orientation = GetGeometryQuaternion(actors[i].transform.rotation.Unity2Ros());
            }
            Publish(message);
        }

        private MessageTypes.Geometry.Point GetGeometryPoint(Vector3 position)
        {
            MessageTypes.Geometry.Point geometryPoint = new MessageTypes.Geometry.Point();
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
            return geometryPoint;
        }

        private MessageTypes.Geometry.Quaternion GetGeometryQuaternion(Quaternion quaternion)
        {
            MessageTypes.Geometry.Quaternion geometryQuaternion = new MessageTypes.Geometry.Quaternion();
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
            return geometryQuaternion;
        }
    }
}