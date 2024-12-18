static class Day17
{
    public static void Run()
    {
        var machine = Console.In.ReadMachine();

        Console.WriteLine(machine);
        Console.WriteLine();

        var output = string.Join(",", machine.Run());
        Console.WriteLine($"Output: {output}");

        long? seed = machine.FindSelfReplicatingSeed(0, machine.Memory.Length - 1, 3);
        Console.WriteLine($"Self-replicating seed: {(seed.HasValue ? seed.ToString() : "N/A")}");
    }

    private static long? FindSelfReplicatingSeed(this Machine machine, long seed, int outputPosition, int stepShift)
    {
        for (int i = 0; i < 1 << stepShift; i++)
        {
            var candidate = (seed << stepShift) | (long)i;
            var output = machine.WithA(candidate).Run().ToArray();

            if (!Enumerable.SequenceEqual(output, machine.Memory[outputPosition..])) continue;

            if (outputPosition == 0) return candidate;

            var solution = FindSelfReplicatingSeed(machine, candidate, outputPosition - 1, stepShift);
            if (solution.HasValue) return solution;
        }

        return null;
    }

    private static Machine ReadMachine(this TextReader text)
    {
        var values = text.ReadLines()
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(4)
            .Select(line => line.ParseLongsNoSign())
            .ToArray();

        long a = values[0].First();
        long b = values[1].First();
        long c = values[2].First();
        long[] memory = values[3].ToArray();

        return Machine.Initialize(a, b, c, memory);
    }

    class Machine
    {
        public long A { get; private set; }
        public long B { get; private set; }
        public long C { get; private set; }
        public int Ip { get; private set; } = 0;
        public long[] Memory { get; private set; } = Array.Empty<long>();

        private Action[] Instructions { get; set; } = [];

        public static Machine Initialize(long a, long b, long c, long[] memory)
        {
            Machine machine = new() { A = a, B = b, C = c, Memory = memory };

            machine.Instructions = [
                machine.Adv, machine.Bxl, machine.Bst, machine.Jnz,
                machine.Bxc, machine.Out, machine.Bdv, machine.Cdv
            ];

            return machine;
        }

        public Machine StartFrom(int ip)
        {
            Machine copy = Initialize(A, B, C, Memory);
            copy.Ip = ip;
            return copy;
        }

        public Machine WithA(long a) =>
            Initialize(a, B, C, Memory);

        public IEnumerable<long> Run()
        {
            while (!Executed)
            {
                Step();
                if (Output.HasValue) yield return Output.Value;
                ClearOutput();
            }
        }

        public bool Executed => Ip >= Memory.Length;

        public void Step() => Operation();

        private Action Operation => Instructions[Memory[Ip]];

        public long? Output { get; private set; }

        public long? ClearOutput()
        {
            long? output = Output;
            Output = null;
            return output;
        }

        private void Adv() { A = A >> (int)(Combo & 31); Ip += 2; }
        private void Bxl() { B = B ^ Literal; Ip += 2; }
        private void Bst() { B = Combo & 0x7; Ip += 2; }
        private void Jnz() { Ip = A != 0 ? (int)Literal : Ip + 2; }
        private void Bxc() { B = B ^ C; Ip += 2; }
        private void Out() { Output = Combo & 0x7; Ip += 2; }
        private void Bdv() { B = A >> (int)(Combo & 31); Ip += 2; }
        private void Cdv() { C = A >> (int)(Combo & 31); Ip += 2; }

        private long Combo => Memory[Ip + 1] switch
        {
            >= 0 and <= 3 => Memory[Ip + 1],
            4 => A,
            5 => B,
            6 => C,
            _ => throw new InvalidOperationException("Invalid combo")
        };

        private long Literal => Memory[Ip + 1];

        public override string ToString() =>
            string.Join(Environment.NewLine,
                Enumerable.Range(0, Memory.Length / 2)
                    .Select(ip => InstructionToString(ip * 2)));

        private string InstructionToString(long ip) => Memory[ip] switch
        {
            0 => $"ADV: A = A >> {ComboToString(ip + 1)}",
            1 => $"BXL: B = B ^ {Memory[ip + 1]}",
            2 => $"BST: B = {ComboToString(ip + 1)} & 0x7",
            3 => $"JNZ: IP = A != 0 to {Memory[ip + 1]}",
            4 => $"BXC: B = B ^ C",
            5 => $"OUT: {ComboToString(ip + 1)} & 0x7",
            6 => $"BDV: B = A >> {ComboToString(ip + 1)}",
            7 => $"CDV: C = A >> {ComboToString(ip + 1)}",
            _ => "N/A"
        };

        private string ComboToString(long ip) =>
            Memory[ip] <= 3 ? Memory[ip].ToString()
            : ((char)('A' + (Memory[ip] - 4))).ToString();
    }
}
