using TruePath;
using TruePath.SystemIo;

namespace ClaudeWrapper;

public static class ClaudeExecutable
{
    public static AbsolutePath? FindOriginal(
        string path,
        ExecutableLookup lookup,
        Func<AbsolutePath, bool>? fileExists = null)
    {
        fileExists ??= static p => p.ExistsFile();

        var searchPaths = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new LocalPath(x));
        foreach (var p in searchPaths)
        {
            foreach (var candidate in lookup.GetCandidates(p / "claude"))
            {
                var executable = candidate.ResolveToCurrentDirectory();
                if (fileExists(executable))
                {
                    return executable;
                }
            }
        }

        return null;
    }
}
