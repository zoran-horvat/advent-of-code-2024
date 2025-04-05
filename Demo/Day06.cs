using System.Data;
using System.Diagnostics;
using System.Drawing;

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
        int obstructionsCount = map.DesignObstructions().Count();
        obstructionStopwatch.Stop();

        Console.WriteLine($"                Map size: {map.Length} x {map[0].Length}");
        Console.WriteLine($"             Steps count: {stepsCount} (in {stepsStopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine($"Obstruction points count: {obstructionsCount} (in {obstructionStopwatch.ElapsedMilliseconds} ms)");
    }

    private static IEnumerable<Point> DesignObstructions(this char[][] map) =>
        map.DesignObstructions(map.Path().ToList());

    private static IEnumerable<Point> DesignObstructions(
        this char[][] map, List<Position> path)
    {
        HashSet<Position> visited = [];
        HashSet<Point> checkedObstructions = new();

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].Point != path[0].Point && 
                path[i].Point != path[i - 1].Point &&
                checkedObstructions.Add(path[i].Point) &&
                path[i].Point.CausesLoop(map, visited, path[i - 1].TurnRight()))
            {
                yield return path[i].Point;
            }

            visited.Add(path[i]);
        }
    }

    private static bool CausesLoop(
        this Point obstruction, char[][] map,
        HashSet<Position> visited, Position position)
    {
        var oldValue = map.Set(obstruction, '#');

        bool loop = false;
        HashSet<Position> newVisited = [];

        while (map.Contains(position.Point))
        {
            if (visited.Contains(position) || !newVisited.Add(position))
            {
                loop = true;
                break;
            }

            position = position.Step(map);
        }

        map.Set(obstruction, oldValue);

        return loop;
    }

    private static IEnumerable<Position> Path(this char[][] map)
    {
        Position position = map.FindStartingPosition();

        while (map.Contains(position.Point))
        {
            yield return position;
            position = position.Step(map);
        }
    }

    private static Position Step(this Position position, char[][] map) =>
        position.StepForward() is Position forward && !map.IsObstruction(forward.Point) ? forward
        : position.TurnRight();

    private static Position StepForward(this Position position) =>
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

    private static bool IsObstruction(this char[][] map, Point point) =>
        map.Contains(point) && map.At(point) == '#';

    private static bool Contains(this char[][] map, Point point) =>
        point.Row >= 0 && point.Row < map.Length &&
        point.Column >= 0 && point.Column < map[point.Row].Length;

    private static char Set(this char[][] map, Point point, char value)
    {
        char oldValue = map[point.Row][point.Column];
        map[point.Row][point.Column] = value;
        return oldValue;
    }

    private static char At(this char[][] map, Point point) =>
        map[point.Row][point.Column];

    private static Position FindStartingPosition(this char[][] map) =>
        map.GetOrientationSigns().First();

    private static IEnumerable<Position> GetOrientationSigns(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        where Orientations.Contains(map[row][col])
        select new Position(new Point(row, col), map[row][col]);

    private static string Orientations = "^>v<";

    private record Position(Point Point, char Orientation);

    private record Point(int Row, int Column);
}