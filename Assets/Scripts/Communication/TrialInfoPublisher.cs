using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
	public class TrialInfoPublisher : UnityPublisher<MessageTypes.SocialSimRos.TrialInfo>
	{
		public string FrameId = "Unity";

	    private MessageTypes.SocialSimRos.TrialInfo message;
	    private bool isInfoUpdated;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void Update()
        {
        	if (isInfoUpdated)
            	Publish(message);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.SocialSimRos.TrialInfo();
        }

        public void UpdateInfo(double distTarget, double distPed,
        	uint numPeopleCollisions, uint numObjectCollisions, bool isComplete, double timeElapsed)
        {
        	message.header.Update();
        	message.dist_to_target = distTarget;
        	message.dist_to_ped = distPed;
        	message.num_collisions = numPeopleCollisions + numObjectCollisions;
        	message.run_complete = isComplete;
        	message.time_elapsed = timeElapsed;
        	isInfoUpdated = true;
        }
	}
}