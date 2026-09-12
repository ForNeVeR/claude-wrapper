// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using ClaudeWrapper;
using TruePath;

var configuration = await WrapperConfiguration.LoadDefault();
var lookup = OperatingSystem.IsWindows()
    ? new ExecutableLookup.Windows(Environment.GetEnvironmentVariable("PATHEXT"))
    : (ExecutableLookup)new ExecutableLookup.Unix();
var claudeExecutable = ClaudeExecutable.FindOriginal(
    Environment.GetEnvironmentVariable("PATH") ?? "",
    lookup);
var engine = new Engine(configuration, new SystemConsole(), new SystemProcessRunner());
return await engine.Run(claudeExecutable, AbsolutePath.CurrentWorkingDirectory, args);
