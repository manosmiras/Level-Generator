using System.Collections.Generic;

public class GraphNode
{
    public string name;
    public int id;
    public List<GraphNode> children = new();

    public GraphNode(string name, int id)
    {
        this.name = name;
        this.id = id;
    }
}
