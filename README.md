# Tiled2ZXNext

Tiled2ZXNext converts Tiled JSON maps into Z80 assembly for a ZX Spectrum Next game. It assembles scenes with sjasmplus, compresses the resulting binaries with ZX0, generates world-neighbour tables, and updates tileset and sprite metadata.

This project is in preview and targets game developers who can adapt their Tiled maps and assembly symbols to the expected scene format. The scene format requires game-specific Tiled properties and assembly symbols; an arbitrary Tiled map will need preparation before conversion. The executable and solution retain the name `Tiled2dot8`.

## Requirements and build

-.NET 10 SDK (or later).
- Tiled JSON maps and their referenced tilesets and templates.
- A GIMP palette (`.gpl`) for scene conversion.
- sjasmplus and ZX0 for the configured scene post-processing pipeline.
- Your game's assembly includes defining the symbols used by the generated data.

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --no-launch-profile -- --help
```

Use `--no-launch-profile` with `dotnet run`: the checked-in launch profiles contain development-machine paths and preset arguments. For command-specific help:

```powershell
dotnet run --no-launch-profile -- scene --help
```

## Scene configuration

Edit [appconfig.json](appconfig.json) to point to your local tools before converting a scene. It is copied to the build output directory; rebuild after changing it if you run the built executable directly.

```json
{
  "SpriteSoftwareSuffix": "_PATTERN_ID",
  "offset": { "x": 0, "y": 0 },
  "assembler": {
    "app": "C:\\tools\\sjasmplus\\sjasmplus.exe",
    "args": "\"%1\" --raw=\"%2\""
  },
  "zip": {
    "app": "C:\\tools\\zx0\\zx0.exe",
    "args": "-f \"%1\" \"%2\""
  }
}
```

The scene command requires the `offset`, `assembler`, and `zip` configuration sections. For the assembler, `%1` is the generated `.asm` path and `%2` is the output `.bin` path. For compression, they are the `.bin` input and `.zx0` output paths. The coordinate offset is applied by processors that use the global offset settings.

## Commands

The paths below are examples; replace them with your project's paths.

### Convert a scene

```powershell
dotnet run --no-launch-profile -- scene -i "C:\game\tiled\chapter01\Chapter01_001.json" -a "C:\game\source" -r "C:\game\source\data\worlds" -p "C:\game\tiled\game.tiled-project" -c "C:\game\source\data\palette\layer2.gpl"
```

| Option | Required | Meaning |
| --- | --- | --- |
| `-i`, `--input` | Yes | Tiled JSON scene file. |
| `-a`, `--approot` | Yes | Game application/source root used to resolve project include paths. |
| `-r`, `--roompath` | Yes | Root directory for generated scene files. |
| `-p`, `--project` | No | Tiled project file providing custom property definitions and includes. Needed when the map relies on them. |
| `-c`, `--palette` | Yes | GIMP palette used for background and fill colours. |

Map properties determine the output names:

| Property | Purpose |
| --- | --- |
| `SceneName` | Assembly label and output filename stem. Use a valid assembly symbol. |
| `WorldName` | Subdirectory beneath `--roompath`. |

For `SceneName = Chapter01_001` and `WorldName = chapter01`, the pipeline writes:

```text
<roompath>/chapter01/
  Chapter01_001.asm
  Chapter01_001.bin
  Chapter01_001.zx0
  list.txt
