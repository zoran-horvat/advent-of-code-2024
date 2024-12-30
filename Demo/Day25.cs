static class Day25
{
    public static void Run()
    {
        var items = Console.In.ReadItems().ToList();

        var totalFits = items.CountFits();

        Console.WriteLine($"Total lock-key fits: {totalFits}");
    }

    private static int CountFits(this IEnumerable<Item> items) =>
        items.OfType<Lock>().Sum(@lock => items.OfType<Key>().Count(@lock.Fits));

    private static IEnumerable<Item> ReadItems(this TextReader text)
    {
        bool isLock = false;
        char pinChar = '.';
        int[] pins = Array.Empty<int>();

        foreach (var line in text.ReadLines().Concat([""]))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                var key = new Key(pins);
                if (isLock) yield return new Lock(key);
                else yield return key;
            
                pins = Array.Empty<int>();
                continue;
            }

            if (pins.Length == 0)
            {
                pins = new int[line.Length];
                isLock = line[0] == '#';
                pinChar = isLock ? '.' : '#';
            }

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == pinChar) pins[i]++;
            }
        }
    }

    private static bool Fits(this Lock @lock, Key key) =>
        @lock.Key.Pins
            .Zip(key.Pins, (lockPin, keyPin) => lockPin >= keyPin)
            .All(x => x);

    abstract record Item;

    record Key(int[] Pins) : Item;
    record Lock(Key Key) : Item;
}
