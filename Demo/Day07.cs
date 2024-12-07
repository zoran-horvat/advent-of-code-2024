static class Day07
{
    public static void Run()
    {
        List<(long target, long first, List<long> other)> equations = Console.In.ReadLists().ToList();
        
        long calibrationResult = equations
            .Where(equation => equation.CanCreateTarget(Add, Multiply))
            .Sum(equation => equation.target);

        long concatCalibrationResult = equations
            .Where(equation => equation.CanCreateTarget(Add, Multiply, Concatenate))
            .Sum(equation => equation.target);

        Console.WriteLine($"            Calibration: {calibrationResult}");
        Console.WriteLine($"Calibration (w/ concat): {concatCalibrationResult}");
    }

    // Naive solution to the first part of the problem:
    //
    // private static bool CanCreateTarget(this (long target, long first, List<long> other) equation)
    // {
    //     HashSet<long> created = new() { equation.first };

    //     foreach (long value in equation.other)
    //     {
    //         var addition = created
    //             .Where(result => equation.target - value >= result)
    //             .Select(result => result + value);
    //         var multiplication = created
    //             .Where(result => equation.target / value >= result)
    //             .Select(result => result * value);

    //         created = new(addition.Concat(multiplication));
    //     }

    //     return created.Contains(equation.target);
    // }

    private static bool CanCreateTarget(this (long target, long first, List<long> other) equation,
                                        params Func<long, long, long, IEnumerable<long>>[] operations)
    {
        HashSet<long> created = new() { equation.first };

        foreach (long value in equation.other)
        {
            var results = created.SelectMany(result =>
                operations.SelectMany(operation =>
                    operation(result, value, equation.target)));
            created = new(results);
        }

        return created.Contains(equation.target);
    }

    private static IEnumerable<long> Add(long left, long right, long target) =>
        target - left >= right ? new[] { left + right } : Enumerable.Empty<long>();

    private static IEnumerable<long> Multiply(long left, long right, long target) =>
        target / left >= right ? new[] { left * right } : Enumerable.Empty<long>();

    private static IEnumerable<long> Concatenate(long left, long right, long target) =>
        long.TryParse(left.ToString() + right.ToString(), out long concatenated) &&
        concatenated <= target
            ? new[] { concatenated }
            : Enumerable.Empty<long>();

    private static IEnumerable<(long target, long first, List<long> other)> ReadLists(this TextReader text) =>
        text.ReadLines()
            .Select(Common.ParseLongsNoSign)
            .Select(list => (list[0], list[1], list.Skip(2).ToList()));
}
