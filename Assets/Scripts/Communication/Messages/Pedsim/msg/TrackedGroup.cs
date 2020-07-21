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
using RosSharp.RosBridgeClient.MessageTypes.Geometry;

namespace RosSharp.RosBridgeClient.MessageTypes.Pedsim
{
    public class TrackedGroup : Message
    {
        [JsonIgnore]
        public const string RosMessageName = "pedsim_msgs/TrackedGroup";

        //  Message defining a tracked group
        // 
        public ulong group_id;
        //  unique identifier of the target, consistent over time
        public Duration age;
        //  age of the group
        public PoseWithCovariance centerOfGravity;
        //  mean and covariance of the group (calculated from its person tracks)
        public ulong[] track_ids;
        //  IDs of the tracked persons in this group. See srl_tracking_msgs/TrackedPersons

        public TrackedGroup()
        {
            this.group_id = 0;
            this.age = new Duration();
            this.centerOfGravity = new PoseWithCovariance();
            this.track_ids = new ulong[0];
        }

        public TrackedGroup(ulong group_id, Duration age, PoseWithCovariance centerOfGravity, ulong[] track_ids)
        {
            this.group_id = group_id;
            this.age = age;
            this.centerOfGravity = centerOfGravity;
            this.track_ids = track_ids;
        }
    }
}
