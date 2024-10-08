<p align="center"> <img alt="Space Station 14" width="880" height="300" src="https://raw.githubusercontent.com/space-wizards/asset-dump/de329a7898bb716b9d5ba9a0cd07f38e61f1ed05/github-logo.svg" /></p>

Cosmatic Drift is a HRP fork of Station 14 that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), their homegrown engine written in C#.

This is the primary repo for Cosmatic Drift. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server.

If you want to host or create content for SS14, this is the repo you need. It contains both RobustToolbox and the content pack for development of new content packs.

## Links

[Discord](https://discord.gg/QVSsG9XS7c) | [Upstream's Website](https://spacestation14.com/) | [Launcher Download](https://spacestation14.io/about/nightlies/)

## Documentation/Wiki

Upstream's [docs site](https://docs.spacestation14.io/) has documentation on SS14s content, engine, game design and more. They also have lots of resources for new contributors to the project. CD specific information is listed in CONTRIBUTING.md

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help. We've got a [list of issues](https://github.com/cosmatic-drift-14/cosmatic-drift/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help either!
Just make sure your changes and pull requests are in accordance with the guidelines in CONTRIBUTING.md

We are not currently accepting translations of the game on our main repository. If you would like to translate the game into another language consider creating a fork or contributing to a fork.

## Building

1. Clone this repo.
2. Run `RUN_THIS.py` to init submodules and download the engine.
3. Compile the solution.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

Almost code for the content repository is licensed under [MIT](https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT). Some code is instead under the [MPL v2.0](https://www.mozilla.org/en-US/MPL/) and must remain open source and must be dual-licensed if mixed with the GPL. All files under the MPL are clearly marked with a file header.

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
