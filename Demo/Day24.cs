using System.Text.RegularExpressions;

static class Day24
{
    public static void Run()
    {
        var input = Console.In.ReadInputs();
        var circuit = Console.In.ReadCircuit();

        long number = circuit.GetOutputNumber(input.Values.ToArray());

        circuit.Print();

        var (fixedCircuit, swaps) = circuit.FixAdder();

        var swappedPins = swaps
            .SelectMany(swap => new[] { swap.oldPin, swap.newPin })
            .Select(pin => pin.Label)
            .Order();

        Console.WriteLine($"      Output: {number}");
        Console.WriteLine($"Swapped pins: {string.Join(",", swappedPins)}");

        Console.WriteLine();
        foreach (var swap in swaps) Console.WriteLine($"    Swap {swap.oldPin.Label} <-> {swap.newPin.Label}");
    }

    private static (Circuit fixedCircuit, List<(Pin oldPin, Pin newPin)> swaps) FixAdder(this Circuit circuit)
    {
        var remainingGates = circuit.Gates.Values.ToList();
        var fixedGates = new List<Gate>();
        int inputBits = circuit.GetInputBitsCount();

        // y01 AND x01 -> ttt
        // y02 XOR x02 -> hqj
        // mpf AND mqs -> mjj
        // ttt OR  mjj -> jwf
        // jwf XOR hqj -> z02

        // y02 AND x02 -> tjn
        // x03 XOR y03 -> htb
        // hqj AND jwf -> cfv   (hqj: prior x XOR y output; jwf: prior OR output)
        // tjn OR  cfv -> vjg
        // vjg XOR htb -> z03

        List<(Pin oldPin, Pin newPin)> swaps = new();

        (Gate? priorInputXor, remainingGates) = remainingGates.Pick(Pin.Create("x02"), Pin.Create("y02"), null, typeof(XorGate));
        if (priorInputXor is null) throw new InvalidOperationException();

        (Gate? priorOutXor, remainingGates) = remainingGates.Pick(null, null, Pin.Create("z02"), typeof(XorGate));
        if (priorOutXor is null) throw new InvalidOperationException();

        (Gate? priorOr, remainingGates) = remainingGates.Pick(null, null, priorOutXor.OtherInput(priorInputXor.Out), typeof(OrGate));
        if (priorOr is null) throw new InvalidOperationException();

        fixedGates.AddRange([priorInputXor, priorOutXor, priorOr]);

        int position = ((PositionalPin)priorOutXor.Out).Position;

        while (position < inputBits - 1)
        {
            position += 1;

            (Gate? inputAnd, remainingGates) = remainingGates.Pick(Pin.Create($"x{position - 1:00}"), Pin.Create($"y{position - 1:00}"), null, typeof(AndGate));
            if (inputAnd is null) throw new InvalidOperationException();

            (Gate? inputXor, remainingGates) = remainingGates.Pick(Pin.Create($"x{position:00}"), Pin.Create($"y{position:00}"), null, typeof(XorGate));
            if (inputXor is null) throw new InvalidOperationException();

            (Gate? andGate, remainingGates) = remainingGates.Pick(priorInputXor.Out, priorOr.Out, null, typeof(AndGate));
            if (andGate is null) throw new InvalidOperationException();

            (Gate? orGate, remainingGates) = remainingGates.Pick(inputAnd.Out, null, null, typeof(OrGate));
            if (orGate is null) (orGate, remainingGates) = remainingGates.Pick(null, andGate.Out, null, typeof(OrGate));
            if (orGate is null) throw new InvalidOperationException();

            if (!orGate.HasInput(inputAnd.Out))
            {   // There is the problem with inputAnd.Out - swap it
                (inputAnd, remainingGates, Pin oldPin, Pin newPin) = inputAnd.Swap(orGate.OtherInput(inputAnd.Out), remainingGates);
                swaps.Add((oldPin, newPin));
            }

            if (!orGate.HasInput(andGate.Out))
            {   // There is the problem with andGate.Out - swap it
                (andGate, remainingGates, Pin oldPin, Pin newPin) = andGate.Swap(orGate.OtherInput(andGate.Out), remainingGates);
                swaps.Add((oldPin, newPin));
            }

            Pin outputPin = Pin.Create($"z{position:00}");
            (Gate? outXor, remainingGates) = remainingGates.Pick(orGate.Out, null, null, typeof(XorGate));
            if (outXor is null) (outXor, remainingGates) = remainingGates.Pick(inputXor.Out, null, null, typeof(XorGate));
            if (outXor is null) (outXor, remainingGates) = remainingGates.Pick(null, null, outputPin, typeof(XorGate));
            if (outXor is null) break;

            if (!outXor.HasInput(orGate.Out) && outXor.HasInput(inputXor.Out))
            {   // There is the problem with orGate.Out - swap it
                (orGate, remainingGates, Pin oldPin, Pin newPin) = orGate.Swap(outXor.OtherInput(orGate.Out), remainingGates);
                swaps.Add((oldPin, newPin));
            }

            if (!outXor.HasInput(inputXor.Out) && outXor.HasInput(orGate.Out))
            {   // There is the problem with inputXor.Out - swap it
                (inputXor, remainingGates, Pin oldPin, Pin newPin) = inputXor.Swap(outXor.OtherInput(orGate.Out), remainingGates);
                swaps.Add((oldPin, newPin));
            }

            if (outXor.Out != outputPin)
            {   // There is the problem with outputXor.Out - swap it
                (outXor, remainingGates, Pin oldPin, Pin newPin) = outXor.Swap(outputPin, remainingGates);
                swaps.Add((oldPin, newPin));
            }

            priorInputXor = inputXor;
            priorOutXor = outXor;
            priorOr = orGate;
        }

        var fixedCircuit = new Circuit(fixedGates.Concat(remainingGates).ToDictionary(gate => gate.Out, gate => gate));

        return (fixedCircuit, swaps);
    }

