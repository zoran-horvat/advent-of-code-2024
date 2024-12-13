static class Day13
{
    public static void Run()
    {
        var machines = Console.In.ReadMachines().ToList();

        long totalCost = machines
            .SelectMany(machine => machine.CheapestPlay())
            .Sum(play => play.cost);

        long farCost = machines
            .Select(machine => machine.GetCorrectedMachine())
            .SelectMany(machine => machine.CheapestPlay())
            .Sum(play => play.cost);

        Console.WriteLine($"Total cost: {totalCost}");
        Console.WriteLine($"  Far cost: {farCost}");
    }

    private static IEnumerable<Machine> ReadMachines(this TextReader text)
    {
        var lines = text.ReadLines().ToList();
        for (int i = 0; i < lines.Count; i += 4)
        {
            var buttonA = lines[i].ParseLongsNoSign().ToPair();
            var buttonB = lines[i + 1].ParseLongsNoSign().ToPair();
            var prize = lines[i + 2].ParseLongsNoSign().ToPair();

            yield return new Machine { A = buttonA, B = buttonB, Prize = prize };
        }
    }

    class Machine
    {
        public (long x, long y) A { get; init; }
        public (long x, long y) B { get; init; }
        public (long x, long y) Prize { get; init; }

        public IEnumerable<(long pressA, long pressB, long cost)> CheapestPlay()
        {
            long discriminant = A.x * B.y - A.y * B.x;
            if (discriminant != 0)
            {
                long aSteps = (Prize.x * B.y - Prize.y * B.x) / discriminant;
                long bSteps = B.x == 0 ? 0 : (Prize.x - aSteps * A.x) / B.x;

                if (IsSolution(aSteps, bSteps)) yield return (aSteps, bSteps, aSteps * 3 + bSteps);
            }
            else if (A.x == 0 || A.y == 0)
            {
                if (B.x == 0 || B.y == 0) yield break;
                if (IsSolution(0, Prize.x / B.x)) yield return (0, Prize.x / B.x, Prize.x / B.x * 3);
            }
            else if (B.x == 0 || B.y == 0)
            {
                if (IsSolution(Prize.y / A.y, 0)) yield return (Prize.y / A.y, 0, Prize.y / A.y * 3);
            }
            else
            {
                for (long bStep = Math.Min(Prize.x / B.x, Prize.y / B.y); bStep >= 0; bStep--)
                {
                    long aStep = (Prize.x - bStep * B.x) / A.x;
                    if (IsSolution(aStep, bStep))
                    {
                        yield return (aStep, bStep, aStep * 3 + bStep);
                        yield break;
                    }
                }
            }
        }

        private bool IsSolution(long aSteps, long bSteps) =>
            aSteps * A.x + bSteps * B.x == Prize.x &&
            aSteps * A.y + bSteps * B.y == Prize.y;

        public Machine GetCorrectedMachine() =>
            new() { A = A, B = B, Prize = (Prize.x + 10000000000000, Prize.y + 10000000000000) };
    }
}
