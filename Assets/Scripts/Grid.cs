using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public LayerMask walkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    
    private LayerMask _weightsWalkableMask;
    private readonly Dictionary<int, int> _walkableRegionsDictionary = new();

    private Node[,] _grid;

    private float _nodeDiameter;
    private int _gridSizeX;
    private int _gridSizeY;

    void Awake()
    {
        _nodeDiameter = nodeRadius * 2;
        // How many nodes can fit into world size x
        _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
        // How many nodes can fit into world size y
        _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

        foreach (var region in walkableRegions)
        {
            _weightsWalkableMask.value |= region.terrainMask.value;
            _walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2),region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize => _gridSizeX * _gridSizeY;
    
    private void CreateGrid()
    {
        _grid = new Node[_gridSizeX, _gridSizeY];
        var worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        // Loop through grid
        for (var x = 0; x < _gridSizeX; x++)
        {
            for (var y = 0; y < _gridSizeY; y++)
            {
                // Get current world position
                var worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                                 + Vector3.forward * (y * _nodeDiameter + nodeRadius);
                // Not walkable if collision
                var isWalkable = Physics.CheckSphere(worldPoint, nodeRadius, walkableMask);
                if (isWalkable)
                {
                    isWalkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                }

                var movementPenalty = 0;

                // raycast
                if (isWalkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, _weightsWalkableMask))
                    {
                        _walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                }

                _grid[x, y] = new Node(isWalkable, false, worldPoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>();

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                var checkX = node.gridX + x;
                var checkY = node.gridY + y;

                if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY >= 0 && checkY < _gridSizeY)
                {
                    neighbours.Add(_grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // Returns the node in the grid that corresponds to the current worldPosition
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        var percentX = (worldPosition.x - transform.position.x) / gridWorldSize.x + 0.5f - (nodeRadius / gridWorldSize.x);
        var percentY = (worldPosition.z - transform.position.z) / gridWorldSize.y + 0.5f - (nodeRadius / gridWorldSize.y);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // TODO: Something wrong here?
        var x = Mathf.RoundToInt((_gridSizeX - 1) * percentX); // int x = Mathf.RoundToInt((gridSizeX + 10) * percentX);
        var y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

        return _grid[x, y];
    }

    // Helper function for visualizing grid
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (_grid != null && displayGridGizmos)
        {
            foreach (var node in _grid)
            {
                // White if walkable, red if unwalkable, green if entry
                Gizmos.color = node.walkable ? new Color(1, 1, 1, 0.25f) : new Color(1, 0, 0, 0.25f);
                if (node.entry)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.25f);
                }
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (_nodeDiameter - 0.1f));
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
