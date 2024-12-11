static class Day11
{
    public static void Run()
    {
        var numbers = Console.In.ReadNumbers().ToArray();

        long count25 = numbers.CountDescendants(25);
        long count75 = numbers.CountDescendants(75);

        Console.WriteLine($"Final count: {count25}");
        Console.WriteLine($"Final count: {count75}");
    }

    private static Dictionary<(long number, int iterations), long> Cache = new();

    private static long CountDescendants(this IEnumerable<long> numbers, int iterations) =>
        numbers.Sum(number => number.CountDescendants(iterations));

    private static long CountDescendants(this long number, int iterations) =>
        Cache.TryGetValue((number, iterations), out long count) ? count
        : Cache[(number, iterations)] = number.CountDescendantsFull(iterations);

    private static long CountDescendantsFull(this long number, int iterations) =>
        iterations == 0 ? 1 : CountDescendants(number.Expand(), iterations - 1);

    private static long[] Expand(this long number) =>        
        number == 0 ? [1]
        : number.DigitsCount() is int digitsCount && digitsCount % 2 == 0 ? number.Split(TenToPower(digitsCount / 2))
        : [number * 2024];

    private static int DigitsCount(this long number) =>
        number < 10 ? 1 : 1 + (number / 10).DigitsCount();

    private static long[] Split(this long number, long divisor) =>
        [number / divisor, number % divisor];

    private static long TenToPower(int power) =>
        power == 0 ? 1 : 10 * TenToPower(power - 1);

    private static IEnumerable<long> ReadNumbers(this TextReader text) =>
        (text.ReadLine() ?? string.Empty).ParseLongsNoSign();
}
