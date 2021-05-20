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
using System;

namespace CZToolKit.GOAP_Raw
{
    [Serializable]
    public class GOAPGoal : GOAPState
    {
        public bool Once;
        public float Priority;
    }
}