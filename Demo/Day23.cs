static class Day23
{
    public static void Run()
    {
        var graph = Console.In.ToGraph();

        int chiefCliques = graph.FindCliques3().Count(nodes => nodes.Any(MaybeChief));
        string password = graph.GetMaximalClique().ToPassword();

        Console.WriteLine($"   Nodes count: {graph.Edges.Keys.Count}");
        Console.WriteLine($" Chief cliques: {chiefCliques}");
        Console.WriteLine($"      Password: {password}");
    }

    private static string ToPassword(this IEnumerable<Node> nodes) =>
        string.Join(",", nodes.Order().Select(node => node.Id));

    private static List<Node> GetMaximalClique(this Graph graph) =>
        graph.BronKerbosch(new(), graph.GetNodes().ToHashSet(), new());

    private static List<Node> BronKerbosch(this Graph graph, HashSet<Node> currentClique, HashSet<Node> candidates, HashSet<Node> visited)
    {
        if (candidates.Count == 0 && visited.Count == 0) return currentClique.ToList();

        List<Node> maximalClique = new();
        foreach (var node in candidates)
        {
            var candidateClique = graph.BronKerbosch(
                new(currentClique) { node },
                candidates.Intersect(graph.Edges[node]).ToHashSet(),
                visited.Intersect(graph.Edges[node]).ToHashSet());

            if (candidateClique.Count > maximalClique.Count) maximalClique = candidateClique;

            candidates.Remove(node);
            visited.Add(node);
        }

        return maximalClique;
    }

    private static bool MaybeChief(this Node node) =>
        node.Id.StartsWith("t");

    private static IEnumerable<List<Node>> FindCliques3(this Graph graph) =>
        from a in graph.GetNodes()
        from b in graph.GetNeighborsOf(a)
        where a < b
        from c in graph.GetNeighborsOf(b).Intersect(graph.GetNeighborsOf(a))
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

    private static IEnumerable<Node> GetNodes(this Graph graph) => graph.Edges.Keys;

    private static IEnumerable<Node> GetNeighborsOf(this Graph graph, Node node) => graph.Edges[node];

    record Graph(Dictionary<Node, HashSet<Node>> Edges);

    record NodeEdges(Node From, HashSet<Node> To);

    record Node(string Id) : IComparable<Node>
    {
        public int CompareTo(Node? other) => Id.CompareTo(other?.Id);

        public static bool operator >(Node a, Node b) => a.Id.CompareTo(b.Id) > 0;
        public static bool operator <(Node a, Node b) => a.Id.CompareTo(b.Id) < 0;
    }
}
