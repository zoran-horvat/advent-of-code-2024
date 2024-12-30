static class Day25
{
    public static void Run()
    {
        var blocks = Console.In.ReadBlocks().ToList();
        
        var totalFits = blocks.GetFits().Count();

        Console.WriteLine($"Total lock-key fits: {totalFits}");
    }

    private static IEnumerable<(List<string> a, List<string> b)> GetFits(this List<List<string>> blocks) =>
        from pair in blocks.Select((block, i) => (block, i))
        let a = pair.block
        from b in blocks.Skip(pair.i + 1)
        where a.Fits(b)
        select (a, b);

    private static bool Fits(this List<string> a, List<string> b) =>
        a.Zip(b).All(pair => 
            pair.First.Zip(pair.Second).All(
                chars => chars.First != '#' || chars.Second != '#'));

    private static IEnumerable<List<string>> ReadBlocks(this TextReader text)
    {
        var block = new List<string>();

        foreach (var line in text.ReadLines())
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                yield return block;
                block = new List<string>();
            }
            else
            {
                block.Add(line);
            }
        }

        if (block.Count > 0) yield return block;
    }
}
