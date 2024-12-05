using System.Text.RegularExpressions;

static class Day03
{
    public static void Run()
    {
        var instructions = Console.In.GetAllLines().SelectMany(Parse).ToList();

        int sum = instructions.OfType<Multiply>().Sum(instruction => instruction.Product);
        int sumWithExclusions = instructions.WithExclusions().Sum(instruction => instruction.Product);

        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine($"Sum with exclusions: {sumWithExclusions}");
    }

    private static IEnumerable<Instruction> Parse(this string line) =>
        Regex.Matches(line, @"(?<mul>mul)\((?<a>\d+),(?<b>\d+)\)|(?<do>do(n't)?)\(\)")
            .Select(match => 
                match.Groups["do"].Success && match.Groups["do"].Value == "do" ? new Continue() as Instruction
                : match.Groups["do"].Success && match.Groups["do"].Value == "don't" ? new Stop()
                : new Multiply(int.Parse(match.Groups["a"].Value), int.Parse(match.Groups["b"].Value)));

    private static IEnumerable<Multiply> WithExclusions(this IEnumerable<Instruction> instructions)
    {
        bool include = true;
        foreach (Instruction instruction in instructions)
        {
            if (instruction is Continue) include = true;
            else if (instruction is Stop) include = false;
            else if (instruction is Multiply multiply && include) yield return multiply;
        }
    }
}

abstract record Instruction;

sealed record Multiply(int A, int B) : Instruction
{
    public int Product => A * B;
}

sealed record Stop : Instruction;

sealed record Continue : Instruction;