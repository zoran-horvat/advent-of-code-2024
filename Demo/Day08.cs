static class Day08
{
    public static void Run()
    {
        char[][] map = Console.In.ReadMap();

        var antennas = map.GetAntennas().ToList();

        var antinodesCount = antennas
            .SelectMany(a => a.GetAntinodes(map, NonResonatingAntinodes))
            .Distinct()
            .Count();
        
        var resonantAntinodesCount = antennas
            .SelectMany(a => a.GetAntinodes(map, ResonatingAntinodes))
            .Distinct()
            .Count();

        Console.WriteLine($"         Antinodes count: {antinodesCount}");
        Console.WriteLine($"Resonant antinodes count: {resonantAntinodesCount}");
    }

    private static bool IsInside(this char[][] map, Position position) =>
        position.Row >= 0 && position.Row < map.Length &&
        position.Col >= 0 && position.Col < map[0].Length;

    private static IEnumerable<Position> GetAntinodes(this AntennaSet antennas, char[][] map, AntinodeGenerator antinodeGenerator) =>
        antennas.GetPositionPairs().SelectMany(pair => map.GetAntinodes(pair.a1, pair.a2, antinodeGenerator));

    private static IEnumerable<Position> NonResonatingAntinodes(this char[][] map, Position antenna, int rowDiff, int colDiff)
    {
        var position = new Position(antenna.Row + rowDiff, antenna.Col + colDiff);
        if (map.IsInside(position)) yield return position;
    }
    
    private static IEnumerable<Position> ResonatingAntinodes(this char[][] map, Position antenna, int rowDiff, int colDiff)
    {
        yield return antenna;
        var position = antenna;
        while (map.IsInside(position))
        {
            yield return position;
            position = new(position.Row + rowDiff, position.Col + colDiff);
        }
    }

    private static IEnumerable<Position> GetAntinodes(
        this char[][] map, Position antenna1, Position antenna2, 
        AntinodeGenerator antinodeGenerator)
    {
        int rowDiff = antenna1.Row - antenna2.Row;
        int colDiff = antenna1.Col - antenna2.Col;

        return antinodeGenerator(map, antenna1, rowDiff, colDiff)
            .Concat(antinodeGenerator(map, antenna2, -rowDiff, -colDiff));
    }

    delegate IEnumerable<Position> AntinodeGenerator(char[][] map, Position antenna, int rowDiff, int colDiff);
    
    private static IEnumerable<(Position a1, Position a2)> GetPositionPairs(this AntennaSet antennas) =>
        antennas.Positions.SelectMany((pos1, index1) =>
            antennas.Positions.Skip(index1 + 1).Select(pos2 => (pos1, pos2)));

    private static IEnumerable<AntennaSet> GetAntennas(this char[][] map) =>
        map.GetIndividualAntennas().GroupBy(
            antenna => antenna.Frequency,
            (frequency, antennas) => new AntennaSet(frequency, antennas.Select(antenna => antenna.Position).ToList()));

    private static IEnumerable<Antenna> GetIndividualAntennas(this char[][] map) =>
        map.SelectMany((row, rowIndex) =>
            row.Select((content, colIndex) => (content, rowIndex, colIndex))
            .Where(cell => cell.content != '.')
            .Select(cell => new Antenna(cell.content, new(cell.rowIndex, cell.colIndex))));

    record AntennaSet(char Frequency, List<Position> Positions);
    record Antenna(char Frequency, Position Position);
    record Position(int Row, int Col);

    private static char[][] ReadMap(this TextReader text) =>
        text.ReadLines().Select(line => line.ToCharArray()).ToArray();
}
