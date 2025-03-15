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

namespace Atom.GOAP_Raw
{
    public static class GOAPPlanner
    {
        /// <summary> 定制最优计划 </summary>
        /// <param name="agent"></param>
        /// <param name="goal"> 目标状态，想要达到的状态</param>
        /// <param name="maxDepth"> </param>
        /// <param name="plan"> 返回一个计划 </param>
        public static bool Plan(IGOAPAgent agent, IGOAPGoal goal, int maxDepth, in Queue<IGOAPAction> plan)
        {
            plan.Clear();
            if (GOAPHelper.IsAchieve(agent.States, goal.Preconditions))
            {
                return true;
            }

            var enumrateBuffer = ObjectPoolService.Spawn<List<GOAPNode>>();
            foreach (var action in agent.Actions)
            {
                action.EvaluateCost();
            }

            // 如果通过构建节点树找到了能够达成目标的计划
            var root = BuildGraph(agent, goal, maxDepth);

            // 成本最低的计划节点
            var cheapestNode = (GOAPNode)null;
            root.DFS(enumrateBuffer);
            for (int i = 0; i < enumrateBuffer.Count; i++)
            {
                var node = enumrateBuffer[i];
                if (node.children.Count > 0)
                {
                    continue;
                }

                if (GOAPHelper.IsAchieve(agent.States, goal.Preconditions))
                {
                    if (cheapestNode == null)
                        cheapestNode = node;
                    else if (cheapestNode.runningCost > node.runningCost)
                        cheapestNode = node;
                }
            }

            // 向上遍历并添加行为到栈中，直至根节点，因为从后向前遍历
            var goapActionStack = ObjectPoolService.Spawn<Stack<IGOAPAction>>();
            while (cheapestNode != null && cheapestNode != root)
            {
                goapActionStack.Push(cheapestNode.action);
                cheapestNode = cheapestNode.parent;
            }

            // 再将栈压入到队列中
            while (goapActionStack.Count > 0)
            {
                plan.Enqueue(goapActionStack.Pop());
            }

            // 回收栈
            ObjectPoolService.Recycle(goapActionStack);

            // 回收节点
            for (int i = 0; i < enumrateBuffer.Count; i++)
            {
                var node = enumrateBuffer[i];
                ObjectPoolService.Recycle(node);
            }

            return true;
        }

        /// <summary> 构建树并返回所有计划 </summary>
        /// <param name="goal">目标计划</param>
        /// <param name="maxDepth">构建的树的最大深度</param>
        /// <returns>是否找到计划</returns>
        private static GOAPNode BuildGraph(IGOAPAgent agent, IGOAPGoal goal, int maxDepth)
        {
            var root = ObjectPoolService.Spawn<GOAPNode>();
            root.Init(null, 0, agent.States, null);
            if (maxDepth < 1)
            {
                return root;
            }

            InnerBuilderGraph(root, 0);
            return root;

            void InnerBuilderGraph(GOAPNode parent, int depth)
            {
                if (depth > maxDepth)
                {
                    return;
                }

                foreach (var action in agent.Actions)
                {
                    // 不允许出现两个连续的相同行为
                    if (parent == null || action == parent.action)
                    {
                        continue;
                    }

                    if (!GOAPHelper.IsAchieve(parent.state, action.Preconditions))
                    {
                        continue;
                    }

                    // 生成动作完成的节点链，成本累加
                    var node = ObjectPoolService.Spawn<GOAPNode>();
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

    public class GOAPNode
    {
        public GOAPNode parent;

        public List<GOAPNode> children = new List<GOAPNode>();

        /// <summary> 运行到此节点的成本 </summary>
        public float runningCost;

        /// <summary> 此节点代表的行为 </summary>
        public IGOAPAction action;

        /// <summary> 模拟运行到此节点时的状态 </summary>
        public Dictionary<string, bool> state = new Dictionary<string, bool>();

        public void Init(GOAPNode parent, float runningCost, Dictionary<string, bool> state, IGOAPAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            foreach (var pair in state)
            {
                this.state[pair.Key] = pair.Value;
            }

            this.action = action;
            parent?.children.Add(this);
        }

        public void DFS(List<GOAPNode> buffer)
        {
            buffer.Clear();
            InnerDFS(this);

            void InnerDFS(GOAPNode p)
            {
                buffer.Add(p);

                for (int i = 0; i < p.children.Count; i++)
                {
                    var c = p.children[i];
                    InnerDFS(c);
                }
            }
        }

        public void BFS(List<GOAPNode> buffer)
        {
            InnerBFS(this);

            void InnerBFS(GOAPNode p)
            {
                buffer.Add(p);

                for (int i = 0; i < p.children.Count; i++)
                {
                    var c = p.children[i];
                    InnerBFS(c);
                }
            }
        }
    }

    [ObjectPool(typeof(GOAPNode))]
    public class GOAPNodePool : ObjectPool<GOAPNode>
    {
        protected override GOAPNode Create()
        {
            return new GOAPNode();
        }

        protected override void OnRecycle(GOAPNode unit)
        {
            unit.parent = null;
            unit.children.Clear();
            unit.runningCost = 0;
            unit.state.Clear();
            unit.action = null;
        }
    }

    [ObjectPool(typeof(Stack<GOAPAction>))]
    public class GOAPActionStackPool : ObjectPool<Stack<GOAPAction>>
    {
        protected override Stack<GOAPAction> Create()
        {
            return new Stack<GOAPAction>(8);
        }

        protected override void OnRecycle(Stack<GOAPAction> unit)
        {
            unit.Clear();
        }
    }
}