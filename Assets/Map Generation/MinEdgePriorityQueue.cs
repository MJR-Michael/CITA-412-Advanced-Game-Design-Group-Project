using System.Collections.Generic;

public class MinEdgePriorityQueue
{
    List<Edge> orderedEdges;
    public MinEdgePriorityQueue()
    {
        orderedEdges = new List<Edge>();
    }

    public void Enqueue(Edge edge)
    {
        //Add edge to ordered edges
        orderedEdges.Add(edge);
        orderedEdges.Sort((obj1, obj2) => obj1.GetEdgeCost().CompareTo(obj2.GetEdgeCost()));
    }

    public Edge Dequeue()
    {
        if (orderedEdges.Count == 0) return null;
        else
        {
            Edge firstEdge = orderedEdges[0];
            orderedEdges.RemoveAt(0);
            return firstEdge;
        }
    }

    public int Count()
    {
        return orderedEdges.Count;
    }

    public void Clear()
    {
        orderedEdges.Clear();
    }
}
