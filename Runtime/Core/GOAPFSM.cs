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

using CZToolKit.SimpleFSM;
using System;

namespace CZToolKit.GOAP_Raw
{
    public class GOAPFSM : CZToolKit.SimpleFSM.FSM
    {
        public float time;

        private IFSMAgent agent;

        public IFSMAgent Agent
        {
            get { return agent; }
        }
    }

    public class GOAPFSMState : IFSMState
    {
        public Action onStart;
        public Action onUpdate;
        public Action onExit;

        public IFSM Owner { get; set; }

        public GOAPFSMState(GOAPFSM _owner)
        {
            Owner = _owner;
        }

        public virtual void OnBegin()
        {
            onStart?.Invoke();
        }

        public virtual void OnUpdate()
        {
            onUpdate?.Invoke();
        }

        public virtual void OnEnd()
        {
            onExit?.Invoke();
        }
    }

    public class IdleState : GOAPFSMState
    {
        public IdleState(GOAPFSM owner) : base(owner)
        {
        }

        public override void OnBegin()
        {
            //await Task.Run(onStart);
            onStart?.Invoke();
        }

        public override void OnUpdate()
        {
            onUpdate.Invoke();
            //await Task.Run(onUpdate);
        }

        public override void OnEnd()
        {
            onExit?.Invoke();
            //await Task.Run(onExit);
        }
    }

    public class PerformState : GOAPFSMState
    {
        public PerformState(GOAPFSM owner) : base(owner)
        {
        }

        public override void OnBegin()
        {
            //await Task.Run(onStart);
            onStart?.Invoke();
        }

        public override void OnUpdate()
        {
            onUpdate.Invoke();
            //await Task.Run(onUpdate);
        }

        public override void OnEnd()
        {
            onExit?.Invoke();
            //await Task.Run(onExit);
        }
    }
}