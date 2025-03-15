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
using UnityRandom = UnityEngine.Random;

namespace Atom.GOAP_Raw
{
    [AddComponentMenu("GOAP/WanderAction")]
    public class WanderActionBridge : MonoBehaviour, IGOAPActionPrivider
    {
        public WanderActionData data = new WanderActionData();

        public IGOAPAction GetGoapAction()
        {
            return new WanderAction(data);
        }

        private void Reset()
        {
            data.preconditions.Add(new GOAPState("HasTarget", false));
            data.effects.Add(new GOAPState("HasTarget", true));
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.green;
            if (data.center != null)
            {
                Gizmos.DrawWireSphere(data.center.transform.position, data.range);
                Gizmos.DrawSphere(data.center.transform.position, 0.5f);
            }

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawMesh(SemicircleMesh(data.radius, (int)data.sector, Vector3.up), transform.position + Vector3.up * 0.2f, transform.rotation);
#endif
        }

#if UNITY_EDITOR
        /// <summary> 绘制半圆 </summary>
        public static void DrawWireSemicircle(Vector3 origin, Vector3 direction, float radius, int angle)
        {
            DrawWireSemicircle(origin, direction, radius, angle, Vector3.up);
        }

        public static void DrawWireSemicircle(Vector3 origin, Vector3 direction, float radius, int angle, Vector3 axis)
        {
            Vector3 leftdir = Quaternion.AngleAxis(-angle / 2, axis) * direction;
            Vector3 rightdir = Quaternion.AngleAxis(angle / 2, axis) * direction;

            Vector3 currentP = origin + leftdir * radius;
            Vector3 oldP;
            if (angle != 360)
            {
                Gizmos.DrawLine(origin, currentP);
            }

            for (int i = 0; i < angle / 10; i++)
            {
                Vector3 dir = Quaternion.AngleAxis(10 * i, axis) * leftdir;
                oldP = currentP;
                currentP = origin + dir * radius;
                Gizmos.DrawLine(oldP, currentP);
            }

            oldP = currentP;
            currentP = origin + rightdir * radius;
            Gizmos.DrawLine(oldP, currentP);
            if (angle != 360)
                Gizmos.DrawLine(currentP, origin);
        }

        public static Mesh SemicircleMesh(float radius, int angle, Vector3 axis)
        {
            Vector3 leftdir = Quaternion.AngleAxis(-angle / 2, axis) * Vector3.forward;
            Vector3 rightdir = Quaternion.AngleAxis(angle / 2, axis) * Vector3.forward;
            int pcount = angle / 10;
            //顶点
            Vector3[] vertexs = new Vector3[3 + pcount];
            vertexs[0] = Vector3.zero;
            int index = 1;
            vertexs[index] = leftdir * radius;
            index++;
            for (int i = 0; i < pcount; i++)
            {
                Vector3 dir = Quaternion.AngleAxis(10 * i, axis) * leftdir;
                vertexs[index] = dir * radius;
                index++;
            }

            vertexs[index] = rightdir * radius;
            //三角面
            int[] triangles = new int[3 * (1 + pcount)];
            for (int i = 0; i < 1 + pcount; i++)
            {
                triangles[3 * i] = 0;
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertexs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
#endif
    }

    [Serializable]
    public class WanderActionData : GOAPActionData
    {
        [Header("巡逻范围")] public GameObject center;
        public float range = 10;
        [Header("视野范围")] public float radius = 5;
        [Header("视野角度")] [Range(0, 360)] public float sector = 90;
        public LayerMask layer;
    }

    [ViewModel(typeof(WanderActionData))]
    public class WanderAction : GOAPAction
    {
        public GameObject Center
        {
            get { return (Data as WanderActionData).center; }
        }

        public float Range
        {
            get { return (Data as WanderActionData).range; }
        }

        public float Radius
        {
            get { return (Data as WanderActionData).radius; }
        }

        public float Sector
        {
            get { return (Data as WanderActionData).sector; }
        }

        public LayerMask Layer
        {
            get { return (Data as WanderActionData).layer; }
        }

        public WanderAction(WanderActionData data) : base(data)
        {
        }

        private NavMeshAgent navMeshAgent;

        [Tooltip("前往下个地点时触发")] public UnityAction onRefindTarget;

        [Tooltip("看到敌人时触发")] public UnityAction onFindedTarget;

        protected override void OnInitialized()
        {
            // navMeshAgent = ((GOAPAgent)Agent).GetComponent<NavMeshAgent>();
        }

        Vector3 targetPos;
        float stayTime;

        public override bool IsRequiredRange => false;

        public override void Enter()
        {
            Debug.Log("徘徊");
            targetPos = UnityRandom.insideUnitSphere * Range + Center.transform.position;
            targetPos.y = 0;
            stayTime = UnityRandom.Range(2, 5);
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.isStopped = false;
        }

        public override GOAPActionStatus Perform()
        {
            // if (Vector3.Distance(targetPos, ((GOAPAgent)Agent).transform.position) <= 2)
            // {
            //     stayTime -= Time.deltaTime;
            //     if (stayTime <= 0)
            //     {
            //         onRefindTarget?.Invoke();
            //         targetPos = UnityRandom.insideUnitSphere * Range + Center.transform.position;
            //         targetPos.y = 0;
            //         stayTime = UnityRandom.Range(2, 5);
            //     }
            // }
            //
            // navMeshAgent.SetDestination(targetPos);
            //
            // Collider[] colliders = Physics.OverlapSphere(((GOAPAgent)Agent).transform.position, Radius, Layer);
            // if (colliders.Length > 0)
            // {
            //     foreach (var item in colliders)
            //     {
            //         if (Vector3.Angle(((GOAPAgent)Agent).transform.forward, item.transform.position - ((GOAPAgent)Agent).transform.position) <= Sector / 2)
            //         {
            //             Agent.Memory.Set("Target", item.gameObject);
            //             return GOAPActionStatus.Success;
            //         }
            //     }
            // }

            return GOAPActionStatus.Running;
        }

        public override void Exit()
        {
            navMeshAgent.isStopped = true;
            // if (_successed)
            //     Agent.SetState("HasTarget", true);
            // else
            //     Agent.SetState("HasTarget", false);
        }
    }
}