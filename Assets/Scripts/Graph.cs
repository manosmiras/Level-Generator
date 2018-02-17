using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public List<GraphNode> nodes = new List<GraphNode>();


    public void Print()
    {
        foreach (GraphNode node in nodes)
        {
            Debug.Log("Parent: " + node.name);
            foreach (GraphNode child in node.children)
            {
                Debug.Log("Child: " + child.name);
            }
        }
    }
    public GraphNode Get(string name)
    {
        foreach (GraphNode node in nodes)
        {
            if (node.name.Equals(name))
                return node;
        }
        return null;
    }

    public GraphNode Get(int id)
    {
        foreach (GraphNode node in nodes)
        {
            if (node.id == id)
                return node;
        }
        return null;
    }

    public int CalculateNumberOfConnections()
    {
        int connections = 0;

        foreach (GraphNode node in nodes)
        {
            foreach (GraphNode child in node.children)
            {
                connections++;
            }
        }

        return connections;
    }
    // Checks if a graph is k-vertex-connected
    public bool IsKConnected(int k)
    {
        foreach (GraphNode node in nodes)
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
        int kConnectivity = 0;
        foreach (GraphNode node in nodes)
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
        int variableKConnectivity = 0;
        foreach (GraphNode node in nodes)
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
        //Debug.Log(v);
        //list<int>::iterator i;
        //for (i = adj[v].begin(); i != adj[v].end(); ++i)
        //    if (!visited[*i])
        //        DFSUtil(*i, visited);

        // Recur for all the vertices
        // adjacent to this vertex
        //for (int i = 0; i < nodes[v].children.Count; ++i)
        //{
        //    if (!visited[nodes[v].id])
        //        DFSUtil(ref i, visited, ref output);
        //}
        foreach (GraphNode child in nodes[v].children)
        {
            if (!visited[child.id])
                DFSUtil(child.id, visited, ref output);
        }
    }

    // Calculates the number of connected components in the graph using Depth-first search
    public int CalculateConnectivity()
    {
        int connectedComponents = 0;

        string output = "";

        // Mark all vertices as not visited
        bool[] visited = new bool[nodes.Count];
        for (int v = 0; v < nodes.Count; v++)
            visited[v] = false;

        for (int v = 0; v < nodes.Count; v++)
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
        //Debug.Log(output);
        //Debug.Log("No of connected components:" + connectedComponents);

        return connectedComponents;
    }

    // Calculates the number of strongly connected components in the graph using Kosaraju's algorithm (USED IN DIRECTED GRAPHS)
    public int CalculateStrongConnectivity()
    {
        string output = "";
        int stronglyConnectedComponents = 0;
        Stack<int> stack = new Stack<int>();
        
        // Mark all the vertices as not visited (For first DFS)
        bool[] visited = new bool[LevelGenerator.graph.nodes.Count];
        for (int v = 0; v < LevelGenerator.graph.nodes.Count; v++)
            visited[v] = false;

        // Fill vertices in stack according to their finishing times
        for (int i = 0; i < LevelGenerator.graph.nodes.Count; i++)
            if (visited[i] == false)
                FillOrder(i, visited, ref stack);

        // Create a reversed graph
        Graph gr = LevelGenerator.graph.GetTranspose();

        // Mark all the vertices as not visited (For second DFS)
        for (int i = 0; i < LevelGenerator.graph.nodes.Count; i++)
            visited[i] = false;
        
        // Now process all vertices in order defined by Stack
        //while (stack.empty() == false)
        while (stack.Count > 0)
        {
            // Pop a vertex from stack
            int v = stack.Peek();
            stack.Pop();
            
            // Print Strongly connected component of the popped vertex
            if (visited[v] == false)
            {
                gr.DFSUtil(v, visited, ref output);
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

        // Recur for all the vertices adjacent to this vertex
        //list<int>::iterator i;
        //for (i = adj[v].begin(); i != adj[v].end(); ++i)
        //    if (!visited[*i])
        //        FillOrder(*i, visited, stack);
        foreach (GraphNode child in LevelGenerator.graph.nodes[v].children)
        {
            if (!visited[child.id])
                FillOrder(child.id, visited, ref stack);
        }

        // All vertices reachable from v are processed by now, push v 
        stack.Push(v);
    }

    Graph GetTranspose()
    {
        Graph graph = new Graph();
        graph.nodes = new List<GraphNode>();
        for (int v = 0; v < nodes.Count; v++)
        {
            // Recur for all the vertices adjacent to this vertex
            //list<int>::iterator i;
            //for (i = adj[v].begin(); i != adj[v].end(); ++i)
            foreach (GraphNode child in nodes)
            {
                graph.nodes.Add(child);
            }
        }
        return graph;
    }
}
