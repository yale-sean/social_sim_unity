//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Gizmo
{
    public class MMqttMessage : Message
    {
        public const string RosMessageName = "gizmo_msgs/MqttMessage";

        public string topic;
        public string payload;

        public MMqttMessage()
        {
            this.topic = "";
            this.payload = "";
        }

        public MMqttMessage(string topic, string payload)
        {
            this.topic = topic;
            this.payload = payload;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.Add(SerializeString(this.topic));
            listOfSerializations.Add(SerializeString(this.payload));

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            var topicStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.topic = DeserializeString(data, offset, topicStringBytesLength);
            offset += topicStringBytesLength;
            var payloadStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.payload = DeserializeString(data, offset, payloadStringBytesLength);
            offset += payloadStringBytesLength;

            return offset;
        }

        public override string ToString()
        {
            return "MMqttMessage: " +
            "\ntopic: " + topic.ToString() +
            "\npayload: " + payload.ToString();
        }
    }
}
