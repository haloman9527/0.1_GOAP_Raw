using System.Collections.Generic;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPGoal
    {
        /// <summary>
        /// 优先级越高目标就越靠前
        /// </summary>
        float Priority { get; }
        
        IReadOnlyDictionary<string, bool> Preconditions { get; }
        
        /// <summary>
        /// 每次评估计划前，先动态评估优先级，然后把优先级最高的目标作为计划目标
        /// </summary>
        void DynamicEvaluatePriority();
    }
}