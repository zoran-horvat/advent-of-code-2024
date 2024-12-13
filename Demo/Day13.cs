static class Day13
{
    public static void Run()
    {
        var machines = Console.In.ReadMachines().ToList();

        long totalCost = machines
            .SelectMany(machine => machine.CheapestPlay())
            .Sum();

        long farCost = machines
            .Select(machine => machine.ToCorrectedMachine())
            .SelectMany(machine => machine.CheapestPlay())
            .Sum();

        Console.WriteLine($"Total cost: {totalCost}");
        Console.WriteLine($"  Far cost: {farCost}");
    }

    private static IEnumerable<long> CheapestPlay(this Machine machine)
    {
        var (a, b, prize) = machine;

        long determinant = a.X * b.Y - a.Y * b.X;

        if (determinant != 0)
        {
            long aSteps = (prize.X * b.Y - prize.Y * b.X) / determinant;
            long bSteps = b.X == 0 ? 0 : (prize.X - aSteps * a.X) / b.X;

            if (machine.IsSolution(aSteps, bSteps)) yield return aSteps * 3 + bSteps;
        }
        else
        {
            for (long bSteps = Math.Min(prize.X / b.X, prize.Y / b.Y); bSteps >= 0; bSteps--)
            {
                long aSteps = (prize.X - bSteps * b.X) / a.X;
                if (machine.IsSolution(aSteps, bSteps))
                {
                    yield return aSteps * 3 + bSteps;
                    yield break;
                }
            }
        }
    }

    private static Machine ToCorrectedMachine(this Machine machine) =>
        machine with { Prize = new(
            machine.Prize.X + 10000000000000,
            machine.Prize.Y + 10000000000000) };

    private static bool IsSolution(this Machine machine, long aSteps, long bSteps) =>
        aSteps * machine.A.X + bSteps * machine.B.X == machine.Prize.X &&
        aSteps * machine.A.Y + bSteps * machine.B.Y == machine.Prize.Y;

    private static IEnumerable<Machine> ReadMachines(this TextReader text)
    {
        var elements = new Coordinates[3];
        int current = 0;

        foreach (string line in text.ReadLines().Where(line => line.Length > 0))
        {
            elements[current++] = new(line.ParseLongsNoSign().ToPair());
            if (current < 3) continue;

            yield return new Machine(elements[0], elements[1], elements[2]);
            current = 0;
        }
    }

    record Machine(Coordinates A, Coordinates B, Coordinates Prize);

    record Coordinates(long X, long Y)
    {
        public Coordinates((long x, long y) pair) : this(pair.x, pair.y) { }
    }
}
