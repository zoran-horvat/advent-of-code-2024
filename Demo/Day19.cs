static class Day19
{
    public static void Run()
    {
        var towels = Console.In.ReadTowels().ToArray();
        var patterns = Console.In.ReadPatterns().ToList();

        var allPossiblePatterns = patterns.BuildAllPossible(towels);

        Console.WriteLine();
        Console.WriteLine($"All possible patterns: {allPossiblePatterns.Count}");
        int enumeratedCount = patterns.Count(allPossiblePatterns.Contains);
        Console.WriteLine($"  Enumerated patterns: {enumeratedCount}");
        Console.WriteLine();

        int possibleCount = patterns.Count(pattern => pattern.CountPossible(towels) > 0);
        long totalCount = patterns.Sum(pattern => pattern.CountPossible(towels));

        Console.WriteLine($"    Possible patterns: {possibleCount}");
        Console.WriteLine($"      Total solutions: {totalCount}");
    }

    private static long CountPossible(this string pattern, string[] towels)
    {
        long[] counts = new long[pattern.Length + 1];
        counts[0] = 1;

        for (int i = 0; i < pattern.Length; i++)
        {
            if (counts[i] == 0) continue;

            string remainingPattern = pattern.Substring(i);
            foreach (string towel in towels.Where(remainingPattern.StartsWith))
            {
                counts[i + towel.Length] += counts[i];
            }
        }

        return counts[pattern.Length];
    }

    private static HashSet<string> BuildAllPossible(this List<string> patterns, string[] towels)
    {
        var allPossible = new HashSet<string>() { string.Empty };

        var allPrefixes = patterns
            .SelectMany(pattern => Enumerable.Range(0, pattern.Length + 1)
                .Select(i => pattern.Substring(0, i)))
                .ToHashSet();

        while (true)
        {
            int previousCount = allPossible.Count;
            var newPatterns = allPossible
                .SelectMany(pattern => towels.Select(towel => pattern + towel))
                .Where(allPrefixes.Contains)
                .ToList();

            allPossible.UnionWith(newPatterns);

            if (allPossible.Count == previousCount) break;

            Console.WriteLine($"Current count: {allPossible.Count}");
        }

        return allPossible;
    }

    private static IEnumerable<string> ReadPatterns(this TextReader text) =>
        text.ReadLines().Where(line => !string.IsNullOrWhiteSpace(line));

    private static IEnumerable<string> ReadTowels(this TextReader text) =>
        text.ReadLine()?.Split(", ", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
}