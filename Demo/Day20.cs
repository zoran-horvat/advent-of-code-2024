using System.Collections.Immutable;

static class Day20
{
    public static void Run()
    {
        char[][] maze = Console.In.ReadMaze();
        var path = maze.FindShortestPath().ToArray();

        int shortestPath = path.Count() - 1;

        int cheatingPaths2 = maze.FindSaves(path, 2).Count(saved => saved >= 100);

        int cheatingPaths20 = maze.FindSaves(path, 20)
            .Count(save => save >= 100);

        Console.WriteLine($" Shortest path: {shortestPath}");
        Console.WriteLine($"Cheating paths: {cheatingPaths2}");
        Console.WriteLine($"Cheating paths: {cheatingPaths20}");
    }

    private static IEnumerable<int> FindSaves(this char[][] maze, Point[] path, int maxCut)
    {
        for (int i = 0; i < path.Length - 1; i++)
        {
            for (int j = i + 1; j < path.Length; j++)
            {
                int distance = path[i].DistanceTo(path[j]);
                if (distance > maxCut) continue;
                if (distance >= j - i) continue;
                yield return j - i - distance;
            }
        }
    }

    private static int DistanceTo(this Point a, Point b) =>
        Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);

    private static IEnumerable<Point> FindShortestPath(this char[][] maze)
    {
        var start = maze.FindStart();
        var end = maze.FindEnd();

        var paths = new Dictionary<Point, ImmutableList<Point>> { [start] = new[] {start}.ToImmutableList() };
        var queue = new Queue<Point>([start]);

        while (queue.TryDequeue(out var current))
        {
            if (current == end) return paths[current];

            foreach (var neighbor in current.GetNeighbors(maze))
            {
                if (paths.ContainsKey(neighbor)) continue;

                paths[neighbor] = paths[current].Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        throw new InvalidOperationException("No path found");
    }

    private static IEnumerable<Point> GetNeighbors(this Point point, char[][] maze) =>
        new[]
        {
            point with { Row = point.Row - 1 }, point with { Row = point.Row + 1 },
            point with { Col = point.Col - 1 }, point with { Col = point.Col + 1 }
        }.Where(maze.IsEmpty);

    private static IEnumerable<Point> Find(this char[][] maze, char target) =>
        from row in Enumerable.Range(0, maze.Length)
        from col in Enumerable.Range(0, maze[0].Length)
        where maze[row][col] == target
        select new Point(row, col);

    private static Point FindStart(this char[][] maze) =>
        maze.Find('S').First();

    private static Point FindEnd(this char[][] maze) =>
        maze.Find('E').First();

    private static bool IsEmpty(this char[][] maze, Point point) =>
        maze.IsInside(point) && maze[point.Row][point.Col] != '#';

    private static bool IsInside(this char[][] maze, Point point) =>
        point.Row >= 0 && point.Row < maze.Length &&
        point.Col >= 0 && point.Col < maze[point.Row].Length;

    private static char[][] ReadMaze(this TextReader text) =>
        text.ReadLines().Select(line => line.ToCharArray()).ToArray();

    record Point(int Row, int Col);
}