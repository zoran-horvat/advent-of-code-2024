static class Day12
{
    public static void Run()
    {
        var map = Console.In.ReadMap();
        var regions = map.GetRegions().ToList();

        int totalCost = regions.Sum(region => region.Cost(map));
        int totalDiscountedCost = regions.Sum(region => region.DiscountedCost(map));

        Console.WriteLine($"           Total cost: {totalCost}");
        Console.WriteLine($"Total discounted cost: {totalDiscountedCost}");
    }

    private static int Cost(this List<(int row, int col)> region, char[][] map) =>
        region.Area() * region.Perimeter(map);

    private static int DiscountedCost(this List<(int row, int col)> region, char[][] map) =>
        region.Area() * region.DiscountedPerimeter(map);

    private static int Area(this List<(int row, int col)> region) =>
        region.Count();
    
    private static int Perimeter(this List<(int row, int col)> region, char[][] map) =>
        region.Sum(point => 4 - point.GetNeighbors(map).Count());

    private static int DiscountedPerimeter(this List<(int row, int col)> region, char[][] map) =>
        region.Perimeter(map) - region.CountContinuations(map);

    private static int CountContinuations(this List<(int row, int col)> region, char[][] map) =>
        region.Count(point => point.ContinuesRight(map)) +
        region.Count(point => point.ContinuesDown(map)) +
        region.Count(point => point.ContinuesLeft(map)) +
        region.Count(point => point.ContinuesUp(map));

    private static bool ContinuesRight(this (int row, int col) point, char[][] map) =>
        map.Same(point, point.Left()) && !map.Same(point, point.Left().Up()) && !map.Same(point, point.Up());

    private static bool ContinuesDown(this (int row, int col) point, char[][] map) =>
        map.Same(point, point.Up()) && !map.Same(point, point.Up().Right()) && !map.Same(point, point.Right());

    private static bool ContinuesLeft(this (int row, int col) point, char[][] map) =>
        map.Same(point, point.Right()) && !map.Same(point, point.Right().Down()) && !map.Same(point, point.Down());

    private static bool ContinuesUp(this (int row, int col) point, char[][] map) =>
        map.Same(point, point.Down()) && !map.Same(point, point.Down().Left()) && !map.Same(point, point.Left());

    private static (int row, int col) Left(this (int row, int col) point) =>
        (point.row, point.col - 1);

    private static (int row, int col) Right(this (int row, int col) point) =>
        (point.row, point.col + 1);

    private static (int row, int col) Up(this (int row, int col) point) =>
        (point.row - 1, point.col);

    private static (int row, int col) Down(this (int row, int col) point) =>
        (point.row + 1, point.col);

    private static bool Same(this char[][] map, (int row, int col) a, (int row, int col) b) => 
        map.IsInside(a) && map.IsInside(b) && map.At(a) == map.At(b);

    private static IEnumerable<List<(int row, int col)>> GetRegions(this char[][] map)
    {
        var pending = map.AllCoordinates().ToHashSet();

        while (pending.Count > 0)
        {
            var start = pending.First();
            pending.Remove(start);
            
            var add = new Queue<(int row, int col)>();
            add.Enqueue(start);

            var region = new List<(int row, int col)>();

            while (add.Count > 0)
            {
                var current = add.Dequeue();
                region.Add(current);

                foreach (var next in current.GetNeighbors(map).Where(pending.Contains))
                {
                    pending.Remove(next);
                    add.Enqueue(next);
                }
            }

            yield return region;
        }
    }

    private static IEnumerable<(int row, int col)> AllCoordinates(this char[][] map) =>
        from row in Enumerable.Range(0, map.Length)
        from col in Enumerable.Range(0, map[row].Length)
        select (row, col);

    private static IEnumerable<(int row, int col)> GetNeighbors(this (int row, int col) point, char[][] map) =>
        new[]
        {
            (point.row - 1, point.col), (point.row + 1, point.col),
            (point.row, point.col - 1), (point.row, point.col + 1)
        }
        .Where(map.IsInside)
        .Where(neighbor => map.At(neighbor) == map.At(point));
    
    private static bool IsInside(this char[][] map, (int row, int col) point) =>
        point.row >= 0 && point.row < map.Length && point.col >= 0 && point.col < map[point.row].Length;

    private static char At(this char[][] map, (int row, int col) point) =>
        map[point.row][point.col];

    private static char[][] ReadMap(this TextReader reader) =>
        reader.ReadLines().Select(line => line.ToCharArray()).ToArray();
}
