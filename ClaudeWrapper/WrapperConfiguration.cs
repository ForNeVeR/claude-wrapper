// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using TruePath;
using TruePath.SystemIo;

namespace ClaudeWrapper;

public record WrapperConfiguration(
    Dictionary<LocalPathPattern, AbsolutePath> ConfigDirectoriesPerPath
)
{
    private const string FileName = ".claude-wrapper.json";

    public static WrapperConfiguration Parse(string json)
    {
        var dto = JsonSerializer.Deserialize(json, WrapperConfigurationJsonContext.Default.WrapperConfigurationDto);
        var entries = dto?.ConfigDirectoriesPerPath ?? [];
        var configDirectoriesPerPath = entries.ToDictionary(
            entry => new LocalPathPattern(entry.Key),
            entry => new AbsolutePath(entry.Value));
        return new WrapperConfiguration(configDirectoriesPerPath);
    }

    public static async Task<WrapperConfiguration> LoadDefault(
        AbsolutePath homeDirectory,
        Func<AbsolutePath, Task<string?>> readFile)
    {
        var configPath = homeDirectory / FileName;
        var content = await readFile(configPath);
        return content is null ? new WrapperConfiguration([]) : Parse(content);
    }

    public static Task<WrapperConfiguration> LoadDefault() =>
        LoadDefault(
            new AbsolutePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
            static async path => path.ExistsFile() ? await File.ReadAllTextAsync(path.Value) : null);
}

internal sealed record WrapperConfigurationDto(Dictionary<string, string>? ConfigDirectoriesPerPath);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(WrapperConfigurationDto))]
internal partial class WrapperConfigurationJsonContext : JsonSerializerContext;
