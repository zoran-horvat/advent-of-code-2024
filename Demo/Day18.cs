using System.Drawing;

static class Day18
{
    public static void Run()
    {
        var fallingBytes = Console.In.ReadPoints();
        int width = 71;
        int height = 71;
        int startTime = 1024;

        var maze = fallingBytes.ToMaze(width, height, startTime);

        int? quickestExit = maze.GetShortestPathOut();
        Console.WriteLine($"Quickest exit: {(quickestExit.HasValue ? quickestExit.ToString() : "No exit found")}");

        var culprit = FindCulprit(fallingBytes, width, height, startTime);

        if (culprit is null) Console.WriteLine("No culprit found");
        else Console.WriteLine($"Culprit: ({culprit.X}, {culprit.Y})");
    }

    private static Point? FindCulprit(this List<Point> fallingBytes, int width, int height, int startTime)
    {
        int fallWithPassage = startTime;
        int fallWithNoPassage = fallingBytes.Count;

        if (!fallingBytes.PathExists(width, height, fallWithPassage)) return null;
        if (fallingBytes.PathExists(width, height, fallWithNoPassage)) return null;

        while (fallWithNoPassage - fallWithPassage > 1)
        {
            int atTime = (fallWithPassage + fallWithNoPassage) / 2;
            if (fallingBytes.PathExists(width, height, atTime)) fallWithPassage = atTime;
            else fallWithNoPassage = atTime;
        }

        if (fallWithNoPassage == fallWithPassage + 1) return fallingBytes[fallWithNoPassage - 1];
        return null;
    }

    private static bool PathExists(this IEnumerable<Point> fallingBytes, int width, int height, int atTime) =>
        fallingBytes.ToMaze(width, height, atTime).PathExists();

    private static bool PathExists(this Maze maze) => maze.GetShortestPathOut() is not null;

    private static int? GetShortestPathOut(this Maze maze)
    {
        var start = maze.GetStart();

        var queue = new PriorityQueue<(Point point, int steps), int>([ ((start, 0), 0) ]);
        var visited = new HashSet<Point>();

        while (queue.Count > 0)
        {
            var (point, steps) = queue.Dequeue();

            if (visited.Contains(point)) continue;
            visited.Add(point);

            if (maze.IsEnd(point)) return steps;

            foreach (var neighbor in maze.GetNeighbors(point))
            {
                queue.Enqueue((neighbor, steps + 1), steps + 1);
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
        .Where(neighbor => maze.IsAvailable(neighbor));

    private static bool IsAvailable(this Maze maze, Point point) =>
        maze.IsInside(point) && !maze.Obstacles.Contains(point);

    private static bool IsEnd(this Maze maze, Point point) => point == maze.GetEnd();

    private static bool IsInside(this Maze maze, Point point) =>
        point.X >= 0 && point.X < maze.Width && point.Y >= 0 && point.Y < maze.Height;

    private static Point GetStart(this Maze maze) => new Point(0, 0);

    private static Point GetEnd(this Maze maze) => new Point(maze.Width - 1, maze.Height - 1);

    private static Maze ToMaze(this IEnumerable<Point> obstacles, int width, int height, int atTime) =>
        new Maze(width, height, obstacles.Take(atTime).ToHashSet());

    record Maze(int Width, int Height, HashSet<Point> Obstacles);
    
    record Point(int X, int Y);

    private static List<Point> ReadPoints(this TextReader text) =>
        text.ReadLines()
            .Select(Common.ParseIntsNoSign)
            .Select(line => new Point(line[0], line[1]))
            .ToList();
}
