using System.Diagnostics;

static class Day07
{
    public static void Run()
    {
        var equations = Console.In.ReadEquations().ToList();

        Stopwatch stopwatch = Stopwatch.StartNew();
        var simpleCalibration = equations
            .Where(eq => eq.CanProduceResult(Addition, Multiplication))
            .Sum(eq => eq.Result);
        stopwatch.Stop();

        Stopwatch stopwatch1 = Stopwatch.StartNew();
        var extendedCalibration = equations
            .Where(eq => eq.CanProduceResult(Addition, Multiplication, Concatenation))
            .Sum(eq => eq.Result);
        stopwatch1.Stop();
        
        Console.WriteLine($" Simple calibration result: {simpleCalibration} in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Extended calibration result: {extendedCalibration} in {stopwatch1.ElapsedMilliseconds} ms");
    }

    private static bool CanProduceResult(this Equation equation, params Operator[] operators)
    {
        HashSet<long> produced = [ equation.Values[0] ];

        foreach (var value in equation.Values[1..])
        {
            var expanded = operators.SelectMany(op => op(equation, produced, value));
            produced = [ ..expanded ];
        }

        return produced.Contains(equation.Result);
    }

    private delegate IEnumerable<long> Operator(Equation equation, IEnumerable<long> a, long b);

    private static IEnumerable<long> Addition(this Equation equation, IEnumerable<long> a, long b) =>
        a.Where(x => equation.Result - x >= b).Select(x => x + b);

    private static IEnumerable<long> Multiplication(this Equation equation, IEnumerable<long> a, long b) =>
        a.Where(x => equation.Result / x >= b).Select(x => x * b);
        
    private static IEnumerable<long> Concatenation(this Equation equation, IEnumerable<long> a, long b)
    {
        string bString = b.ToString();
        foreach (long x in a)
        {
            string concatenated = x.ToString() + bString;
            if (long.TryParse(concatenated, out long value) && value <= equation.Result)
                yield return value;
        }
    }

    private static IEnumerable<Equation> ReadEquations(this TextReader reader) =>
        reader.ReadLines()
            .Select(Common.ParseLongsNoSign)
            .Select(values => new Equation(values[0], values[1..]));

    record Equation(long Result, List<long> Values);
}
