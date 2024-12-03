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

    public static List<int> ParseInts(this string line) =>
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

    public static IEnumerable<string> GetAllLines(this TextReader reader)
    {
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }
}