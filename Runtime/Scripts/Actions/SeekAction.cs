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
using Animancer;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace CZToolKit.GOAP_Raw
{

    //[NodeTooltip("追逐敌人到一定距离后停下")]
    [AddComponentMenu("GOAP/SeekAction")]
    public class SeekAction : GOAPAction
    {
        public string targetMemoryKey = "Target";
        public float stopDistance = 2;

        [Header("超时")]
        [Tooltip("超时将不再追击敌人")]
        public float timeout = 10;

        public AnimancerComponent anim;
        public AnimationClip animationClip;

        GameObject target;
        NavMeshAgent navMeshAgent;
        float startTime;
        AnimancerState state;
        public UnityAction onPrePerform { get; }
        public UnityAction onPerform { get; }
        public UnityAction onSuccess { get; }
        public UnityAction onFailed { get; }
        private void Reset()
        {
            ActionName = "追逐";
            cost = 1;

            SetPrecondition("HasTarget", true);
            SetPrecondition("InAttackRange", false);

            SetEffect("InAttackRange", true);
        }

        protected override void OnInitialized()
        {
            navMeshAgent = Agent.GetComponent<NavMeshAgent>();
        }

        public override void OnPrePerform()
        {
            Agent.Memory.TryGetData(targetMemoryKey, out target);
            startTime = Time.time;
            navMeshAgent.stoppingDistance = stopDistance;
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;
            onPrePerform?.Invoke();
            Debug.Log("追逐");
            state = anim.Play(animationClip, 0.1f);
        }

        public override GOAPActionStatus OnPerform()
        {
            if (target == null || !target.activeSelf || Time.time - startTime > timeout)
            {
                Debug.Log("追不上");
                return GOAPActionStatus.Failure;
            }
            if (Vector3.Distance(Agent.transform.position, target.transform.position) <= stopDistance)
            {
                return GOAPActionStatus.Success;
            }
            navMeshAgent.destination = target.transform.position;
            onPerform?.Invoke();
            return GOAPActionStatus.Running;
        }

        public override void OnPostPerform(bool _successed)
        {
            navMeshAgent.isStopped = true;
            if (_successed)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailed?.Invoke();
                Agent.Memory.SetData<GameObject>(targetMemoryKey, null);
                Agent.SetState("HasTarget", false);
                Agent.SetState("InAttackRange", false);
            }

            anim.Stop(animationClip);
        }
    }
}
