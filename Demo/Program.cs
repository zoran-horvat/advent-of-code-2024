Action[] problemSolutions = [Day01.Run, Day02.Run, Day03.Run, Day04.Run];

foreach (int index in ProblemIndices())
{
    Console.WriteLine("Paste your input:");
    problemSolutions[index]();
}

IEnumerable<int> ProblemIndices()
{
    string prompt = $"{Environment.NewLine}Enter the problem number [1-{problemSolutions.Length}] (ENTER to quit): ";
    Console.Write(prompt);
    while (true)
    {
        string input = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(input)) yield break;
        if (int.TryParse(input, out int number) && number >= 1 && number <= problemSolutions.Length) yield return number - 1;
        Console.Write(prompt);
    }
}