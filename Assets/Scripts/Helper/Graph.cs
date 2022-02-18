using System.Collections.Generic;

/// <summary>
/// Class for a graph with generic data type
/// </summary>
/// <typeparam name="T">any</typeparam>
public class Graph<T>
{
    private List<GraphNode<T>> nodes;
    private Dictionary<T, GraphNode<T>> map;

    public Graph()
    {
        nodes = new List<GraphNode<T>>();
        map = new Dictionary<T, GraphNode<T>>();
    }

    /// <summary>
    /// Get all nodes of the graph
    /// </summary>
    /// <returns></returns>
    public List<GraphNode<T>> GetNodes()
    {
        return nodes;
    }

    /// <summary>
    /// Build a tree using breadth-first search
    /// </summary>
    /// <returns>a graph that is actually a tree</returns>
    public Graph<T> BuildTree()
    {
        Graph<T> graph = new Graph<T>();
        BFS(graph);
        return graph;
    }

    /// <summary>
    /// Add an directed edge betweet item t1 and t2
    /// </summary>
    /// <param name="t1">element</param>
    /// <param name="t2">element</param>
    public void Connect(T t1, T t2)
    {
        GraphNode<T> node1 = Get(t1);
        GraphNode<T> node2 = Get(t2);
        node1.AddOut(node2);
    }

    /// <summary>
    /// Get the graph node that holds the element t
    /// </summary>
    /// <param name="t">element</param>
    /// <returns>the graph node</returns>
    public GraphNode<T> Get(T t)
    {
        if (!map.ContainsKey(t))
        {
            int id = nodes.Count;
            GraphNode<T> node = new GraphNode<T>(id, t);
            nodes.Add(node);
            map.Add(t, node);
        }

        return map[t];
    }

    /// <summary>
    /// Performs a topological sort on the graph and
    /// returns the elements in a linear list.
    /// </summary>
    /// <returns>linear list of the elements</returns>
    public List<T> TopSort()
    {
        List<T> list = new List<T>();
        List<GraphNode<T>> tmpNodes = new List<GraphNode<T>>();
        Dictionary<GraphNode<T>, int> count = new Dictionary<GraphNode<T>, int>();
        
        foreach (GraphNode<T> node in nodes)
        {
            tmpNodes.Add(node);
            count.Add(node, node.InDeg);
        }

        while (tmpNodes.Count > 0)
        {
            if (tmpNodes.Count > 1)
            {
                tmpNodes.Sort(delegate (GraphNode<T> node1, GraphNode<T> node2)
                {
                    return count[node1] < count[node2] ? -1 : 1;
                });
            }

            GraphNode<T> node = tmpNodes[0];

            if (count[node] == 0)
            {
                list.Add(node.GetT());
                count.Remove(node);
                tmpNodes.RemoveAt(0);

                foreach (GraphNode<T> node2 in node.GetNeighbors())
                {
                    if (count.ContainsKey(node2))
                        count[node2]--;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Perform the breadth-first search on the graph
    /// </summary>
    /// <param name="graph">the graph</param>
    private void BFS(Graph<T> graph)
    {
        List<GraphNode<T>> visited = new List<GraphNode<T>>();
        Queue<GraphNode<T>> queue = new Queue<GraphNode<T>>();
        queue.Enqueue(nodes[0]);

        while (queue.Count > 0)
        {
            GraphNode<T> node = queue.Dequeue();
            T t1 = node.GetT();

            if (!visited.Contains(node))
            {
                visited.Add(node);

                foreach (GraphNode<T> neighbor in node.GetNeighbors())
                {
                    if (!visited.Contains(neighbor))
                    {
                        T t2 = neighbor.GetT();
                        graph.Connect(t1, t2);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        string s = "Graph: " + nodes.Count + " Nodes\n";

        foreach (GraphNode<T> node in nodes)
        {
            s += node.ToString() + "\n";
        }

        return s;
    }
}