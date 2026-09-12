// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace ClaudeWrapper;

public interface IConsole
{
    void WriteLine(string message);
}

public sealed class SystemConsole : IConsole
{
    public void WriteLine(string message) => Console.WriteLine(message);
}
