static class Day09
{
    public static void Run()
    {
        var disk = Console.In.ReadDisk().ToArray();

        long checksum = disk.Compact(MoveBlocks).Sum(file => file.Checksum);
        long filesMoveChecksum = disk.Compact(MoveFiles).Sum(file => file.Checksum);

        Console.WriteLine($"             Checksum: {checksum}");
        Console.WriteLine($"Moving files checksum: {filesMoveChecksum}");
    }

    private static IEnumerable<FileSection> Compact(this IEnumerable<Fragment> fragments, BlocksConstraint blocksConstraint)
    {
        var files = fragments.OfType<FileSection>().OrderByDescending(file => file.Position);
        var gaps = fragments.OfType<Gap>().OrderBy(gap => gap.Position).ToList();

        foreach (FileSection file in files)
        {
            var remainingGaps = new List<Gap>();
            int pendingBlocks = file.Length;

            foreach (var gap in gaps.Where(gap => gap.Position < file.Position))
            {
                int move = Math.Min(pendingBlocks, gap.Length);

                if (move < blocksConstraint(file))
                {
                    remainingGaps.Add(gap);
                    continue;
                }

                if (move > 0) yield return file with { Position = gap.Position, Length = move };
                pendingBlocks -= move;

                if (gap.Remove(move) is Gap remainder) remainingGaps.Add(remainder);
            }

            if (pendingBlocks > 0) yield return file with { Length = pendingBlocks };
            gaps = remainingGaps;
        }
    }

    private static int MoveBlocks(FileSection file) => 0;

    private static int MoveFiles(FileSection file) => file.Length;

    delegate int BlocksConstraint(FileSection file);

    private static IEnumerable<Fragment> ReadDisk(this TextReader text)
    {
        int position = 0;
        foreach ((int? fileId, int blocks) in text.ReadSpec())
        {
            if (fileId.HasValue) yield return new FileSection(fileId.Value, position, blocks);
            else yield return new Gap(position, blocks);
            position += blocks;
        }
    }

    abstract record Fragment(int Position, int Length);

    record FileSection(int FileId, int Position, int Length) : Fragment(Position, Length)
    {
        // pos               + (pos + 1)         + ... + (pos + len - 1)
        // (pos + len - 1)   + (pos + len - 2)   + ... + pos
        // -------------------------------------------------------------
        // (2*pos + len - 1) + (2*pos + len - 1) + ... + (2*pos + len - 1)
        // len * (2*pos + len - 1) / 2
        // (*fileId)
        public long Checksum => (long)FileId * Length * (2 * Position + Length - 1) / 2;
    }

    record Gap(int Position, int Length) : Fragment(Position, Length)
    {
        public Gap? Remove(int blocks) =>
            blocks >= Length ? null : new Gap(Position + blocks, Length - blocks);
    }

    private static IEnumerable<(int? fileId, int blocks)> ReadSpec(this TextReader text) =>
        (text.ReadLine() ?? string.Empty)
            .Select(c => (int)(c - '0'))
            .Select((int value, int i) => (i % 2 == 0 ? (int?)(i / 2) : null, value));
}
