using System.Collections.Generic;
using System;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {
    private Queue<PathRequest> _pathRequestQueue = new();
    private PathRequest _currentPathRequest;

    private static PathRequestManager _instance;
    private Pathfinding _pathfinding;
    private bool _isProcessingPath;

    private void Awake()
    {
        _instance = this;
        _pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback, Unit.UnitType type)
    {
        var newRequest = new PathRequest(pathStart, pathEnd, callback, type);
        _instance._pathRequestQueue.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinding.StartFindPath(_currentPathRequest.start, _currentPathRequest.end, _currentPathRequest.type);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        _currentPathRequest.callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }

    private struct PathRequest
    {
        public Vector3 start;
        public Vector3 end;
        public Action<Vector3[], bool> callback;
        public Unit.UnitType type;
        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback, Unit.UnitType type)
        {
            this.start = start;
            this.end = end;
            this.callback = callback;
            this.type = type;
        }
    }
}
