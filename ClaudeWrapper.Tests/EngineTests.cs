// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using TruePath;

namespace ClaudeWrapper.Tests;

public class EngineTests
{
    private static string RootedDir(string name) =>
        OperatingSystem.IsWindows() ? $"C:\\{name}" : $"/{name}";

    private sealed class FakeConsole : IConsole
    {
        public List<string> Messages { get; } = [];
        public void WriteLine(string message) => Messages.Add(message);
    }

    private sealed class FakeProcessRunner(int? result) : IProcessRunner
    {
        public int CallCount { get; private set; }
        public ProcessStartInfo? LastStartInfo { get; private set; }

        public Task<int?> RunAsync(ProcessStartInfo startInfo)
        {
            CallCount++;
            LastStartInfo = startInfo;
            return Task.FromResult(result);
        }
    }

    [Test]
    public async Task ClaudeExecutableMissing_ReturnsOneAndWritesMessage_WithoutStartingProcess()
    {
        var console = new FakeConsole();
        var processRunner = new FakeProcessRunner(0);
        var engine = new Engine(new WrapperConfiguration([]), console, processRunner);

        var exitCode = await engine.Run(null, new AbsolutePath(RootedDir("work")), []);

        await Assert.That(exitCode).IsEqualTo(1);
        await Assert.That(console.Messages).Contains("Cannot find the main Claude Code executable.");
        await Assert.That(processRunner.CallCount).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessFailsToStart_ReturnsTwoAndWritesMessage()
    {
        var console = new FakeConsole();
        var processRunner = new FakeProcessRunner(null);
        var engine = new Engine(new WrapperConfiguration([]), console, processRunner);
        var claude = new AbsolutePath(RootedDir("bin")) / "claude.exe";

        var exitCode = await engine.Run(claude, new AbsolutePath(RootedDir("work")), []);

        await Assert.That(exitCode).IsEqualTo(2);
        await Assert.That(console.Messages).Contains($"Cannot start \"{claude.Value}\".");
    }

    [Test]
    public async Task ProcessStarts_ReturnsItsExitCode()
    {
        var console = new FakeConsole();
        var processRunner = new FakeProcessRunner(42);
        var engine = new Engine(new WrapperConfiguration([]), console, processRunner);
        var claude = new AbsolutePath(RootedDir("bin")) / "claude.exe";

        var exitCode = await engine.Run(claude, new AbsolutePath(RootedDir("work")), []);

        await Assert.That(exitCode).IsEqualTo(42);
    }

    [Test]
    public async Task PassesExecutablePathAndArgsToProcessStartInfo()
    {
        var processRunner = new FakeProcessRunner(0);
        var engine = new Engine(new WrapperConfiguration([]), new FakeConsole(), processRunner);
        var claude = new AbsolutePath(RootedDir("bin")) / "claude.exe";

        await engine.Run(claude, new AbsolutePath(RootedDir("work")), ["foo", "bar"]);

        await Assert.That(processRunner.LastStartInfo).IsNotNull();
        await Assert.That(processRunner.LastStartInfo!.FileName).IsEqualTo(claude.Value);
        await Assert.That(processRunner.LastStartInfo!.ArgumentList).IsEquivalentTo(["foo", "bar"]);
    }

    [Test]
    public async Task WorkingDirMatchesConfiguredPattern_SetsConfigDirEnvironmentVariable()
    {
        var configDir = new AbsolutePath(RootedDir("configs\\project"));
        var configuration = new WrapperConfiguration(new Dictionary<LocalPathPattern, AbsolutePath>
        {
            [new LocalPathPattern("**/project/**")] = configDir,
        });
        var processRunner = new FakeProcessRunner(0);
        var engine = new Engine(configuration, new FakeConsole(), processRunner);
        var claude = new AbsolutePath(RootedDir("bin")) / "claude.exe";
        var workingDir = new AbsolutePath(RootedDir("project")) / "sub";

        await engine.Run(claude, workingDir, []);

        await Assert.That(processRunner.LastStartInfo!.Environment.TryGetValue("CLAUDE_CONFIG_DIR", out var value))
            .IsTrue();
        await Assert.That(processRunner.LastStartInfo!.Environment["CLAUDE_CONFIG_DIR"]).IsEqualTo(configDir.Value);
    }

    [Test]
    public async Task WorkingDirDoesNotMatchAnyPattern_DoesNotSetConfigDirEnvironmentVariable()
    {
        var configuration = new WrapperConfiguration(new Dictionary<LocalPathPattern, AbsolutePath>
        {
            [new LocalPathPattern("**/project/**")] = new AbsolutePath(RootedDir("configs\\project")),
        });
        var processRunner = new FakeProcessRunner(0);
        var engine = new Engine(configuration, new FakeConsole(), processRunner);
        var claude = new AbsolutePath(RootedDir("bin")) / "claude.exe";
        var workingDir = new AbsolutePath(RootedDir("unrelated"));

        await engine.Run(claude, workingDir, []);

        await Assert.That(processRunner.LastStartInfo!.Environment.ContainsKey("CLAUDE_CONFIG_DIR")).IsFalse();
    }
}
