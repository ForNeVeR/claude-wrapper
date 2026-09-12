<!--
SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

claude-wrapper [![Status Zero][status-zero]][andivionian-status-classifier]
==============
Wrapper executable to run Claude Code within different environments.

Installation
------------
TBD.

Usage
-----
Simply run `claude` from the package (preferably — put it into the `PATH`). It will discover the actual `claude` executable in your system, and run it, according to the configuration.

Configuration
-------------
Put the configuration file into `~/.claude-wrapper.json`. Contents:
```json
{
    "configDirectoriesPerPath": {
        "**/path glob/**": "C:\\Users\\yourname\\.claude-different-config-path"
    }
}
```

This allows the wrapper to choose the correct configuration directory for Claude running in different projects on the local system.

Documentation
-------------
- [Changelog][docs.changelog]
- [Contributor Guide][docs.contributing]
- [Maintainer Guide][docs.maintaining]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-zero-
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
[reuse]: https://reuse.software/
[docs.changelog]: CHANGELOG.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[docs.maintaining]: MAINTAINING.md
[reuse.spec]: https://reuse.software/spec-3.3/
