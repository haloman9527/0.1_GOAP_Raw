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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAction
    {
        string ActionName { get; }

        bool IsUsable();

        void DynamicallyEvaluateCost();

        bool IsProceduralPrecondition(Dictionary<string, bool> currentState);

        void OnPrePerform();

        GOAPActionStatus OnPerform();

        void OnPostPerform(bool _successed);
    }
}