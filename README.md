# IconChop

**IconChop** is a Windows desktop app that splits a single image containing multiple icons (an *icon sheet* or *sprite sheet*) into individual icon files. It detects the grid of icons automatically, lets you choose which ones to export, and saves them as PNG and/or multi-size ICO files. All resulting icons are written with transparent backgrounds. There
is also support for Windows Explorer context menu to easily open an icon sheet.

## What it does

- **Load an icon sheet** — Open a PNG, BMP, JPG, GIF, or TIFF image that has several icons arranged in a grid (e.g. from a design tool or icon pack).
- **Auto-detect icons** — The app detects the background (transparent or a solid color from the image edges) and finds each icon cell in the grid, then shows a preview of every detected icon.
- **Select and export** — Click preview tiles to select or deselect icons. Export selected icons only, or leave nothing selected to export all. Choose output sizes (16×16 up to 512×512), output format (PNG, ICO, or both), and an output folder. Background is removed (made transparent) in the exported files.
- **Explorer integration** — Optionally add **“Open with IconChop”** to the Windows Explorer right-click menu for image files (.png, .jpg, etc.) so you can open a sheet directly from Explorer.
- **Auto-reload** — When “Auto-reload input” is enabled, the app watches the source file and reloads it when it changes (handy when editing the sheet in another program).

## Background detection and boundaries

IconChop infers the sheet background from the image edges and uses it to find icon boundaries. **If an icon contains pixels the same color as the sheet background and those pixels touch the edge of the icon cell**, the app may misidentify the boundary (e.g. crop too much or merge cells). In that case, use a version of the icon sheet with a **different background color** that does not appear on the boundaries of the icons—for example pink, bright green, or another color not used in the icons. Export from your design tool with that temporary background, run IconChop, then the exported PNGs/ICOs will still have a transparent background where that color was removed.

## Requirements

- **Windows** (Windows Forms app)
- **.NET 8** (or use the self-contained build so no separate runtime is needed)

## Building and running

From the project folder (where `IconChop.csproj` is):

```powershell
dotnet build
dotnet run
```

For a release package:

- **Both variants (local):**  
  `.\Create-Release-All.ps1`  
  Produces `release\IconChop-<version>.zip` and `release\IconChop-<version>-self-contained.zip`.

- **Framework-dependent only** (needs .NET 8 on the target PC):  
  `.\Create-Release.ps1`

- **Self-contained only** (single-folder deploy, no .NET install needed):  
  `.\Create-Release-SelfContained.ps1`

### GitHub releases (automated)

1. **Bump version and start a release:** In GitHub, go to **Actions → Version bump**, click **Run workflow**, choose **patch** (or **minor** / **major**), then **Run**. The workflow updates `IconChop.csproj`, commits, and pushes a tag (e.g. `v1.0.1`).
2. **Build and publish:** The **Release** workflow runs on that tag: it builds both the framework-dependent and self-contained packages and creates a GitHub Release with both zip files attached.

## Usage summary

1. **Load** an icon sheet with **Load Image...** or by dragging a file onto the source image area (or use **Open with IconChop** from Explorer if you’ve added it).
2. Check the **preview** — detected icons appear as tiles; click to toggle selection (yellow = selected). Use **Select all** / **Deselect all** if needed.
3. Set **Output Sizes** (e.g. 32×32, 48×48, 64×64), **Output format** (PNG / ICO / Both), **Output prefix**, and **Output directory**.
4. Click **Chop** to export. If any icons are selected, only those are exported; otherwise all detected icons are exported. Files are named like `icon_001_32x32.png`, `icon_001.ico`, etc.

Settings (last file, output folder, sizes, format, prefix, window position) are saved and restored when you next run the app.

## License

See repository or project for license details.
