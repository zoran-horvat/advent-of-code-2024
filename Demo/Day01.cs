static class Day01
{
    public static void Run()
    {
        (List<int> left, List<int> right) = Console.In.LoadLists();

        var dist = left.Order()
            .Zip(right.Order(), (x, y) => Math.Abs(x - y))
            .Sum();

        var index = left
            .GroupBy(x => x, (value, appearances) => (value: value, count: appearances.Count()))
            .ToDictionary(pair => pair.value, pair => pair.count);

        var similarity = right
            .Where(index.ContainsKey)
            .Sum(rightValue => rightValue * index[rightValue]);

        Console.WriteLine($"Total items: {left.Count}");
        Console.WriteLine($"   Distance: {dist}");
        Console.WriteLine($" Similarity: {similarity}");
    }

    private static (List<int> a, List<int> b) LoadLists(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign).Transpose().ToPair();
}