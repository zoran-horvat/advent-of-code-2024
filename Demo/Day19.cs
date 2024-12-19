static class Day19
{
    public static void Run()
    {
        var towels = Console.In.ReadTowels().ToArray();
        var patterns = Console.In.ReadPatterns().ToList();

        int possibleCount = patterns.Count(pattern => pattern.CountPossibleDynamic(towels) > 0);
        long totalCount = patterns.Sum(pattern => pattern.CountPossibleDynamic(towels));

        Console.WriteLine($"Possible patterns: {possibleCount}");
        Console.WriteLine($"  Total solutions: {totalCount}");
    }

    private static long CountPossibleDynamic(this string pattern, string[] towels)
    {
        long[] count = new long[pattern.Length + 1];
        count[0] = 1;

        for (int i = 0; i < pattern.Length; i++)
        {
            var candidates = towels
                .Where(towel => i + towel.Length <= pattern.Length)
                .Where(towel => pattern.Substring(i).StartsWith(towel));

            foreach (string towel in candidates) count[i + towel.Length] += count[i];
        }

        return count[pattern.Length];
    }

    private static bool IsPossibleRecursive(this string pattern, string[] towels) =>
        pattern.Length == 0 || towels.Any(firstTowel => pattern.IsPossibleRecursive(firstTowel, towels));

    private static bool IsPossibleRecursive(this string pattern, string firstTowel, string[] allTowels) =>
        pattern.StartsWith(firstTowel) && pattern.Substring(firstTowel.Length).IsPossibleRecursive(allTowels);

    private static string[] ReadTowels(this TextReader text) =>
        text.ReadLine()?.Split(", ", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

    private static IEnumerable<string> ReadPatterns(this TextReader text) =>
        text.ReadLines().Where(line => !string.IsNullOrWhiteSpace(line));
}
