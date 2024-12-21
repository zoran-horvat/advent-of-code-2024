static class Day01
{
    public static void Run()
    {
        (List<int> left, List<int> right) = Console.In.LoadLists();

        var distance = left.Order()
            .Zip(right.Order(), (x, y) => Math.Abs(x - y))
            .Sum();

        var similarity = 
            left.Join(right, l => l, r => r, (a, b) => b)
            .Sum();

        Console.WriteLine($"Total items: {left.Count}");
        Console.WriteLine($"   Distance: {distance}");
        Console.WriteLine($" Similarity: {similarity}");
    }

    private static (List<int> a, List<int> b) LoadLists(this TextReader text) =>
        text.ReadLines().Select(Common.ParseIntsNoSign).Transpose().ToPair();
}