static class Day01
{
    public static void Run()
    {
        (List<int> left, List<int> right) = Console.In.LoadLists();

        var dist = left.Order()
            .Zip(right.Order(), (x, y) => Math.Abs(x - y))
            .Sum();

        var similarity = right
            .Where(new HashSet<int>(left).Contains)
            .Sum();

        Console.WriteLine($"Total items: {left.Count}");
        Console.WriteLine($"Distance: {dist}");
        Console.WriteLine($"Similarity: {similarity}");
    }

    private static (List<int> a, List<int> b) LoadLists(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign).Transpose().ToPair();
}