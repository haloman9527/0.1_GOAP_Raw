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

using CZToolKit.Blackboard;
using System.Collections.Generic;
using System.Linq;
using CZToolKit.SimpleFSM;
using CZToolKit.VM;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace CZToolKit.GOAP_Raw
{
    [AddComponentMenu("GOAP/GOAP Agent")]
    public class GOAPAgent : MonoBehaviour, IGOAPAgent, IFSMAgent
    {
        #region 变量

        [Space(10), Tooltip("要达成的目标")] public List<GOAPGoal> goals = new List<GOAPGoal>();

        [Tooltip("状态预设")] public List<GOAPState> initialStates = new List<GOAPState>();

        /// <summary> 计划最大深度，<1无限制 </summary>
        [Space(10), Header("Settings")] [SerializeField]
        UpdateMode updateMode = UpdateMode.Normal;

        [Tooltip("计划最大深度，<1无限制")] public int maxDepth = 0;

        /// <summary> 两次搜索计划之间的间隔 </summary>
        [Min(0), Tooltip("两次搜索计划之间的间隔")] public float interval = 3;

        /// <summary> 计划异常终止是否要重置间隔 </summary>
        [Tooltip("计划异常终止是否立即重新搜寻计划")] public bool replanOnFailed = true;


        private GOAPFSM fsm;
        private GOAPMachine planner;
        private Dictionary<string, bool> states;
        private Blackboard<string> memory = new Blackboard<string>();
        private Queue<GOAPAction> storedActionQueue;
        private Queue<GOAPAction> actionQueue;

        #endregion

        #region 公共属性

        public GOAPFSM FSM
        {
            get { return fsm; }
        }

        public GOAPMachine Planner
        {
            get { return planner; }
        }

        public Blackboard<string> Memory
        {
            get { return memory; }
        }

        public List<GOAPGoal> Goals
        {
            get { return goals; }
        }

        public Dictionary<string, bool> States
        {
            get { return states; }
        }

        /// <summary> 当前计划 </summary>
        public IReadOnlyCollection<GOAPAction> StoredActionQueue
        {
            get { return storedActionQueue; }
        }

        /// <summary> 当前行为队列 </summary>
        public IReadOnlyCollection<GOAPAction> ActionQueue
        {
            get { return actionQueue; }
        }

        /// <summary> 当前行为 </summary>
        public GOAPAction CurrentAction { get; private set; }

        /// <summary> 当前目的，没有为空 </summary>
        public GOAPGoal CurrentGoal { get; private set; }

        public bool HasGoal
        {
            get { return CurrentGoal != null; }
        }

        public bool HasPlan
        {
            get { return ActionQueue != null && ActionQueue.Count > 0; }
        }

        /// <summary> 下此搜寻计划的时间 </summary>
        public float NextPlanTime { get; set; } = 0;

        public float DeltaTime { get; set; } = 0;

        #endregion

        protected virtual void Awake()
        {
            var actionBridges = GetComponentsInChildren<IGOAPActionBridge>();
            var actions = new GOAPAction[actionBridges.Length];
            for (int i = 0; i < actionBridges.Length; i++)
            {
                actions[i] = (GOAPAction)ViewModelFactory.CreateViewModel(actionBridges[i].GetGoapActionData());
            }

            states = new Dictionary<string, bool>();
            foreach (var state in initialStates)
            {
                states[state.Key] = state.Value;
            }

            fsm = new GOAPFSM();
            fsm.Init(this);
            planner = new GOAPMachine();
            planner.Init(this, actions);
            goals.QuickSort((a, b) => a.Priority.CompareTo(b.Priority));
            storedActionQueue = new Queue<GOAPAction>();
            actionQueue = new Queue<GOAPAction>();
        }

        protected virtual void Start()
        {
            var idleState = new IdleState(FSM)
            {
                onStart = () => { },
                onExit = () => { }
            };
            idleState.onUpdate = IdleUpdate;

            var performActionState = new GOAPFSMState(FSM)
            {
                onStart = () => { },
                onExit = () => { }
            };
            performActionState.onUpdate = PerformUpdate;

            FSM.PushState("IdleState", idleState);
            FSM.PushState("PerformActionState", performActionState);
            FSM.JumpTo("IdleState");
        }

        private void IdleUpdate()
        {
            if (NextPlanTime > FSM.time) return;

            NextPlanTime = FSM.time + interval;

            // 搜寻计划
            foreach (var goal in goals)
            {
                planner.Plan(goal, maxDepth, ref storedActionQueue);
                if (storedActionQueue.Count != 0)
                {
                    CurrentGoal = goal;
                    break;
                }
            }

            if (storedActionQueue.Count > 0)
            {
                actionQueue.Clear();
                foreach (var action in storedActionQueue)
                    actionQueue.Enqueue(action);

                //转换状态
                FSM.JumpTo("PerformActionState");
            }
            else
            {
                CurrentGoal = null;
            }
        }

        private void PerformUpdate()
        {
            if (HasPlan)
            {
                // 如果当前有计划(目标尚未完成)
                var action = actionQueue.Peek();
                if (CurrentAction != action)
                {
                    CurrentAction = action;
                    action.OnBeforePerform();
                }

                Debug.Log(action == null);
                // 成功 or 失败
                var status = action.OnPerform();

                switch (status)
                {
                    case GOAPActionStatus.Success:
                    {
                        foreach (var effect in action.Effects)
                        {
                            SetState(effect.Key, effect.Value);
                        }

                        action.OnAfterPerform(true);
                        actionQueue.Dequeue();
                        CurrentAction = null;
                        break;
                    }
                    case GOAPActionStatus.Failure:
                    {
                        if (replanOnFailed)
                            EnforceReplan();
                        else
                            AbortPlan();
                        return;
                    }
                    default:
                        break;
                }
            }
            else
            {
                // 如果没有计划(目标已完成)
                // 如果目标为一次性，移除掉
                if (CurrentGoal != null && CurrentGoal.Once)
                    Goals.Remove(CurrentGoal);

                // 当前目标设置为空
                CurrentGoal = null;
                FSM.JumpTo("IdleState");
            }
        }

        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.AnimatePhysics)
                Evaluate(Time.fixedDeltaTime);
        }

        private void Update()
        {
            switch (updateMode)
            {
                case UpdateMode.Normal:
                    Evaluate(Time.deltaTime);
                    break;
                case UpdateMode.UnscaledTime:
                    Evaluate(Time.unscaledDeltaTime);
                    break;
            }
        }

        public void Evaluate(float _deltaTime)
        {
            DeltaTime = _deltaTime;
            FSM.time += DeltaTime;
            FSM.Update();
        }

        /// <summary> 终止计划(在<see cref="interval"/>之后才会重新搜寻计划) </summary>
        public void AbortPlan()
        {
            if (HasPlan)
                actionQueue.Clear();

            if (CurrentAction != null)
                CurrentAction.OnAfterPerform(false);

            CurrentAction = null;
            CurrentGoal = null;
            FSM.JumpTo("IdleState");
        }

        /// <summary> 终止当前计划，并立即重新搜寻计划 </summary>
        public void EnforceReplan()
        {
            AbortPlan();
            NextPlanTime = Time.time;
            FSM.Update();
        }

        public virtual bool GetState(string _key, bool _fallback = false)
        {
            bool result;
            if (States.TryGetValue(_key, out result))
                return result;
            return _fallback;
        }

        /// <summary> 设置状态 </summary>
        public virtual void SetState(string _key, bool _value)
        {
            States[_key] = _value;
        }

        /// <summary> 移除状态 </summary>
        public virtual void RemoveState(string _key)
        {
            States.Remove(_key);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "GOAP/GOAP_Scene_Icon.png", true);
        }
    }
}