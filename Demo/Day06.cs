using System.Diagnostics;

static class Day06
{
    public static void Run()
    {
        char[][] map = Console.In.ReadLines()
            .Select(row => row.ToCharArray())
            .ToArray();

        Stopwatch stepsStopwatch = Stopwatch.StartNew();
        int stepsCount = map.Path()
            .Select(state => state.Point)
            .Distinct()
            .Count();
        stepsStopwatch.Stop();

        Stopwatch obstructionStopwatch = Stopwatch.StartNew();
        var startingPos = map.FindStartingPosition();
        int obstructionPointsCount = map.Path().Select(state => state.Point)
            .Where(coord => coord != startingPos.Point)
            .Distinct()
            .Count(coord => map.WhatIf(coord, map => map.ContainsLoop()));
        obstructionStopwatch.Stop();

        Console.WriteLine($"             Steps count: {stepsCount} (in {stepsStopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine($"Obstruction points count: {obstructionPointsCount} (in {obstructionStopwatch.ElapsedMilliseconds} ms)");
    }

    private static IEnumerable<Position> Path(this char[][] map)
    {
        Position position = map.FindStartingPosition();
        yield return position;

        HashSet<Position> visited = [ position ];

        while (map.Contains(position.Point))
        {
            Position next = position.StepStraight();
            if (map.IsObstacle(next.Point)) next = position.TurnRight();

            if (!map.Contains(next.Point)) yield break;

            yield return next;

            if (!visited.Add(next)) yield break;

            position = next;
        }
    }

    private static bool ContainsLoop(this char[][] map) =>
        map.Path().Aggregate(
            (loop: false, steps: new HashSet<Position>()),
            (path, step) => (path.loop || !path.steps.Add(step), path.steps))
            .loop;

    private static T WhatIf<T>(this char[][] map, Point putObstacleAt, Func<char[][], T> func)
    {
        char original = map.At(putObstacleAt);
        map[putObstacleAt.Row][putObstacleAt.Column] = '#';

        T result = func(map);

        map[putObstacleAt.Row][putObstacleAt.Column] = original;

        return result;
    }

    private static Position StepStraight(this Position position) =>
        position with { Point = position.Point.Step(position.Orientation) };
    
    private static Position TurnRight(this Position position) =>
        position with { Orientation = position.Orientation.TurnRight() };
    
    private static char TurnRight(this char orientation) =>
        Orientations[(Orientations.IndexOf(orientation) + 1) % Orientations.Length];
    
    private static Point Step(this Point point, char direction) =>
        direction switch
        {
            '^' => new Point(point.Row - 1, point.Column),
            '>' => new Point(point.Row, point.Column + 1),
            'v' => new Point(point.Row + 1, point.Column),
            _ => new Point(point.Row, point.Column - 1)
        };

    private static bool CanStepTo(this char[][] map, Point point) =>
        map.Contains(point) && AvailableCells.Contains(map.At(point));

    private static bool IsObstacle(this char[][] map, Point point) =>
        map.Contains(point) && map.At(point) == '#';

    private static bool Contains(this char[][] map, Point point) =>
        point.Row >= 0 && point.Row < map.Length &&
        point.Column >= 0 && point.Column < map[point.Row].Length;

    private static char At(this char[][] map, Point point) =>
        map[point.Row][point.Column];

    private static Position FindStartingPosition(this char[][] map) =>
        map.GetOrientationSigns().First();

    private static IEnumerable<Position> GetOrientationSigns(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where Orientations.Contains(map[row][col])
        select new Position(new Point(row, col), map[row][col]);

    private static string AvailableCells = Orientations + ".";

    private static string Orientations = "^>v<";

    private record Position(Point Point, char Orientation);

    private record Point(int Row, int Column);
}