```

The binary and compressed file depend on successful external tool execution. `list.txt` lists the `.zx0` files in the world output directory in sorted order. Re-running conversion overwrites generated files.

### Generate a world table

```powershell
dotnet run --no-launch-profile -- world -i "C:\game\tiled\chapter01.world" -o "C:\game\source\data\chapter01_world.inc"
```

Both options are required. The command emits left, right, top, and bottom neighbour IDs and coordinate offsets, ordered by scene ID. Create the output parent directory first.

Map filenames must end in three digits followed by `.json`, for example `Chapter01_001.json`: those digits become the scene ID. The current format stores one neighbour per direction and uses ID `0` for no neighbour. Use unique nonzero IDs that fit in a byte and layouts with at most one neighbour along each edge.

### Generate pattern metadata

```powershell
dotnet run --no-launch-profile -- patterns -i "C:\game\tilesheets,C:\game\sprites"
```

`--input` is required and accepts comma-separated directories (no spaces around commas). Each directory is searched recursively for `.tsx` files. The command creates or updates a `.metadata` file alongside each referenced tileset image, recording tile dimensions, columns, rows, and image path. It expects image-based tilesets with a nonzero column count.

### Assign tilesheet IDs

```powershell
dotnet run --no-launch-profile -- tilesheet -i "C:\game\tilesheets"
```

`--input` is required. The command recursively scans `.tsx` files, selects tilesheets with `Type = 0` (also the default when missing), sorts them by existing `TileSheetId`, and assigns consecutive IDs starting at zero. Changed tilesheets are rewritten in place.

### Update a sprite enum

```powershell
dotnet run --no-launch-profile -- sprite -i "C:\game\source\data\sprites\_sw_Index.inc" -p "C:\game\tiled\game.tiled-project" -t "SoftwareSprites"
```

All three options are required. `--type` names an existing enum property type in the Tiled project. The command replaces its values with sprite names extracted from assembly definitions and rewrites the project file in place.

Input lines should begin with a symbol ending in `_PATTERN_ID`, followed by whitespace and its definition:

```asm
HERO_PATTERN_ID equ 0
CRATE_PATTERN_ID equ 1
```

Blank lines and lines beginning with `;` are ignored. The enum receives `HERO` and `CRATE`.

## Preparing scenes in Tiled

The converter dispatches visible group layers by name, ignoring case:

| Group | Content |
| --- | --- |
| `layer2` | Tile layers for Layer 2 graphics; layers ending in `_input` are skipped. |
| `background` | Background fill areas. |
| `collision` | Collision objects. |
| `location`  | Named location. |
| `path` | Paths. |
| `gameobject` | Game objects with components. |
| `static` | Static tile sprites, including sprites in nested visible layers. |

Keep a root `layer2` group with a tile layer as its first child: scene dimensions are read from that child. The palette must contain the background colour; when no `BackgroundColour` layer is defined, the converter uses black.

Layer and object properties are part of the game's data contract. Block `Type` values must match the engine consuming the assembly. See [ProcessScene.cs](ProcessScene.cs) for dispatch and [Process](Process) for each processor's property lookups. A complete sample game project is not included.

Static sprites use tile objects with a GID. A tile or tileset `Name` property supplies the `<NAME>_PATTERN_ID` symbol. Hidden objects and hidden child layers are excluded, and non-tile objects are skipped. Tile metadata is looked up by tile ID, allowing sparse metadata entries.

For software `SpriteComponent` objects, use `SpriteName` directly or a tile GID with a `Name` property. Also supply `SpriteIndex` (numeric index) or `FlagName` (symbolic flag). Optional `OffSetX` and `OffSetY` properties support offsets on either or both axes. Other supported component classes are `ParameterComponent`, `SpriteHWComponent`, `ColliderComponent`, `AnimationComponent`, `ForceComponent`, and `PathComponent`; their properties are defined in [ProcessObjectComponents.cs](Process/Components/ProcessObjectComponents.cs).

## Validation

Build and run the sprite regression checks from the repository root using PowerShell (requires .NET 10):

```powershell
dotnet build
pwsh -File ./tests/sprite-regressions.ps1
```

The checks cover nested static sprites, visibility and record counts, sparse tile metadata, software sprites without GIDs, and single-axis offsets. They do not invoke sjasmplus or ZX0 or validate a complete game build.

## Current limitations

-. The implementation and post-processing paths are oriented toward Windows.
-. `scene --verbose` currently prints diagnostic messages and skips conversion; omit it when exporting scenes.
-. The project depends on game-specific metadata and assembly includes, and does not validate every input constraint before processing.
-. The `tilemap` processor exists but its group name is absent from the recognized group list, so it is not currently dispatched as a tilemap block.
-. The checked-in configuration and launch profiles contain development-machine paths and must be updated before use.
