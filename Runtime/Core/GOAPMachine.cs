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
    public class GOAPMachine
    {
        /// <summary> 节点对象池，节点对象重复利用 </summary>
        private GOAPNodePool NodePool { get; } = new GOAPNodePool();

        private StackPool<GOAPAction> Stack_Pool { get; } = new StackPool<GOAPAction>();

        private IGOAPAgent agent;
        private GOAPAction[] actions;
        private List<GOAPNode> enumrateBuffer = new List<GOAPNode>();

        public IGOAPAgent Agent => agent;
        public GOAPAction[] Actions => actions;

        public void Init(IGOAPAgent agent, GOAPAction[] actions)
        {
            this.agent = agent;
            this.actions = actions;
            foreach (var action in this.actions)
            {
                action.Init(this);
            }
        }

        /// <summary> 定制最优计划 </summary>
        /// <param name="goal"> 目标状态，想要达到的状态</param>
        /// <param name="maxDepth"> </param>
        /// <param name="plan"> 返回一个计划 </param>
        public void Plan(GOAPGoal goal, int maxDepth, ref Queue<GOAPAction> plan)
        {
            plan.Clear();
            if (agent.States.TryGetValue(goal.key, out bool value) && value.Equals(goal.value))
                return;

            foreach (var action in actions)
            {
                action.DynamicallyEvaluateCost();
            }

            // 如果通过构建节点树找到了能够达成目标的计划
            var treeRoot = BuildGraph(goal, maxDepth);

            // 成本最低的计划节点
            var cheapestNode = (GOAPNode)null;
            treeRoot.DFS(enumrateBuffer);
            for (int i = 0; i < enumrateBuffer.Count; i++)
            {
                var node = enumrateBuffer[i];
                if (node.children.Count > 0)
                    continue;
                if (node.state.TryGetValue(goal.key, out bool v) && v == goal.value)
                {
                    if (cheapestNode == null)
                        cheapestNode = node;
                    else if (cheapestNode.runningCost > node.runningCost)
                        cheapestNode = node;
                }
            }

            // 向上遍历并添加行为到栈中，直至根节点，因为从后向前遍历
            var goapActionStack = Stack_Pool.Spawn();
            while (cheapestNode != null && cheapestNode != treeRoot)
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
            Stack_Pool.Recycle(goapActionStack);

            // 回收节点
            for (int i = 0; i < enumrateBuffer.Count; i++)
            {
                var node = enumrateBuffer[i];
                NodePool.Recycle(node);
            }
        }

        /// <summary> 构建树并返回所有计划 </summary>
        /// <param name="goal">目标计划</param>
        /// <param name="maxDepth">构建的树的最大深度</param>
        /// <returns>是否找到计划</returns>
        private GOAPNode BuildGraph(GOAPGoal goal, int maxDepth)
        {
            var treeRoot = NodePool.Acquire(null, 0, agent.States, null);
            if (maxDepth < 1)
                return treeRoot;
            InnerBuilderGraph(treeRoot, 0);
            return treeRoot;

            void InnerBuilderGraph(GOAPNode parent, int depth)
            {
                if (depth > maxDepth)
                    return;

                foreach (var action in actions)
                {
                    // 不允许出现两个连续的相同行为
                    if (parent == null || action == parent.action)
                        continue;

                    if (!IsAchieve(parent.state, action.Preconditions))
                        continue;

                    // 生成动作完成的节点链，成本累加
                    var node = NodePool.Acquire(parent, parent.runningCost + action.Cost, parent.state, action);
                    foreach (var effect in action.Effects)
                    {
                        node.state[effect.Key] = effect.Value;
                    }

                    // 如果当前状态不能达成目标，继续构建树
                    if (!node.state.TryGetValue(goal.key, out bool value) || value != goal.value)
                        InnerBuilderGraph(node, ++depth);
                }
            }
        }

        /// <summary> l是否达成r，r包含l </summary>
        public static bool IsAchieve(Dictionary<string, bool> l, Dictionary<string, bool> r)
        {
            if (r.Count == 0)
                return true;
                
            foreach (var pair in r)
            {
                if (!l.TryGetValue(pair.Key, out bool value) && pair.Value)
                    return false;
                if (pair.Value != value)
                    return false;
            }

            return true;
        }
    }

    public class GOAPNode
    {
        public GOAPNode parent;

        public List<GOAPNode> children = new List<GOAPNode>();

        /// <summary> 运行到此节点的成本 </summary>
        public float runningCost;

        /// <summary> 此节点代表的行为 </summary>
        public GOAPAction action;

        /// <summary> 运行到此节点时的状态 </summary>
        public Dictionary<string, bool> state = new Dictionary<string, bool>();

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

    public class GOAPNodePool : BaseObjectPool<GOAPNode>
    {
        protected override GOAPNode Create()
        {
            return new GOAPNode();
        }

        protected override void OnRelease(GOAPNode unit)
        {
            unit.parent = null;
            unit.children.Clear();
            unit.runningCost = 0;
            unit.state.Clear();
            unit.action = null;
        }

        public GOAPNode Acquire(GOAPNode parent, float runningCost, Dictionary<string, bool> state, GOAPAction action)
        {
            var unit = base.Spawn();
            unit.parent = parent;
            unit.runningCost = runningCost;
            foreach (var pair in state)
            {
                unit.state[pair.Key] = pair.Value;
            }

            unit.action = action;
            parent?.children.Add(unit);
            return unit;
        }
    }

    public class StackPool<T> : BaseObjectPool<Stack<T>>
    {
        protected override Stack<T> Create()
        {
            return new Stack<T>(8);
        }

        protected override void OnRelease(Stack<T> unit)
        {
            unit.Clear();
        }
    }
}