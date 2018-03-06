using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Unit : MonoBehaviour {

    public enum UnitType
    {
        Fast,
        Slow,
        Safe,
        Dangerous
    }

    public Transform target;
    float speed = 10;
    Vector3[] path;
    int targetIndex;
    public UnitType type;
    public bool followPath = false;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Target").transform;
        //if (target != null)
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound, type);
    }

    private void Update()
    {
        // If target not found yet
        //if (target == null)
        //{
        //    // Retry
        //    target = GameObject.FindGameObjectWithTag("Target").transform;
        //    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound, type);
        //}
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        if (pathSuccesful)
        {
            path = newPath;
            if (followPath)
            {
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }

        }

    }
    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    targetIndex = 0;
                    path = new Vector3[0];
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                switch (type)
                {
                    case UnitType.Fast:
                        Gizmos.color = Color.blue;
                        break;
                    case UnitType.Slow:
                        Gizmos.color = Color.yellow;
                        break;
                    case UnitType.Safe:
                        Gizmos.color = Color.green;
                        break;
                    case UnitType.Dangerous:
                        Gizmos.color = Color.red;
                        break;
                    default:
                        break;
                }
                
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
