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
    //[NodeTooltip("寻找一个掩体躲起来，使用NavMesh移动")]
    [AddComponentMenu("GOAP/Movement/Cover")]
    public class Cover : NavMeshMovement
    {
        [Tooltip("The distance to search for cover")]
        public float maxCoverDistance = 1000;
        [Tooltip("The layermask of the available cover positions")]
        public LayerMask availableLayerCovers;
        [Tooltip("The maximum number of raycasts that should be fired before the agent gives up looking for an agent to find cover behind")]
        public int maxRaycasts = 100;
        [Tooltip("How large the step should be between raycasts")]
        public float rayStep = 1;
        [Tooltip("Once a cover point has been found, multiply this offset by the normal to prevent the agent from hugging the wall")]
        public float coverOffset = 2;
        [Tooltip("Should the agent look at the cover point after it has arrived?")]
        public bool lookAtCoverPoint = false;
        [Tooltip("The agent is done rotating to the cover point when the square magnitude is less than this value")]
        public float rotationEpsilon = 0.5f;
        [Tooltip("Max rotation delta if lookAtCoverPoint")]
        public float maxLookAtRotationDelta;

        private Vector3 coverPoint;
        // The position to reach, offsetted from coverPoint
        private Vector3 coverTarget;
        // Was cover found?
        private bool foundCover;

        public override void OnPrePerform()
        {
            RaycastHit hit;
            int raycastCount = 0;
            var direction = Agent.transform.forward;
            float step = 0;
            coverTarget = Agent.transform.position;
            foundCover = false;
            // Keep firing a ray until too many rays have been fired
            while (raycastCount < maxRaycasts)
            {
                var ray = new Ray(Agent.transform.position, direction);
                if (Physics.Raycast(ray, out hit, maxCoverDistance, availableLayerCovers.value))
                {
                    // A suitable agent has been found. Find the opposite side of that agent by shooting a ray in the opposite direction from a point far away
                    if (hit.collider.Raycast(new Ray(hit.point - hit.normal * maxCoverDistance, hit.normal), out hit, Mathf.Infinity))
                    {
                        coverPoint = hit.point;
                        coverTarget = hit.point + hit.normal * coverOffset;
                        foundCover = true;
                        break;
                    }
                }
                // Keep sweeiping along the y axis
                step += rayStep;
                direction = Quaternion.Euler(0, Agent.transform.eulerAngles.y + step, 0) * Vector3.forward;
                raycastCount++;
            }

            if (foundCover)
                SetDestination(coverTarget);

            base.OnPrePerform();
        }

        public override GOAPActionStatus OnPerform()
        {
            if (!foundCover) return GOAPActionStatus.Failure;
            if (HasArrived())
            {
                var rotation = Quaternion.LookRotation(coverPoint - Agent.transform.position);
                // Return success if the agent isn't going to look at the cover point or it has completely rotated to look at the cover point
                if (!lookAtCoverPoint || Quaternion.Angle(Agent.transform.rotation, rotation) < rotationEpsilon)
                {
                    return GOAPActionStatus.Success;
                }
                else
                {
                    // Still needs to rotate towards the target
                    Agent.transform.rotation = Quaternion.RotateTowards(Agent.transform.rotation, rotation, maxLookAtRotationDelta);
                }
            }
            return GOAPActionStatus.Running;
        }
    }
}