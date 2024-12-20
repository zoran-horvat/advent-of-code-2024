using System.Collections.Immutable;
using System.Data;

static class Day20
{
    public static void Run()
    {
        char[][] maze = Console.In.ReadMaze();
        var path = maze.FindShortestPath().ToList();

        int shortestPath = path.Count() - 1;

        int cheatingPaths2 = maze.FindCheatingSaves(path, 2)
            .Count(saved => saved <= shortestPath - 100);

        int cheatingPaths20 = maze.FindCheatingSaves(path, 20)
            .Count(save => save <= shortestPath - 100);

        Console.WriteLine($"Shortest path: {shortestPath}");
        Console.WriteLine($"Cheating paths: {cheatingPaths2}");
        Console.WriteLine($"Cheating paths: {cheatingPaths20}");
    }

    private static IEnumerable<int> FindCheatingSaves(this char[][] maze, IEnumerable<Point> path, int maxCut)
    {
        var stepsFromStart = path
            .Select((point, steps) => (point, steps))
            .ToDictionary();

        var stepsToEnd = path.Reverse()
            .Select((point, distance) => (point, distance))
            .ToDictionary();

        return path
            .SelectMany(point =>
                maze.GetNeighborhood(point, maxCut).Select(neighbor => (point, neighbor)))
            .Where(cheat => stepsToEnd.ContainsKey(cheat.neighbor))
            .Select(cheat =>
                stepsFromStart[cheat.point] +
                cheat.point.DistanceTo(cheat.neighbor) +
                stepsToEnd[cheat.neighbor]);
    }

    private static IEnumerable<Point> GetNeighborhood(this char[][] maze, Point point, int maxDistance) =>
        from row in Enumerable.Range(point.Row - maxDistance, 2 * maxDistance + 1)
        let remainingDist = maxDistance - Math.Abs(point.Row - row)
        from col in Enumerable.Range(point.Col - remainingDist, 2 * remainingDist + 1)
        let neighbor = new Point(row, col)
        where maze.IsInside(neighbor)
        where point.DistanceTo(neighbor) >= 2
        select neighbor;

    private static int DistanceTo(this Point a, Point b) =>
        Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);

    private static IEnumerable<Point> FindShortestPath(this char[][] maze)
    {
        var start = maze.FindStart();
        var distance = new Dictionary<Point, ImmutableList<Point>> { [start] = new[] {start}.ToImmutableList() };
        var queue = new Queue<Point>([start]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (maze.IsEnd(current)) return distance[current];

            foreach (var neighbor in current.GetNeighbors(maze))
            {
                if (distance.ContainsKey(neighbor)) continue;

                distance[neighbor] = distance[current].Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        throw new InvalidOperationException("No path found");
    }

    private static IEnumerable<Point> GetAllNeighbors(this Point point, char[][] maze) => new[]
    {
        point with { Row = point.Row - 1 }, point with { Row = point.Row + 1 },
        point with { Col = point.Col - 1 }, point with { Col = point.Col + 1 }
    }.Where(maze.IsInside);

    private static IEnumerable<Point> GetNeighbors(this Point point, char[][] maze) =>
        point.GetAllNeighbors(maze).Where(maze.IsEmpty);

    private static Point FindStart(this char[][] maze) =>
        (from row in Enumerable.Range(0, maze.Length)
         from col in Enumerable.Range(0, maze[0].Length)
         where maze[row][col] == 'S'
         select new Point(row, col)).First();

    private static bool IsEnd(this char[][] maze, Point point) =>
        maze.IsInside(point) && maze[point.Row][point.Col] == 'E';

    private static bool IsEmpty(this char[][] maze, Point point) =>
        maze.IsInside(point) && maze[point.Row][point.Col] != '#';

    private static bool IsInside(this char[][] maze, Point point) =>
        point.Row >= 0 && point.Row < maze.Length && point.Col >= 0 && point.Col < maze[point.Row].Length;

    private static char[][] ReadMaze(this TextReader text) =>
        text.ReadLines().Select(line => line.ToCharArray()).ToArray();

    record Point(int Row, int Col);
}
