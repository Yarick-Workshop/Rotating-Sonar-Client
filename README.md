<!-- Thanks to this Guy: https://github.com/santisoler/cc-licenses?tab=readme-ov-file#cc-attribution-noncommercial-sharealike-40-international-->
[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa] This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa]. [![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg

# Rotating-Sonar-Client

A desktop scan display client for real-time range/angle data from sonar or lidar-style sensors. It can connect via serial (COM port), parse angle:distance readings, and render an interactive PPI-style scan display using OpenGL 1.1.

⚠️ If anything is unclear, ask in the [Telegram group or use the other contact options below](#contact).

## Table of contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation & Build](#installation--build)
- [Usage](#usage)
  - [Command-line options](#command-line-options)
  - [Hotkeys](#hotkeys)
- [Configuration](#configuration)
  - [Color formats](#supported-color-formats)
- [Development](#development)
- [Support future projects](#support-future-projects)
- [Contact](#contact)

## Features

- Real-time serial data ingestion from a sensor device (or fake data for testing)
- Interactive scan display visualization with zoom and point style toggles
- Configurable colors, sweep, range grid, and scan point rendering via JSON settings
- Cross-platform (Windows/Linux) .NET 8 console + OpenGL window
- FPS, zoom, and latest-sweep overlays for debugging

## Requirements

- .NET 8 SDK or runtime
- A serial port (real or virtual) or use `-fake` mode
- OpenGL 1.1 capable GPU/driver (most modern systems)

## Installation & Build

Clone the repo and build from the solution root:

```bash
git clone https://github.com/Yarick-Workshop/Rotating-Sonar-Client.git
cd Rotating-Sonar-Client
dotnet build
```

Or run directly:

```bash
dotnet run --project Rotating.Sonar.Client.Console
```

## Usage

Run the console app and connect to your range/angle sensor device.

### Command-line options

```text
Usage: dotnet run -- -port <port_name> [-rate <baud_rate>] [-visualize] [-fake]
```

- `-port <port_name>` — COM port (e.g. `/dev/ttyUSB0` or `COM3`). If omitted or not found, the list of available ports is displayed and the app exits. Use `-fake` for UI testing without a real port.
- `-rate <baud_rate>` — baud rate (default 9600)
- `-visualize` — open the scan display window
- `-fake` — generate random test data (no hardware needed)

Examples:

```bash
dotnet run -- -port /dev/ttyUSB0 -visualize
dotnet run -- -fake -visualize
```

### Hotkeys

> **Note:** The following hotkeys are available only during visualization mode.

| Hotkey(s)       | Description                                 |
|-----------------|---------------------------------------------|
| P               | Toggle point style                          |
| F, F3           | Toggle FPS display on/off                   |
| Z               | Toggle zoom display on/off                  |
| S               | Toggle latest sweep display on/off          |
| F11, Alt+Enter  | Toggle fullscreen mode                      |
| Ctrl+=, Ctrl+-  | Zoom in / zoom out (`=` / `-`; numpad too)  |
| Ctrl+0          | Reset zoom to default                       |
| Ctrl + wheel    | Zoom in / out with mouse wheel              |
| Escape          | Close the application window                |

## Configuration

Color and rendering settings live in `Rotating.Sonar.Client.Console/appsettings.json` (copied to output).
The main configuration groups are `Visualizer.ScanPoints`, `Visualizer.Sweep`, and `Visualizer.RangeGrid`.

### Supported Color Formats

Color settings support:

- Hex: `#RRGGBB` and `#RRGGBBAA`
- Numeric: `R,G,B` and `R,G,B,A` (0-255 per component)

Examples:

- `#FFFF00`
- `#FFFF0080`
- `255,255,0`
- `255,255,0,128`

See the settings types in `Rotating.Sonar.Client.Common/Settings` for all tunable values.

## Development

The solution contains three projects:

- `Rotating.Sonar.Client.Console` — entry point, command-line parsing, appsettings file, and app orchestration
- `Rotating.Sonar.Client.Common` — shared COM port listeners, serial port helpers, app settings, and color parsing
- `Rotating.Sonar.Client.Visualizer` — OpenGL scan display, point buffer, rendering primitives, and visualizer window

Keep serial port I/O (`System.IO.Ports`) and settings/color serialization in `Rotating.Sonar.Client.Common`.
Follow the class-per-file rule and keep one top-level class per `.cs` file.

## Support future projects

You can support this and later work on **Patreon**: [patreon.com/YarickWorkshop](https://www.patreon.com/YarickWorkshop).

## Contact

New projects and updates:

- Telegram: [t.me/YarickWorkshop](https://t.me/YarickWorkshop)
- YouTube: [@yarick-workshop](https://www.youtube.com/@yarick-workshop)

**Questions?** Message me on Telegram 👆 or email **techno.man.983@gmail.com**. The channels are mainly in Russian, but **English is fine**.

⚠️ **Note:** I don’t reply to YouTube comments (even though I read them). Why? It is a HUGE secret 🙃
