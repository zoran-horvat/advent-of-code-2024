using System.Diagnostics;

static class Day15
{
    public static void Run()
    {
        var state = Console.In.ReadMap().ToState();
        var instructions = Console.In.ReadInstructions().ToList();

        Stopwatch sw = Stopwatch.StartNew();

        var finalState = state.Simulate(instructions);
        var finalStateScaled = state.ScaledUp().Simulate(instructions);

        int totalGps = finalState.ToGps();
        int scaledGps = finalStateScaled.ToGps();
        sw.Stop();

        Console.WriteLine($"       Total GPS: {totalGps}");
        Console.WriteLine($"Total scaled GPS: {scaledGps}");
        Console.WriteLine($"    Completed in: {sw.ElapsedMilliseconds} ms");

        Console.Out.Print(finalState.ToMap());
        Console.Out.Print(finalStateScaled.ToMap());
    }

    private static void Print(this TextWriter writer, char[][] map) =>
        map.Select(row => new string(row)).ToList().ForEach(writer.WriteLine);

    private static char[][] ToMap(this State state)
    {
        int width = state.Walls.Max(point => point.Col) + 1;
        int height = state.Walls.Max(point => point.Row) + 1;

        char[][] map = Enumerable.Range(0, height).Select(_ => Enumerable.Repeat('.', width).ToArray()).ToArray();

        state.Walls.ForEach(point => map[point.Row][point.Col] = '#');

        void drawBox(Box box)
        {
            if (box.Points.Length == 1)
            {
                map[box.Points[0].Row][box.Points[0].Col] = 'O';
            }
            else
            {
                map[box.Points[0].Row][box.Points[0].Col] = '[';
                map[box.Points[1].Row][box.Points[1].Col] = ']';
            }
        }

        map[state.Robot.Row][state.Robot.Col] = '@';

        state.Boxes.ForEach(drawBox);

        return map;
    }

    private static State Simulate(this State state, IEnumerable<Direction> instructions) =>
        instructions.Aggregate(state.Copy(), (current, direction) => current.Move(direction));

    private static State Move(this State state, Direction direction)
    {
        var pressingAt = state.Robot.Move(direction);
        if (state.Walls.Contains(pressingAt)) return state;

        var boxesToPush = state.GetPushingGroup(direction).ToList();
        if (!boxesToPush.CanMoveAll(direction, state)) return state;

        var boxesToStay = state.Boxes.ToHashSet().Except(boxesToPush);

        return state with
        {
            Boxes =  boxesToPush.MoveAll(direction).Concat(boxesToStay).ToArray(),
            Robot = pressingAt
        };

        // Same thing, mutable:
        // var pressingAt = state.Robot.Move(direction);
        // if (state.Walls.Contains(pressingAt)) return state;

        // var boxesToPush = state.GetPushingGroup(direction).ToHashSet();
        // if (!boxesToPush.CanMoveAll(direction, state)) return state;

        // for (int i = 0; i < state.Boxes.Length; i++)
        // {
        //     if (!boxesToPush.Contains(state.Boxes[i])) continue;
        //     state.Boxes[i] = state.Boxes[i].Move(direction);
        // }

        // return state with { Robot = pressingAt };
    }

    private static IEnumerable<Point> GetRobots(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == '@'
        select new Point(row, col);

    private static IEnumerable<Box> MoveAll(this IEnumerable<Box> boxes, Direction direction) =>
        boxes.Select(box => box.Move(direction));

    private static bool CanMoveAll(this IEnumerable<Box> boxes, Direction direction, State state) =>
        boxes.All(box => box.CanMove(direction, state));

    private static IEnumerable<Box> GetPushingGroup(this State state, Direction direction)
    {
        HashSet<Box> notPushingBoxes = state.Boxes.ToHashSet();
        Dictionary<Point, Box> pointToBox = state.Boxes.SelectMany(GetPoints).ToDictionary();

        Queue<Point> pressurePoints = new([state.Robot.Move(direction)]);

        while (pressurePoints.Count > 0)
        {
            Point current = pressurePoints.Dequeue();
            if (!pointToBox.TryGetValue(current, out Box? pushingBox)) continue;

            if (notPushingBoxes.Contains(pushingBox))
            {
                notPushingBoxes.Remove(pushingBox);
                yield return pushingBox;
            }

            pushingBox.Points
                .Select(point => point.Move(direction))
                .Where(point => !pushingBox.Points.Contains(point))
                .ForEach(pressurePoints.Enqueue);
        }
    }

    private static IEnumerable<(Point point, Box box)> GetPoints(this Box box) =>
        box.Points.Select(point => (point, box));

    private static HashSet<Box> ToHashSet(this IEnumerable<Box> boxes) =>
        new HashSet<Box>(boxes,
            EqualityComparer<Box>.Create(
                (a, b) => a?.Points[0] == b?.Points[0],
                box => box?.Points[0].GetHashCode() ?? 0));

    private static Box Move(this Box box, Direction direction) =>
        new(box.Points.Select(point => point.Move(direction)).ToArray());

    private static bool CanMove(this Box box, Direction direction, State state) =>
        box.Points.All(point => !state.Walls.Contains(point.Move(direction)));

    private static Point Move(this Point point, Direction direction) =>
        new(point.Row + direction.RowStep, point.Col + direction.ColStep);

    private static int ToGps(this State state) => state.Boxes.Sum(ToGps);

    private static int ToGps(this Box box) => box.Points[0].ToGps();

    private static int ToGps(this Point point) => point.Row * 100 + point.Col;

    private static Direction ToDirection(this char direction) => direction switch
    {
        '>' => new(0, 1),
        '<' => new(0, -1),
        '^' => new(-1, 0),
        'v' => new(1, 0),
        _ => throw new ArgumentException($"Invalid direction: {direction}")
    };

    private static State ScaledUp(this State state) =>
        new(state.Boxes.Select(ScaledUp).ToArray(),
            state.Robot.ScaledUpRobot(),
            state.Walls.SelectMany(ScaledUpWall).ToHashSet());

    private static Box ScaledUp(this Box box) => box.Points switch
    {
        [ (int row, int col) ] => new Box([ new(row, 2 * col), new(row, 2 * col + 1) ]),
        _ => throw new ArgumentException("The box is already scaled")
    };

    private static Point ScaledUpRobot(this Point point) =>
        new(point.Row, 2 * point.Col);

    private static Point[] ScaledUpWall(this Point wall) =>
        [ new(wall.Row, 2 * wall.Col), new(wall.Row, 2 * wall.Col + 1) ];

    private static State Copy(this State state) =>
        state with { Boxes = state.Boxes.ToArray() };

    private static State ToState(this char[][] map) =>
        new(map.GetBoxes().ToArray(), map.GetRobots().First(), map.GetWalls().ToHashSet());

    private static IEnumerable<Point> GetWalls(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == '#'
        select new Point(row, col);

    private static IEnumerable<Box> GetBoxes(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where map[row][col] == 'O'
        select new Box([ new Point(row, col) ]);

    private static char[][] ReadMap(this TextReader text) =>
        text.ReadLines()
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Select(row => row.ToCharArray())
            .ToArray();

    private static IEnumerable<Direction> ReadInstructions(this TextReader text) =>
        text.ReadLines().SelectMany(line => line.Select(c => c.ToDirection()));

    record Box(Point[] Points);

    record State(Box[] Boxes, Point Robot, HashSet<Point> Walls);

    record Point(int Row, int Col);

    record Direction(int RowStep, int ColStep);
}
