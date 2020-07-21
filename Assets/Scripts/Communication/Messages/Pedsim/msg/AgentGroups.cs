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
    public class AgentGroups : Message
    {
        [JsonIgnore]
        public const string RosMessageName = "pedsim_msgs/AgentGroups";

        public Header header;
        public AgentGroup[] groups;

        public AgentGroups()
        {
            this.header = new Header();
            this.groups = new AgentGroup[0];
        }

        public AgentGroups(Header header, AgentGroup[] groups)
        {
            this.header = header;
            this.groups = groups;
        }
    }
}
