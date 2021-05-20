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
    [AddComponentMenu("GOAP/Movement/Wander")]
    public class Wander : NavMeshMovement
    {
        [Tooltip("Minimum distance ahead of the current position to look ahead for a destination")]
        public float minWanderDistance = 20;
        [Tooltip("Maximum distance ahead of the current position to look ahead for a destination")]
        public float maxWanderDistance = 20;
        [Tooltip("The amount that the agent rotates direction")]
        public float wanderRate = 2;
        [Tooltip("The minimum length of time that the agent should pause at each destination")]
        public float minPauseDuration = 0;
        [Tooltip("The maximum length of time that the agent should pause at each destination (zero to disable)")]
        public float maxPauseDuration = 0;
        [Tooltip("The maximum number of retries per tick (set higher if using a slow tick time)")]
        public float targetRetries = 1;

        private float pauseTime;
        private float destinationReachTime;

        private void Reset()
        {
            ActionName = "徘徊";
        }

        // There is no success or fail state with wander - the agent will just keep wandering
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