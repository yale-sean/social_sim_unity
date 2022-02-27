//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Diagnostic
{
    public class MDiagnosticStatus : Message
    {
        public const string RosMessageName = "diagnostic_msgs/DiagnosticStatus";

        //  This message holds the status of an individual component of the robot.
        //  
        //  Possible levels of operations
        public const sbyte OK = 0;
        public const sbyte WARN = 1;
        public const sbyte ERROR = 2;
        public const sbyte STALE = 3;
        public sbyte level;
        //  level of operation enumerated above 
        public string name;
        //  a description of the test/component reporting
        public string message;
        //  a description of the status
        public string hardware_id;
        //  a hardware unique string
        public MKeyValue[] values;
        //  an array of values associated with the status

        public MDiagnosticStatus()
        {
            this.level = 0;
            this.name = "";
            this.message = "";
            this.hardware_id = "";
            this.values = new MKeyValue[0];
        }

        public MDiagnosticStatus(sbyte level, string name, string message, string hardware_id, MKeyValue[] values)
        {
            this.level = level;
            this.name = name;
            this.message = message;
            this.hardware_id = hardware_id;
            this.values = values;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.Add(new[] { (byte)this.level });
            listOfSerializations.Add(SerializeString(this.name));
            listOfSerializations.Add(SerializeString(this.message));
            listOfSerializations.Add(SerializeString(this.hardware_id));

            listOfSerializations.Add(BitConverter.GetBytes(values.Length));
            foreach (var entry in values)
                listOfSerializations.Add(entry.Serialize());

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            this.level = (sbyte)data[offset]; ;
            offset += 1;
            var nameStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.name = DeserializeString(data, offset, nameStringBytesLength);
            offset += nameStringBytesLength;
            var messageStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.message = DeserializeString(data, offset, messageStringBytesLength);
            offset += messageStringBytesLength;
            var hardware_idStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.hardware_id = DeserializeString(data, offset, hardware_idStringBytesLength);
            offset += hardware_idStringBytesLength;

            var valuesArrayLength = DeserializeLength(data, offset);
            offset += 4;
            this.values = new MKeyValue[valuesArrayLength];
            for (var i = 0; i < valuesArrayLength; i++)
            {
                this.values[i] = new MKeyValue();
                offset = this.values[i].Deserialize(data, offset);
            }

            return offset;
        }

        public override string ToString()
        {
            return "MDiagnosticStatus: " +
            "\nlevel: " + level.ToString() +
            "\nname: " + name.ToString() +
            "\nmessage: " + message.ToString() +
            "\nhardware_id: " + hardware_id.ToString() +
            "\nvalues: " + System.String.Join(", ", values.ToList());
        }
    }
}
