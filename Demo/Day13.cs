static class Day13
{
    public static void Run()
    {
        var machines = Console.In.ReadMachines().ToList();

        long totalCost = machines.SelectMany(GetCheapestPlay).ToCost();
        long farCost = machines.Select(ToCorrectedMachine).SelectMany(GetCheapestPlay).ToCost();

        Console.WriteLine($"    Total cost: {totalCost}");
        Console.WriteLine($"Corrected cost: {farCost}");
    }

    private static Machine ToCorrectedMachine(this Machine machine) =>
        machine with { Prize = new(machine.Prize.X + 10000000000000, machine.Prize.Y + 10000000000000) };

    private static long ToCost(this IEnumerable<(long aPresses, long bPresses)> buttonPresses) =>
        buttonPresses.Sum(pair => pair.aPresses * 3 + pair.bPresses);

    private static IEnumerable<(long aPresses, long bPresses)> GetCheapestPlay(this Machine machine)
    {
        // a * ax + b * bx = px
        // a * ay + b * by = py
        // --------------------
        // D = ax * by - ay * bx
        // a = (px * by - py * bx) / D   (D != 0)
        // b = (py * ax - px * ay) / D   (D != 0)
        // (a.k.a. Cramer's rule)

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

    private static IEnumerable<Machine> ReadMachines(this TextReader text) =>
        text.ReadCoordinateTriplets()
            .Select(coor => new Machine(coor[0], coor[1], coor[2]));

    private static IEnumerable<Coordinates[]> ReadCoordinateTriplets(this TextReader text) =>
        text.ReadCoordinates()
            .Select((coor, index) => (coor, index))
            .GroupBy(tuple => tuple.index / 3)
            .Select(group => group.Select(tuple => tuple.coor).ToArray());

    private static IEnumerable<Coordinates> ReadCoordinates(this TextReader text) =>
        text.ReadLines()
            .Where(line => line.Length > 0)
            .Select(Common.ParseLongsNoSign)
            .Select(values => new Coordinates(values[0], values[1]));

    record Coordinates(long X, long Y);

    record Machine(Coordinates A, Coordinates B, Coordinates Prize);
}