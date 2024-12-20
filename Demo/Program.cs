Action[] problemSolutions =
[
    Day01.Run, Day02.Run, Day03.Run, Day04.Run, Day05.Run,
    Day06.Run, Day07.Run, Day08.Run, Day09.Run, Day10.Run,
    Day11.Run, Day12.Run, Day13.Run, Day14.Run, Day15.Run,
    Day16.Run, Day17.Run, Day18.Run, Day19.Run, Day20.Run
];

foreach (int index in ProblemIndices())
{
    Console.WriteLine("Paste your input:");
    problemSolutions[index]();
}

IEnumerable<int> ProblemIndices()
{
    string prompt = $"{Environment.NewLine}Enter the day number [1-{problemSolutions.Length}] (ENTER to quit): ";
    Console.Write(prompt);
    while (true)
    {
        string input = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(input)) yield break;
        if (int.TryParse(input, out int number) && number >= 1 && number <= problemSolutions.Length) yield return number - 1;
        Console.Write(prompt);
    }
}