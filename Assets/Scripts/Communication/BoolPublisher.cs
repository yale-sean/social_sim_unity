using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class BoolPublisher : UnityPublisher<MessageTypes.Std.Bool>
	{
	    private MessageTypes.Std.Bool message;
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
            message = new MessageTypes.Std.Bool();
        }

        public void UpdateInfo(bool data)
        {
            message.data = data;
        	isInfoUpdated = true;
        }
	}
}