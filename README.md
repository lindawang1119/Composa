# Composa

This is an unofficial Simplified Chinese community fork based on upstream Composa v1.4.0.
See [简体中文使用说明](README.zh-CN.md) for language switching and the Windows x64 portable build.

A layer-based image editor for compositing and retouching, with Photoshop-style tools and shortcuts. It is a from-scratch implementation of [Compositor](https://github.com/robbietilton/Compositor), Robbie Tilton's free and open-source macOS app.

https://github.com/user-attachments/assets/e215c376-2bb3-44d2-af26-3117d2c70348

The macOS app is written in Swift on top of AppKit, SwiftUI, CoreImage, Metal and Vision, so it cannot be compiled for anything else. Composa rebuilds the same editor from scratch in C# with .NET 10, [Avalonia](https://avaloniaui.net/) and [SkiaSharp](https://github.com/mono/SkiaSharp), which run on Linux, Windows and macOS alike.

Composa is developed on Linux, on X11 and Wayland (through XWayland), and that is where it gets the most use. Windows builds are published too and the full test suite runs on Windows for every change, but the Windows version is newer and has seen far less real use. A macOS build is planned.

## Features

### Layers
- Layers and folders with 24 blend modes, grouped in the menu as Photoshop groups them, and opacity
- Layer effects: Stroke (outside or inside), Drop Shadow, Outer Glow, Inner Glow, Color Overlay and Inner Shadow, each switchable, editable with a live preview and copied between layers by Alt-dragging
- Layer masks on layers, folders and adjustment layers: paint, fill, gradient, invert, blur, apply, disable
- Clipping masks (Alt-click a layer, or Ctrl+Alt+G)
- Adjustment layers: Hue/Saturation, Levels, Curves, Exposure, Gradient Map, Color Lookup, Grain, Brightness/Contrast, Black & White, Color Balance, Invert, and the live Gaussian Blur, Motion Blur and Add Noise, which work on everything beneath them
- Merge Down, Merge Layers, Merge Group (Ctrl+E) and Flatten Image
- Duplicate (several at once, stacked together above the topmost), rename inline, reorder and nest by drag and drop; Alt-drag to duplicate
- Copy and paste whole layers, folders and adjustments included, within a project or into another tab, where they arrive centered; a right-click menu on every row for the layer, its folder and its mask
- Swipe down the eye column to show or hide many layers; Alt-click an eye to solo a layer

### Transform
- Non-destructive move, scale, rotate and flip: images keep their full resolution however small you make them
- Free distort by Ctrl-dragging a corner; the handles follow the corners, which keep distorting once the layer is distorted, and a corner dragged past the opposite edge folds the layer over itself
- Ctrl-drag moves the current layer with any tool active, as Photoshop's temporary Move tool does
- Auto Select picks the layer under the pointer, including one stacked on a selected background that covers the canvas; turn it off to drag the current layer from anywhere (Ctrl-click still picks)
- Transform several layers, or a whole folder, together
- Snapping to canvas and layer edges and centers, with guides
- Exact values for position, size and angle; arrow keys nudge (Shift for 10 px)
- Live shape layers (rectangle, rounded rectangle, ellipse, line) that are redrawn sharp when scaled
- Rulers, guides dragged out of them, a layout grid with its own color, style, spacing and subdivisions (View > Grid Settings), and snapping of moves, marquees, shapes, selection outlines and crops to guides, grid, layers and the canvas (View > Snap To)

### Selections
- Rectangle and Ellipse Marquee, Freehand and Polygonal Lasso, Magic tool with Wand (similar colors) and Object (the thing under the click) modes
- Add, subtract and intersect; move the outline; move or duplicate the pixels inside
- Select All, Inverse, Subject, Color Range (click colors on the canvas, with fuzziness and invert), Expand, Contract, Feather (also as buttons with amounts in the tool bar); load a layer's pixels or mask as a selection
- Content-Aware Fill, which can also extend an image past its edges

### Painting and retouching
- Brush and Eraser with size, hardness, stroke-level opacity and Smoothing, which trails the pointer so a shaky hand still draws a smooth line; Shift-click for straight lines
- Spot Healing Brush (content-aware)
- Clone Stamp, aligned or not, sampling one layer or all of them
- Smear tool: Liquify (push), Blur, Smudge, Dodge and Burn
- Gradient tool (linear or radial, to background or to transparent) that stays adjustable: drag either end, Enter applies
- Type tool: type straight onto the canvas as point text or in a dragged-out paragraph box, with font, size, style, color, alignment, tracking and leading in the tool bar, the font, style and color per letter; text stays editable and sharp when scaled
- Eyedropper and a full color picker
- Pen pressure varies the brush size on graphics tablets
- Every painting tool also works on masks

### Adjustments and filters
- Levels (with Auto and a histogram), Curves, Hue/Saturation (master and six color ranges, Colorize), Exposure, Gradient Map, Grain, Brightness/Contrast, Invert; Color Lookup through `.cube` and `.3dl` tables, with four bundled film looks drawn on your picture, and Export Look to write your own adjustments as a `.cube`
- Black & White with Photoshop's six color weights, so reds and greens stay apart instead of flattening into one gray, and an optional tint for sepia or cyanotype; Color Balance for shadows, midtones and highlights separately, with Preserve Luminosity
- Gaussian Blur and Motion Blur that spread past a layer's edges, Sharpen, Add Noise (uniform or Gaussian), Lens Correction
- Finishing filters: Vignette in any color (on an empty layer it paints across the whole canvas), Bloom / Glow, Tonal Contrast and Dither (Atkinson, Floyd-Steinberg, Bayer, halftone, Mac patterns and ASCII, in two colors or the picture's own)
- Camera Raw Filter: a grade panel with Light, Color (Auto white balance and an eyedropper), Effects (texture, clarity, dehaze, glow, vignette, grain), Curve, Color Mixer, Color Grading, Detail, Optics and Calibration, each group switchable off without losing its sliders, with a histogram of the result
- Live previews, limited to the selection when there is one

### Canvas and files
- Multiple projects in tabs
- Crop with a ratio picker (Original, 1:1, 4:3, 3:4, 16:9, 9:16), snapping, Shift to keep proportions, Alt for symmetric cropping; with a selection the crop box starts at its bounds
- Trim to transparent pixels or to a corner's color, on the edges you choose
- Zoom In and Zoom Out step through fixed stops, so ten steps in and ten out land back where they started
- Canvas Size, Image Size, and quarter-turn rotation of the canvas or of single layers
- Smooth downsampling when zoomed out, crisp pixels and a pixel grid when zoomed in
- Open PNG, JPEG, WebP, BMP and GIF (and HEIC, AVIF, TIFF and SVG through ImageMagick when it is installed); drop files onto the window; paste images from other apps. An SVG placed into a document is drawn to fit the canvas, so a small icon comes in sharp
- Open camera RAW files (Canon, Nikon, Sony, Fujifilm, DNG and more) through ImageMagick when it is installed: a develop step with exposure, temperature and tint and a live preview comes first, working on a 16-bit decode, so you choose what to keep before the image becomes an 8-bit layer
- Open GIMP files, `.xcf` from 2.10 through 3.2: layers, folders, masks, opacity, blend modes, guides and every precision come in, with the same report of conversions before anything is applied
- Open Photoshop files, `.psd` and Large Document `.psb`: layers, folders, masks, clipping, opacity, blend modes, solid fill shapes, adjustments and simple horizontal text come in editable, and a report lists everything that has to be converted before anything is applied; dropped onto an open document, a Photoshop file arrives inside a folder
- Export PNG, JPEG (with a live preview of the compression and the file size) and WebP; Copy Merged
- Undo history limited by memory, not by a fixed step count
- Tools that come in groups open beside their toolbar button when it is held or right-clicked, as in Photoshop: the marquees, the lassos, Magic Wand and Object Selection, Brush and Eraser, the Smear modes and the shapes
- Tool settings stick between launches: Auto Select, the transform controls, the pixel grid, rulers, guides, the grid and its settings, Snap and the Snap To options keep what you last set them to
- Autosave for crash recovery: unsaved work is copied to `~/.cache/composa/recovery` every two minutes and offered back after an unclean exit

A user guide covering every tool, menu and the AI control is in [docs/](docs/README.md).

### AI control

Composa can be driven by an AI agent through the [Model Context Protocol](https://modelcontextprotocol.io). Tick **Help > Allow AI Control** (off by default, remembered between launches) and the running application answers on a private, per-user pipe; the status bar says "AI connected" while an agent is attached. Every tool goes through the same editing commands the window uses, so an agent's change shows up as it happens and Ctrl+Z takes it back like any other step.

An MCP client reaches the application through `composa --mcp`, a bridge that carries the client's stdio to the pipe. The bridge outlives the application: while Composa is not running the tool list is empty, and each time it is started the tools appear again, so Composa can be started, quit and updated without touching the client. For Claude Code, from the folder you work in:

```bash
claude mcp add composa -- composa --mcp
```

Add `--launch` after `--mcp` and the bridge starts Composa when it is not running at the moment the client connects; a later quit is yours and is left alone.

The tools create a document, list and describe the open ones, place an image file as a layer, add a layer, fill it, add text, paint brush strokes, add shapes and lines, apply every adjustment (in place or as an adjustment layer) and every filter but Camera Raw, including Painterly, which repaints a layer in brush strokes that follow the picture so a photo becomes a painting that is still that photo, make and modify selections (marquee, lasso, wand, object, subject), select, rename, hide, reorder, duplicate, delete, move, resize and rotate layers, set opacity and blend mode, open a project or image file, save the project, export it as PNG, JPEG or WebP, undo, and render the canvas to a PNG so the agent can see what it did, with a labelled grid to read coordinates from or a region at full size. For drawing by hand there are the colors at points (sample_color), the picture's edges as polylines in canvas coordinates (trace_edges) and many strokes in one call (paint_strokes), so an agent's lines and colors can come from the picture instead of from a guess. The document list, a document's layers and its render are also resources (`composa://documents`, `composa://documents/1`, `composa://documents/1/image`) for a client that attaches context instead of calling tools. More follow. The macOS app instead watches its project folder for changes made by other programs; Composa has the agent talk to the editor.

## Differences from the macOS app

- Projects are saved as `.cmps` files: a zip archive with a JSON manifest and one PNG per layer and mask. Projects saved by the macOS app (`.comp` packages) cannot be opened.
- Remove Background, Select > Subject and the Magic tool's Object mode find the subject with a segmentation model run on your own machine (U²-Net lite for any subject, MODNet for people), where the macOS app uses Apple's Vision framework; the plain-backdrop method, exact on product shots, remains as a choice and as the fallback.
- HEIC, AVIF, TIFF, SVG and camera RAW open through ImageMagick, because Skia does not decode them itself. The Windows build includes it; on Linux they open when ImageMagick (`magick` or `convert`) is installed. SVG files are drawn by ImageMagick's librsvg rather than by macOS's own renderer, so an SVG that leans on features librsvg lacks may look different.
- A mask always moves and scales with its layer; it cannot be unlinked and transformed on its own.
- Layers cannot be dragged between tabs. Copy and paste (Ctrl+C, Ctrl+V) carries whole layers across when nothing is selected, and pixels when something is; layers pasted into another project arrive centered on its canvas.
- Hue/Saturation offers the master and six fixed color ranges; the ranges' widths are not adjustable.
- Double-clicking a slider types an exact value; a Reset button then appears on its left. The macOS app resets on double-click and types in a separate field.
- Point text grows from the edge its alignment reads from (right-aligned text grows leftward); the macOS app keeps the top-left corner.
- Layer effects are drawn on the CPU from a cached image; while a brush stroke is in progress they follow the pixels the stroke started from and catch up when it ends.
- Photoshop files are opened, never written. Horizontal text with one style stays editable; vertical, sheared or unevenly scaled text, smart objects and paths other than solid rectangles and ellipses arrive as pixels, layer effects are dropped, and adjustments other than Levels, Curves, Hue/Saturation, Brightness/Contrast, Exposure, Black & White, Color Balance and Invert are skipped; every such change is listed before the import goes ahead. Only 8-bit RGB `.psd` and `.psb` files open (no CMYK or 16-bit).
- Camera RAW files open only through ImageMagick's LibRaw delegate. The develop step applies exposure and white balance to the 16-bit decoded frame rather than to the sensor data, as Apple's RAW pipeline does on macOS, so its temperature and tint are relative to the camera's reading and there is no tone Boost control.
- The Camera Raw Filter has no Geometry group (Upright and guided lines), no vectorscope, no Option-drag clipping views, no point colors and no sharpening-mask overlay; its white-balance eyedropper works on the thumbnail in the panel rather than on the canvas, because the panel is a dialog. The filter renders on the full layer while you drag, so a very large layer answers more slowly than the macOS preview does.
- Layer masks always move with their layer, so the layer menu has no Link Mask item.
- A project changed on disk by another program while it is open is not reloaded; the macOS app watches its project folder and reloads it. Composa's projects are single files that nothing else edits.
- The transform bar's angle field has no dial; the dials sit beside the angle fields in the effects and blur dialogs.
- Composa tells you when a newer version is available and downloads it when you ask, but never installs it by itself: Install hands the checked file to its installer, and nothing is downloaded or replaced behind your back. See [Update checks](#update-checks).

Beyond the macOS app, this version adds Ctrl-drag to move a layer with any tool, Bold and Italic text, Brightness/Contrast, Sharpen, Dodge and Burn, WebP export, canvas and layer rotation, pen pressure, and autosave with crash recovery. Its Photoshop import also opens flattened files and zip-compressed layers, keeps solid color fill layers live, and maps Brightness/Contrast, Exposure, Invert, Black & White and Color Balance adjustments.

## Download

Every release publishes these on the [releases page](https://github.com/dvdstelt/Composa/releases), for x86-64 and arm64. None of them needs .NET installed.

| Format | For | Notes |
| --- | --- | --- |
| `-setup.exe` | Windows 10 and 11 | Installs for your account without administrator rights, adds a Start menu entry and the `.cmps` file type. |
| `-win-x64.zip` | Windows, no installation | Extract anywhere and run `composa.exe`. |
| `.AppImage` | Any Linux distribution | One file, no installation. Mark it executable and run it. |
| `.deb` | Debian, Ubuntu, Mint, Pop!_OS | Installs the launcher, icons and the `.cmps` file type. |
| `.rpm` | Fedora, RHEL, openSUSE | As above. |
| `.tar.gz` | Anything else, or no root | Extract and run `install.sh` for a per-user install. |

Check a download against the `sha256sums.txt` published with it:

```bash
sha256sum -c sha256sums.txt --ignore-missing
```

On Windows, compare the output of this with the line for that file:

```powershell
Get-FileHash .\composa-*-setup.exe
```

### Windows

Run `composa-<version>-win-x64-setup.exe`, or `win-arm64` on a Windows on Arm machine. It installs for your account only; if you have administrator rights it also offers to install for every user. The file type registration makes Composa the program for `.cmps` projects and adds it to **Open with** for images, without taking any image type away from the program that opens it today. Uninstall it from **Settings > Apps** like anything else. Your preferences stay behind in `%APPDATA%\Composa`.

Composa's Windows builds are not code signed yet, so the first time you run the installer or `composa.exe`, Microsoft Defender SmartScreen stops it with "Windows protected your PC". Click **More info**, check that the publisher reads "Unknown publisher" and the file name is the one you downloaded, then click **Run anyway**. SmartScreen shows this for any program that is unsigned or has not yet been downloaded often enough to build a reputation; it is not a detection of anything in the file. Checking the download against `sha256sums.txt` is the way to know it is the file that was published.

### AppImage

```bash
chmod +x Composa-*.AppImage && ./Composa-*.AppImage
```

### Debian, Ubuntu and derivatives

```bash
sudo apt install ./composa_*_amd64.deb
```

### Fedora, RHEL and openSUSE

```bash
sudo dnf install ./composa-*.x86_64.rpm
```

### Tarball, installed for one user

```bash
tar xzf composa-*-linux-x64.tar.gz && cd composa-*-linux-x64 && ./install.sh
```

That installs under `~/.local`, so it needs no root. Set `PREFIX` to install elsewhere.

### Update checks

Composa checks once a day whether a newer version has been released, and shows a dismissable strip when there is one, with its release notes, Skip this version and Download.

Download fetches the release's file for the way your copy was installed and the processor it runs on (the `.deb`, `.rpm`, AppImage or tarball, the Windows installer or zip) into your Downloads folder, and checks it against the release's `sha256sums.txt` before offering it. Show in Folder then opens the file manager with it selected, and Install, where there is an installer, hands it over: on Windows Composa quits, asking about unsaved work, and starts the setup; a `.deb` or `.rpm` opens in your software installer, with the `sudo apt install` or `sudo dnf install` command beside it to copy. A downloaded AppImage is made executable. Nothing is downloaded until you press Download, nothing starts until you press Install, and Composa never replaces its own files. The Windows builds are unsigned, so SmartScreen may warn when the setup starts, as it does for a setup downloaded from the releases page. More in [Settings and updates](docs/settings-and-updates.md#updates).

The check is a single anonymous `GET` to `https://api.github.com/repos/dvdstelt/Composa/releases/latest`. It sends no version number, no identifier, no machine details and no telemetry of any kind, and GitHub sees only what any visitor to that URL would show. If the request fails, nothing is reported and nothing is retried until the next day. A download sends the same and nothing more.

Turn it off under **Help > Check for Updates Automatically**, or set `COMPOSA_DISABLE_UPDATE_CHECK=1`, which is there so a distribution packager can switch it off without patching code. **Help > Check for Updates** still works when the automatic check is off.

The `.deb` and `.rpm` check too: they are downloaded from the releases page and installed by hand, so no repository will offer you the next version. Upgrade with Download and Install in the strip, or by installing the new release's file over the old one (`sudo apt install ./composa_*.deb` or `sudo dnf install ./composa-*.rpm`). A package built for a repository with `UPDATE_CHANNEL=managed` never checks on its own; there, the menu item says that the package manager owns updates rather than pointing you around it.

### ImageMagick

HEIC, AVIF, TIFF, SVG and camera RAW files open through ImageMagick.

On Windows it is included: the download carries [Magick.NET](https://github.com/dlemstra/Magick.NET), so nothing has to be installed separately. Its licences, including those of the LGPL libraries it contains for RAW and HEIC, are in `THIRD-PARTY-NOTICES.txt` and `ImageMagick-NOTICE.txt` next to `composa.exe`.

On Linux the packages recommend rather than require it, because every distribution ships ImageMagick and it is needed only for those formats. Install it if you want them; everything else works without it.

## Requirements

- Windows 10 or 11, x64 or arm64. Nothing else: .NET and ImageMagick are bundled.
- Linux, x86-64 or arm64, with Fontconfig and the usual X11 libraries, present on any desktop distribution. .NET is bundled; ImageMagick is optional, to open HEIC, AVIF, TIFF, SVG and camera RAW.
- macOS is not built yet.
- To build from source: the .NET 10 SDK, plus `rpmbuild` if you want the `.rpm` and [Inno Setup 6](https://jrsoftware.org/isinfo.php) if you want the Windows installer.

## Build from source

Build every package for the current architecture:
```bash
scripts/package/all.sh
```
Or just the portable tarball:
```bash
scripts/publish.sh
```
The Windows zip and installer, from Git Bash on Windows. On Linux, add `--no-installer` to build the zip alone, since Inno Setup runs only on Windows:
```bash
scripts/package/windows.sh win-x64
```

Run it straight from the checkout while developing:

```bash
dotnet run --project src/Composa.App
```

Open files from the command line:

```bash
dotnet run --project src/Composa.App -- photo.jpg project.cmps
```

## Tests

```bash
dotnet test
```

- `tests/Composa.Core.Tests` drives the editor through `EditorSession`: compositing, selections, every brush mode, healing, filters, canvas operations, project files, regressions found in review, and a fuzz test that runs thousands of random edits, undos and redos while checking the document stays consistent.
- `tests/Composa.App.Tests` runs the real window with Avalonia's headless platform and Skia rendering. Every tool, the layers panel, typing on the canvas, guides and the dialogs are driven with pointer, key and text events, and screenshots of the window and each dialog are written to `artifacts/screenshots/`, which is the way to review UI changes without a display.

## Shortcuts

| Keys | Action |
| --- | --- |
| V M L W C | Move, Marquee, Lasso, Magic, Crop (M and L again switch variants) |
| B E J S R | Brush, Eraser, Spot Healing, Clone Stamp, Smear |
| G U T I H Z | Gradient, Shape, Type, Eyedropper, Hand, Zoom |
| Tab | Switch the current tool's mode (Wand/Object, Brush/Eraser, the shape, and so on) |
| Ctrl+drag | Move the current layer with any tool |
| Space, middle button, Ctrl+wheel | Pan, zoom at the cursor |
| Ctrl+0, Ctrl+1 | Fit canvas, actual pixels |
| [ ] and { } | Brush size and hardness |
| 1 to 0 | Brush opacity, or layer opacity with the Move tool |
| X, D | Swap colors, reset to black and white |
| Alt+Backspace, Ctrl+Backspace | Fill with foreground, background |
| Shift+Backspace | Content-Aware Fill |
| Ctrl+A, Ctrl+D, Ctrl+Shift+I | Select all, deselect, inverse |
| Ctrl+J | Duplicate layer, or layer via copy with a selection |
| Ctrl+G, Ctrl+E, Ctrl+Alt+G | Group, merge, clipping mask |
| Ctrl+L, Ctrl+M, Ctrl+U, Ctrl+I | Levels, Curves, Hue/Saturation, Invert |
| \ | Switch between painting the layer and its mask |
| Ctrl+R, Ctrl+', Ctrl+; | Rulers, grid, guides |
| Ctrl+Shift+;, Ctrl+Alt+; | Snap, lock guides |
| Ctrl+Alt+A | Select Subject |
| While typing: Ctrl+Enter, Escape, Alt+arrows | Finish, cancel, tracking and leading |

Every shortcut can be changed in Help > Keyboard Shortcuts (F1).

## License

MIT, see [LICENSE](LICENSE). Compositor for macOS is Copyright (c) 2026 Wonder Assembly LLC, also MIT.
