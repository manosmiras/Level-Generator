using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public List<GraphNode> nodes = new();
    
    public void Print()
    {
        foreach (var node in nodes)
        {
            Debug.Log("Parent: " + node.name);
            foreach (var child in node.children)
            {
                Debug.Log("Child: " + child.name);
            }
        }
    }
    
    public GraphNode Get(string name)
    {
        foreach (var node in nodes)
        {
            if (node.name.Equals(name))
                return node;
        }
        return null;
    }

    public GraphNode Get(int id)
    {
        foreach (var node in nodes)
        {
            if (node.id == id)
                return node;
        }
        return null;
    }

    public int CalculateNumberOfConnections()
    {
        var connections = 0;

        foreach (var node in nodes)
        {
            foreach (var child in node.children)
            {
                connections++;
            }
        }

        return connections;
    }
    // Checks if a graph is k-vertex-connected
    public bool IsKConnected(int k)
    {
        foreach (var node in nodes)
        {
            // If k is larger than the amount of children in the node, 
            // the graph contains a node that is less than k connected
            if (k > node.children.Count)
                return false;
        }
        // All nodes are at least k connected
        return true;
    }

    public int CalculateKConnectivity(int k)
    {
        var kConnectivity = 0;
        foreach (var node in nodes)
        {
            // If k is larger than the amount of children in the node, 
            // the graph contains a node that is less than k connected
            if (node.children.Count >= k)
                kConnectivity++;
        }
        return kConnectivity;
    }
    // Returns the number of children each node has
    public int CalculateVariableKConnectivity()
    {
        var variableKConnectivity = 0;
        foreach (var node in nodes)
        {
            // If k is larger than the amount of children in the node, 
            // the graph contains a node that is less than k connected
            variableKConnectivity += node.children.Count;
        }
        return variableKConnectivity;
    }

    // https://www.geeksforgeeks.org/connected-components-in-an-undirected-graph/
    public void DFSUtil(int v, bool[] visited, ref string output)
    {
        // Mark the current node as visited and print it
        visited[v] = true;
        //Debug.Log(v);
        output += v.ToString() + " ";

        foreach (var child in nodes[v].children)
        {
            if (!visited[child.id])
                DFSUtil(child.id, visited, ref output);
        }
    }

    // Calculates the number of connected components in the graph using Depth-first search
    public int CalculateConnectivity()
    {
        var connectedComponents = 0;

        var output = "";

        // Mark all vertices as not visited
        var visited = new bool[nodes.Count];
        for (var v = 0; v < nodes.Count; v++)
            visited[v] = false;

        for (var v = 0; v < nodes.Count; v++)
        {
            if (visited[v] == false)
            {
                // print all reachable vertices
                // from v
                DFSUtil(v, visited, ref output);
                output += "\n";
                connectedComponents++;
            }
        }
        Debug.Log(output);
        Debug.Log("No of connected components:" + connectedComponents);

        return connectedComponents;
    }

    // Calculates the number of strongly connected components in the graph using Kosaraju's algorithm (USED IN DIRECTED GRAPHS)
    public int CalculateStrongConnectivity()
    {
        var output = "";
        var stronglyConnectedComponents = 0;
        var stack = new Stack<int>();
        
        // Mark all the vertices as not visited (For first DFS)
        var visited = new bool[LevelGenerator.graph.nodes.Count];
        for (var v = 0; v < LevelGenerator.graph.nodes.Count; v++)
            visited[v] = false;

        // Fill vertices in stack according to their finishing times
        for (var i = 0; i < LevelGenerator.graph.nodes.Count; i++)
            if (visited[i] == false)
                FillOrder(i, visited, ref stack);

        // Create a reversed graph
        var graph = LevelGenerator.graph.GetTranspose();

        // Mark all the vertices as not visited (For second DFS)
        for (var i = 0; i < LevelGenerator.graph.nodes.Count; i++)
            visited[i] = false;
        
        // Now process all vertices in order defined by Stack
        while (stack.Count > 0)
        {
            // Pop a vertex from stack
            var v = stack.Peek();
            stack.Pop();
            
            // Print Strongly connected component of the popped vertex
            if (visited[v] == false)
            {
                graph.DFSUtil(v, visited, ref output);
                output += "\n";
                stronglyConnectedComponents++;
            }
        }
        Debug.Log(output);
        return stronglyConnectedComponents;
    }

    static void FillOrder(int v, bool[] visited, ref Stack<int> stack)
    {
        // Mark the current node as visited and print it
        visited[v] = true;
        
        foreach (var child in LevelGenerator.graph.nodes[v].children)
        {
            if (!visited[child.id])
                FillOrder(child.id, visited, ref stack);
        }

        // All vertices reachable from v are processed by now, push v 
        stack.Push(v);
    }

    private Graph GetTranspose()
    {
        var graph = new Graph();
        graph.nodes = new List<GraphNode>();
        for (var v = 0; v < nodes.Count; v++)
        {
            foreach (var child in nodes)
            {
                graph.nodes.Add(child);
            }
        }
        return graph;
    }
}
