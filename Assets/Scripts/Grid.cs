using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    //public Transform start;
    public LayerMask unwalkableMask;
    public LayerMask walkableMask;
    public LayerMask entryMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    LayerMask weightsWalkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        // How many nodes can fit into world size x
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        // How many nodes can fit into world size y
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            weightsWalkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2),region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    // Creates the grid
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        // Loop through grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Get current world position
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius)
                                                    + Vector3.forward * (y * nodeDiameter + nodeRadius);
                // Not walkable if collision
                bool walkable = Physics.CheckSphere(worldPoint, nodeRadius, walkableMask);//!(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                bool entry = false;//Physics.CheckSphere(worldPoint, nodeRadius, entryMask);
                if (walkable)
                {
                    walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                }
                //if (!walkable)
                //{
                //    walkable = Physics.CheckSphere(worldPoint, nodeRadius, entryMask);
                //    //walkable = entry;
                //}
                //else
                //{
                //    //walkable = Physics.CheckSphere(worldPoint, nodeRadius, entryMask);
                //}



                int movementPenalty = 0;

                // raycast
                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, weightsWalkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                }

                grid[x, y] = new Node(walkable, entry, worldPoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // Returns the node in the grid that corresponds to the current worldPosition
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        //float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        //float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        float percentX = (worldPosition.x - transform.position.x) / gridWorldSize.x + 0.5f - (nodeRadius / gridWorldSize.x);
        float percentY = (worldPosition.z - transform.position.z) / gridWorldSize.y + 0.5f - (nodeRadius / gridWorldSize.y);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // TODO: Something wrong here?
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); // int x = Mathf.RoundToInt((gridSizeX + 10) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    // Helper function for visualizing grid
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node node in grid)
            {
                // White if walkable, red if unwalkable, green if entry
                Gizmos.color = node.walkable ? new Color(1, 1, 1, 0.25f) : new Color(1, 0, 0, 0.25f);
                if (node.entry)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.25f);
                }
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                //if (node.movementPenalty > 0)
                //{
                //    Gizmos.color = new Color(1, 0, 0, 0.25f);
                //    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                //}
            }
        }
    }
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
