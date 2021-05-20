#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEngine;

namespace CZToolKit.GOAP_Raw.Actions.Movement
{
    //[NodeTooltip("Search for a target by combining the wander, within hearing range, and the within seeing range tasks using the Unity NavMesh.")]
    [AddComponentMenu("GOAP/Movement/Search")]
    public class Search : NavMeshMovement
    {
        [Tooltip("Minimum distance ahead of the current position to look ahead for a destination")]
        public float minWanderDistance = 20;
        [Tooltip("Maximum distance ahead of the current position to look ahead for a destination")]
        public float maxWanderDistance = 20;
        [Tooltip("The amount that the agent rotates direction")]
        public float wanderRate = 1;
        [Tooltip("The minimum length of time that the agent should pause at each destination")]
        public float minPauseDuration = 0;
        [Tooltip("The maximum length of time that the agent should pause at each destination (zero to disable)")]
        public float maxPauseDuration = 0;
        [Tooltip("The maximum number of retries per tick (set higher if using a slow tick time)")]
        public int targetRetries = 1;
        [Tooltip("The field of view angle of the agent (in degrees)")]
        public float fieldOfViewAngle = 90;
        [Tooltip("The distance that the agent can see")]
        public float viewDistance = 30;
        [Tooltip("The LayerMask of the objects to ignore when performing the line of sight check")]
        public LayerMask ignoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        [Tooltip("Should the search end if audio was heard?")]
        public bool senseAudio = true;
        [Tooltip("How far away the unit can hear")]
        public float hearingRadius = 30;
        [Tooltip("The raycast offset relative to the pivot position")]
        public Vector3 offset;
        [Tooltip("The target raycast offset relative to the pivot position")]
        public Vector3 targetOffset;
        [Tooltip("The LayerMask of the objects that we are searching for")]
        public LayerMask objectLayerMask;
        [Tooltip("Specifies the maximum number of colliders that the physics cast can collide with")]
        public int maxCollisionCount = 200;
        [Tooltip("Should the target bone be used?")]
        public bool useTargetBone;
        [Tooltip("The target's bone if the target is a humanoid")]
        public HumanBodyBones targetBone;
        [Tooltip("Should a debug look ray be drawn to the scene view?")]
        public bool drawDebugRay;
        [Tooltip("The further away a sound source is the less likely the agent will be able to hear it. " +
                 "Set a threshold for the the minimum audibility level that the agent can hear")]
        public float audibilityThreshold = 0.05f;
        [Tooltip("The object that is found")]
        public GameObject returnedObject;

        private float pauseTime;
        private float destinationReachTime;

        private Collider[] overlapColliders;
        public override GOAPActionStatus OnPerform()
        {
            if (HasArrived())
            {
                // The agent should pause at the destination only if the max pause duration is greater than 0
                if (maxPauseDuration > 0)
                {
                    if (destinationReachTime == -1)
                    {
                        destinationReachTime = Time.time;
                        pauseTime = Random.Range(minPauseDuration, maxPauseDuration);
                    }
                    if (destinationReachTime + pauseTime <= Time.time)
                    {
                        // Only reset the time if a destination has been set.
                        if (TrySetTarget())
                        {
                            destinationReachTime = -1;
                        }
                    }
                }
                else
                {
                    TrySetTarget();
                }
            }

            // Detect if any objects are within sight
            if (overlapColliders == null)
            {
                overlapColliders = new Collider[maxCollisionCount];
            }
            returnedObject = MovementUtility.WithinSight(Agent.transform, offset, fieldOfViewAngle, viewDistance, overlapColliders, objectLayerMask, targetOffset, ignoreLayerMask, useTargetBone, targetBone, drawDebugRay);
            // If an object was seen then return success
            if (returnedObject != null)
            {
                return GOAPActionStatus.Success;
            }
            // Detect if any object are within audio range (if enabled)
            if (senseAudio)
            {
                returnedObject = MovementUtility.WithinHearingRange(Agent.transform, offset, audibilityThreshold, hearingRadius, overlapColliders, objectLayerMask);
                // If an object was heard then return success
                if (returnedObject != null)
                {
                    return GOAPActionStatus.Success;
                }
            }

            // No object has been seen or heard so keep searching
            return GOAPActionStatus.Running;
        }

        private bool TrySetTarget()
        {
            var direction = Agent.transform.forward;
            var validDestination = false;
            var attempts = targetRetries;
            var destination = Agent.transform.position;
            while (!validDestination && attempts > 0)
            {
                direction = direction + Random.insideUnitSphere * wanderRate;
                destination = Agent.transform.position + direction.normalized * Random.Range(minWanderDistance, maxWanderDistance);
                validDestination = SamplePosition(destination);
                attempts--;
            }
            if (validDestination)
            {
                SetDestination(destination);
            }
            return validDestination;
        }

    }
}