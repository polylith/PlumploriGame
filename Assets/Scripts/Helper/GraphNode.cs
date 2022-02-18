using System.Collections.Generic;

/// <summary>
/// <para>
/// Data structure of a node in a graph that holds
/// the generic type T.
/// </para>
/// <para>
/// Each node has incoming and outgoing edges.
/// </para>
/// </summary>
/// <typeparam name="T">any</typeparam>
public class GraphNode<T>
{
    public int OutDeg { get => outMap.Count;  }
    public int InDeg { get => inMap.Count; }

    private readonly int id;
    private readonly T t;
    private List<GraphNode<T>> nodes;
    private Dictionary<T, GraphNode<T>> outMap;
    private Dictionary<T, GraphNode<T>> inMap;

    public GraphNode(int id, T t)
    {
        this.id = id;
        this.t = t;
        nodes = new List<GraphNode<T>>();
        outMap = new Dictionary<T, GraphNode<T>>();
        inMap = new Dictionary<T, GraphNode<T>>();
    }

    public T GetT()
    {
        return t;
    }

    public void AddOut(GraphNode<T> node)
    {
        if (outMap.ContainsKey(node.t))
            return;

        nodes.Add(node);
        outMap.Add(node.t, node);
        node.AddIn(this);
    }

    private void AddIn(GraphNode<T> node)
    {
        if (inMap.ContainsKey(node.t))
            return;

        inMap.Add(node.t, node);
    }

    public List<GraphNode<T>> GetNeighbors()
    {
        return nodes;
    }

    public override string ToString()
    {
        string s = "Node " + id + " ";

        if (null == t)
            s += "NULL";
        else
            s += t.ToString();

        return s;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is GraphNode<T>))
            return false;

        GraphNode<T> node = (GraphNode<T>)o;
        return null != t && t.Equals(node.t);
    }

    public override int GetHashCode() => new { t }.GetHashCode();
}