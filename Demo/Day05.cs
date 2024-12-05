static class Day05
{
    public static void Run()
    {
        var forbiddenSorts = Console.In.ReadSortOrder()
            .Select(pair => (before: pair.after, after: pair.before))
            .ToHashSet();

        List<List<int>> allPrints = Console.In.ReadPrintedPages().ToList();

        var middlePagesSum = allPrints
            .Where(pages => pages.IsInCorrectOrder(forbiddenSorts))
            .Sum(pages => pages.MiddlePage());

        var correctedMiddlePagesSum = allPrints
            .Where(pages => !pages.IsInCorrectOrder(forbiddenSorts))
            .Sum(pages => pages.FixSortOrder(forbiddenSorts).ToList().MiddlePage());

        Console.WriteLine($"          Middle pages sum: {middlePagesSum}");
        Console.WriteLine($"Corrected middle pages sum: {correctedMiddlePagesSum}");
    }

    private static IEnumerable<(int before, int after)> ReadSortOrder(this TextReader text) =>
        text.GetAllLines().TakeWhile(line => !string.IsNullOrWhiteSpace(line)).Select(ToSortOrder);

    private static (int before, int after) ToSortOrder(this string line)
    {
        var parts = line.Split("|");
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static IEnumerable<List<int>> ReadPrintedPages(this TextReader text) =>
        text.GetAllLines().Select(Common.ParseIntsNoSign);
    
    private static bool IsInCorrectOrder(this List<int> pages, HashSet<(int before, int after)> forbiddenSorts) =>
        pages.ExpandPageSortOrder().All(pair => !forbiddenSorts.Contains(pair));

    private static IEnumerable<(int before, int after)> ExpandPageSortOrder(this List<int> pages) =>
        pages.SelectMany((page, index) => pages.ExpandPageSortOrder(index));

    private static IEnumerable<(int before, int after)> ExpandPageSortOrder(this List<int> pages, int index) =>
        pages.Skip(index + 1).Select(page => (before: pages[index], after: page));

    private static int MiddlePage(this List<int> pages) =>
        pages[pages.Count / 2];

    private static IEnumerable<int> FixSortOrder(this List<int> pages, HashSet<(int before, int after)> forbiddenSorts)
    {
        var pending = pages.ToList();
        while (pending.Any())
        {
            int next = pending.GetCandidatesForNext(forbiddenSorts).First();    // Should never fail
            pending.Remove(next);
            yield return next;
        }
    }

    private static IEnumerable<int> GetCandidatesForNext(this List<int> pages, HashSet<(int before, int after)> forbiddenSorts) =>
        pages.Where(page => pages.ExpandFullPageSortOrder(page).All(pair => !forbiddenSorts.Contains(pair))); 

    private static IEnumerable<(int before, int after)> ExpandFullPageSortOrder(this List<int> pages, int before) =>
        pages.Where(cur => cur != before).Select(other => (before: before, after: other));
}
