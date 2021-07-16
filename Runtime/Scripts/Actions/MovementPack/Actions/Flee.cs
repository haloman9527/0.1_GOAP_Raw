#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#region ע ��
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
    [AddComponentMenu("GOAP/Movement/Flee")]
    public class Flee : NavMeshMovement
    {
        [Tooltip("The agent has fleed when the magnitude is greater than this value")]
        public float fleedDistance = 20;
        [Tooltip("The distance to look ahead when fleeing")]
        public float lookAheadDistance = 5;
        [Tooltip("The GameObject that the agent is fleeing from")]
        public GameObject target;

        private bool hasMoved;
        private void Reset()
        {
            ActionName = "����";
        }

        public override void OnPrePerform()
        {
            base.OnPrePerform();
            hasMoved = false;
            SetDestination(Target());
        }

        // Flee from the target. Return success once the agent has fleed the target by moving far enough away from it
        // Return running if the agent is still fleeing
        public override GOAPActionStatus OnPerform()
        {
            if (Vector3.Magnitude(Agent.transform.position - target.transform.position) > fleedDistance)
            {
                return GOAPActionStatus.Success;
            }

            if (HasArrived())
            {
                if (!hasMoved)
                {
                    return GOAPActionStatus.Failure;
                }
                if (!SetDestination(Target()))
                {
                    return GOAPActionStatus.Failure;
                }
                hasMoved = false;
            }
            else
            {
                // If the agent is stuck the task shouldn't continue to return a status of running.
                var velocityMagnitude = Velocity().sqrMagnitude;
                if (hasMoved && velocityMagnitude <= 0f)
                {
                    return GOAPActionStatus.Failure;
                }
                hasMoved = velocityMagnitude > 0f;
            }

            return GOAPActionStatus.Running;
        }

        // Flee in the opposite direction
        private Vector3 Target()
        {
            return Agent.transform.position + (Agent.transform.position - target.transform.position).normalized * lookAheadDistance;
        }

        // Return false if the position isn't valid on the NavMesh.
        protected override bool SetDestination(Vector3 destination)
        {
            if (!SamplePosition(destination))
            {
                return false;
            }
            return base.SetDestination(destination);
        }
    }
}