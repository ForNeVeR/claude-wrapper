// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using TruePath;

namespace ClaudeWrapper.Tests;

public class ExecutableLookupTests
{
    private static readonly LocalPath Command = new LocalPath("bin") / "claude";

    // TUnit's IsEquivalentTo compares structurally, recursing through LocalPath.Parent forever, so the candidates
    // are compared as strings.
    private static List<string> CandidateValues(ExecutableLookup lookup) =>
        lookup.GetCandidates(Command).Select(c => c.Value).ToList();

    [Test]
    public async Task Windows_ExpandsEachPathExtEntry_InOrder()
    {
        var candidates = CandidateValues(new ExecutableLookup.Windows(".cmd;.exe"));

        await Assert.That(candidates).IsEquivalentTo([
            Command.WithExtension(".cmd").Value,
            Command.WithExtension(".exe").Value,
        ]);
    }

    [Test]
    public async Task Windows_NullPathExt_YieldsNoCandidates()
    {
        var candidates = CandidateValues(new ExecutableLookup.Windows(null));
        await Assert.That(candidates).IsEmpty();
    }

    [Test]
    public async Task Windows_NeverYieldsTheExtensionlessCommand()
    {
        var candidates = CandidateValues(new ExecutableLookup.Windows(".exe"));
        await Assert.That(candidates).DoesNotContain(Command.Value);
    }

    [Test]
    public async Task Unix_YieldsTheExtensionlessCommandOnly()
    {
        var candidates = CandidateValues(new ExecutableLookup.Unix());
        await Assert.That(candidates).IsEquivalentTo([Command.Value]);
    }
}
