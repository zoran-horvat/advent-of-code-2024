static class Day17
{
    public static void Run()
    {
        var machine = Console.In.ReadMachine();

        Console.WriteLine();
        machine.Print();
        Console.WriteLine();

        var output = machine.Run().ToArray();
        long seed = machine.FindSelfReplicatingSeeds(0).Min();

        Console.WriteLine($"               Output: {string.Join(",", output)}");
        Console.WriteLine($"Self-replicating seed: {seed}");
    }

    private static IEnumerable<long> FindSelfReplicatingSeeds(this Machine machine, int offset) =>
        machine
            .GetHigherSeeds(offset)
            .SelectMany(ExtendSeed)
            .Where(seed => machine.Run(seed).SequenceEqual(machine.Memory[offset..]));

    private static IEnumerable<long> ExtendSeed(long seed) =>
        Enumerable.Range(0, 8).Select(lower => (seed << 3) | (long)lower);

    private static IEnumerable<long> GetHigherSeeds(this Machine machine, int offset) =>
        offset == machine.Memory.Length - 1 ? [0]
        : machine.FindSelfReplicatingSeeds(offset + 1).ToArray();

    private static IEnumerable<byte> Run(this Machine machine, long seed) =>
        (machine with { A = seed, B = 0, C = 0, IP = 0 }).Run();

    private static long? FindSelfReplicatingSeed(this Machine machine, long seed, int outputPosition, int stepShift)
    {
        for (int i = 0; i < 1 << stepShift; i++)
        {
            var candidate = (seed << stepShift) | (long)i;
            var output = (machine with { A = candidate }).Run().ToArray();

            if (!Enumerable.SequenceEqual(output, machine.Memory[outputPosition..])) continue;

            if (outputPosition == 0) return candidate;

            var solution = FindSelfReplicatingSeed(machine, candidate, outputPosition - 1, stepShift);
            if (solution.HasValue) return solution;
        }

        return null;
    }

    private static void Print(this Machine machine) =>
        machine.GetProgram().Select(ToPrintable).ToList().ForEach(Console.WriteLine);

    private static IEnumerable<Instruction> GetProgram(this Machine machine)
    {
        while (true)
        {
            (var instruction, machine) = machine.GetNextInstruction();
            if (instruction is null) yield break;
            yield return instruction;
        }
    }

    private static string ToPrintable(this Instruction instruction) =>
        instruction.Opcode switch
        {
            0 => $"A <- A >> {instruction.Operand.ToPrintableCombo()}",
            1 => $"B <- B XOR {instruction.Operand}",
            2 => $"B <- {instruction.Operand.ToPrintableCombo()} MOD 8",
            3 => $"IP <- {instruction.Operand} (IF A = 0)",
            4 => "B <- B XOR C",
            5 => $"Output {instruction.Operand.ToPrintableCombo()} MOD 8",
            6 => $"B <- A >> {instruction.Operand.ToPrintableCombo()}",
            7 => $"C <- A >> {instruction.Operand.ToPrintableCombo()}",
            _ => throw new InvalidOperationException("Invalid opcode")
        };

    private static string ToPrintableCombo(this byte operand) =>
        operand switch
        {
            <= 3 => $"{operand}",
            4 => "A",
            5 => "B",
            6 => "C",
            _ => throw new InvalidOperationException("Invalid combo")
        };

    private static IEnumerable<byte> Run(this Machine machine)
    {
        while (true)
        {
            (var instruction, machine) = machine.GetNextInstruction();
            if (instruction is null) yield break;

            var operand = instruction.ResolveOperand(machine);

            (var output, machine) = instruction.Execute(machine);
            if (output.HasValue) yield return output.Value;
        }
    }

    private static (byte? output, Machine machine) Execute(this Instruction instruction, Machine machine) =>
        (instruction.Opcode, instruction.ResolveOperand(machine)) switch
        {
            (0, long op) => (null, machine with { A = machine.A >> (int)op }),
            (1, long op) => (null, machine with { B = machine.B ^ op }),
            (2, long op) => (null, machine with { B = op & 0x7 }),
            (3, long op) => (null, machine.A == 0 ? machine : machine with { IP = (int)op }),
            (4, _) => (null, machine with { B = machine.B ^ machine.C }),
            (5, long op) => ((byte)(op & 0x7), machine),
            (6, long op) => (null, machine with { B = machine.A >> (int)op }),
            (7, long op) => (null, machine with { C = machine.A >> (int)op }),
            _ => throw new InvalidOperationException("Invalid opcode")
        };

    private static long ResolveOperand(this Instruction instruction, Machine machine) =>
        instruction.Opcode == 4 ? 0
        : instruction.Opcode == 1 || instruction.Opcode == 3 ? (int)instruction.Operand
        : instruction.ResolveCombo(machine);

    private static long ResolveCombo(this Instruction instruction, Machine machine) =>
        instruction.Operand switch
        {
            <= 3 => (int)instruction.Operand,
            4 => machine.A,
            5 => machine.B,
            6 => machine.C,
            _ => throw new InvalidOperationException("Invalid combo")
        };

    private static (Instruction? instruction, Machine machine) GetNextInstruction(this Machine machine) =>
        machine.IP >= machine.Memory.Length ? (null, machine)
        : (machine.FetchInstruction(), machine.AdvanceIP()); 

    private static Instruction FetchInstruction(this Machine machine) =>
        new(machine.Memory[machine.IP], machine.Memory[machine.IP + 1]);
    
    private static Machine AdvanceIP(this Machine machine) =>
        machine with { IP = machine.IP + 2 };

    private static IEnumerable<(string field, int[] values)> ParseInput(this TextReader text) =>
        text.ReadLines()
            .Select(line => line.Split(": "))
            .Where(parts => parts.Length == 2)
            .Select(parts => (
                field: parts[0],
                values: parts[1].Split(',').Select(int.Parse).ToArray()));

    private static Machine ReadMachine(this TextReader text) =>
        text.ParseInput().Aggregate(
            new Machine(0, 0, 0, 0, Array.Empty<byte>()),
            (machine, fieldValues) =>
            {
                return fieldValues.field switch
                {
                    "Register A" => machine with { A = fieldValues.values[0] },
                    "Register B" => machine with { B = fieldValues.values[0] },
                    "Register C" => machine with { C = fieldValues.values[0] },
                    _ => machine with { Memory = fieldValues.values.Select(v => (byte)v).ToArray() },
                };
            }
        );

    record Machine(long A, long B, long C, int IP, byte[] Memory);
    
    record Instruction(byte Opcode, byte Operand);
}