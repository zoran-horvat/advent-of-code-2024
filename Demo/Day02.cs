static class Day02
{
    public static void Run()
    {
        List<List<int>> allLists = Console.In.LoadLists();

        int safeCount = allLists.Count(IsSafe);
        int tolerantSafeCount = allLists.Count(list => list.Expand().Any(IsSafe));

        Console.WriteLine($"        Total count: {allLists.Count}");
        Console.WriteLine($"         Safe count: {safeCount}");
        Console.WriteLine($"Tolerant safe count: {tolerantSafeCount}");
    }

    private static IEnumerable<List<int>> Expand(this List<int> values) =>
        new[] {values}.Concat(Enumerable.Range(0, values.Count).Select(i => values.ExceptAt(i)));

    private static bool IsSafe(this List<int> values) =>
        values.Count < 2 || values.IsSafe(Math.Sign(values[1] - values[0]));

    private static bool IsSafe(this List<int> values, int diffSign) =>
        values.ToPairs().All(pair => pair.IsSafe(diffSign));

    private static IEnumerable<(int prev, int next)> ToPairs(this List<int> values)
    {
        using var enumerator = values.GetEnumerator();
        if (!enumerator.MoveNext()) yield break;

        int prev = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return (prev, enumerator.Current);
            prev = enumerator.Current;
        }
    }

    private static bool IsSafe(this (int a, int b) pair, int diffSign) =>
        Math.Abs(pair.b - pair.a) >= 1 &&
        Math.Abs(pair.b - pair.a) <= 3 &&
        Math.Sign(pair.b - pair.a) == diffSign;

    private static List<int> ExceptAt(this List<int> values, int index) =>
        values.Take(index).Concat(values.Skip(index + 1)).ToList();

    private static List<List<int>> LoadLists(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign).ToList();
}