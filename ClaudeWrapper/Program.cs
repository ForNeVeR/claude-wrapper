// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using ClaudeWrapper;

var configuration = await WrapperConfiguration.LoadDefault();
var engine = new Engine(configuration);
return engine.Run(args);
