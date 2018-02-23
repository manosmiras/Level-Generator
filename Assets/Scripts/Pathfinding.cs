using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {
    PathRequestManager requestManager;
    Grid grid;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos, Unit.UnitType type)
    {
        StartCoroutine(FindPath(startPos, targetPos, type));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, Unit.UnitType type)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        //print("target position is: " + targetPos);

        // Get start and target node based on startPos and targetPos respectively
        // TODO: fix NodeFromWorldPoint method
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        //print(targetNode.worldPosition);
        if (startNode.walkable && targetNode.walkable)
        {

            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Reached destination
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    //print("Path found in: " + sw.ElapsedMilliseconds + "ms");
                    pathSuccess = true;

                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;
                    int newMovementCostToNeighbour = 0;
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
                        default:
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
            //UnityEngine.Debug.Log("total path cost: " + targetNode.gCost);
            //if(type == Unit.UnitType.Fast)
            //    LevelGenerator.shortestPathCost = targetNode.gCost;
            if (type == Unit.UnitType.Safe)
                LevelGenerator.shortestPathCost = targetNode.gCost;
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        //print("end node (target) position is: " + endNode.worldPosition);
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    public int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        else
            return 14 * distX + 10 * (distY - distX);
    }
}
