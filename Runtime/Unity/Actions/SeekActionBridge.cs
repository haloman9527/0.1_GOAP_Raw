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
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;
using Atom;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Atom.GOAP_Raw
{
    //[NodeTooltip("追逐敌人到一定距离后停下")]
    [AddComponentMenu("GOAP/SeekAction")]
    public class SeekActionBridge : MonoBehaviour, IGOAPActionPrivider
    {
        public SeekActionData data = new SeekActionData();

        public IGOAPAction GetGoapAction()
        {
            return new SeekAction(data);
        }

        private void Reset()
        {
            data.initialCost = 1;
            data.preconditions.Add(new GOAPState("HasTarget", true));
            data.preconditions.Add(new GOAPState("InAttackRange", false));
            data.effects.Add(new GOAPState("InAttackRange", true));
        }
    }

    [Serializable]
    public class SeekActionData : GOAPActionData
    {
        public string targetMemoryKey = "Target";
        public float stopDistance = 2;

        [Header("超时")] [Tooltip("超时将不再追击敌人")] public float timeout = 10;
    }

    [ViewModel(typeof(SeekActionData))]
    public class SeekAction : GOAPAction
    {
        private GameObject target;
        private NavMeshAgent navMeshAgent;
        private float startTime;

        public UnityAction onPrePerform { get; }
        public UnityAction onPerform { get; }
        public UnityAction onSuccess { get; }
        public UnityAction onFailed { get; }

        public string TargetMemoryKey
        {
            get { return (Data as SeekActionData).targetMemoryKey; }
        }

        public float StopDistance
        {
            get { return (Data as SeekActionData).stopDistance; }
        }

        public float Timeout
        {
            get { return (Data as SeekActionData).timeout; }
        }


        public SeekAction(SeekActionData data) : base(data)
        {
        }

        protected override void OnInitialized()
        {
            // navMeshAgent = ((GOAPAgent)Agent).GetComponent<NavMeshAgent>();
        }

        public override bool IsRequiredRange => false;

        public override void Enter()
        {
            Agent.Memory.TryGet(TargetMemoryKey, out target);
            startTime = Time.time;
            navMeshAgent.stoppingDistance = StopDistance;
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;
            onPrePerform?.Invoke();
            Debug.Log("追逐");
        }

        public override GOAPActionStatus Perform()
        {
            if (target == null || !target.activeSelf || Time.time - startTime > Timeout)
            {
                Debug.Log("追不上");
                return GOAPActionStatus.Failure;
            }

            // if (Vector3.Distance(((GOAPAgent)Agent).transform.position, target.transform.position) <= StopDistance)
            // {
            //     return GOAPActionStatus.Success;
            // }

            navMeshAgent.destination = target.transform.position;
            onPerform?.Invoke();
            return GOAPActionStatus.Running;
        }

        public override void Exit()
        {
            navMeshAgent.isStopped = true;
            // if (_successed)
            // {
            //     onSuccess?.Invoke();
            // }
            // else
            // {
            //     onFailed?.Invoke();
            //     Agent.Memory.Set<GameObject>(TargetMemoryKey, null);
            //     Agent.SetState("HasTarget", false);
            //     Agent.SetState("InAttackRange", false);
            // }
        }
    }
}