using System.Data;
using System.Diagnostics;

static class Day06
{
    public static void Run()
    {
        char[][] map = Console.In.ReadLines()
            .Select(row => row.ToCharArray())
            .ToArray();

        Stopwatch pathTime = Stopwatch.StartNew();
        int pathLength = map.GetPath().Count();
        pathTime.Stop();

        Stopwatch obstructionTime = Stopwatch.StartNew();
        int obstructionPoints = map.GetObstructionPoints().Count();
        obstructionTime.Stop();

        Console.WriteLine($"       Path Length: {pathLength} ({pathTime.ElapsedMilliseconds} ms)");
        Console.WriteLine($"Obstruction Points: {obstructionPoints} ({obstructionTime.ElapsedMilliseconds} ms)");
    }

    private static IEnumerable<Point> GetObstructionPoints(this char[][] map) =>
        map.GetObstructionPoints(map.FindStartingPosition());

    private static IEnumerable<Point> GetObstructionPoints(this char[][] map, Position start)
    {
        HashSet<Position> visited = [];
        HashSet<Point> checkedObstructions = [ start.Point ];

        foreach (Position current in map.Walk(start))
        {
            visited.Add(current);
            Point ahead = current.StepForward().Point;

            if (!map.Contains(ahead) || map.IsObstruction(ahead)) continue;
            if (!checkedObstructions.Add(ahead)) continue;
            if (!ahead.CausesLoop(map, current.TurnRight(), visited)) continue;

            yield return ahead;
        }
    }

    private static bool CausesLoop(
        this Point obstruction, char[][] map, Position start,
        HashSet<Position> visitedBefore)
    {
        HashSet<Position> visited = [];
        Position current = start;

        while (map.Contains(current.Point) &&
               !visitedBefore.Contains(current) &&
               visited.Add(current))
        {
            current = current.Step(map, obstruction);
        }

        return map.Contains(current.Point);
    }

    private static IEnumerable<Point> GetPath(this char[][] map) =>
        map.Walk(map.FindStartingPosition())
            .Select(position => position.Point)
            .Distinct();

    private static IEnumerable<Position> Walk(this char[][] map, Position start)
    {
        for (Position pos = start; map.Contains(pos.Point); pos = pos.Step(map))
        {
            yield return pos;
        }
    }

    private static Position Step(this Position position, char[][] map, Point obstruction) =>
        position.StepForward() is Position forward && !map.IsObstruction(forward.Point, obstruction) ? forward
        : position.TurnRight();

    private static Position Step(this Position position, char[][] map) =>
        position.StepForward() is Position forward && !map.IsObstruction(forward.Point) ? forward
        : position.TurnRight();

    private static bool IsObstruction(this char[][] map, Point point, Point obstruction) =>
        map.Contains(point) && (map.At(point) == '#' || point == obstruction);
    
    private static bool IsObstruction(this char[][] map, Point point) =>
        map.Contains(point) && map.At(point) == '#';

    private static bool Contains(this char[][] map, Point point) =>
        point.Row >= 0 && point.Row < map.Length &&
        point.Column >= 0 && point.Column < map[point.Row].Length;

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
            _ => new Point(point.Row, point.Column - 1),
        };

    private static Position FindStartingPosition(this char[][] map) =>
        map.AllPoints()
            .Where(point => Orientations.Contains(map.At(point)))
            .Select(point => new Position(point, map.At(point)))
            .First();

    private static IEnumerable<Point> AllPoints(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from column in Enumerable.Range(0, map[row].Length)
        select new Point(row, column);

    private static char At(this char[][] map, Point point) =>
        map[point.Row][point.Column];

    private static string Orientations = "^>v<";

    private record struct Position(Point Point, char Orientation);

    private record struct Point(int Row, int Column);
}