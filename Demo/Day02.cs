static class Day02
{
    public static void Run()
    {
        List<List<int>> values = Console.In.LoadLists();

        int safeCount = values.Count(IsSafe);
        int tolerantSafeCount = values.Count(list => list.IsSafe() || list.IsSafeWithOneLess());

        Console.WriteLine($"Total count: {values.Count}");
        Console.WriteLine($"Safe count: {safeCount}");
        Console.WriteLine($"Tolerant safe count: {tolerantSafeCount}");
    }

    private static bool IsSafe(this List<int> values) =>
        values.Zip(values.Skip(1), (x, y) => (x, y))
            .All(pair => pair.IsSafe(Math.Sign(values[1] - values[0])));

    private static bool IsSafe(this (int a, int b) pair, int sign) =>
        Math.Abs(pair.b - pair.a) >= 1 &&
        Math.Abs(pair.b - pair.a) <= 3 &&
        Math.Sign(pair.b - pair.a) == sign;

    private static IEnumerable<List<int>> WithOneLess(this List<int> values) =>
        Enumerable.Range(0, values.Count).Select(i => values.ExceptAt(i));

    private static bool IsSafeWithOneLess(this List<int> values) =>
        values.WithOneLess().Any(IsSafe);

    private static List<int> ExceptAt(this List<int> values, int index) =>
        values.Take(index).Concat(values.Skip(index + 1)).ToList();

    private static List<List<int>> LoadLists(this TextReader text) =>
        text.GetAllLines().Select(Common.ParseIntsNoSign).ToList();
}