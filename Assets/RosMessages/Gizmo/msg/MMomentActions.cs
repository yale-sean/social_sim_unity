//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Gizmo
{
    public class MMomentActions : Message
    {
        public const string RosMessageName = "gizmo_msgs/MomentActions";

        public MMomentAction[] actions;

        public MMomentActions()
        {
            this.actions = new MMomentAction[0];
        }

        public MMomentActions(MMomentAction[] actions)
        {
            this.actions = actions;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();

            listOfSerializations.Add(BitConverter.GetBytes(actions.Length));
            foreach (var entry in actions)
                listOfSerializations.Add(entry.Serialize());

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {

            var actionsArrayLength = DeserializeLength(data, offset);
            offset += 4;
            this.actions = new MMomentAction[actionsArrayLength];
            for (var i = 0; i < actionsArrayLength; i++)
            {
                this.actions[i] = new MMomentAction();
                offset = this.actions[i].Deserialize(data, offset);
            }

            return offset;
        }

        public override string ToString()
        {
            return "MMomentActions: " +
            "\nactions: " + System.String.Join(", ", actions.ToList());
        }
    }
}