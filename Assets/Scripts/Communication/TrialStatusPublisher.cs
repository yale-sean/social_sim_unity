﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TrialStatusPublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        public Transform robotTransform;
        public GameObject targetObject;
        public TrialAgentManager pedSpawner;
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
                } else if (timeElapsed >= timeLimit)
                {
                    trialInfo.UpdateInfo(distTarget, distPed,
                        numPeopleCollisions, numObjectCollisions, false, timeElapsed);
                    SetRunning(false);
                }

                // rotate the target
                targetObject.transform.Rotate(0.0f, 2.0f, 0.0f, Space.World);

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
                               List<Vector3> peoplePositions, List<Quaternion> peopleRotations,
                               double timeLimit)
        {
            if (isRunning)
                return;
            robotToNavmesh(robotPosition, robotRotation);
            targetToNavmesh(targetPosition, targetRotation);

            pedSpawner.GenerateAgents(peoplePositions, peopleRotations);

            SetRunning(true);
            startTime = Time.realtimeSinceStartup;
            this.timeLimit = timeLimit;
            Publish(message);
            trialCount++;
            Debug.Log("Started Trial #" + trialCount);
        }

        private Vector3 sampleNavmesh(Vector3 position, int maxDistance = 10) {
            UnityEngine.AI.NavMeshHit hit;
            UnityEngine.AI.NavMesh.SamplePosition(position, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas);
            return hit.position;
        }

        /// move the robot to the navmesh position closest to the requested robot position
        private void robotToNavmesh(Vector3 robotPosition, Quaternion robotRotation) {
            robotTransform.position = sampleNavmesh(robotPosition);
            // ensure the robot is always oriented upright
            Vector3 rot = robotRotation.eulerAngles;
            rot = new Vector3(0,rot.y,0);
            robotTransform.rotation = Quaternion.Euler(rot);
        }

        /// move the target (goal position) to the navmesh position closest to the requested position
        private void targetToNavmesh(Vector3 targetPosition, Quaternion targetRotation) {
            targetObject.transform.position = sampleNavmesh(targetPosition);
            //targetObject.transform.rotation = targetRotation;
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