// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using ClaudeWrapper;
using TruePath;

var configuration = await WrapperConfiguration.LoadDefault();
var claudeExecutable = ClaudeExecutable.FindOriginal(
    Environment.GetEnvironmentVariable("PATH") ?? "",
    Environment.GetEnvironmentVariable("PATHEXT"));
var engine = new Engine(configuration, new SystemConsole(), new SystemProcessRunner());
return await engine.Run(claudeExecutable, AbsolutePath.CurrentWorkingDirectory, args);
