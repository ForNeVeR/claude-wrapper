using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;
using TruePath;

namespace ClaudeWrapper;

public class Engine(WrapperConfiguration configuration, IConsole console, IProcessRunner processRunner)
{
    private AbsolutePath? FindConfigLocation(AbsolutePath workingDir)
    {
        // The globbing matcher only ever matches paths relative to a root, so the patterns are matched against the
        // working directory path with its root (the drive letter on Windows) stripped off.
        var root = Path.GetPathRoot(workingDir.Value);
        if (root == null)
        {
            throw new Exception($"Cannot determine root for directory \"{workingDir.Value}\".");
        }

        foreach (var (pattern, config) in configuration.ConfigDirectoriesPerPath)
        {
            var matcher = new Matcher();
            matcher.AddInclude(pattern.Value);
            if (matcher.Match(root, workingDir.Value).HasMatches)
            {
                return config;
            }
        }

        return null;
    }

    public async Task<int> Run(AbsolutePath? claudeExecutable, AbsolutePath workingDir, IReadOnlyList<string> args)
    {
        if (claudeExecutable is not { } claude)
        {
            console.WriteLine("Cannot find the main Claude Code executable.");
            return 1;
        }

        var psi = new ProcessStartInfo(claude.Value, args);
        if (FindConfigLocation(workingDir) is {} location)
            psi.Environment["CLAUDE_CONFIG_DIR"] = location.Value;
        var exitCode = await processRunner.RunAsync(psi);
        if (exitCode is null)
        {
            console.WriteLine($"Cannot start \"{claude.Value}\".");
            return 2;
        }

        return exitCode.Value;
    }
}
