// #region 注 释
//
// /***
//  *
//  *  Title:
//  *  
//  *  Description:
//  *  
//  *  Date:
//  *  Version:
//  *  Writer: 半只龙虾人
//  *  Github: https://github.com/haloman9527
//  *  Blog: https://www.haloman.net/
//  *
//  */
//
// #endregion
//
// using Moyo.Blackboard;
// using System.Collections.Generic;
// using System.Linq;
// using Moyo.SimpleFSM;
// using Moyo;
// using UnityEngine;
// using UnityEngine.Serialization;
// using UnityEngine.UIElements;
//
// namespace Moyo.GOAP_Raw
// {
//     [AddComponentMenu("GOAP/GOAP Agent")]
//     public class UnityGOAPAgent : MonoBehaviour
//     {
//         #region 变量
//
//         [Tooltip("要达成的目标")] public List<GOAPGoal> goals = new List<GOAPGoal>();
//
//         [Tooltip("状态预设")] public List<GOAPState> initialStates = new List<GOAPState>();
//
//         [Space(10), Header("Settings")] [SerializeField]
//         UpdateMode updateMode = UpdateMode.Normal;
//
//         [Tooltip("计划最大深度，<1无限制")] public int maxDepth = 0;
//
//         /// <summary> 两次搜索计划之间的间隔 </summary>
//         [Tooltip("两次搜索计划之间的间隔")] public float interval = 3;
//
//         private GOAPFSM fsm;
//         private GOAPPlanner planner;
//         private Dictionary<string, bool> states;
//         private Blackboard<string> memory = new Blackboard<string>();
//         private GOAPGoal currentGoal;
//         private GOAPAction currentAction;
//         private Queue<GOAPAction> storedActionQueue;
//         private Queue<GOAPAction> actionQueue;
//
//         #endregion
//
//         #region 公共属性
//
//         public List<GOAPGoal> Goals
//         {
//             get { return goals; }
//         }
//
//         public Dictionary<string, bool> States
//         {
//             get { return states; }
//         }
//
//         public GOAPFSM FSM
//         {
//             get { return fsm; }
//         }
//
//         public GOAPPlanner Planner
//         {
//             get { return planner; }
//         }
//
//         public Blackboard<string> Memory
//         {
//             get { return memory; }
//         }
//
//         /// <summary> 当前计划 </summary>
//         public IReadOnlyCollection<GOAPAction> StoredActionQueue
//         {
//             get { return storedActionQueue; }
//         }
//
//         /// <summary> 当前剩余行为队列 </summary>
//         public IReadOnlyCollection<GOAPAction> ActionQueue
//         {
//             get { return actionQueue; }
//         }
//
//         /// <summary> 当前行为 </summary>
//         public GOAPAction CurrentAction
//         {
//             get { return currentAction; }
//         }
//
//         /// <summary> 当前目的，没有为空 </summary>
//         public GOAPGoal CurrentGoal
//         {
//             get { return currentGoal; }
//         }
//
//         public bool HasGoal
//         {
//             get { return currentGoal != null; }
//         }
//
//         public bool HasPlan
//         {
//             get { return actionQueue != null && actionQueue.Count > 0; }
//         }
//
//         /// <summary> 下此搜寻计划的时间 </summary>
//         public float NextPlanTime { get; set; } = 0;
//
//         public float DeltaTime { get; set; } = 0;
//
//         #endregion
//
//         protected virtual void Awake()
//         {
//             var actionBridges = GetComponentsInChildren<IGOAPActionPrivider>();
//             var actions = new GOAPAction[actionBridges.Length];
//             for (int i = 0; i < actionBridges.Length; i++)
//             {
//                 actions[i] = (GOAPAction)ViewModelFactory.CreateViewModel(actionBridges[i].GetGoapAction());
//             }
//
//             states = new Dictionary<string, bool>();
//             foreach (var state in initialStates)
//             {
//                 states[state.key] = state.value;
//             }
//
//             goalsComparer = new GoalComparer();
//             fsm = new GOAPFSM();
//             fsm.Init(this);
//             planner = new GOAPPlanner();
//             planner.Init(this);
//             goals.QuickSort((a, b) => a.priority.CompareTo(b.priority));
//             storedActionQueue = new Queue<GOAPAction>();
//             actionQueue = new Queue<GOAPAction>();
//         }
//
//         protected virtual void Start()
//         {
//             var idleState = new IdleState(FSM)
//             {
//                 onStart = () => { },
//                 onExit = () => { }
//             };
//             idleState.onUpdate = IdleUpdate;
//             FSM.PushState("IdleState", idleState);
//
//             var performActionState = new PerformState(FSM)
//             {
//                 onStart = () => { },
//                 onExit = () => { }
//             };
//             performActionState.onUpdate = PerformUpdate;
//             FSM.PushState("PerformActionState", performActionState);
//
//             FSM.JumpTo("IdleState");
//         }
//
//         private void IdleUpdate()
//         {
//             if (NextPlanTime > FSM.time) return;
//
//             NextPlanTime = FSM.time + Mathf.Max(interval, 0);
//             goals.QuickSort(GoalComparer.Default);
//             var finalGoal = (GOAPGoal)null;
//             // 搜寻计划
//             foreach (var goal in goals)
//             {
//                 planner.Plan(goal, maxDepth, ref storedActionQueue);
//                 if (storedActionQueue.Count != 0)
//                 {
//                     finalGoal = goal;
//                     break;
//                 }
//             }
//
//             if (storedActionQueue.Count > 0)
//             {
//                 currentGoal = finalGoal;
//                 actionQueue.Clear();
//                 foreach (var action in storedActionQueue)
//                     actionQueue.Enqueue(action);
//
//                 //转换状态
//                 FSM.JumpTo("PerformActionState");
//             }
//         }
//
//         private void PerformUpdate()
//         {
//             if (HasPlan)
//             {
//                 // 如果当前有计划(目标尚未完成)
//                 var action = actionQueue.Peek();
//                 if (currentAction != action)
//                 {
//                     currentAction = action;
//                     action.OnBegin();
//                 }
//
//                 // 成功 or 失败
//                 var status = action.OnPerform();
//
//                 switch (status)
//                 {
//                     case GOAPActionStatus.Success:
//                     {
//                         foreach (var effect in action.Effects)
//                         {
//                             SetState(effect.Key, effect.Value);
//                         }
//
//                         action.OnEnd(true);
//                         actionQueue.Dequeue();
//                         break;
//                     }
//                     case GOAPActionStatus.Failure:
//                     {
//                         AbortPlan();
//                         return;
//                     }
//                 }
//             }
//             else
//             {
//                 // 如果没有计划(目标已完成)
//                 // 如果目标为一次性，移除掉
//                 if (currentGoal != null && currentGoal.once)
//                     Goals.Remove(currentGoal);
//
//                 // 当前目标设置为空
//                 currentGoal = null;
//                 currentAction = null;
//                 FSM.JumpTo("IdleState");
//             }
//         }
//
//         private void FixedUpdate()
//         {
//             if (updateMode == UpdateMode.AnimatePhysics)
//                 Evaluate(Time.fixedDeltaTime);
//         }
//
//         private void Update()
//         {
//             switch (updateMode)
//             {
//                 case UpdateMode.Normal:
//                     Evaluate(Time.deltaTime);
//                     break;
//                 case UpdateMode.UnscaledTime:
//                     Evaluate(Time.unscaledDeltaTime);
//                     break;
//             }
//         }
//
//         public void Evaluate(float _deltaTime)
//         {
//             DeltaTime = _deltaTime;
//             FSM.time += DeltaTime;
//             FSM.Update();
//         }
//
//         /// <summary> 终止计划(在<see cref="interval"/>之后才会重新搜寻计划) </summary>
//         public void AbortPlan()
//         {
//             currentAction?.OnEnd(false);
//             currentGoal = null;
//             currentAction = null;
//             actionQueue?.Clear();
//             FSM.JumpTo("IdleState");
//         }
//
//         /// <summary> 终止当前计划，并立即重新搜寻计划 </summary>
//         public void EnforceReplan()
//         {
//             AbortPlan();
//             NextPlanTime = Time.time;
//         }
//
//         public virtual bool GetState(string _key, bool _fallback = false)
//         {
//             bool result;
//             if (States.TryGetValue(_key, out result))
//                 return result;
//             return _fallback;
//         }
//
//         /// <summary> 设置状态 </summary>
//         public virtual void SetState(string _key, bool _value)
//         {
//             States[_key] = _value;
//         }
//
//         /// <summary> 移除状态 </summary>
//         public virtual void RemoveState(string _key)
//         {
//             States.Remove(_key);
//         }
//
//         private void OnDrawGizmos()
//         {
//             Gizmos.DrawIcon(transform.position, "GOAP/GOAP_Scene_Icon.png", true);
//         }
//
//         public class GoalComparer : IComparer<GOAPGoal>
//         {
//             public static GoalComparer Default { get; } = new GoalComparer();
//             
//             public int Compare(GOAPGoal x, GOAPGoal y)
//             {
//                 return -(x.priority.CompareTo(y.priority));
//             }
//         }
//     }
// }