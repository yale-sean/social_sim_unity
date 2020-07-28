using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TrialStatusPublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        public Transform robotTransform;
        public GameObject targetObject;
        public AgentManager pedSpawner;
        public float reachedDestinationThreshold = 0.5f;
        public bool isRunning;

        private MessageTypes.Std.Bool message;
        private double startTime;
        private double timeLimit;
        private double timeElapsed;
        private double distTarget;
        private double distPed;
        private uint numPeopleCollisions;
        private uint numObjectCollisions;
        private TrialInfoPublisher trialInfo;

        private int trialCount = 0;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
            trialInfo = GetComponent<TrialInfoPublisher>();
        }

        private void Update()
        {
            if (isRunning)
            {
                targetObject.SetActive(true);
                distTarget = Vector3.Distance(robotTransform.position, targetObject.transform.position);
                distPed = Math.Min(distPed, DistanceToNearestPedestrian());
                timeElapsed = Time.realtimeSinceStartup - startTime;

                if (distTarget < reachedDestinationThreshold)
                {
                    trialInfo.UpdateInfo(distTarget, distPed,
                        numPeopleCollisions, numObjectCollisions, true, timeElapsed);
                    SetRunning(false);
                }
                else if (timeElapsed >= timeLimit)
                {
                    trialInfo.UpdateInfo(distTarget, distPed,
                        numPeopleCollisions, numObjectCollisions, false, timeElapsed);
                    SetRunning(false);
                }
            }
            Publish(message);
        }

        public void IncrementPeopleCollisions()
        {
            numPeopleCollisions++;
        }

        public void IncrementObjectCollisions()
        {
            numObjectCollisions++;
        }

        public uint GetPeopleCollisions()
        {
            return numPeopleCollisions;
        }

        public uint GetObjectCollisions()
        {
            return numObjectCollisions;
        }

        private double DistanceToNearestPedestrian()
        {
            GameObject[] actors = GameObject.FindGameObjectsWithTag("Actor");
            double minDist = Double.MaxValue;
            foreach (GameObject actor in actors)
            {
                minDist = Math.Min(minDist, Vector3.Distance(robotTransform.position, actor.transform.position));
            }
            return minDist;
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Bool(false);
        }

        public void StartTrial(Vector3 robotPosition, Quaternion robotRotation,
                               Vector3 targetPosition, Quaternion targetRotation,
                               uint numPeds, double timeLimit)
        {
            if (isRunning)
                return;
            robotTransform.position = robotPosition;
            robotTransform.rotation = robotRotation;
            targetObject.transform.position = targetPosition;
            targetObject.transform.rotation = targetRotation;
            pedSpawner.agentCount = (int) numPeds;
            pedSpawner.GenerateAgents();

            SetRunning(true);
            startTime = Time.realtimeSinceStartup;
            this.timeLimit = timeLimit;
            Publish(message);
            trialCount++;
            Debug.Log("Started Trial #" + trialCount);
        }

        private void SetRunning(bool isRunning)
        {
            this.isRunning = isRunning;
            message.data = isRunning;
            if (isRunning == false)
            {
                targetObject.SetActive(false);
                pedSpawner.DestroyAll();
                distPed = Double.MaxValue;
                numPeopleCollisions = 0;
                numObjectCollisions = 0;
            }
        }

    }
}