    private static (Gate swappedGate, List<Gate> otherGates, Pin oldPin, Pin newPin) Swap(this Gate gate, Pin newOutput, List<Gate> otherGates) =>
        (gate with { Out = newOutput }, otherGates.Select(other => other.Swap(gate.Out, newOutput)).ToList(), gate.Out, newOutput);

    private static Pin OtherInput(this Gate gate, Pin pin) =>
        gate.In1 == pin ? gate.In2 : gate.In1;

    private static bool HasInput(this Gate gate, Pin pin) =>
        gate.In1 == pin || gate.In2 == pin;

    private static Gate Swap(this Gate gate, Pin out1, Pin out2) =>
        gate.Out != out1 && gate.Out != out2 ? gate
        : gate with { Out = gate.Out == out1 ? out2 : out1 };

    private static (Gate? gate, List<Gate> remaining) Pick(this List<Gate> gates, Pin? in1, Pin? in2, Pin? @out, Type operation)
    {
        foreach (var gate in gates)
        {
            if (gate.GetType() != operation) continue;
            if (in1 is not null && gate.In1 != in1 && gate.In2 != in1) continue;
            if (in2 is not null && gate.In1 != in2 && gate.In2 != in2) continue;
            if (@out is not null && gate.Out != @out) continue;

            return (gate, gates.Except(new[] { gate }).ToList());
        }

        return (null, gates);
    }

    private static void Print(this Circuit circuit)
    {
        foreach (var (pin, gates) in circuit.GetBitAdders())
        {
            Console.WriteLine($"{pin.Label}: {string.Join(", ", gates.Select(ToPrintable))}");
        }
    }

    private static string ToPrintable(this Gate gate) =>
        $"{gate.In1.Label} " +
        $"{gate.GetType().Name.Replace("Gate", "").ToUpper()} " +
        $"{gate.In2.Label} -> {gate.Out.Label}";


    private static IEnumerable<(PositionalPin output, List<Gate> gates)> GetBitAdders(this Circuit circuit)
    {
        var outputs = circuit.GetPinGroup('z').OrderBy(pin => pin.Position);
        var visitedGates = new HashSet<Gate>();

        foreach (var pin in outputs)
        {
            var segment = pin.FindSources(circuit).Concat([ pin ])
                .Where(circuit.Gates.ContainsKey)
                .Select(pin => circuit.Gates[pin])
                .Where(gate => !visitedGates.Contains(gate))
                .ToList();

            foreach (var gate in segment) visitedGates.Add(gate);

            yield return (pin, segment);
        }
    }

    private static IEnumerable<Pin> FindSources(this Pin pin, Circuit circuit)
    {
        if (!circuit.Gates.ContainsKey(pin)) yield break;

        var visited = new HashSet<Pin>();

        var (in1, in2, _) = circuit.Gates[pin];
        var queue = new Queue<Pin>([ in1, in2 ]);

        while (queue.Count > 0)
        {
            Pin current = queue.Dequeue();

            if (visited.Contains(current)) continue;
            visited.Add(current);

            yield return current;
            if (!circuit.Gates.ContainsKey(current)) continue;

            (in1, in2, _) = circuit.Gates[current];
            queue.Enqueue(in1);
            queue.Enqueue(in2);
        }
    }

