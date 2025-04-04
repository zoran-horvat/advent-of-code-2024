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
            .Select(state => (state.row, state.col))
            .Distinct()
            .Count();
        stepsStopwatch.Stop();

        Stopwatch obstructionStopwatch = Stopwatch.StartNew();
        var startingPos = map.GetStartingPosition();
        int obstructionPointsCount = map.Path().Select(state => (state.row, state.col))
            .Where(coord => coord != (startingPos.row, startingPos.col))
            .Distinct()
            .Count(coord => map.WhatIf(coord, map => map.ContainsLoop()));
        obstructionStopwatch.Stop();

        Console.WriteLine($"             Steps count: {stepsCount} (in {stepsStopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine($"Obstruction points count: {obstructionPointsCount} (in {obstructionStopwatch.ElapsedMilliseconds} ms)");
    }

    private static IEnumerable<(int row, int col, char orientation)> Path(this char[][] map)
    {
        (int row, int col, char orientation) state = map.GetStartingPosition();
        yield return state;

        HashSet<(int row, int col, char orientation)> visited = new() { state };

        while (true)
        {
            state = state.orientation switch
            {
                '^' when map.ContainsObstacle(state.row - 1, state.col) =>
                    (state.row, state.col, '>'),
                '^' => (state.row - 1, state.col, state.orientation),
                '>' when map.ContainsObstacle(state.row, state.col + 1) =>
                    (state.row, state.col, 'v'),
                '>' => (state.row, state.col + 1, state.orientation),
                'v' when map.ContainsObstacle(state.row + 1, state.col) =>
                    (state.row, state.col, '<'),
                'v' => (state.row + 1, state.col, state.orientation),
                '<' when map.ContainsObstacle(state.row, state.col - 1) =>
                    (state.row, state.col, '^'),
                _ => (state.row, state.col - 1, state.orientation),
            };

            if (map.IsInside(state.row, state.col)) yield return state;
            else yield break;

            if (!visited.Add(state)) yield break;
        }
    }

    private static bool ContainsLoop(this char[][] map) =>
        map.Path().Aggregate(
            (loop: false, steps: new HashSet<(int row, int col, char orientation)>()),
            (path, step) => (path.loop || !path.steps.Add(step), path.steps))
            .loop;

    private static T WhatIf<T>(this char[][] map, (int row, int col) putObstacleAt, Func<char[][], T> func)
    {
        char original = map[putObstacleAt.row][putObstacleAt.col];
        map[putObstacleAt.row][putObstacleAt.col] = '#';

        T result = func(map);

        map[putObstacleAt.row][putObstacleAt.col] = original;

        return result;
    }

    private static bool ContainsObstacle(this char[][] map, int row, int col) =>
        map.IsInside(row, col) && map[row][col] == '#';

    private static bool IsInside(this char[][] map, int row, int col) =>
        row >= 0 && row < map.Length && col >= 0 && col < map[0].Length;

    private static (int row, int col, char orientation) GetStartingPosition(this char[][] map) =>
        map.GetContent().First(tuple => Orientations.Contains(tuple.content));

    private static IEnumerable<(int row, int col, char content)> GetContent(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        select (row, col, map[row][col]);

    private static string Orientations = "^>v<";
}
