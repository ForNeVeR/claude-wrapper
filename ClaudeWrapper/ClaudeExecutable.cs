using TruePath;
using TruePath.SystemIo;

namespace ClaudeWrapper;

public static class ClaudeExecutable
{
    public static AbsolutePath? FindOriginal(string path, string? pathExt, Func<AbsolutePath, bool>? fileExists = null)
    {
        fileExists ??= static p => p.ExistsFile();

        var extensions = pathExt?.Split(Path.PathSeparator) ?? [];
        var searchPaths = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new LocalPath(x));
        foreach (var p in searchPaths)
        {
            var claude = p / "claude";
            var executables = extensions.Select(ext => claude.WithExtension(ext).ResolveToCurrentDirectory());
            foreach (var executable in executables)
            {
                if (fileExists(executable))
                {
                    return executable;
                }
            }
        }

        return null;
    }
}
