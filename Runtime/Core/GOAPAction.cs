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

using System;
using System.Collections.Generic;
using CZToolKit.VM;
using UnityEngine;

namespace CZToolKit.GOAP_Raw
{
    [Serializable]
    public abstract class GOAPActionData
    {
        /// <summary> 行为的名称 </summary>
        [Tooltip("行为名称")] public string name;

        /// <summary> 行为的初始执行成本 </summary>
        [Tooltip("行为的初始执行成本")] public float initialCost = 1;

        /// <summary> 执行此行为的条件 </summary>
        [Tooltip("执行此行为的条件")] public List<GOAPState> preconditions = new List<GOAPState>();

        /// <summary> 行为可以造成的效果 </summary>
        [Tooltip("行为可以造成的效果")] public List<GOAPState> effects = new List<GOAPState>();
    }

    public abstract class GOAPAction : ViewModel, IGOAPAction
    {
        private GOAPMachine owner;
        private Dictionary<string, bool> preconditions;
        private Dictionary<string, bool> effects;

        [Tooltip("进入行为时触发")] public Action onEnter;

        [Tooltip("退出行为时触发")] public Action onExit;

        public string Name
        {
            get { return GetPropertyValue<string>(nameof(GOAPActionData.name)); }
            set { SetPropertyValue(nameof(GOAPActionData.name), value); }
        }

        /// <summary> 行为的执行成本 </summary>
        public float Cost
        {
            get { return GetPropertyValue<float>(nameof(GOAPActionData.initialCost)); }
            set { SetPropertyValue(nameof(GOAPActionData.initialCost), value); }
        }

        /// <summary> 执行此行为的前提条件 </summary>
        public Dictionary<string, bool> Preconditions
        {
            get { return preconditions; }
        }

        /// <summary> 此技能对世界状态造成的修改 </summary>
        public Dictionary<string, bool> Effects
        {
            get { return effects; }
        }

        public GOAPMachine Owner
        {
            get { return owner; }
        }

        /// <summary> 行为所属代理 </summary>
        public IGOAPAgent Agent
        {
            get { return owner.Agent; }
        }

        public GOAPAction(GOAPActionData data)
        {
            this[nameof(GOAPActionData.name)] = new BindableProperty<string>(() => data.name, v => data.name = v);
            this[nameof(GOAPActionData.initialCost)] = new BindableProperty<float>(() => data.initialCost, v => data.initialCost = v);
            this.preconditions = new Dictionary<string, bool>();
            this.effects = new Dictionary<string, bool>();

            foreach (var pair in data.preconditions)
            {
                this.preconditions[pair.Key] = pair.Value;
            }

            foreach (var pair in data.effects)
            {
                this.effects[pair.Key] = pair.Value;
            }
        }

        public void Init(GOAPMachine owner)
        {
            this.owner = owner;
            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }

        /// <summary> 是否行为是否可用</summary>
        public virtual bool IsUsable()
        {
            return true;
        }

        /// <summary> 动态评估成本 </summary>
        public virtual void DynamicallyEvaluateCost()
        {
        }

        /// <summary> 匹配计划过程中检查能否执行(应用计划执行过程中会导致的状态改变) </summary>
        public virtual bool IsProceduralPrecondition(Dictionary<string, bool> currentState)
        {
            return true;
        }

        public virtual void OnBeforePerform()
        {
        }

        public abstract GOAPActionStatus OnPerform();

        public virtual void OnAfterPerform(bool _successed)
        {
        }

        /// <summary> 添加一条前提条件 </summary>
        public void SetPrecondition(string key, bool value)
        {
            Preconditions[key] = value;
        }

        /// <summary> 移除一条前提条件 </summary>
        public void RemovePrecondition(string key)
        {
            Preconditions.Remove(key);
        }

        /// <summary> 添加一条效果 </summary>
        public void SetEffect(string key, bool value)
        {
            Effects[key] = value;
        }

        /// <summary> 移除一条效果 </summary>
        public void RemoveEffect(string key)
        {
            Effects.Remove(key);
        }
    }
}