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
    [AddComponentMenu("GOAP/Movement/Seek")]
    public class Seek : NavMeshMovement
    {
        [Tooltip("The GameObject that the agent is seeking")]
        public GameObject target;
        [Tooltip("If target is null then use the target position")]
        public Vector3 targetPosition;

        private void Reset()
        {
            target = null;
            targetPosition = Vector3.zero;

            SetPrecondition("HasTarget", true);

            SetEffect("InXXXRange", true);
        }

        public override bool IsUsable()
        {
            return !Agent.GetState("HasTarget");
        }

        public override void OnPrePerform()
        {
            base.OnPrePerform();
            Agent.Memory.TryGet("Target", out target);
            SetDestination(Target());
        }

        // Seek the destination. Return success once the agent has reached the destination.
        // Return running if the agent hasn't reached the destination yet
        public override GOAPActionStatus OnPerform()
        {
            if (HasArrived())
                return GOAPActionStatus.Success;
            SetDestination(Target());
            return  GOAPActionStatus.Running;
        }

        // Return targetPosition if target is null
        private Vector3 Target()
        {
            if (target != null)
                return target.transform.position;
            return targetPosition;
        }
    }
}