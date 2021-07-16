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
#region ע ��
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEngine;

namespace CZToolKit.GOAP_Raw.Actions.Movement
{
    //[NodeTooltip("���趨·��Ѳ�ߣ�ʹ��NavMesh�ƶ�")]
    [AddComponentMenu("GOAP/Movement/Patrol")]
    public class Patrol : NavMeshMovement
    {
        [Tooltip("Should the agent patrol the waypoints randomly?")]
        public bool randomPatrol = false;
        [Tooltip("The length of time that the agent should pause when arriving at a waypoint")]
        public float waypointPauseDuration = 0;
        [Tooltip("The waypoints to move to")]
        //public List<GameObject> waypoints = new List<GameObject>();
        public GameObject[] waypoints;

        // The current index that we are heading towards within the waypoints array
        private int waypointIndex;
        private float waypointReachedTime;

        private void Reset()
        {
            randomPatrol = false;
            waypointPauseDuration = 0;
            //waypoints.Value.Clear();

            SetPrecondition("HasTarget", false);

            SetEffect("HasTarget", true);
        }

        public override void OnPrePerform()
        {
            base.OnPrePerform();

            // initially move towards the closest waypoint
            float distance = Mathf.Infinity;
            float localDistance;
            for (int i = 0; i < waypoints.Length; ++i)
            {
                if ((localDistance = Vector3.Magnitude(Agent.transform.position - waypoints[i].transform.position)) < distance)
                {
                    distance = localDistance;
                    waypointIndex = i;
                }
            }
            waypointReachedTime = -1;
            SetDestination(Target());
        }

        // Patrol around the different waypoints specified in the waypoint array. Always return a task status of running. 
        public override GOAPActionStatus OnPerform()
        {
            if (waypoints.Length == 0)
            {
                return GOAPActionStatus.Failure;
            }
            if (HasArrived())
            {
                if (waypointReachedTime == -1)
                {
                    waypointReachedTime = Time.time;
                }
                // wait the required duration before switching waypoints.
                if (waypointReachedTime + waypointPauseDuration <= Time.time)
                {
                    if (randomPatrol)
                    {
                        if (waypoints.Length == 1)
                        {
                            waypointIndex = 0;
                        }
                        else
                        {
                            // prevent the same waypoint from being selected
                            var newWaypointIndex = waypointIndex;
                            while (newWaypointIndex == waypointIndex)
                            {
                                newWaypointIndex = Random.Range(0, waypoints.Length);
                            }
                            waypointIndex = newWaypointIndex;
                        }
                    }
                    else
                    {
                        waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    }
                    SetDestination(Target());
                    waypointReachedTime = -1;
                }
            }

            return GOAPActionStatus.Running;
        }

        // Return the current waypoint index position
        private Vector3 Target()
        {
            if (waypointIndex >= waypoints.Length)
            {
                return Agent.transform.position;
            }
            return waypoints[waypointIndex].transform.position;
        }
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (waypoints == null) return;
            var oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; ++i)
            {
                if (waypoints[i] != null)
                {
                    UnityEditor.Handles.SphereHandleCap(0, waypoints[i].transform.position, waypoints[i].transform.rotation, 1, EventType.Repaint);
                }
            }
            UnityEditor.Handles.color = oldColor;
#endif
        }
    }
}