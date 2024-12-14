static class Day14
{
    public static void Run()
    {
        var robots = Console.In.ReadRobots().ToList();
        Coordinates roomSize = new(101, 103);
        int time = 100;

        var safetyFactor = robots.Move(time, roomSize).GetSafetyFactor(roomSize);

        Console.WriteLine($"Safety factor: {safetyFactor}");

        int t = 0;
        while (true)
        {
            if (t == int.MaxValue)
            {
                Console.WriteLine("No solution found. Better luck next Easter.");
                break;
            }

            t += 1;
            if (t % 1000 == 0) Console.WriteLine($"Time: {t:#,##0}");
            if (robots.Move(t, roomSize).MaybeChristmasTree())
            {
                robots.Move(t, roomSize).Print(t, roomSize);
                Console.Write($"Keep searching? (y/n) ");
                if (Console.ReadLine()?.ToLower() == "n") break;
            }
        }
    }

    private static bool MaybeChristmasTree(this IEnumerable<Robot> robots) =>
        robots.GetGroupSizes().Max() >= robots.Count() / 3;

    private static IEnumerable<int> GetGroupSizes(this IEnumerable<Robot> robots)
    {
        var pending = robots.Select(robot => robot.Position).Distinct().ToHashSet();
        
        while (pending.Count > 0)
        {
            var pivot = pending.First();
            pending.Remove(pivot);

            var add = new Queue<Coordinates>();
            add.Enqueue(pivot);

            int groupSize = 0;

            while (add.Count > 0)
            {
                var current = add.Dequeue();
                groupSize += 1;

                var neighbors = new[]
                {
                    current with { X = current.X - 1 }, current with { X = current.X + 1 },
                    current with { Y = current.Y - 1 }, current with { Y = current.Y + 1 }
                };

                foreach (var next in neighbors.Where(pending.Contains))
                {
                    pending.Remove(next);
                    add.Enqueue(next);
                }
            }

            yield return groupSize;
        }
    }

    private static void Print(this IEnumerable<Robot> robots, int time, Coordinates roomSize)
    {
        char[][] map = Enumerable.Range(0, roomSize.Y).Select(_ => Enumerable.Repeat('.', roomSize.X).ToArray()).ToArray();
        robots.ToList().ForEach(robot => map[robot.Position.Y][robot.Position.X] = '#');

        Console.WriteLine();
        Console.WriteLine($"Time: {time}");
        map.ToList().ForEach(row => Console.WriteLine(new string(row)));
    }

    private static int GetSafetyFactor(this IEnumerable<Robot> robots, Coordinates roomSize) =>
        robots.ToQuadrantCounts(roomSize).Aggregate(1, (safety, quadrant) => safety * quadrant.count);

    private static IEnumerable<(int quadrant, int count)> ToQuadrantCounts(this IEnumerable<Robot> robots, Coordinates roomSize) =>
        robots.Select(robot => (quadrant: robot.ToQuadrant(roomSize), count: 1))
            .Concat<(int quadrant, int count)>([(1, 0), (2, 0), (3, 0), (4, 0)])
            .Where(pair => pair.quadrant != 0)
            .GroupBy(pair => pair.quadrant, (quadrant, group) => (quadrant, count: group.Sum(pair => pair.count)));

    private static int ToQuadrant(this Robot robot, Coordinates roomSize) =>
        robot.Position.X > roomSize.X / 2 && robot.Position.Y < roomSize.Y / 2 ? 1
        : robot.Position.X < roomSize.X / 2 && robot.Position.Y < roomSize.Y / 2 ? 2
        : robot.Position.X < roomSize.X / 2 && robot.Position.Y > roomSize.Y / 2 ? 3
        : robot.Position.X > roomSize.X / 2 && robot.Position.Y > roomSize.Y / 2 ? 4
        : 0;

    private static IEnumerable<Robot> Move(this IEnumerable<Robot> robots, int time, Coordinates roomSize) =>
        robots.Select(robot => robot.Move(time, roomSize));

    private static Robot Move(this Robot robot, int time, Coordinates roomSize) => 
        robot with { Position = robot.Position.Move(robot.Velocity, time, roomSize) };

    private static Coordinates Move(this Coordinates position, Coordinates velocity, int time, Coordinates roomSize) =>
        new(position.X.Move(velocity.X, time, roomSize.X), position.Y.Move(velocity.Y, time, roomSize.Y));

    private static int Move(this int position, int velocity, int time, int roomSize) =>
        ((position + velocity * time) % roomSize + roomSize) % roomSize;

    private static IEnumerable<Robot> ReadRobots(this TextReader text) =>
        text.ReadLines().Select(line => line.ParseInts())
            .Select(list => new Robot(new(list[0], list[1]), new(list[2], list[3])));

    record Robot(Coordinates Position, Coordinates Velocity);

    record Coordinates(int X, int Y);
}
