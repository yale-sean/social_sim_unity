//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.Sensor
{
    public class MIlluminance : Message
    {
        public const string RosMessageName = "sensor_msgs/Illuminance";

        //  Single photometric illuminance measurement.  Light should be assumed to be
        //  measured along the sensor's x-axis (the area of detection is the y-z plane).
        //  The illuminance should have a 0 or positive value and be received with
        //  the sensor's +X axis pointing toward the light source.
        //  Photometric illuminance is the measure of the human eye's sensitivity of the
        //  intensity of light encountering or passing through a surface.
        //  All other Photometric and Radiometric measurements should
        //  not use this message.
        //  This message cannot represent:
        //  Luminous intensity (candela/light source output)
        //  Luminance (nits/light output per area)
        //  Irradiance (watt/area), etc.
        public MHeader header;
        //  timestamp is the time the illuminance was measured
        //  frame_id is the location and direction of the reading
        public double illuminance;
        //  Measurement of the Photometric Illuminance in Lux.
        public double variance;
        //  0 is interpreted as variance unknown

        public MIlluminance()
        {
            this.header = new MHeader();
            this.illuminance = 0.0;
            this.variance = 0.0;
        }

        public MIlluminance(MHeader header, double illuminance, double variance)
        {
            this.header = header;
            this.illuminance = illuminance;
            this.variance = variance;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.AddRange(header.SerializationStatements());
            listOfSerializations.Add(BitConverter.GetBytes(this.illuminance));
            listOfSerializations.Add(BitConverter.GetBytes(this.variance));

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            offset = this.header.Deserialize(data, offset);
            this.illuminance = BitConverter.ToDouble(data, offset);
            offset += 8;
            this.variance = BitConverter.ToDouble(data, offset);
            offset += 8;

            return offset;
        }

        public override string ToString()
        {
            return "MIlluminance: " +
            "\nheader: " + header.ToString() +
            "\nilluminance: " + illuminance.ToString() +
            "\nvariance: " + variance.ToString();
        }
    }
}
