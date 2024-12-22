static class Day21
{
    public static void Run()
    {
        var codes = Console.In.ReadLines().ToList();

        KeyboardMap map2 = MapRobots(2);
        KeyboardMap map25 = MapRobots(25);

        long totalCommands2 = codes.Sum(code => code.GetComplexity(map2));
        long totalCommands25 = codes.Sum(code => code.GetComplexity(map25));

        Console.WriteLine($"Total commands: {totalCommands2}");
        Console.WriteLine($"Total commands: {totalCommands25}");
    }

    private static long GetComplexity(this string code, KeyboardMap map) =>
        code.GetCommandsLength(map) * int.Parse(code[..^1]);

    private static KeyboardMap MapRobots(int robotsCount) =>
        Enumerable.Range(0, robotsCount + 1).Aggregate(
            RobotKeyboard.MapHuman(),
            (map, mapped) => (mapped == robotsCount ? DoorKeyboard : RobotKeyboard).MapRobot(map));

    private static KeyboardMap MapRobot(this Keyboard keyboard, KeyboardMap @operator)
    {
        var map = keyboard.GetKeyPairs().ToDictionary(
            pair => pair,
            pair => keyboard.GetShortestCommandsLength(pair.from, pair.to, @operator));

        return new(keyboard, map);
    }

    private static long GetShortestCommandsLength(this Keyboard keyboard, Key from, Key to, KeyboardMap @operator) =>
        keyboard.GetAllPaths(from, to).Min(path => path.GetCommandsLength(@operator));

    private static long GetCommandsLength(this string path, KeyboardMap @operator)
    {
        char state = 'A';
        long commands = 0;

        foreach (char command in path)
        {
            commands += @operator.GetCommandsLength(state, command);
            state = command;
        }

        return commands;
    }

    private static long GetCommandsLength(this KeyboardMap map, char from, char to) =>
        map.Distances[(map.Keyboard.Find(from), map.Keyboard.Find(to))];

    private static IEnumerable<string> GetAllPaths(this Keyboard keyboard, Key from, Key to)
    {
        var pending = new Queue<(Key reached, string path)>([ (from, string.Empty) ]);

        while (pending.Count > 0)
        {
            var (reached, path) = pending.Dequeue();

            if (reached == to)
            {
                yield return path + "A";
                continue;
            }

            var verticalStep = Math.Sign(to.Position.Row - reached.Position.Row);
            var horizontalStep = Math.Sign(to.Position.Col - reached.Position.Col);

            var nextVertically = reached.Position with { Row = reached.Position.Row + verticalStep };
            var nextHorizontally = reached.Position with { Col = reached.Position.Col + horizontalStep };

            if (verticalStep != 0 && nextVertically.IsValid(keyboard))
            {
                pending.Enqueue((keyboard.GetKey(nextVertically), path + verticalStep));
            }

            if (horizontalStep != 0 && nextHorizontally.IsValid(keyboard))
            {
                pending.Enqueue((keyboard.GetKey(nextHorizontally), path + horizontalStep));
            }
        }
    }

    private static bool IsValid(this Point point, Keyboard keyboard) =>
        point.Row >= 0 && point.Row < keyboard.Rows.Length &&
        point.Col >= 0 && point.Col < keyboard.Rows[point.Row].Length &&
        keyboard.Rows[point.Row][point.Col] != ' ';

    private static Key GetKey(this Keyboard keyboard, Point point) =>
        keyboard.GetKeys().First(key => key.Position == point);

    private static KeyboardMap MapHuman(this Keyboard keyboard) =>
        new(keyboard, keyboard.GetKeyPairs().ToDictionary(pair => pair, _ => 1L));

    private static IEnumerable<(Key from, Key to)> GetKeyPairs(this Keyboard keyboard) =>
        from fromKey in keyboard.GetKeys()
        from toKey in keyboard.GetKeys()
        select (fromKey, toKey);

    private static IEnumerable<Key> GetKeys(this Keyboard keyboard) =>
        from row in Enumerable.Range(0, keyboard.Rows.Length)
        from col in Enumerable.Range(0, keyboard.Rows[row].Length)
        let key = keyboard.Rows[row][col]
        where key != ' '
        select new Key(key, new Point(row, col));

    private static Key Find(this Keyboard keyboard, char key) =>
        keyboard.GetKeys().First(k => k.Value == key);

    private static Keyboard DoorKeyboard => new Keyboard([ "789", "456", "123", " 0A" ]);

    private static Keyboard RobotKeyboard => new Keyboard([ " ^A", "<v>" ]);

    record KeyboardMap(Keyboard Keyboard, Dictionary<(Key from, Key to), long> Distances);

    record Key(char Value, Point Position);

    record Point(int Row, int Col);

    record Keyboard(string[] Rows);
}
