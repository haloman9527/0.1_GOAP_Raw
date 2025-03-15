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
using Atom;
using UnityEngine;

namespace Atom.GOAP_Raw
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
        private Dictionary<string, bool> preconditions;
        private Dictionary<string, bool> effects;

        public GOAPActionData Data { get; private set; }
        /// <summary> 行为的执行成本 </summary>
        public float Cost { get; protected set; }

        /// <summary> 执行此行为的前提条件 </summary>
        public IReadOnlyDictionary<string, bool> Preconditions => preconditions;

        /// <summary> 此技能对世界状态造成的修改 </summary>
        public IReadOnlyDictionary<string, bool> Effects => effects;

        public abstract bool IsRequiredRange { get; }

        /// <summary> 行为所属代理 </summary>
        public IGOAPAgent Agent { get; private set; }

        public GOAPAction(GOAPActionData data)
        {
            this.Data = data;
            this.Cost = data.initialCost;
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

        public void Init(IGOAPAgent agent)
        {
            this.Agent = agent;
            this.OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }

        public virtual bool IsUsable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 每次规划计划时，都会调用此方法，用于动态评估成本
        /// </summary>
        public virtual void EvaluateCost()
        {
        }

        public virtual bool IsDynamicUseable()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsInRange()
        {
            throw new NotImplementedException();
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual GOAPActionStatus Perform()
        {
            throw new NotImplementedException();
        }
    }
}