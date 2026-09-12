// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using TruePath;
using TruePath.SystemIo;

namespace ClaudeWrapper;

public static class ClaudeExecutable
{
    /// <param name="ownDirectory">
    /// The directory of this wrapper's own executable, if known. The wrapper is normally installed as
    /// <c>claude</c> on the <c>PATH</c> itself, so its directory has to be skipped: otherwise the lookup would
    /// resolve to the wrapper again instead of the original Claude Code executable.
    /// </param>
    /// <remarks>
    /// The <paramref name="ownDirectory"/> comparison is a plain platform-aware path comparison; a symlinked or
    /// otherwise aliased duplicate of our own directory on the <c>PATH</c> will not be recognized.
    /// </remarks>
    public static AbsolutePath? FindOriginal(
        string path,
        ExecutableLookup lookup,
        Func<AbsolutePath, bool>? fileExists = null,
        AbsolutePath? ownDirectory = null)
    {
        fileExists ??= static p => p.ExistsFile();

        var searchPaths = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new LocalPath(x).ResolveToCurrentDirectory());
        foreach (var p in searchPaths)
        {
            if (ownDirectory is { } own && p == own)
            {
                continue;
            }

            foreach (var executable in lookup.GetCandidates(p / "claude").Select(c => c.ResolveToCurrentDirectory()))
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
