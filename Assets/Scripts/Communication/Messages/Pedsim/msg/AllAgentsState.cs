/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */

using Newtonsoft.Json;

using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.MessageTypes.Pedsim;

namespace RosSharp.RosBridgeClient.MessageTypes.Pedsim
{
    public class AllAgentsState : Message
    {
        [JsonIgnore]
        public const string RosMessageName = "pedsim_msgs/AllAgentsState";

        public Header header;
        public AgentState[] agent_states;

        public AllAgentsState()
        {
            this.header = new Header();
            this.agent_states = new AgentState[0];
        }

        public AllAgentsState(Header header, AgentState[] agent_states)
        {
            this.header = header;
            this.agent_states = agent_states;
        }
    }
}
