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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using UnityEngine;

namespace CZToolKit.GOAP_Raw.Actions.Movement
{
    public abstract class GroupMovement : GOAPAction
    {
        protected abstract bool SetDestination(int index, Vector3 target);

        protected abstract Vector3 Velocity(int index);
    }
}