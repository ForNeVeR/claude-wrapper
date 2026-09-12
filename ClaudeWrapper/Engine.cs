using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;
using TruePath;

namespace ClaudeWrapper;

public class Engine(WrapperConfiguration configuration)
{
    private AbsolutePath? FindConfigLocation(AbsolutePath workingDir)
    {
        foreach (var (pattern, config) in configuration.ConfigDirectoriesPerPath)
        {
            var matcher = new Matcher();
            matcher.AddInclude(pattern.Value);
            if (matcher.Match(workingDir.Value).HasMatches)
            {
                return config;
            }
        }

        return null;
    }

    public async Task<int> Run(AbsolutePath workingDir, IEnumerable<string> args)
    {
        var mainClaudeLocation = ClaudeExecutable.FindOriginal(
            Environment.GetEnvironmentVariable("PATH") ?? "",
            Environment.GetEnvironmentVariable("PATHEXT")
        );
        if (mainClaudeLocation is not { } claude)
        {
            Console.WriteLine("Cannot find the main Claude Code executable.");
            return 1;
        }

        var psi = new ProcessStartInfo(claude.Value, args);
        if (FindConfigLocation(workingDir) is {} location)
            psi.Environment["CLAUDE_CONFIG_DIR"] = location.Value;
        using var process = Process.Start(psi);
        if (process is null)
        {
            Console.WriteLine($"Cannot start \"{claude.Value}\".");
            return 2;
        }

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
