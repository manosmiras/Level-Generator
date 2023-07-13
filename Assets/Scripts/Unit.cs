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
    public UnitType type;
    public bool followPath;
    
    private float _speed = 10;
    private Vector3[] _path;
    private int _targetIndex;

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
            _path = newPath;
            if (followPath)
            {
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }

        }

    }
    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = _path[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                _targetIndex++;
                if (_targetIndex >= _path.Length)
                {
                    _targetIndex = 0;
                    _path = new Vector3[0];
                    yield break;
                }
                currentWaypoint = _path[_targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, _speed * Time.deltaTime);
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
        {
            for (var i = _targetIndex; i < _path.Length; i++)
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
                
                Gizmos.DrawCube(_path[i], Vector3.one);

                if (i == _targetIndex)
                {
                    Gizmos.DrawLine(transform.position, _path[i]);
                }
                else
                {
                    Gizmos.DrawLine(_path[i - 1], _path[i]);
                }
            }
        }
    }
}
