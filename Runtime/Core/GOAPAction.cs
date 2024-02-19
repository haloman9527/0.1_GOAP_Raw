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

using System;
using System.Collections.Generic;
using CZToolKit;
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
        private float runtimeCost;

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
            get { return runtimeCost; }
            set { runtimeCost = value; }
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
            this.runtimeCost = data.initialCost;
            this.preconditions = new Dictionary<string, bool>();
            this.effects = new Dictionary<string, bool>();

            foreach (var pair in data.preconditions)
            {
                this.preconditions[pair.key] = pair.value;
            }

            foreach (var pair in data.effects)
            {
                this.effects[pair.key] = pair.value;
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

        /// <summary>
        /// 当确定成功或者失败时，调用此方法
        /// </summary>
        /// <param name="success"></param>
        protected void SelfStop(bool success)
        {
            
        }

        /// <summary>
        /// 每次规划计划时，都会调用此方法，用于动态评估成本
        /// </summary>
        public virtual void DynamicallyEvaluateCost()
        {
        }

        public virtual void OnBeforePerform()
        {
        }

        public abstract GOAPActionStatus OnPerform();

        public virtual void OnAfterPerform(bool _successed)
        {
        }
    }
}