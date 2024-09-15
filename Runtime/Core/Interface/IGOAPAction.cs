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
 *  Blog: https://www.haloman.net/
 *
 */
#endregion

using System.Collections.Generic;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAction
    {
        float Cost { get; }
        
        /// <summary>
        /// 行为的静态前提条件
        /// </summary>
        IReadOnlyDictionary<string, bool> Preconditions { get; }
        
        /// <summary>
        /// 行为所能达成的效果
        /// </summary>
        IReadOnlyDictionary<string, bool> Effects { get; }
        
        /// <summary> 行为与范围绑定 </summary>
        /// <returns></returns>
        bool IsRequiredRange { get; }
        
        /// <summary> 行为是否可用，不可用则不匹配计划 </summary>
        /// <returns></returns>
        bool IsUsable();
        
        /// <summary> 规划计划前先评估成本 </summary>
        /// <returns></returns>
        void EvaluateCost();

        /// <summary> 行为此时是否可用，不可用则计划失败 </summary>
        /// <returns></returns>
        bool IsDynamicUseable();
        
        /// <summary> 已经处于范围内 </summary>
        /// <returns></returns>
        bool IsInRange();
        
        void Enter();

        void Exit();

        GOAPActionStatus Perform();
    }
}