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
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using CZToolKit.Common;
using System.Collections.Generic;
using System.Linq;
using CZToolKit.GraphProcessor;
using UnityEngine;

namespace CZToolKit.GOAP_Raw
{
    [AddComponentMenu("GOAP/GOAP Agent")]
    public class GOAPAgent : MonoBehaviour
    {
        #region 变量
        [SerializeField]
        [Space(10), Tooltip("要达成的目标")]
        List<GOAPGoal> goals = new List<GOAPGoal>();

        [SerializeField]
        [Tooltip("状态预设")]
        List<GOAPState> preState = new List<GOAPState>();

        /// <summary> 计划最大深度，<1无限制 </summary>
        [Space(10), Header("Settings")]
        [SerializeField]
        UpdateMode updateMode = UpdateMode.Normal;

        [Tooltip("计划最大深度，<1无限制")]
        public int maxDepth = 0;

        /// <summary> 两次搜索计划之间的间隔 </summary>
        [Min(0), Tooltip("两次搜索计划之间的间隔")]
        public float interval = 3;

        /// <summary> 计划异常终止是否要重置间隔 </summary>
        [Tooltip("计划异常终止是否立即重新搜寻计划")]
        public bool replanOnFailed = true;

        Queue<GOAPAction> storedActionQueue;
        Queue<GOAPAction> actionQueue;
        #endregion

        #region 公共属性

        public IGOAP Provider { get; private set; }
        public GOAPFSM FSM { get; private set; }
        public GOAPPlanner Planner { get; private set; }
        public GOAPAction[] AvailableActions { get; private set; }
        public Blackboard Memory { get; private set; } = new Blackboard();
        public List<GOAPGoal> Goals { get { return goals; } private set { goals = value; } }
        public Dictionary<string, bool> States { get; private set; }
        /// <summary> 当前计划 </summary>
        public IReadOnlyCollection<GOAPAction> StoredActionQueue { get { return storedActionQueue; } }
        /// <summary> 当前行为队列 </summary>
        public IReadOnlyCollection<GOAPAction> ActionQueue { get { return actionQueue; } }
        /// <summary> 当前行为 </summary>
        public GOAPAction CurrentAction { get; private set; }
        /// <summary> 当前目的，没有为空 </summary>
        public GOAPGoal CurrentGoal { get; private set; }
        public bool HasGoal { get { return CurrentGoal != null; } }
        public bool HasPlan { get { return ActionQueue != null && ActionQueue.Count > 0; } }
        /// <summary> 下此搜寻计划的时间 </summary>
        public float NextPlanTime { get; set; } = 0;

        public float DeltaTime { get; set; } = 0;
        #endregion

        protected virtual void Awake()
        {
            storedActionQueue = new Queue<GOAPAction>();
            actionQueue = new Queue<GOAPAction>();
            Provider = GetComponent<IGOAP>();
            Planner = new GOAPPlanner();
            FSM = new GOAPFSM();
            AvailableActions = GetComponentsInChildren<GOAPAction>();
            foreach (var action in AvailableActions)
            {
                action.Initialize(this);
            }
            Goals = Goals.OrderByDescending(goal => goal.Priority).ToList();
        }

        protected virtual void Start()
        {
            States = new Dictionary<string, bool>();
            foreach (var item in preState)
            {
                States[item.Key] = item.Value;
            }

            IdleState idleState = new IdleState(FSM)
            {
                onStart = () => { },
                onExit = () => { }
            };
            idleState.onUpdate = () =>
                 {
                     if (NextPlanTime > FSM.time) return;

                     NextPlanTime = FSM.time + interval;

                     // 搜寻计划
                     foreach (GOAPGoal goal in Goals)
                     {
                         Planner.Plan(AvailableActions, States, goal, maxDepth, ref storedActionQueue);
                         if (StoredActionQueue.Count != 0)
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

                         //通知计划找到
                         if (Provider != null)
                             Provider.PlanFound(CurrentGoal, actionQueue);
                         //转换状态
                         FSM.JumpTo("PerformActionState");
                     }
                     else
                     {
                         //通知计划没找到
                         if (Provider != null)
                             Provider.PlanFailed(Goals);
                         CurrentGoal = null;
                     }
                 };

            GOAPFSMState performActionState = new GOAPFSMState(FSM)
            {
                onStart = () => { },
                onExit = () => { }
            };
            performActionState.onUpdate = () =>
            {
                if (HasPlan)
                {
                    // 如果当前有计划(目标尚未完成)
                    GOAPAction action = actionQueue.Peek();
                    if (CurrentAction != action)
                    {
                        CurrentAction = action;
                        action.OnPrePerform();
                    }
                    // 成功 or 失败
                    GOAPActionStatus status = action.OnPerform();

                    switch (status)
                    {
                        case GOAPActionStatus.Success:
                            foreach (var effect in action.Effects)
                            {
                                SetState(effect.Key, effect.Value);
                            }
                            action.OnPostPerform(true);
                            if (Provider != null)
                                Provider.ActionFinished(action.Effects);
                            actionQueue.Dequeue();
                            CurrentAction = null;
                            break;
                        case GOAPActionStatus.Failure:
                            if (replanOnFailed)
                                EnforceReplan();
                            else
                                AbortPlan();
                            return;
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

                    // 通知计划完成
                    if (Provider != null)
                        Provider.PlanFinished();

                    // 当前目标设置为空
                    CurrentGoal = null;
                    FSM.JumpTo("IdleState");
                }
            };

            FSM.PushState("IdleState", idleState);
            FSM.PushState("PerformActionState", performActionState);
            FSM.JumpTo("IdleState");
        }

        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.AnimatePhysics)
                Evaluate(Time.fixedDeltaTime);
        }

        protected virtual void Update()
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
            {
                CurrentAction.OnPostPerform(false);
                // 如果动作执行失败，转换到空闲状态，并通知因为该动作导致计划失败
                if (Provider != null)
                    Provider.PlanAborted(CurrentAction);
            }

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
        public virtual void SetState(string _key, bool _value) { States[_key] = _value; }

        /// <summary> 移除状态 </summary>
        public virtual void RemoveState(string _key) { States.Remove(_key); }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "GOAP/GOAP_Scene_Icon.png", true);
        }
    }
}
