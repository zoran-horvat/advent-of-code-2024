static class Day12
{
    public static void Run()
    {
        var map = Console.In.ReadMap();
        var regions = map.GetRegions().ToList();

        int totalCost = regions.Sum(region => region.Cost(map));
        int discountedCost = regions.Sum(region => region.DiscountedCost(map));

        Console.WriteLine($"     Total cost: {totalCost}");
        Console.WriteLine($"Discounted cost: {discountedCost}");
    }

    private static int DiscountedCost(this List<(int row, int col)> region, char[][] map) =>
        region.DiscountedPerimeter(map) * region.Area();

    private static int DiscountedPerimeter(this List<(int row, int col)> region, char[][] map) =>
        region.Perimeter(map) - region.Sum(point => point.CountContinuingFences(map));

    private static bool ContinuesFence(
        this (int row, int col) point, (int rowDiff, int colDiff) preceding,
        (int rowDiff, int colDiff) outside, char[][] map) =>
        !point.IsNeighbor(point.Move(outside), map) &&
        point.IsNeighbor(point.Move(preceding), map) &&
        !point.IsNeighbor(point.Move(preceding).Move(outside), map);
    
    private static (int row, int col) Move(
        this (int row, int col) point, (int rowDiff, int colDiff) direction) =>
        (point.row + direction.rowDiff, point.col + direction.colDiff);
    
    private static bool IsNeighbor(
        this (int row, int col) point, (int row, int col) other, char[][] map) =>
        map.Contains(other) && map.At(other) == map.At(point);
    
    private static bool ContinuesFenceLeftward(this (int row, int col) point, char[][] map) =>
        point.ContinuesFence((0, 1), (1, 0), map);
    
    private static bool ContinuesFenceUpward(this (int row, int col) point, char[][] map) =>
        point.ContinuesFence((1, 0), (0, -1), map);

    private static bool ContinuesFenceRightward(this (int row, int col) point, char[][] map) =>
        point.ContinuesFence((0, -1), (-1, 0), map);

    private static bool ContinuesFenceDownward(this (int row, int col) point, char[][] map) =>
        point.ContinuesFence((-1, 0), (0, 1), map);

    private static int CountContinuingFences(this (int row, int col) point, char[][] map) =>
        new []
        {
            point.ContinuesFenceLeftward(map), point.ContinuesFenceUpward(map),
            point.ContinuesFenceRightward(map), point.ContinuesFenceDownward(map)
        }.Count(x => x);

    private static int Cost(this List<(int row, int col)> region, char[][] map) =>
        region.Area() * region.Perimeter(map);

    private static int Perimeter(this List<(int row, int col)> region, char[][] map) =>
        region.Sum(point => 4 - point.GetNeighbors(map).Count());

    private static int Area(this List<(int row, int col)> region) =>
        region.Count;

    private static IEnumerable<List<(int row, int col)>> GetRegions(this char[][] map)
    {
        var pending = map.AllPoints().ToHashSet();

        while (pending.Count > 0)
        {
            var region = new List<(int row, int col)>();

            var start = pending.First();
            pending.Remove(start);
            
            var add = new Queue<(int row, int col)>();
            add.Enqueue(start);

            while (add.TryDequeue(out var point))
            {
                region.Add(point);

                foreach (var neighbor in point.GetNeighbors(map))
                {
                    if (pending.Remove(neighbor)) add.Enqueue(neighbor);
                }
            }

            yield return region;
        }
    }

    private static IEnumerable<(int row, int col)> GetNeighbors(this (int row, int col) point, char[][] map) =>
        new[]
        {
            (point.row - 1, point.col), (point.row + 1, point.col),
            (point.row, point.col - 1), (point.row, point.col + 1)
        }
        .Where(map.Contains)
        .Where(neighbor => map.At(neighbor) == map.At(point));

    private static IEnumerable<(int row, int col)> AllPoints(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        select (row, col);

    private static char At(this char[][] map, (int row, int col) point) =>
        map[point.row][point.col];
    
    private static bool Contains(this char[][] map, (int row, int col) point) =>
        point.row >= 0 && point.row < map.Length &&
        point.col >= 0 && point.col < map[point.row].Length;

    private static char[][] ReadMap(this TextReader reader) =>
        reader.ReadLines().Select(line => line.ToCharArray()).ToArray();
}