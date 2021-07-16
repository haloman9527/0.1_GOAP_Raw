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
using UnityEngine;

namespace CZToolKit.GOAP_Raw.Actions.Movement
{
    //[NodeTooltip("Move towards the specified position. The position can either be specified by a transform or position. If the transform " +
    //                 "is used then the position will not be used.")]
    [AddComponentMenu("GOAP/Movement/MoveTowards")]
    public class MoveTowards : GOAPAction
    {
        [Tooltip("The speed of the agent")]
        public float speed;
        [Tooltip("The agent has arrived when the magnitude is less than this value")]
        public float arriveDistance = 0.1f;
        [Tooltip("Should the agent be looking at the target position?")]
        public bool lookAtTarget = true;
        [Tooltip("Max rotation delta if lookAtTarget is enabled")]
        public float maxLookAtRotationDelta;
        [Tooltip("The GameObject that the agent is moving towards")]
        public GameObject target;
        [Tooltip("If target is null then use the target position")]
        public Vector3 targetPosition;


        // Return targetPosition if targetTransform is null
        private Vector3 Target()
        {
            if (target == null || target == null)
            {
                return targetPosition;
            }
            return target.transform.position;
        }

        public override GOAPActionStatus OnPerform()
        {
            var position = Target();
            // Return a task status of success once we've reached the target
            if (Vector3.Magnitude(Agent.transform.position - position) < arriveDistance)
            {
                return GOAPActionStatus.Success;
            }
            // We haven't reached the target yet so keep moving towards it
            Agent.transform.position = Vector3.MoveTowards(Agent.transform.position, position, speed * Time.deltaTime);
            if (lookAtTarget && (position - Agent.transform.position).sqrMagnitude > 0.01f)
            {
                Agent.transform.rotation = Quaternion.RotateTowards(Agent.transform.rotation, Quaternion.LookRotation(position - Agent.transform.position), maxLookAtRotationDelta);
            }
            return GOAPActionStatus.Running;
        }
    }
}