static class Day05
{
    public static void Run()
    {
        var sortOrder = Console.In.ReadSortOrder().ToHashSet();
        IComparer<int> comparer = Comparer<int>.Create((a, b) => 
            sortOrder.Contains((a, b)) ? -1
            : sortOrder.Contains((b, a)) ? 1
            : 0);

        List<List<int>> allPrints = Console.In.ReadPrintedPages().ToList();

        var middlePagesSum = allPrints
            .Where(pages => pages.IsSorted(comparer))
            .Sum(pages => pages.MiddlePage());

        var correctedMiddlePagesSum = allPrints
            .Where(pages => !pages.IsSorted(comparer))
            .Sum(pages => pages.Order(comparer).MiddlePage());

        Console.WriteLine($"          Middle pages sum: {middlePagesSum}");
        Console.WriteLine($"Corrected middle pages sum: {correctedMiddlePagesSum}");
    }
    
    private static int MiddlePage(this List<int> pages) =>
        pages[pages.Count / 2];

    private static int MiddlePage(this IEnumerable<int> pages)
    {
        using var half = pages.GetEnumerator();
        using var full = pages.GetEnumerator();

        while (full.MoveNext() && half.MoveNext() && full.MoveNext()) { }

        return half.Current;
    }

    private static bool IsSorted(this List<int> pages, IComparer<int> comparer) =>
        pages.SelectMany((prev, index) => pages[(index + 1)..].Select(next => (prev, next)))
            .All(pair => comparer.Compare(pair.prev, pair.next) <= 0);

    private static IEnumerable<(int before, int after)> ReadSortOrder(this TextReader text) =>
        text.ReadLines().TakeWhile(line => !string.IsNullOrWhiteSpace(line)).Select(ToSortOrder);

    private static (int before, int after) ToSortOrder(this string line)
    {
        var parts = line.Split("|");
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static IEnumerable<List<int>> ReadPrintedPages(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign);
}
