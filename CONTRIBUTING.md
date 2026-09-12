<!--
SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Contributor Guide
=================

Prerequisites
-------------
To work with the project, you'll need [.NET SDK 10][dotnet-sdk] or later.

Build
-----
Use the following shell command:

```console
$ dotnet build
```

Test
----
Use the following shell command:

```console
$ dotnet test
```

License Automation
------------------
If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```fsharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: MIT
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use the [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license MIT --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```

(Feel free to attribute the changes to "ClaudeWrapper contributors <https://github.com/ForNeVeR/claude-wrapper>" instead of your name in a multi-author file, or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

File Encoding Changes
---------------------
If the automation asks you to update the file encoding (line endings or UTF-8 BOM) in certain files, run the following PowerShell script ([PowerShell Core][powershell] is recommended to run this script):
```console
$ pwsh -c "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.3.0 -Force && Test-Encoding -AutoFix"
```

The `-AutoFix` switch will automatically fix the encoding issues, and you'll only need to commit and push the changes.

Publish a Release Build
------------------------
The release CI publishes a [Native AOT][native-aot] build per platform (RID): a single self-contained native executable that doesn't require .NET to be installed. To reproduce one locally, run:
```console
$ dotnet publish ClaudeWrapper --configuration Release --runtime <rid> --output publish/<rid>
```

Replace `<rid>` with a target platform: `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-arm64`, or `osx-x64` (see the full list in `scripts/github-actions.fsx`). The output directory (`publish/<rid>`, git-ignored) will contain the `claude` executable (`claude.exe` on Windows) together with the `LICENSE.txt` and `README.md` files.

Native AOT has additional requirements (see [the prerequisites][native-aot.prerequisites] for details):
- the target OS should match the OS you are building on (cross-architecture builds, e.g. `osx-x64` on an arm64 Mac, or `win-arm64` on x64 Windows, are supported);
- a native toolchain is required:
  - on Windows: Visual Studio with the **Desktop development with C++** workload,
  - on Linux: `clang` (and `zlib1g-dev` or equivalent),
  - on macOS: Xcode Command Line Tools.

GitHub Actions
--------------
If you want to update the GitHub Actions used in the project, edit the file that generated them: `scripts/github-actions.fsx`.

Then run the following shell command:
```console
$ dotnet fsi scripts/github-actions.fsx
```

[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
[native-aot]: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/
[native-aot.prerequisites]: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#prerequisites
[powershell]: https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell
[reuse]: https://reuse.software/
