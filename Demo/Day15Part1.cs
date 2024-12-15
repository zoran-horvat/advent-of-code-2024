static class Day15Part1
{
    public static void Run()
    {
        var state = Console.In.ReadMap().ToState();
        var instructions = Console.In.ReadInstructions().ToList();

        int totalGps = state.Simulate(instructions).Map.ToGps();

        Console.WriteLine($"Total GPS: {totalGps}");
    }

    private static void Print(this TextWriter writer, char[][] map) =>
        map.Select(row => new string(row)).ToList().ForEach(writer.WriteLine);

    private static State Simulate(this State state, IEnumerable<Direction> instructions) =>
        instructions.Aggregate(state.Copy(), (current, direction) => current.Move(direction));

    private static State Move(this State state, Direction direction)
    {
        var nextRobot = state.Robot.Move(direction);
        
        var pushEnd = nextRobot;
        while (state.Map.IsBox(pushEnd))
        {
            pushEnd = pushEnd.Move(direction);
        }

        if (!state.Map.IsEmpty(pushEnd)) return state;

        state.Map.Swap(pushEnd, nextRobot);
        state.Map.Swap(nextRobot, state.Robot);

        return state with { Robot = nextRobot };
    }

    private static State Copy(this State state) =>
        new(state.Map.Select(row => row.ToArray()).ToArray(), state.Robot);

    private static State ToState(this char[][] map) =>
        new(map, map.GetRobots().First());

    private static IEnumerable<Point> GetRobots(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == '@'
        select new Point(row, col);

    private static char[][] ReadMap(this TextReader text) =>
        text.ReadLines()
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Select(row => row.ToCharArray())
            .ToArray();

    private static IEnumerable<Direction> ReadInstructions(this TextReader text) =>
        text.ReadLines().SelectMany(line => line.Select(c => c.ToDirection()));

    private static void Swap(this char[][] map, Point a, Point b) =>
        (map[a.Row][a.Col], map[b.Row][b.Col]) = (map[b.Row][b.Col], map[a.Row][a.Col]);

    private static bool IsEmpty(this char[][] map, Point point) =>
        map[point.Row][point.Col] == '.';

    private static bool IsBox(this char[][] map, Point point) =>
        map[point.Row][point.Col] == 'O';

    private static Point Move(this Point point, Direction direction) =>
        new(point.Row + direction.RowStep, point.Col + direction.ColStep);

    private static int ToGps(this char[][] map) =>
        map.GetAllBoxes().Sum(ToGps);

    private static IEnumerable<Point> GetAllBoxes(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == 'O'
        select new Point(row, col);

    private static int ToGps(this Point point) => point.Row * 100 + point.Col;

    private static Direction ToDirection(this char direction) => direction switch
    {
        '>' => new(0, 1),
        '<' => new(0, -1),
        '^' => new(-1, 0),
        'v' => new(1, 0),
        _ => throw new ArgumentException($"Invalid direction: {direction}")
    };

    record State(char[][] Map, Point Robot);

    record Point(int Row, int Col);

    record Direction(int RowStep, int ColStep);
}
