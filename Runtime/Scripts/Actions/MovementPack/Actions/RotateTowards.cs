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
    //[NodeTooltip("Rotates towards the specified rotation. The rotation can either be specified by a transform or rotation. If the transform "+
    //                 "is used then the rotation will not be used.")]
    [AddComponentMenu("GOAP/Movement/RotateTowards")]
    public class RotateTowards : GOAPAction
    {
        [Tooltip("Should the 2D version be used?")]
        public bool usePhysics2D;
        [Tooltip("The agent is done rotating when the angle is less than this value")]
        public float rotationEpsilon = 0;
        [Tooltip("The maximum number of angles the agent can rotate in a single tick")]
        public float maxLookAtRotationDelta = 1;
        [Tooltip("Should the rotation only affect the Y axis?")]
        public bool onlyY;
        [Tooltip("The GameObject that the agent is rotating towards")]
        public GameObject target;
        [Tooltip("If target is null then use the target rotation")]
        public Vector3 targetRotation;

        public override GOAPActionStatus OnPerform()
        {
            var rotation = Target();
            // Return a task status of success once we are done rotating
            if (Quaternion.Angle(Agent.transform.rotation, rotation) < rotationEpsilon)
            {
                return GOAPActionStatus.Success;
            }
            // We haven't reached the target yet so keep rotating towards it
            Agent.transform.rotation = Quaternion.RotateTowards(Agent.transform.rotation, rotation, maxLookAtRotationDelta);
            return GOAPActionStatus.Running;
        }

        // Return targetPosition if targetTransform is null
        private Quaternion Target()
        {
            if (target == null || target == null)
            {
                return Quaternion.Euler(targetRotation);
            }
            var position = target.transform.position - Agent.transform.position;
            if (onlyY)
            {
                position.y = 0;
            }
            if (usePhysics2D)
            {
                var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
                return Quaternion.AngleAxis(angle, Vector3.forward);
            }
            return Quaternion.LookRotation(position);
        }
    }
}