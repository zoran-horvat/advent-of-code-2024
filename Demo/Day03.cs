using System.Text.RegularExpressions;

static class Day03
{
    public static void Run()
    {
        var instructions = Console.In.ReadLines().SelectMany(Parse).ToList();

        int sum = instructions.OfType<Multiply>().Evaluate();
        int sumWithExclusions = instructions.Evaluate();

        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine($"Sum with exclusions: {sumWithExclusions}");
    }

    private static IEnumerable<Instruction> Parse(this string line) =>
        Regex.Matches(line, @"(?<mul>mul)\((?<a>\d+),(?<b>\d+)\)|(?<do>do(n't)?)\(\)")
            .Select(match => 
                match.Groups["do"].Success && match.Groups["do"].Value == "do" ? new Continue() as Instruction
                : match.Groups["do"].Success && match.Groups["do"].Value == "don't" ? new Stop()
                : new Multiply(int.Parse(match.Groups["a"].Value), int.Parse(match.Groups["b"].Value)));

    private static int Evaluate(this IEnumerable<Instruction> instructions) => instructions.Aggregate(
        (sum: 0, include: true),
        (acc, instruction) => instruction switch
        {
            Continue => (acc.sum, true),
            Stop => (acc.sum, false),
            Multiply multiply when acc.include => (acc.sum + multiply.Product, acc.include),
            _ => acc
        }).sum;
}

abstract record Instruction;

sealed record Multiply(int A, int B) : Instruction
{
    public int Product => A * B;
}

sealed record Stop : Instruction;

sealed record Continue : Instruction;