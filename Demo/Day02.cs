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
        Enumerable.Range(0, values.Count - 1).All(values.IsSafeAt);

    private static bool IsSafeAt(this List<int> values, int index) =>
        values[index] != values[index + 1] &&
        Math.Abs(values[index] - values[index + 1]) <= 3 &&
        (index == 0 || values.SlopeAt(index) == values.SlopeAt(index - 1));

    private static int SlopeAt(this List<int> values, int index) =>
        Math.Sign(values[index + 1] - values[index]);

    private static List<int> ExceptAt(this List<int> values, int index) =>
        values.Take(index).Concat(values.Skip(index + 1)).ToList();

    private static List<List<int>> LoadLists(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign).ToList();
}