    private static int GetInputBitsCount(this Circuit circuit) =>
        circuit.GetPinGroup('x').Concat(circuit.GetPinGroup('y')).Max(pin => pin.Position) + 1;

    private static long GetOutputNumber(this Circuit circuit, Signal[] input) =>
        circuit.GetOutput(input)
            .Where(signal => signal.Value)
            .Select(signal => signal.Pin)
            .OfType<PositionalPin>()
            .Aggregate(0L, (value, pin) => value | (1L << pin.Position));

    private static IEnumerable<Signal> GetOutput(this Circuit circuit, Signal[] input)
    {
        var signals = input.ToDictionary(signal => signal.Pin, signal => signal);
        foreach (var outputPin in circuit.GetPinGroup('z'))
        {
            yield return outputPin.Evaluate(circuit, signals);
        }
    }

    private static IEnumerable<PositionalPin> GetPinGroup(this Circuit circuit, char group) =>
        circuit.Gates.Values
            .SelectMany(gate => new[] { gate.In1, gate.In2, gate.Out })
            .OfType<PositionalPin>()
            .Where(pin => pin.Group == group)
            .Distinct();

    private static Signal Evaluate(this Pin pin, Circuit circuit, Dictionary<Pin, Signal> signals)
    {
        if (signals.ContainsKey(pin)) return signals[pin];

        var gate = circuit.Gates[pin];
        var in1 = gate.In1.Evaluate(circuit, signals);
        var in2 = gate.In2.Evaluate(circuit, signals);

        var result = gate switch
        {
            AndGate => new Signal(gate.Out, in1.Value & in2.Value),
            OrGate => new Signal(gate.Out, in1.Value | in2.Value),
            XorGate => new Signal(gate.Out, in1.Value ^ in2.Value),
            _ => throw new ArgumentException()
        };

        signals[result.Pin] = result;
        return result;
    }

    private static Circuit ReadCircuit(this TextReader text) =>
        new(text.ReadGates().ToDictionary(gate => gate.Out, gate => gate));

    private static IEnumerable<Gate> ReadGates(this TextReader text) =>
        text.ReadLines()
            .Select(line => Regex.Match(line, @"(?<in1>[^\s]+)\s(?<op>AND|OR|XOR)\s(?<in2>[^\s]+)\s->\s(?<pin>.+)"))
            .Where(match => match.Success)
            .Select(match => (
                in1: Pin.Create(match.Groups["in1"].Value),
                in2: Pin.Create(match.Groups["in2"].Value),
                op: match.Groups["op"].Value,
                @out: Pin.Create(match.Groups["pin"].Value)))
            .Select(line => line.op switch
            {
                "AND" => new AndGate(line.in1, line.in2, line.@out) as Gate,
                "OR" => new OrGate(line.in1, line.in2, line.@out),
                "XOR" => new XorGate(line.in1, line.in2, line.@out),
                _ => throw new ArgumentException()
            });

    private static Dictionary<Pin, Signal> ReadInputs(this TextReader text) =>
        text.ReadLines().TakeWhile(line => !string.IsNullOrEmpty(line))
            .Select(line => (pin: line[..line.IndexOf(':')], value: line[^1] == '1'))
            .Select(line => (pin: Pin.Create(line.pin), line.value))
            .ToDictionary(line => line.pin, line => new Signal(line.pin, line.value));

    record Circuit(Dictionary<Pin, Gate> Gates);

    record Signal(Pin Pin, bool Value);

    abstract record Gate(Pin In1, Pin In2, Pin Out);

    record AndGate(Pin In1, Pin In2, Pin Out) : Gate(In1, In2, Out);
    record OrGate(Pin In1, Pin In2, Pin Out) : Gate(In1, In2, Out);
    record XorGate(Pin In1, Pin In2, Pin Out) : Gate(In1, In2, Out);

    abstract record Pin(string Label)
    {
        public static Pin Create(string label) =>
            int.TryParse(label[1..], out int position) ? new PositionalPin(label, label[0], position)
            : new InnerPin(label);
    }

    record InnerPin(string Label) : Pin(Label);
    record PositionalPin(string Label, char Group, int Position) : Pin(Label);
}
