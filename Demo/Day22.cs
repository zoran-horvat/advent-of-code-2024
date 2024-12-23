static class Day22
{
    public static void Run()
    {
        var numbers = Console.In.ReadNumbers().ToList();

        long sum = numbers.Sum(number => number.Evolve(Steps).ElementAt(2000));
        int bestBuy = numbers.Select(number => number.ToBuys(2000)).GetBestBuy();

        Console.WriteLine($"Sum after 2000 steps: {sum}");
        Console.WriteLine($"            Best buy: {bestBuy}");
    }

    private static IEnumerable<Offer> ToBuys(this long number, int stepsCount) =>
        number.Evolve(Steps).Take(stepsCount + 1).ToLastDigits().ToDiffs().ToOffers().ToBuys();

    private static int GetBestBuy(this IEnumerable<IEnumerable<Offer>> clientBuys)
    {
        Dictionary<Pattern, int> totalBuys = new();

        foreach (var buys in clientBuys)
        {
            foreach (var buy in buys)
            {
                int previousTotal = totalBuys.TryGetValue(buy.Pattern, out int total) ? total : 0;
                totalBuys[buy.Pattern] = previousTotal + buy.Price;
            }
        }

        return totalBuys.Values.Max();
    }

    private static IEnumerable<Offer> ToBuys(this IEnumerable<Offer> offers)
    {
        HashSet<Pattern> buys = new();
        return offers.Where(offer => buys.Add(offer.Pattern));
    }

    private static IEnumerable<Offer> ToOffers(this IEnumerable<(int diff, int price)> diffs)
    {
        using var enumerator = diffs.GetEnumerator();

        if (!enumerator.MoveNext()) yield break;
        var (a, _) = enumerator.Current;

        if (!enumerator.MoveNext()) yield break;
        var (b, _) = enumerator.Current;

        if (!enumerator.MoveNext()) yield break;
        var (c, _) = enumerator.Current;

        while (enumerator.MoveNext())
        {
            var (d, price) = enumerator.Current;
            if (price >= 0) yield return new(new(a, b, c, d), price);
            
            (a, b, c) = (b, c, d);
        }
    }

    private static IEnumerable<(int diff, int price)> ToDiffs(this IEnumerable<int> numbers)
    {
        using IEnumerator<int> enumerator = numbers.GetEnumerator();

        if (!enumerator.MoveNext()) yield break;
        int previous = enumerator.Current;

        while (enumerator.MoveNext())
        {
            yield return (enumerator.Current - previous, enumerator.Current);
            previous = enumerator.Current;
        }
    }

    private static IEnumerable<int> ToLastDigits(this IEnumerable<long> numbers) =>
        numbers.Select(number => (int)(number % 10));

    record Offer(Pattern Pattern, int Price);

    record Pattern(int A, int B, int C, int D);

    private static Func<long, long>[] Steps { get; } =
    [
        x => x << 6,
        x => x >> 5,
        x => x * 2048
    ];

    private static IEnumerable<long> Evolve(this long value, IEnumerable<Func<long, long>> transforms)
    {
        yield return value;
        while (true)
        {
            value = transforms.Aggregate(value, (current, transform) => current.EvolveStep(transform));
            yield return value;
        }
    }

    private static long EvolveStep(this long value, Func<long, long> transform) =>
        transform(value).MixWith(value).Prune();

    private static long MixWith(this long a, long b) => a ^ b;

    private static long Prune(this long value) => value % 16777216;

    private static IEnumerable<long> ReadNumbers(this TextReader text) =>
        text.ReadLines().Select(long.Parse);
}
