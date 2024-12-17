using System.Collections.Immutable;

static class Day16
{
    public static void Run()
    {
        var maze = Console.In.ReadMaze();
        int stepCost = 1;
        int turnCost = 1000;

        var (cheapestPath, paths) = maze.FindCheapestPath(stepCost, turnCost);
        int pathLengths = paths.Distinct().Count();

        Console.WriteLine($"Cheapest path: {cheapestPath}");
        Console.WriteLine($"Total path lengths: {pathLengths}");
    }

    record ReachedState(int Cost, ImmutableList<Point> Footsteps);

    private static ReachedState ToReachedState(this Point point) =>
        new(0, new[] { point }.ToImmutableList());

    private static ReachedState Add(this ReachedState reached, Point point, int cost) =>
        new(cost, reached.Footsteps.Add(point));

    private static ReachedState MergeWith(this ReachedState reached, ReachedState? other) =>
        other is null ? reached
        : reached.Cost < other.Cost ? reached
        : reached with { Footsteps = reached.Footsteps.AddRange(other.Footsteps) };

    private static ReachedState FindCheapestPath(this char[][] maze, int stepCost, int turnCost)
    {
        var startNode = new State(maze.GetStartPosition(), new Direction(0, 1));

        var reach = new Dictionary<State, ReachedState>()
        {
            [startNode] = startNode.Position.ToReachedState()
        };

        var visited = new HashSet<State>();
        var queue = new PriorityQueue<State, int>([ (startNode, 0) ]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (maze.IsEnd(current.Position)) return reach[current];

            if (visited.Contains(current)) continue;
            visited.Add(current);

            var (cost, footsteps) = reach[current];

            (State state, int cost)[] neighbors = 
            [
                (current.StepForward(), cost + stepCost),
                (current.TurnLeft(), cost + turnCost),
                (current.TurnRight(), cost + turnCost)
            ];

            foreach (var neighbor in neighbors)
            {
                if (!maze.IsEmpty(neighbor.state.Position)) continue;
                if (reach.TryGetValue(neighbor.state, out ReachedState? reachedNeighbor) &&
                    neighbor.cost > reachedNeighbor.Cost) continue;

                reach[neighbor.state] = reach[current]
                    .Add(neighbor.state.Position, neighbor.cost)
                    .MergeWith(reachedNeighbor);

                queue.Enqueue(neighbor.state, neighbor.cost);
            }
        }

        throw new InvalidOperationException("No way there is no way!");
    }

    private static State StepForward(this State node) =>
        node with { Position = node.Position.Move(node.Orientation) };
    
    private static State TurnLeft(this State state) =>
        state with { Orientation = new(-state.Orientation.ColStep, state.Orientation.RowStep) };
    
    private static State TurnRight(this State state) =>
        state with { Orientation = new(state.Orientation.ColStep, -state.Orientation.RowStep) };

    private static Point Move(this Point point, Direction direction) =>
        new(point.Row + direction.RowStep, point.Col + direction.ColStep);

    private static Point GetStartPosition(this char[][] maze) =>
        maze.GetContent().First(pair => pair.content == 'S').point;

    private static bool IsEnd(this char[][] maze, Point point) => maze.At(point) == 'E';

    private static bool IsEmpty(this char[][] maze, Point point) => maze.At(point) != '#';

    private static IEnumerable<(Point point, char content)> GetContent(this char[][] maze) =>
        from row in Enumerable.Range(0, maze.Length)
        from col in Enumerable.Range(0, maze[row].Length)
        select (new Point(row, col), maze[row][col]);

    private static char At(this char[][] maze, Point point) =>
        maze[point.Row][point.Col];

    private static char[][] ReadMaze(this TextReader text) =>
        text.ReadLines().Select(line => line.ToCharArray()).ToArray();

    record State(Point Position, Direction Orientation);

    record Direction(int RowStep, int ColStep);

    record Point(int Row, int Col);
}
