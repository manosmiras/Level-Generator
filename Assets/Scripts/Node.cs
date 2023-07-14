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

    public Node(bool walkable, bool entry, Vector3 worldPosition, int gridX, int gridY, int movementPenalty)
    {
        this.walkable = walkable;
        this.entry = entry;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.movementPenalty = movementPenalty;
    }

    private int FCost => gCost + hCost;

    public int HeapIndex { get; set; }

    public int CompareTo(Node nodeToCompare)
    {
        var compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        return -compare;
    }
}
