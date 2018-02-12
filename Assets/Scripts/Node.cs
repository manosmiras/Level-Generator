using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node> {
    public bool walkable;
    public bool entry;
    public Vector3 worldPosition;

    public Node parent;
    public int gridX;
    public int gridY;
    public int movementPenalty;

    public int gCost;
    public int hCost;

    int heapIndex;

    public Node(bool _walkable, bool _entry, Vector3 _worldPos, int _gridX, int _gridY, int _penalty)
    {
        walkable = _walkable;
        entry = _entry;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    public int fCost
    {
        get { return gCost + hCost; }
        set { }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        return -compare;
    }
}
