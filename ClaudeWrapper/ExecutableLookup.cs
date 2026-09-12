// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using TruePath;

namespace ClaudeWrapper;

/// <summary>Platform rules for resolving a bare command name to an executable file.</summary>
public abstract class ExecutableLookup
{
    private ExecutableLookup()
    {
    }

    /// <summary>Returns the files to probe for the passed bare <paramref name="command"/> name.</summary>
    public abstract IEnumerable<LocalPath> GetCandidates(LocalPath command);

    /// <summary>On Windows, a command name resolves through the <c>PATHEXT</c> extensions.</summary>
    /// <remarks>
    /// An extension-less file is deliberately not a candidate: it is normally an npm shell script Windows cannot
    /// execute, and <c>claude.cmd</c> should win instead.
    /// </remarks>
    public sealed class Windows(string? pathExt) : ExecutableLookup
    {
        public override IEnumerable<LocalPath> GetCandidates(LocalPath command) =>
            (pathExt?.Split(';') ?? []).Select(ext => command.WithExtension(ext));
    }

    /// <summary>On Unix, a command name resolves to the extension-less file itself; there's no <c>PATHEXT</c>.</summary>
    public sealed class Unix : ExecutableLookup
    {
        public override IEnumerable<LocalPath> GetCandidates(LocalPath command) => [command];
    }
}
