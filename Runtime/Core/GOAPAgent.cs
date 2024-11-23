using System;
using System.Collections.Generic;
using Jiange.Blackboard;

namespace Jiange.GOAP_Raw
{
    public class GOAPAgent : IGOAPAgent
    {
        public AgentState State { get; private set; } = AgentState.Idle;

        public IGOAPAction CurrentAction { get; private set; }

        public List<IGOAPAction> Actions { get; }

        public Dictionary<string, bool> States { get; }

        public List<IGOAPGoal> Goals { get; }

        public List<IGOAPSensor> Sensors { get; }

        public Blackboard<string> Memory { get; } = new Blackboard<string>();

        public Events<string> Events { get; } = new Events<string>();

        public Queue<IGOAPAction> Plan { get; } = new Queue<IGOAPAction>();

        public GOAPAgent(Dictionary<string, bool> states = null, List<IGOAPAction> actions = null, List<IGOAPGoal> goals = null, List<IGOAPSensor> sensors = null)
        {
            this.Actions = actions == null ? new List<IGOAPAction>() : actions;
            this.States = states == null ? new Dictionary<string, bool>() : states;
            this.Goals = goals == null ? new List<IGOAPGoal>() : goals;
            this.Sensors = sensors == null ? new List<IGOAPSensor>() : sensors;
        }

        public void Update()
        {
            // 传感器感知世界
            foreach (var sensor in Sensors)
            {
                sensor.Sense();
            }

            switch (State)
            {
                case AgentState.Idle:
                {
                    this.IdleLogic();
                    break;
                }
                case AgentState.Moving:
                {
                    this.MovingLogic();
                    break;
                }
                case AgentState.Performing:
                {
                    this.PerformingLogic();
                    break;
                }
            }
        }

        public T AddSensor<T>() where T : IGOAPSensor, new()
        {
            var sensor = new T();
            Sensors.Add(sensor);
            return sensor;
        }

        public void AbortPlan()
        {
            switch (State)
            {
                case AgentState.Idle:
                {
                    break;
                }
                case AgentState.Moving:
                case AgentState.Performing:
                {
                    CurrentAction?.Exit();
                    Plan.Clear();
                    break;
                }
            }
            
            ChangeState(AgentState.Idle);
        }

        private void ChangeState(AgentState state)
        {
            if (this.State == state)
            {
                return;
            }

            switch (this.State)
            {
                case AgentState.Idle:
                    break;
                case AgentState.Moving:
                    break;
                case AgentState.Performing:
                    break;
            }

            this.State = state;
            
            switch (this.State)
            {
                case AgentState.Idle:
                    break;
                case AgentState.Moving:
                    break;
                case AgentState.Performing:
                    break;
            }
        }

        private void IdleLogic()
        {
            // 间隔规划逻辑
            Plan.Clear();

            for (int i = 0; i < Goals.Count; i++)
            {
                Goals[i].DynamicEvaluatePriority();
            }

            Goals.HeapSort(GOAPGoalPriorityComparer.Default);

            int index = -1;
            for (int i = 0; i < Goals.Count; i++)
            {
                // 如果目标已经达成，就跳过这个计划
                if (GOAPHelper.IsAchieve(States, Goals[i].Preconditions))
                {
                    continue;
                }

                index = i;
                break;
            }

            // 没有可实现的目标
            if (index == -1)
            {
                return;
            }

            // 根据动态评估的优先级，得到的优先级最高的
            var goal = Goals[index];

            Actions.HeapSort(GOAPActionComparer.Default);

            foreach (var action in Actions)
            {
                if (!action.IsUsable())
                {
                    return;
                }

                action.EvaluateCost();
            }

            GOAPPlanner.Plan(this, goal, 5, Plan);
        }

        private void MovingLogic()
        {
            if (!CurrentAction.IsRequiredRange)
            {
                ChangeState(AgentState.Performing);
                return;
            }

            if (CurrentAction.IsInRange())
            {
                ChangeState(AgentState.Performing);
                return;
            }

            // 要有目标
            // 移动逻辑
        }

        private void PerformingLogic()
        {
            if (CurrentAction == null)
            {
                CurrentAction = Plan.Dequeue();
            }

            // 计划完成，没有行为可做了
            if (CurrentAction == null)
            {
                AbortPlan();
                return;
            }

            // 行为不可用，计划失败
            if (!CurrentAction.IsDynamicUseable())
            {
                AbortPlan();
                return;
            }

            if (CurrentAction.IsRequiredRange && !CurrentAction.IsInRange())
            {
                ChangeState(AgentState.Moving);
                return;
            }

            var result = CurrentAction.Perform();
            switch (result)
            {
                case GOAPActionStatus.Failure:
                {
                    AbortPlan();
                    return;
                }
                case GOAPActionStatus.Success:
                {
                    CurrentAction = Plan.Dequeue();
                    break;
                }
                case GOAPActionStatus.Running:
                {
                    break;
                }
            }
        }
    }
}