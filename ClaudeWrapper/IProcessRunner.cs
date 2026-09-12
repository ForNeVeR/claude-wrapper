// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace ClaudeWrapper;

public interface IProcessRunner
{
    /// <returns>The process exit code, or <see langword="null"/> if the process failed to start.</returns>
    Task<int?> RunAsync(ProcessStartInfo startInfo);
}

public sealed class SystemProcessRunner : IProcessRunner
{
    public async Task<int?> RunAsync(ProcessStartInfo startInfo)
    {
        using var process = Process.Start(startInfo);
        if (process is null) return null;

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
