using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Graph
{
    static public List<GraphNode> nodes = new List<GraphNode>();


    public static void Print()
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
    public static GraphNode Get(string name)
    {
        foreach (GraphNode node in nodes)
        {
            if (node.name.Equals(name))
                return node;
        }
        return null;
    }

    public static GraphNode Get(int id)
    {
        foreach (GraphNode node in nodes)
        {
            if (node.id == id)
                return node;
        }
        return null;
    }
    // https://www.geeksforgeeks.org/connected-components-in-an-undirected-graph/
    public static void DFSUtil(int v, bool[] visited, ref string output)
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

    public static int CalculateConnectivity()
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
}
