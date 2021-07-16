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
    //[NodeTooltip("跟随一个目标，使用NavMesh移动")]
    [AddComponentMenu("GOAP/Movement/Follow")]
    public class Follow : NavMeshMovement
    {
        [Tooltip("The GameObject that the agent is following")]
        public GameObject target;
        [Tooltip("Start moving towards the target if the target is further than the specified distance")]
        public float moveDistance = 2;

        private Vector3 lastTargetPosition;
        private bool hasMoved;

        private void Reset()
        {
            ActionName = "跟随";
        }

        public override void OnPrePerform()
        {
            base.OnPrePerform();
            if (target == null)
            {
                return;
            }

            lastTargetPosition = target.transform.position + Vector3.one * (moveDistance + 1);
            hasMoved = false;
        }

        public override GOAPActionStatus OnPerform()
        {
            if (target == null)
                return GOAPActionStatus.Failure;

            // Move if the target has moved more than the moveDistance since the last time the agent moved.
            var targetPosition = target.transform.position;
            if ((targetPosition - lastTargetPosition).magnitude >= moveDistance)
            {
                SetDestination(targetPosition);
                lastTargetPosition = targetPosition;
                hasMoved = true;
            }
            else
            {
                // Stop moving if the agent is within the moveDistance of the target.
                if (hasMoved && (targetPosition - Agent.transform.position).magnitude < moveDistance)
                {
                    Stop();
                    hasMoved = false;
                    lastTargetPosition = targetPosition;
                }
            }

            return GOAPActionStatus.Running;
        }
    }
}