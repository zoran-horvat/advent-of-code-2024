using System.Text.RegularExpressions;

static class Common
{
    public static (T a, T b) ToPair<T>(this List<T> list) => list switch
    {
        [T a, T b] => (a, b),
        _ => throw new ArgumentException()
    };

    public static List<List<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> values) =>
        values.Aggregate(
            new List<List<T>>(),
            (acc, row) =>
            {
                var i = 0;
                foreach (var cell in row)
                {
                    if (acc.Count <= i) acc.Add(new List<T>());
                    acc[i++].Add(cell);
                }
                return acc;
            });

    public static List<int> ParseIntsNoSign(this string line) =>
        Regex.Matches(line, @"\d+").Select(match => int.Parse(match.Value)).ToList();

    public static List<int> ParseInts(this string line) =>
        Regex.Matches(line, @"-?\d+").Select(match => int.Parse(match.Value)).ToList();

    public static List<long> ParseLongsNoSign(this string line) =>
        Regex.Matches(line, @"\d+").Select(match => long.Parse(match.Value)).ToList();

    public static IEnumerable<string> ReadLines(this TextReader reader)
    {
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (var item in sequence)
        {
            action(item);
            yield return item;
        }
    }
}