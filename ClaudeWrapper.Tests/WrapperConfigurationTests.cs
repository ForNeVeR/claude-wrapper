// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using TruePath;

namespace ClaudeWrapper.Tests;

public class WrapperConfigurationTests
{
    private static string RootedDir(string name) =>
        OperatingSystem.IsWindows() ? $"C:\\{name}" : $"/{name}";

    [Test]
    public async Task Parse_EmptyObject_ReturnsEmptyDictionary()
    {
        var config = WrapperConfiguration.Parse("{}");
        await Assert.That(config.ConfigDirectoriesPerPath).IsEmpty();
    }

    [Test]
    public async Task Parse_NullDictionary_ReturnsEmptyDictionary()
    {
        var config = WrapperConfiguration.Parse("""{ "configDirectoriesPerPath": null }""");
        await Assert.That(config.ConfigDirectoriesPerPath).IsEmpty();
    }

    [Test]
    public async Task Parse_SingleEntry_MapsPatternAndPath()
    {
        var dir = RootedDir("configs\\project");
        var json = $$"""
        {
            "configDirectoriesPerPath": {
                "**/project/**": {{JsonSerializer.Serialize(dir)}}
            }
        }
        """;

        var config = WrapperConfiguration.Parse(json);

        await Assert.That(config.ConfigDirectoriesPerPath).Count().IsEqualTo(1);
        var entry = config.ConfigDirectoriesPerPath.Single();
        await Assert.That(entry.Key).IsEqualTo(new LocalPathPattern("**/project/**"));
        await Assert.That(entry.Value).IsEqualTo(new AbsolutePath(dir));
    }

    [Test]
    public async Task Parse_MultipleEntries_MapsAll()
    {
        var dirA = RootedDir("configs\\a");
        var dirB = RootedDir("configs\\b");
        var json = $$"""
        {
            "configDirectoriesPerPath": {
                "a/**": {{JsonSerializer.Serialize(dirA)}},
                "b/**": {{JsonSerializer.Serialize(dirB)}}
            }
        }
        """;

        var config = WrapperConfiguration.Parse(json);

        await Assert.That(config.ConfigDirectoriesPerPath).Count().IsEqualTo(2);
        await Assert.That(config.ConfigDirectoriesPerPath[new LocalPathPattern("a/**")]).IsEqualTo(new AbsolutePath(dirA));
        await Assert.That(config.ConfigDirectoriesPerPath[new LocalPathPattern("b/**")]).IsEqualTo(new AbsolutePath(dirB));
    }

    [Test]
    public async Task LoadDefault_ComputesPathFromHomeDirectory()
    {
        var home = new AbsolutePath(RootedDir("home"));
        AbsolutePath? requestedPath = null;

        await WrapperConfiguration.LoadDefault(home, path =>
        {
            requestedPath = path;
            return Task.FromResult<string?>(null);
        });

        await Assert.That(requestedPath).IsEqualTo(home / ".claude-wrapper.json");
    }

    [Test]
    public async Task LoadDefault_FileMissing_ReturnsEmptyConfig()
    {
        var home = new AbsolutePath(RootedDir("home"));

        var config = await WrapperConfiguration.LoadDefault(home, _ => Task.FromResult<string?>(null));

        await Assert.That(config.ConfigDirectoriesPerPath).IsEmpty();
    }

    [Test]
    public async Task LoadDefault_FilePresent_ParsesItsContent()
    {
        var home = new AbsolutePath(RootedDir("home"));
        var dir = RootedDir("configs\\project");
        var json = $$"""
        {
            "configDirectoriesPerPath": {
                "**/project/**": {{JsonSerializer.Serialize(dir)}}
            }
        }
        """;

        var config = await WrapperConfiguration.LoadDefault(home, _ => Task.FromResult<string?>(json));

        await Assert.That(config.ConfigDirectoriesPerPath).Count().IsEqualTo(1);
        await Assert.That(config.ConfigDirectoriesPerPath[new LocalPathPattern("**/project/**")]).IsEqualTo(new AbsolutePath(dir));
    }
}
