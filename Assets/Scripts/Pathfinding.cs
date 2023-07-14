using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour {
    private PathRequestManager _requestManager;
    private Grid _grid;

    private void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos, Unit.UnitType type)
    {
        StartCoroutine(FindPath(startPos, targetPos, type));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, Unit.UnitType type)
    {
        var waypoints = Array.Empty<Vector3>();
        var pathSuccess = false;

        //print("target position is: " + targetPos);

        // Get start and target node based on startPos and targetPos respectively
        // TODO: fix NodeFromWorldPoint method
        var startNode = _grid.NodeFromWorldPoint(startPos);
        var targetNode = _grid.NodeFromWorldPoint(targetPos);
        //print(targetNode.worldPosition);
        if (startNode.walkable && targetNode.walkable)
        {

            var openSet = new Heap<Node>(_grid.MaxSize);
            var closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                var currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Reached destination
                if (currentNode == targetNode)
                {
                    pathSuccess = true;

                    break;
                }

                foreach (var neighbour in _grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;
                    var newMovementCostToNeighbour = 0;
                    switch (type)
                    {
                        case Unit.UnitType.Fast:
                            newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                            break;
                        case Unit.UnitType.Slow:
                            newMovementCostToNeighbour = currentNode.gCost - GetDistance(currentNode, neighbour);
                            break;
                        case Unit.UnitType.Safe:
                            newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                            break;
                        case Unit.UnitType.Dangerous:
                            newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) - neighbour.movementPenalty;
                            break;
                    }

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            if (type == Unit.UnitType.Safe)
                LevelGenerator.shortestPathCost = targetNode.gCost;
            waypoints = RetracePath(startNode, targetNode);
        }
        _requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        var path = new List<Node>();
        var currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        var waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        var waypoints = new List<Vector3>();
        var directionOld = Vector2.zero;

        for (var i = 1; i < path.Count; i++)
        {
            var directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        var distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        var distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}
