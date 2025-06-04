using System.Diagnostics;

static class Day15
{
    public static void Run()
    {
        var state = Console.In.ReadState();
        var steps = Console.In.ReadSteps().ToList();

        Stopwatch stopwatch = Stopwatch.StartNew();
        var totalGps = state.Apply(steps).Boxes.Index.Values.Sum(GetGps);
        var scaledGps = state.Scale().Apply(steps).Boxes.Index.Values.Sum(GetGps) / 2;
        stopwatch.Stop();

        Console.WriteLine($" Total GPS: {totalGps}");
        Console.WriteLine($"Scaled GPS: {scaledGps}");
        Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
    }

    private static State Scale(this State state) =>
        new State(
            state.Robot.ScaleRobot(),
            state.Boxes.Index.Values.Select(box => box.Position.ScaleBox()).ToBoxes(),
            state.Walls.SelectMany(ScaleWall).ToHashSet());
    
    private static Point ScaleRobot(this Point robot) =>
        new(robot.Row, robot.Column * 2);

    private static Box ScaleBox(this Point box) =>
        new(new(box.Row, box.Column * 2), 2);
    
    private static IEnumerable<Point> ScaleWall(this Point wall) =>
        [ wall with { Column = wall.Column * 2 }, wall with { Column = wall.Column * 2 + 1 } ];

    private static int GetGps(this Box box) =>
        box.Position.Row * 100 + box.Position.Column;

    private static State Apply(this State state, IEnumerable<Step> steps) =>
        steps.Aggregate(state, Apply);

    private static State Apply(this State state, Step step) =>
        state.GetMove(step).ApplyTo(state);

    private static State ApplyTo(this (Step step, IEnumerable<Box> boxes) move, State state) =>
        new State(
            state.Robot.Apply(move.step),
            state.Boxes.Index.Values.Except(move.boxes).Concat(move.boxes.Apply(move.step)).ToBoxes(),
            state.Walls);

    private static IEnumerable<Box> Apply(this IEnumerable<Box> boxes, Step step) =>
        boxes.Select(box => box with { Position = box.Position.Apply(step) });
    
    private static Point Apply(this Point point, Step step) =>
        new(point.Row + step.RowDiff, point.Column + step.ColumnDiff);

    private static (Step step, IEnumerable<Box> boxes) GetMove(this State state, Step step)
    {
        var boxesToMove = new HashSet<Box>();

        var pushing = new List<Point>() { state.Robot.Apply(step) };
        while (pushing.Count > 0)
        {
            if (pushing.Any(state.Walls.Contains)) return (new Step(0, 0), Enumerable.Empty<Box>());
            pushing = state.Boxes.GetBoxes(pushing)
                .Where(boxesToMove.Add)
                .SelectMany(ToPositions)
                .Select(pair => pair.position.Apply(step))
                .ToList();
        }

        return (step, boxesToMove);
    }

    private static State ReadState(this TextReader reader) =>
        reader.ReadMap().ToState();

    private static State ToState(this char[][] map) =>
        new(map.FindRobot(), map.FindBoxes().ToBoxes(), map.FindWalls().ToHashSet());

    private static Point FindRobot(this char[][] map) =>
        map.PositionsOf('@').Single();

    private static IEnumerable<Box> FindBoxes(this char[][] map) =>
        map.PositionsOf('O').Select(pos => new Box(pos, 1));
    
    private static IEnumerable<Point> FindWalls(this char[][] map) =>
        map.PositionsOf('#');
    
    private static IEnumerable<Point> PositionsOf(this char[][] map, char c) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == c
        select new Point(row, col);

    private static IEnumerable<Step> ReadSteps(this TextReader reader) =>
        reader.ReadLines().SelectMany(line => line.ToCharArray()).Select(ToStep);

    private static Step ToStep(this char arrow) => arrow switch
    {
        '>' => new(0, 1),
        '<' => new(0, -1),
        '^' => new(-1, 0),
        _ => new(1, 0),
    };

    private static char[][] ReadMap(this TextReader reader) =>
        reader.ReadLines()
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.ToCharArray())
            .ToArray();
    
    private static IEnumerable<Box> GetBoxes(this Boxes boxes, IEnumerable<Point> positions) =>
        positions.Where(boxes.Index.ContainsKey)
            .Select(pos => boxes.Index[pos])
            .Distinct();

    private static Boxes ToBoxes(this IEnumerable<Box> boxes) =>
        new Boxes(boxes.SelectMany(ToPositions).ToDictionary(pair => pair.position, pair => pair.box));

    private static IEnumerable<(Point position, Box box)> ToPositions(this Box box) =>
        Enumerable.Range(0, box.Size)
            .Select(i => (new Point(box.Position.Row, box.Position.Column + i), box));
        
    record State(Point Robot, Boxes Boxes, HashSet<Point> Walls);

    record Boxes(Dictionary<Point, Box> Index);

    record struct Box(Point Position, int Size);

    record struct Step(int RowDiff, int ColumnDiff);

    record struct Point(int Row, int Column);
}
