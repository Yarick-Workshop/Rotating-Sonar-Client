# Rotating-Sonar-Client
A desktop client to visualize data from Rotating-Sonar-Arduino

## Hotkeys

> **Note:** The following hotkeys are available only during visualization mode.

| Hotkey(s)      | Description                                 |
|---------------|---------------------------------------------|
| P             | Toggle point style                          |
| F, F3         | Toggle FPS display on/off                   |
| Z             | Toggle zoom display on/off                  |
| F11, Alt+Enter| Toggle fullscreen mode                      |
| Ctrl+=, Ctrl+- | Zoom in / zoom out (`=` / `-`; numpad `+` / `-` too) |
| Ctrl+0        | Reset zoom to default                       |
| Ctrl + wheel  | Zoom in / out with mouse wheel              |
| Escape        | Close the application window                |

## Supported Color Formats

Color settings in `Rotating.Sonar.Client.Console/appsettings.json` support:

- Hex: `#RRGGBB` and `#RRGGBBAA`
- Numeric: `R,G,B` and `R,G,B,A` (0-255 per component)

Examples:

- `#FFFF00`
- `#FFFF0080`
- `255,255,0`
- `255,255,0,128`
