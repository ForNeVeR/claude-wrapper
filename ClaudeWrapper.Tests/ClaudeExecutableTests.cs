// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using TruePath;

namespace ClaudeWrapper.Tests;

public class ClaudeExecutableTests
{
    private static string RootedDir(string name) =>
        OperatingSystem.IsWindows() ? $"C:\\{name}" : $"/{name}";

    [Test]
    public async Task EmptyPath_ReturnsNull()
    {
        var result = ClaudeExecutable.FindOriginal("", null, _ => true);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task NullPathExt_NoCandidatesProduced_ReturnsNull()
    {
        var dir = RootedDir("bin");
        var result = ClaudeExecutable.FindOriginal(dir, null, _ => true);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task NoMatchingFile_ReturnsNull()
    {
        var dir = RootedDir("bin");
        var result = ClaudeExecutable.FindOriginal(dir, ".exe", _ => false);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task MatchingFile_ReturnsItsPath()
    {
        var dir = RootedDir("bin");
        var expected = new AbsolutePath(dir) / "claude.exe";

        var result = ClaudeExecutable.FindOriginal(dir, ".exe", p => p == expected);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ChecksTheExecutableItself_NotTheBareDirectory()
    {
        // Regression test: FindOriginal must probe "<dir>/claude<ext>", not "<dir><ext>".
        var dir = RootedDir("bin");
        var buggyCandidate = new AbsolutePath(dir).WithExtension("exe");

        var result = ClaudeExecutable.FindOriginal(dir, ".exe", p => p == buggyCandidate);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TriesExtensionsInOrder_ReturnsFirstMatch()
    {
        var dir = RootedDir("bin");
        var claudeCmd = new AbsolutePath(dir) / "claude.cmd";
        var claudeExe = new AbsolutePath(dir) / "claude.exe";

        var result = ClaudeExecutable.FindOriginal(dir, ".cmd;.exe", p => p == claudeCmd || p == claudeExe);

        await Assert.That(result).IsEqualTo(claudeCmd);
    }

    [Test]
    public async Task FallsBackToLaterExtension_WhenEarlierOneDoesNotExist()
    {
        var dir = RootedDir("bin");
        var claudeExe = new AbsolutePath(dir) / "claude.exe";

        var result = ClaudeExecutable.FindOriginal(dir, ".cmd;.exe", p => p == claudeExe);

        await Assert.That(result).IsEqualTo(claudeExe);
    }

    [Test]
    public async Task FirstMatchingSearchDirectoryWins()
    {
        var dir1 = RootedDir("first");
        var dir2 = RootedDir("second");
        var claudeInDir1 = new AbsolutePath(dir1) / "claude.exe";
        var claudeInDir2 = new AbsolutePath(dir2) / "claude.exe";

        var result = ClaudeExecutable.FindOriginal(
            string.Join(Path.PathSeparator, dir1, dir2),
            ".exe",
            p => p == claudeInDir1 || p == claudeInDir2);

        await Assert.That(result).IsEqualTo(claudeInDir1);
    }

    [Test]
    public async Task SkipsNonMatchingDirectory_FindsMatchInLaterDirectory()
    {
        var dir1 = RootedDir("first");
        var dir2 = RootedDir("second");
        var claudeInDir2 = new AbsolutePath(dir2) / "claude.exe";

        var result = ClaudeExecutable.FindOriginal(
            string.Join(Path.PathSeparator, dir1, dir2),
            ".exe",
            p => p == claudeInDir2);

        await Assert.That(result).IsEqualTo(claudeInDir2);
    }

    [Test]
    public async Task EmptyEntriesInPath_AreIgnored()
    {
        var dir = RootedDir("bin");
        var expected = new AbsolutePath(dir) / "claude.exe";
        var path = $"{Path.PathSeparator}{dir}{Path.PathSeparator}";

        var result = ClaudeExecutable.FindOriginal(path, ".exe", p => p == expected);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task DefaultFileExistsCheck_IsUsed_WhenNotProvided()
    {
        var dir = RootedDir("nonexistent-claude-wrapper-test-dir");
        var result = ClaudeExecutable.FindOriginal(dir, ".exe");
        await Assert.That(result).IsNull();
    }
}
