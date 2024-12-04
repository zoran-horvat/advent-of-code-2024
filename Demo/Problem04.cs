using System.Text.RegularExpressions;

static class Problem04
{
    public static void Run()
    {
        var matrix = Console.In.GetAllLines().ToList();
        int rows = matrix.Count;
        int cols = matrix[0].Length;

        int count = matrix.GetAllStrings(rows, cols).Sum(s => s.CountWord("XMAS"));
        int xcount = matrix.GetAllXes(rows, cols).Count(x => ValidXs.Contains(x));

        Console.WriteLine($"XMAS count: {count}");
        Console.WriteLine($"X-MAS count: {xcount}");
    }

    private static string[] ValidXs = [ "MASMAS", "MASSAM", "SAMMAS", "SAMSAM" ];

    private static IEnumerable<string> GetAllXes(this List<string> matrix, int rows, int cols) =>
        GetXCenters(rows, cols).Select(center => matrix.GetX(center.row, center.col));

    private static string GetX(this List<string> matrix, int row, int col) => new string([
        matrix[row - 1][col - 1], matrix[row][col], matrix[row + 1][col + 1],
        matrix[row - 1][col + 1], matrix[row][col], matrix[row + 1][col - 1]]);

    private static IEnumerable<(int row, int col)> GetXCenters(int rows, int cols) =>
        Enumerable.Range(1, rows - 2).SelectMany(row => Enumerable.Range(1, cols - 2).Select(col => (row, col)));

    private static int CountWord(this string s, string word) =>
        Regex.Matches(s, Regex.Escape(word)).Count;

    private static IEnumerable<string> GetAllStrings(this IEnumerable<string> matrix, int rows, int cols) =>
        matrix.Rows().Concat(matrix.Columns(cols)).Concat(matrix.Diagonals(rows, cols)).Concat(matrix.Antidiagonals(rows, cols)).TwoWay();

    private static IEnumerable<string> Rows(this IEnumerable<string> matrix) => matrix;

    private static IEnumerable<string> Columns(this IEnumerable<string> matrix, int cols) =>
        Enumerable.Range(0, cols).Select(i => new string(matrix.Select(row => row[i]).ToArray()));

    private static IEnumerable<string> Diagonals(this IEnumerable<string> matrix, int rows, int cols) =>
        Enumerable.Range(0, cols).Select(col => matrix.Diagonal(0, col, cols))
            .Concat(Enumerable.Range(1, rows - 1).Select(row => matrix.Diagonal(row, 0, cols)));
    
    private static IEnumerable<string> Antidiagonals(this IEnumerable<string> matrix, int rows, int cols) =>
        matrix.Reverse().Diagonals(rows, cols);

    private static string Diagonal(this IEnumerable<string> matrix, int startRow, int startCol, int cols) =>
        new string(matrix.Skip(startRow).Take(cols - startCol).Select((row, i) => row[startCol + i]).ToArray());

    private static IEnumerable<string> TwoWay(this IEnumerable<string> strings) =>
        strings.SelectMany(s => new[] { s, new string(s.Reverse().ToArray()) });
}
