static class Day13
{
    public static void Run()
    {
        var machines = Console.In.ReadMachines().ToList();

        long totalCost = machines.SelectMany(GetCheapestPlay).ToCost();
        long correctedCost = machines.Select(ToCorrectedMachine).SelectMany(GetCheapestPlay).ToCost();

        Console.WriteLine($"Total cost: {totalCost}");
        Console.WriteLine($"Corrected cost: {correctedCost}");
    }

    private static long ToCost(this IEnumerable<(long aPresses, long bPresses)> buttonPresses) =>
        buttonPresses.Sum(pair => pair.aPresses * 3 + pair.bPresses);

    private static IEnumerable<(long aPresses, long bPresses)> GetCheapestPlay(this Machine machine)
    {
        // a * ax + b * bx = px
        // a * ay + b * by = py
        // --------------------
        // D = ax * by - ay * bx
        // a = (px * by - py * bx) / D
        // b = (ax * py - ay * px) / D
        
        var (a, b, prize) = machine;

        long determinant = a.X * b.Y - a.Y * b.X;

        if (determinant == 0) return Enumerable.Empty<(long, long)>();

        long aPresses = (prize.X * b.Y - prize.Y * b.X) / determinant;
        return machine.ToButtonPresses(aPresses);
    }

    private static IEnumerable<(long aPresses, long bPresses)> ToButtonPresses(this Machine machine, long aPresses)
    {
        long bPresses = (machine.Prize.X - aPresses * machine.A.X) / machine.B.X;

        if (aPresses * machine.A.X + bPresses * machine.B.X != machine.Prize.X) yield break;
        if (aPresses * machine.A.Y + bPresses * machine.B.Y != machine.Prize.Y) yield break;

        yield return (aPresses, bPresses);
    }

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

    private static Machine ToCorrectedMachine(this Machine machine) =>
        machine with { Prize = new(
            machine.Prize.X + 10000000000000,
            machine.Prize.Y + 10000000000000) };

    record Machine(Coordinates A, Coordinates B, Coordinates Prize);

    record Coordinates(long X, long Y)
    {
        public Coordinates((long x, long y) pair) : this(pair.x, pair.y) { }
    }
}
