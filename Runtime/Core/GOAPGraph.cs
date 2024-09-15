using System.Collections.Generic;

namespace CZToolKit.GOAP_Raw
{
    public class GOAPGraph
    {
        private List<GOAPNode> rootNodes = new List<GOAPNode>();
        
        /// <summary> 构建树并返回所有计划 </summary>
        /// <param name="agent"></param>
        /// <param name="goal">目标计划</param>
        /// <param name="maxDepth">构建的树的最大深度</param>
        /// <returns>是否找到计划</returns>
        private GOAPNode BuildGraph(IGOAPAgent agent, IGOAPGoal goal, int maxDepth)
        {
            var root = ObjectPools.Instance.Spawn<GOAPNode>();
            root.Init(null, 0, agent.States, null);
            if (maxDepth < 1)
                return root;
            InnerBuilderGraph(root, 0);
            return root;

            void InnerBuilderGraph(GOAPNode parent, int depth)
            {
                if (depth > maxDepth)
                    return;

                foreach (var action in agent.Actions)
                {
                    if (!GOAPHelper.IsAchieve(parent.state, action.Preconditions))
                    {
                        continue;
                    }

                    // 生成动作完成的节点链，成本累加
                    var node = ObjectPools.Instance.Spawn<GOAPNode>();
                    node.Init(parent, parent.runningCost + action.Cost, parent.state, action);
                    foreach (var effect in action.Effects)
                    {
                        node.state[effect.Key] = effect.Value;
                    }

                    // 如果当前状态不能达成目标，继续构建树
                    if (!GOAPHelper.IsAchieve(node.state, goal.Preconditions))
                    {
                        InnerBuilderGraph(node, ++depth);
                    }
                }
            }
        }
    }
}