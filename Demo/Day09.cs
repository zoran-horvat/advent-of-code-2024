static class Day09
{
    public static void Run()
    {
        var disk = Console.In.ReadDisk().ToArray();

        long checksum = disk.Compact().Sum(fragment => fragment.Checksum);
        long entireFilesChecksum = disk.CompactEntireFiles().Sum(fragment => fragment.Checksum);

        Console.WriteLine($"             Checksum: {checksum}");
        Console.WriteLine($"Entire files checksum: {entireFilesChecksum}");
    }

    private static IEnumerable<Fragment> CompactEntireFiles(this IEnumerable<Fragment> files)
    {
        List<Gap> gaps = files.Gaps().ToList();

        foreach (Fragment file in files.Reverse())
        {
            var moveTo = gaps
                .Where(gap => gap.Position < file.Position && gap.Length >= file.Length)
                .OrderBy(gap => gap.Position)
                .Take(1)
                .ToList();

            if (moveTo.Count == 0)
            {
                yield return file;
                continue;
            }

            yield return file with { Position = moveTo[0].Position };

            var gap = moveTo[0];
            gaps.Remove(gap);

            if (gap.Length > file.Length)
            {
                gaps.Add(new Gap(gap.Position + file.Length, gap.Length - file.Length));
            }
        }
    }

    private static IEnumerable<Gap> Gaps(this IEnumerable<Fragment> fragments)
    {
        int position = 0;
        foreach (Fragment fragment in fragments)
        {
            if (fragment.Position > position) yield return new Gap(position, fragment.Position - position);
            position = fragment.Position + fragment.Length;
        }
    }

    private static IEnumerable<Fragment> Compact(this IEnumerable<Fragment> files)
    {
        List<Fragment> disk = files.ToList();
        int compactedUntil = 0;
        int currentFile = 0;

        while (currentFile < disk.Count)
        {
            int gap = disk[currentFile].Position - compactedUntil;
            if (gap == 0)
            {
                compactedUntil += disk[currentFile].Length;
                yield return disk[currentFile];
                currentFile += 1;
            }
            else if (gap >= disk[^1].Length)
            {
                yield return disk[^1] with { Position = compactedUntil };
                compactedUntil += disk[^1].Length;
                disk.RemoveAt(disk.Count - 1);
            }
            else    // gap < disk[^1].Length
            {
                yield return new Fragment(disk[^1].FileId, compactedUntil, gap);
                compactedUntil += gap;
                disk[^1] = disk[^1] with { Length = disk[^1].Length - gap};
            }
        }
    }

    private static IEnumerable<Fragment> ReadDisk(this TextReader text)
    {
        int position = 0;
        foreach ((int? fileId, int blocks) in text.ReadSpec())
        {
            if (fileId.HasValue) yield return new Fragment(fileId.Value, position, blocks);
            position += blocks;
        }
    }

    record Fragment(int FileId, int Position, int Length)
    {
        // pos               + (pos + 1)         + ... + (pos + len - 1)
        // (pos + len - 1)   + (pos + len - 2)   + ... + pos
        // -------------------------------------------------------------
        // (2*pos + len - 1) + (2*pos + len - 1) + ... + (2*pos + len - 1)
        // len * (2*pos + len - 1) / 2
        // (*fileId)
        public long Checksum => (long)FileId * Length * (2 * Position + Length - 1) / 2;
    }

    record Gap(int Position, int Length);

    private static IEnumerable<(int? fileId, int blocks)> ReadSpec(this TextReader text) =>
        (text.ReadLine() ?? string.Empty)
            .Select(c => (int)(c - '0'))
            .Select((int value, int i) => (i % 2 == 0 ? (int?)(i / 2) : null, value));
}
