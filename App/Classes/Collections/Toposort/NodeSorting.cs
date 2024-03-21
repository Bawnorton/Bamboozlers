namespace Bamboozlers.Classes.Collections.Toposort;

public abstract class NodeSorting
{
    public static bool Sort<T>(List<T> nodes, Comparer<T> comparer) where T : SortableNode<T>
    {
	    var toposort = new List<T>(nodes.Count);
	    
	    foreach (var node in nodes)
	    {
		    ForwardVisit(node, toposort);
	    }
	    
	    ClearStatus(toposort);
	    toposort.Reverse();
	    
	    var nodeToScc = new Dictionary<T, NodeScc<T>>();
	    
	    foreach (var node in toposort)
	    {
		    if (node.Visited) continue;
		    
		    var sccNodes = new List<T>();
		    BackwardVisit(node, sccNodes);
		    sccNodes.Sort(comparer);
		    var scc = new NodeScc<T>(sccNodes);
			    
		    foreach (var nodeInScc in sccNodes)
		    {
			    nodeToScc[nodeInScc] = scc;
		    }
	    }

	    ClearStatus(toposort);
	    
	    foreach (var scc in nodeToScc.Values)
	    {
		    foreach (var subsequentNode in scc.Nodes.SelectMany(node => node.SubsequntNodes))
		    {
			    if (!nodeToScc.TryGetValue(subsequentNode, out var subsequentScc)) continue;

			    if (subsequentScc == scc) continue;
			    scc.SubsequentSccs.Add(subsequentScc);
			    subsequentScc.InDegree++;
		    }
	    }

	    var pq = new PriorityQueue<NodeScc<T>, NodeScc<T>>(new NodeSccComparer<T>(comparer));
	    nodes.Clear();
	    
	    foreach (var scc in nodeToScc.Values.Where(scc => scc.InDegree == 0))
	    {
		    pq.Enqueue(scc, scc);
		    scc.InDegree = -1;
	    }
	    
	    var noCycle = true;
	    
	    while (pq.Count > 0)
	    {
		    var scc = pq.Dequeue();
		    nodes.AddRange(scc.Nodes);
		    
		    if (scc.Nodes.Count > 1)
		    {
			    noCycle = false;
		    }
		    
		    foreach (var subsequentScc in scc.SubsequentSccs)
		    {
			    subsequentScc.InDegree--;
			    
			    if (subsequentScc.InDegree == 0)
			    {
				    pq.Enqueue(subsequentScc, subsequentScc);
			    }
		    }
	    }
	    
	    return noCycle;
    }
    
    private static void ForwardVisit<T>(T node, ICollection<T> toposort) where T : SortableNode<T>
	{
	    if (node.Visited) return;
	    
	    node.Visited = true;
	    
	    foreach (var data in node.SubsequntNodes)
	    {
		    ForwardVisit(data, toposort);
	    }
	    
	    toposort.Add(node);
	}
    
    private static void ClearStatus<T>(IEnumerable<T> nodes) where T : SortableNode<T>
	{
	    foreach (var node in nodes)
	    {
		    node.Visited = false;
	    }
	}

	private static void BackwardVisit<T>(T node, ICollection<T> sccNodes) where T : SortableNode<T>
	{
		if (node.Visited) return;
		
		node.Visited = true;
		sccNodes.Add(node);

		foreach (var data in node.PreviousNodes)
		{
			BackwardVisit(data, sccNodes);
		}
	}
	
	private class NodeScc<T>(List<T> nodes)
		where T : SortableNode<T>
	{
		public List<T> Nodes { get; } = nodes;
		public List<NodeScc<T>> SubsequentSccs { get; } = [];
		public int InDegree { get; set; }
	}
	
	private class NodeSccComparer<T>(Comparer<T> comparer) : IComparer<NodeScc<T>>
		where T : SortableNode<T>
	{
		public int Compare(NodeScc<T>? x, NodeScc<T>? y)
		{
			return x switch
			{
				null when y == null => 0,
				null => -1,
				_ => y == null ? 1 : comparer.Compare(x.Nodes[0], y.Nodes[0])
			};
		}
	}
}