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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GOAP_Raw
{
    public abstract class GOAPAction : MonoBehaviour, IGOAPAction
    {
        /// <summary> 行为的名称 </summary>
        [Tooltip("行为名称")] [SerializeField] string actionName;

        /// <summary> 行为的执行成本 </summary>
        [Tooltip("此行为的执行成本")] public float cost = 1;

        /// <summary> 执行此行为的前提条件 </summary>
        [Tooltip("此行为的前提条件")] [SerializeField] List<GOAPState> preconditions = new List<GOAPState>();

        /// <summary> 行为执行成功造成的效果 </summary>
        [Tooltip("行为可以造成的效果")] [SerializeField] List<GOAPState> effects = new List<GOAPState>();

        [Tooltip("进入行为时触发")] public Action onEnter;

        [Tooltip("退出行为时触发")] public Action onExit;

        public string ActionName { get { return actionName; } set { actionName = value; } }
        /// <summary> 行为所属代理 </summary>
        public GOAPAgent Agent { get; private set; }
        /// <summary> 冷却时间(可重载) </summary>
        public virtual float CooldownTime => 0f;
        /// <summary> 冷却计时器 </summary>
        public Cooldown Cooldown { get; private set; } = new Cooldown();

        /// <summary> 执行此行为的前提条件 </summary>
        public List<GOAPState> Preconditions { get { return preconditions; } }

        /// <summary> 此技能对世界状态造成的修改 </summary>
        public List<GOAPState> Effects { get { return effects; } }

        public void Initialize(GOAPAgent _agent)
        {
            Agent = _agent;
            OnInitialized();
        }

        protected virtual void OnInitialized() { }

        /// <summary> 是否行为是否可用</summary>
        public virtual bool IsUsable() { return true; }

        /// <summary> 动态评估成本 </summary>
        public virtual void DynamicallyEvaluateCost() { }

        /// <summary> 匹配计划过程中检查能否执行(应用计划执行过程中会导致的状态改变) </summary>
        public virtual bool IsProceduralPrecondition(Dictionary<string, bool> currentState) { return true; }

        public virtual void OnPrePerform() { }

        public abstract GOAPActionStatus OnPerform();

        public virtual void OnPostPerform(bool _successed) { }

        /// <summary> 添加一条前提条件 </summary>
        public void SetPrecondition(string _key, bool _value)
        {
            GOAPState state = preconditions.Find(item => item.Key == _key);
            if (state == null)
                preconditions.Add(new GOAPState(_key, _value));
            else
                state.Value = _value;
        }

        /// <summary> 移除一条前提条件 </summary>
        public void RemovePrecondition(string _key)
        {
            preconditions.RemoveAll(item => item.Key == _key);
        }

        /// <summary> 添加一条效果 </summary>
        public void SetEffect(string _key, bool _value)
        {
            GOAPState state = effects.Find(item => item.Key == _key);
            if (state == null)
                effects.Add(new GOAPState(_key, _value));
            else
                state.Value = _value;
        }

        /// <summary> 移除一条效果 </summary>
        public void RemoveEffect(string _key)
        {
            effects.RemoveAll(item => item.Key == _key);
        }
    }
}
