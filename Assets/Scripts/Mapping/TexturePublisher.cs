using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TexturePublisher : UnityPublisher<MessageTypes.Sensor.CompressedImage>
    {
        // image message
        public MessageTypes.Sensor.CompressedImage message;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        // Update is called once per frame
        void Update()
        {
            message.header.Update();
            Publish(message);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.CompressedImage();
            message.format = "jpeg";
        }

    }
}