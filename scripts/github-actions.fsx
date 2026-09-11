let licenseHeader = """
# SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: MIT

# This file is auto-generated.""".Trim()

#r "nuget: Generaptor, 1.12.0"

open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands

let workflows = [

    let workflow name steps =
        workflow name [
            header licenseHeader
            yield! steps
        ]

    let dotNetJob id steps =
        job id [
            setEnv "DOTNET_CLI_TELEMETRY_OPTOUT" "1"
            setEnv "DOTNET_NOLOGO" "1"
            setEnv "NUGET_PACKAGES" "${{ github.workspace }}/.github/nuget-packages"

            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "Set up .NET SDK",
                usesSpec = Auto "actions/setup-dotnet"
            )
            step(
                name = "Cache NuGet packages",
                usesSpec = Auto "actions/cache",
                options = Map.ofList [
                    "key", "${{ runner.os }}.nuget.${{ hashFiles('**/*.*proj', '**/*.props') }}"
                    "path", "${{ env.NUGET_PACKAGES }}"
                ]
            )

            yield! steps
        ]

    let mainTriggers = [
        onPushTo "main"
        onPushTo "renovate/**"
        onPullRequestTo "main"
        onSchedule "0 0 * * 6"
        onWorkflowDispatch
    ]

    workflow "main" [
        name "Main"
        yield! mainTriggers

        dotNetJob "verify-workflows" [
            runsOn "ubuntu-24.04"
            step(run = "dotnet fsi ./scripts/github-actions.fsx verify")
        ]

        dotNetJob "check" [
            strategy(failFast = false, matrix = [
                "image", [
                    "macos-26"
                    "ubuntu-24.04"
                    "ubuntu-24.04-arm"
                    "windows-11-arm"
                    "windows-2025"
                ]
            ])
            runsOn "${{ matrix.image }}"

            step(
                name = "Build",
                run = "dotnet build"
            )
            step(
                name = "Test",
                run = "dotnet test",
                timeoutMin = 10
            )
        ]

        job "licenses" [
            runsOn "ubuntu-24.04"
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "REUSE license check",
                usesSpec = Auto "fsfe/reuse-action"
            )
        ]

        job "encoding" [
            runsOn "ubuntu-24.04"
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "Verify encoding",
                shell = "pwsh",
                run = "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.3.0 -Force && Test-Encoding"
            )
        ]

        job "todos" [
            runsOn "ubuntu-24.04"
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            step(
                name = "Check TODOs",
                uses = "ForNeVeR/Todosaurus/action@v1",
                options = Map.ofList [
                    "github-token", "${{ secrets.GITHUB_TOKEN }}"
                    "strict", "true"
                ]
            )
        ]
    ]

    let releaseRids = [
        "win-x64"
        "win-arm64"
        "linux-x64"
        "linux-arm64"
        "osx-arm64"
    ]

    let getVersionStep = step(
        id = "version",
        name = "Get version",
        shell = "pwsh",
        run = "echo \"version=$(scripts/Get-Version.ps1 -RefName $env:GITHUB_REF)\" >> $env:GITHUB_OUTPUT"
    )

    workflow "release" [
        name "Release"
        yield! mainTriggers
        onPushTags "v*"

        dotNetJob "publish" [
            strategy(failFast = false, matrix = [
                "rid", releaseRids
            ])
            runsOn "ubuntu-24.04"
            getVersionStep
            step(
                name = "Publish",
                run = "dotnet publish ClaudeWrapper --configuration Release --runtime ${{ matrix.rid }} --self-contained -p:Version=${{ steps.version.outputs.version }} --output publish/${{ matrix.rid }}"
            )
            step(
                name = "Archive the published output",
                run = "cd publish/${{ matrix.rid }} && zip -r ../../ClaudeWrapper.${{ steps.version.outputs.version }}.${{ matrix.rid }}.zip . && cd ../.."
            )
            step(
                name = "Upload artifacts",
                usesSpec = Auto "actions/upload-artifact",
                options = Map.ofList [
                    "name", "ClaudeWrapper.${{ matrix.rid }}"
                    "path", "ClaudeWrapper.${{ steps.version.outputs.version }}.${{ matrix.rid }}.zip"
                ]
            )
        ]

        job "release" [
            needs "publish"
            jobPermission(PermissionKind.Contents, AccessKind.Write)
            runsOn "ubuntu-24.04"
            step(
                name = "Check out the sources",
                usesSpec = Auto "actions/checkout"
            )
            getVersionStep
            step(
                name = "Download artifacts",
                usesSpec = Auto "actions/download-artifact",
                options = Map.ofList [
                    "pattern", "ClaudeWrapper.*"
                    "path", "artifacts"
                    "merge-multiple", "true"
                ]
            )
            step(
                name = "Read changelog",
                usesSpec = Auto "ForNeVeR/ChangelogAutomation.action",
                options = Map.ofList [
                    "output", "./release-notes.md"
                ]
            )
            step(
                condition = "startsWith(github.ref, 'refs/tags/v')",
                name = "Create a release",
                usesSpec = Auto "softprops/action-gh-release",
                options = Map.ofList [
                    "body_path", "./release-notes.md"
                    "files", "./artifacts/*.zip"
                    "name", "ClaudeWrapper v${{ steps.version.outputs.version }}"
                ]
            )
        ]
    ]
]
exit <| EntryPoint.Process fsi.CommandLineArgs workflows
