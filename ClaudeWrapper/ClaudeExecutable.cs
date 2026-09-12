using TruePath;
using TruePath.SystemIo;

namespace ClaudeWrapper;

public static class ClaudeExecutable
{
    public static AbsolutePath? FindOriginal(string path, string? pathExt)
    {
        var extensions = pathExt?.Split(Path.PathSeparator) ?? [];
        var searchPaths = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new LocalPath(x));
        foreach (var p in searchPaths)
        {
            var claude = p / "claude";
            var executables = extensions.Select(ext => p.WithExtension(ext).ResolveToCurrentDirectory());
            foreach (var executable in executables)
            {
                if (executable.ExistsFile())
                {
                    return executable;
                }
            }
        }

        return null;
    }
}
