static class Day18
{
    public static void Run()
    {
        var fallingBytes = Console.In.ReadPoints().ToList();
        int width = 71;
        int height = 71;
        int startTime = 1024;

        var maze = fallingBytes.Take(startTime).ToMaze(width, height);

        var shortestPath = maze.GetShortestPathOut();
        var culprit = FindCulprit(fallingBytes, width, height);

        if (shortestPath.HasValue) Console.WriteLine($"Shortest path out: {shortestPath.Value} steps");
        else Console.WriteLine("No path out found");

        Console.WriteLine($"Culprit byte at {culprit.X}, {culprit.Y}");
    }

    private static Point FindCulprit(this List<Point> fallingBytes, int width, int height)
    {
        int withPassage = 0;
        int noPassage = fallingBytes.Count;

        while (noPassage - withPassage > 1)
        {
            int atTime = (withPassage + noPassage) / 2;
            if (fallingBytes.Take(atTime).ToMaze(width, height).PathExists()) withPassage = atTime;
            else noPassage = atTime;
        }

        return fallingBytes[noPassage - 1];
    }

    private static bool PathExists(this Maze maze) =>
        maze.GetShortestPathOut() is not null;

    private static int? GetShortestPathOut(this Maze maze)
    {
        var start = maze.GetStart();
        var end = maze.GetEnd();

        var queue = new PriorityQueue<Point, int>([(start, 0)]);
        var visited = new HashSet<Point>();

        while (queue.TryDequeue(out var current, out var steps))
        {
            if (!visited.Add(current)) continue;
            if (current == end) return steps;

            foreach (var neighbor in maze.GetNeighbors(current))
            {
                queue.Enqueue(neighbor, steps + 1);
            }
        }

        return null;
    }

    private static IEnumerable<Point> GetNeighbors(this Maze maze, Point point) =>
        new[]
        {
            new Point(point.X - 1, point.Y), new Point(point.X + 1, point.Y),
            new Point(point.X, point.Y - 1), new Point(point.X, point.Y + 1)
        }
        .Where(maze.IsAvailable);

    private static bool IsAvailable(this Maze maze, Point point) =>
        maze.Contains(point) && !maze.Obstacles.Contains(point);

    private static bool Contains(this Maze maze, Point point) =>
        point.X >= 0 && point.X < maze.Width && point.Y >= 0 && point.Y < maze.Height;

    private static Point GetStart(this Maze maze) =>
        new Point(0, 0);

    private static Point GetEnd(this Maze maze) =>
        new Point(maze.Width - 1, maze.Height - 1);

    private static Maze ToMaze(this IEnumerable<Point> obstacles, int width, int height) =>
        new Maze(width, height, obstacles.ToHashSet());

    private static IEnumerable<Point> ReadPoints(this TextReader text) =>
        text.ReadLines()
            .Select(Common.ParseIntsNoSign)
            .Select(line => new Point(line[0], line[1]));

    record Maze(int Width, int Height, HashSet<Point> Obstacles);

    record Point(int X, int Y);
}