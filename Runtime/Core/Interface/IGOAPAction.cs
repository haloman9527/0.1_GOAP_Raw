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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using System.Collections.Generic;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAction
    {
        string Name { get; }

        bool IsUsable();

        void DynamicallyEvaluateCost();

        bool IsProceduralPrecondition(Dictionary<string, bool> currentState);

        void OnBeforePerform();

        GOAPActionStatus OnPerform();

        void OnAfterPerform(bool successed);
    }
}