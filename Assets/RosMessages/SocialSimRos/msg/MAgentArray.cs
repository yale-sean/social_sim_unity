//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.SocialSimRos
{
    public class MAgentArray : Message
    {
        public const string RosMessageName = "social_sim_ros/AgentArray";

        //  Message defining an array of all agent entries
        public MHeader header;
        //  Age of the track
        public MAgent[] agents;
        //  Array containing the entries for the N agents in the current environment

        public MAgentArray()
        {
            this.header = new MHeader();
            this.agents = new MAgent[0];
        }

        public MAgentArray(MHeader header, MAgent[] agents)
        {
            this.header = header;
            this.agents = agents;
        }
        public override List<byte[]> SerializationStatements()
        {
            var listOfSerializations = new List<byte[]>();
            listOfSerializations.AddRange(header.SerializationStatements());
            
            listOfSerializations.Add(BitConverter.GetBytes(agents.Length));
            foreach(var entry in agents)
                listOfSerializations.Add(entry.Serialize());

            return listOfSerializations;
        }

        public override int Deserialize(byte[] data, int offset)
        {
            offset = this.header.Deserialize(data, offset);
            
            var agentsArrayLength = DeserializeLength(data, offset);
            offset += 4;
            this.agents= new MAgent[agentsArrayLength];
            for(var i = 0; i < agentsArrayLength; i++)
            {
                this.agents[i] = new MAgent();
                offset = this.agents[i].Deserialize(data, offset);
            }

            return offset;
        }

        public override string ToString()
        {
            return "MAgentArray: " +
            "\nheader: " + header.ToString() +
            "\nagents: " + System.String.Join(", ", agents.ToList());
        }
    }
}