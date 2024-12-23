static class Day23
{
    public static void Run()
    {
        var graph = Console.In.ToGraph();

        int chiefCliques = graph.FindCliques3().Count(nodes => nodes.Any(MaybeChief));

        var maximalClique = graph.BronKerbosch(new(), graph.Edges.Keys.ToHashSet(), new()).Order();
        string password = string.Join(",", maximalClique.Select(node => node.Id));

        Console.WriteLine($"   Nodes count: {graph.Edges.Keys.Count}");
        Console.WriteLine($" Chief cliques: {chiefCliques}");
        Console.WriteLine($"      Password: {password}");
    }

    private static List<Node> BronKerbosch(this Graph graph, HashSet<Node> r, HashSet<Node> p, HashSet<Node> x)
    {
        if (p.Count == 0 && x.Count == 0) return r.ToList();

        List<Node> winner = new();
        foreach (var pNode in p)
        {
            HashSet<Node> r1 = new(r) { pNode };
            HashSet<Node> p1 = p.Intersect(graph.Edges[pNode]).ToHashSet();
            HashSet<Node> x1 = x.Intersect(graph.Edges[pNode]).ToHashSet();
            
            var candidate = graph.BronKerbosch(r1, p1, x1);
            if (candidate.Count > winner.Count) winner = candidate;

            p.Remove(pNode);
            x.Add(pNode);
        }

        return winner;
    }

    private static bool MaybeChief(this Node node) =>
        node.Id.StartsWith("t");

    private static IEnumerable<List<Node>> FindCliques3(this Graph graph) =>
        from a in graph.Edges.Keys
        from b in graph.Edges[a]
        where a < b
        from c in graph.Edges[b].Intersect(graph.Edges[a])
        where b < c
        select new List<Node> { a, b, c };

    private static Graph ToGraph(this TextReader text) =>
        new(text.ReadEdges().ToDictionary(edges => edges.From, edges => edges.To));

    private static IEnumerable<NodeEdges> ReadEdges(this TextReader text) =>
        text.ReadNodePairs()
            .GroupBy(pair => pair.from, (from, edges) => (from, edges: edges.Select(edge => edge.to)))
            .Select(pair => new NodeEdges(pair.from, pair.edges.ToHashSet()));

    private static IEnumerable<(Node from, Node to)> ReadNodePairs(this TextReader text) =>
        text.ReadLines()
            .Select(line => line.Split("-"))
            .Select(pair => (from: new Node(pair[0]), to: new Node(pair[1])))
            .SelectMany(edge => new[] { (edge.from, edge.to), (edge.to, edge.from) });

    record Graph(Dictionary<Node, HashSet<Node>> Edges);

    record NodeEdges(Node From, HashSet<Node> To);

    record Node(string Id) : IComparable<Node>
    {
        public int CompareTo(Node? other) => Id.CompareTo(other?.Id);

        public static bool operator >(Node a, Node b) => a.Id.CompareTo(b.Id) > 0;
        public static bool operator <(Node a, Node b) => a.Id.CompareTo(b.Id) < 0;
    }
}
