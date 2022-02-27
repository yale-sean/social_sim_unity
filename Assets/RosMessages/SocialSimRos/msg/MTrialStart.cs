//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.SocialSimRos
{
    public class MTrialStart : Message
    {
        public const string RosMessageName = "social_sim_ros/TrialStart";

        //  Message containing the parameters to start an A-B navigation trial
        public MHeader header;
        public string trial_name;
        //  Which trial name are we running
        public ushort trial_number;
        //  Which trial number are we running
        public Geometry.MPose spawn;
        //  Robot spawn position
        public Geometry.MPose target;
        //  Robot target position
        public Geometry.MPoseArray people;
        //  People spawn positions
        public double time_limit;
        //  Time limit for the trial (in seconds)

        public MTrialStart()
        {
            this.header = new MHeader();
            this.trial_name = "";
            this.trial_number = 0;
            this.spawn = new Geometry.MPose();
            this.target = new Geometry.MPose();
            this.people = new Geometry.MPoseArray();
            this.time_limit = 0.0;
        }

        public MTrialStart(MHeader header, string trial_name, ushort trial_number, Geometry.MPose spawn, Geometry.MPose target, Geometry.MPoseArray people, double time_limit)
        {
            this.header = header;
            this.trial_name = trial_name;
            this.trial_number = trial_number;
            this.spawn = spawn;
            this.target = target;
            this.people = people;
            this.time_limit = time_limit;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.AddRange(header.SerializationStatements());
            listOfSerializations.Add(SerializeString(this.trial_name));
            listOfSerializations.Add(BitConverter.GetBytes(this.trial_number));
            listOfSerializations.AddRange(spawn.SerializationStatements());
            listOfSerializations.AddRange(target.SerializationStatements());
            listOfSerializations.AddRange(people.SerializationStatements());
            listOfSerializations.Add(BitConverter.GetBytes(this.time_limit));

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            offset = this.header.Deserialize(data, offset);
            var trial_nameStringBytesLength = DeserializeLength(data, offset);
            offset += 4;
            this.trial_name = DeserializeString(data, offset, trial_nameStringBytesLength);
            offset += trial_nameStringBytesLength;
            this.trial_number = BitConverter.ToUInt16(data, offset);
            offset += 2;
            offset = this.spawn.Deserialize(data, offset);
            offset = this.target.Deserialize(data, offset);
            offset = this.people.Deserialize(data, offset);
            this.time_limit = BitConverter.ToDouble(data, offset);
            offset += 8;

            return offset;
        }

        public override string ToString()
        {
            return "MTrialStart: " +
            "\nheader: " + header.ToString() +
            "\ntrial_name: " + trial_name.ToString() +
            "\ntrial_number: " + trial_number.ToString() +
            "\nspawn: " + spawn.ToString() +
            "\ntarget: " + target.ToString() +
            "\npeople: " + people.ToString() +
            "\ntime_limit: " + time_limit.ToString();
        }
    }
}
