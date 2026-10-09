# Changelog

All notable changes to Composa are recorded here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.4.0+zh.1] - 2026-10-09

### Added

- English / 简体中文 interface selection in Help > Language / 语言. The saved selection takes effect after restarting Composa.
- Simplified Chinese translations for menus, toolbars, status hints, the Layers and History panels, and major dialogs.

## [1.4.0] - 2026-10-03

Models that run on your own machine, and the first features taken from Lolly: Select Subject, Object Selection and Remove Background find the subject of an ordinary photo, Image Size enlarges with invented detail, a Color Lookup adjustment with bundled film looks and a `.cube` export, and GIMP files open as layers.

### Added

- Select > Subject, the Object Selection tool and Remove Background find the subject with a model run on your machine, so they work on ordinary photos and not only on a plain backdrop. One Detect choice, in the Object Selection options bar and in the Remove Background dialog, picks Any subject (U²-Net lite), Person (MODNet, which follows hair) or Plain backdrop, the method that was there before and still the exact one for product shots. Nothing is sent anywhere and nothing is downloaded: the models ship with Composa, under the Apache-2.0 licence recorded in the third-party notices. A model takes about half a second; a small window with Cancel appears when it takes longer, and its answer is kept so a second click on the same picture is immediate. Remove Background with a model adds a layer mask instead of erasing, so a wrong edge can be painted back. An agent passes `detect` to select_subject, select_object and filter_remove_background.
- The Object Selection tool takes a dragged box: the model looks at the box alone, so a small object in a large picture is found as well as a large one, and everything it finds inside the box is selected. An agent passes `width` and `height` to select_object for the same.
- Image > Image Size has a Resample choice: Automatic, the way it always worked; Nearest Neighbor, hard-edged blocks for pixel art; and Enhance, which enlarges each photo layer with a model run on your machine (Real-ESRGAN, BSD-3-Clause, recorded in the third-party notices) and invents fine detail where plain resampling would soften it. A progress window counts the tiles and says how long is left, and Cancel changes nothing. The choice is remembered. An agent gets Image Size as the image_size tool, with the same three choices.
- Layer > Enhance Resolution gives a raster layer that was placed small and scaled up enough pixels for its size on the canvas, through the same model Image Size's Enhance runs on your machine, and keeps its place, size and turn, so a logo placed large becomes crisp without the document changing. An agent gets it as the enhance_layer_resolution tool.
- Image > Adjustments > Color Lookup, and the same as an adjustment layer: grade the picture through a 3D lookup table, the `.cube` and `.3dl` files that DaVinci Resolve, Lightroom and look packs exchange. Four film looks come bundled, each drawn on the picture you are editing as a small tile so you see what it does before you choose it; Load File… adds a table of your own beside them, and Amount mixes the look into the original. A loaded table travels inside the project file, so the document opens the same anywhere; project files that use it are format version 6. An agent gets it as the adjust_color_lookup tool.
- File > Export Look as .cube bakes the document's adjustment layers into one lookup table, at 17, 33 or 65 points, that any editor can load, so a look built here can go to another program. What a table cannot hold (masked, clipped or grouped layers, grain, noise and blurs) is listed before anything is written. An agent gets it as the export_look tool.
- A Photoshop Color Lookup layer made from a `.cube` or `.3dl` file opens as a Composa Color Lookup with the same table; one made from an ICC profile stays reported as unsupported.
- Save Look… in the Camera Raw Filter writes the grade as a `.cube` lookup table for other editors: Light, Color, Color Grading, Curve, Color Mixer and Calibration go in, and Effects, Detail and Optics, which change pixels by their neighbours or their place, are left out and named.
- GIMP files open as layers: `.xcf` from GIMP 2.10, 3.0 and 3.2, with folders, masks, opacity, visibility, offsets, both generations of blend modes, guides and the resolution, in every precision GIMP saves (8 to 32-bit integers and 16 to 64-bit floats, linear or not), converted to Composa's 8-bit sRGB. Text in one style can be retyped; text with markup, GIMP 3's layer effects, vector and link layers arrive as pixels, and channels, paths and profiles are left out; `.xcf.gz` opens too, and bzip2 or xz files say how to save them instead; the same dialog Photoshop files use lists every conversion before anything is opened. Dropped onto an open document, a GIMP file arrives inside a folder. An agent's open_document opens a GIMP file that needs nothing converted.
- Grain Extract and Grain Merge blend modes, GIMP's pair for frequency separation, beside Subtract and Divide in the blend menu. A GIMP file's layers in these modes now render as GIMP renders them instead of being approximated by Subtract and Linear Dodge, and the import dialog no longer reports them.

### Changed

- Remove Background moved from the Filter menu to the Image menu, beside the adjustments: it changes what the layer shows rather than how its pixels look.
- The third-party notices ship with every build, including the Linux packages, because every build now carries the bundled film looks and the models that run on your machine, whose licences they record.

## [1.3.0] - 2026-09-30

The update strip downloads the new version and hands it to its installer, a History panel goes back any number of steps, and the catch-up with Compositor 1.3.3 to 1.4: Select > Color Range, fonts per letter, the Dither filter and the smaller items.

### Added

- Select > Color Range: click a color in the image to select it everywhere, then adjust Fuzziness and add or remove colors with the eyedroppers, or with Shift and Alt. Invert selects everything else, such as the subject in front of a green screen. The panel sits beside the canvas rather than over it, shows the selection in black and white, and the marching ants follow on the canvas as you go; OK keeps the selection as one undo step. An agent gets it as the select_color_range tool.
- The font, Bold and Italic can differ from letter to letter: select some of the text while typing and choose a family or tick Bold or Italic, and only those letters take it, as a color already does. The menu says (Multiple) for a selection in several families, and choosing one from it puts them all in that family. Project files that use this are format version 5.
- Filter > Dither turns a layer into dithered pixels, from the classic Mac's Atkinson look to Floyd-Steinberg, Bayer grids, halftone dots, lines and diamonds, the old Mac fill patterns and ASCII drawn as readable characters laid out like lines of text. Pixel Size makes chunky pixels, square or round like an LED screen; Tones, Diffusion, Density and Contrast shape the result; colors can be black and white, two colors picked from swatches that preview on the layer, or the picture's own. An agent gets it as the filter_dither tool.
- A History panel under the Layers panel lists every step, oldest first, and goes back or forward any number of them in one click, as Photoshop's does. Press on the list and drag to scrub through the steps with the canvas following. Steps Redo would bring back are dimmed until the next change drops them, the step in the saved file carries a disk, and each step has an icon for what it did. Click its header to collapse it, drag the line above it to size it, and use the new Window menu to hide or show it; the layout is remembered between launches.
- View > Grid Settings: the layout grid's color (Photoshop's set or a custom one), solid, dashed or dotted lines, their opacity, the pixels between gridlines (2 to 4096) and the subdivisions per square (1 to 64, never finer than a pixel). The grid shows while the dialog is open and follows every change; Cancel puts it back. The settings are remembered between launches like the other view options.
- Marquees, shapes and a selection outline being moved snap to the View > Snap To targets (guides, the grid, layer edges and the canvas edges), as moved layers and crop boxes already did. Ctrl places them freely, and Shift pressed while moving an outline keeps it on one axis.
- New Canvas opens on the size of the image on the clipboard: a Clipboard preset of that size is listed first and selected, so what you paste next fills the canvas exactly, and every other preset is a choice away.
- Layer > Merge Visible combines the visible layers and leaves the hidden ones; Layer > Stamp Visible (Ctrl+Alt+Shift+E) puts the picture as it looks on a new layer on top and keeps every layer.
- Image > Reveal All grows the canvas back to every layer, the way back from a crop; Image > Duplicate opens a copy of the document in a new tab.
- Edit > Paste Special: Paste in Place (Ctrl+Shift+V) keeps the place pixels were copied from even partly outside the canvas, and Paste Into (Ctrl+Alt+Shift+V) pastes onto a new layer masked by the selection.
- Right-click a document's tab for Copy Image (its whole flattened picture, whatever is selected), Duplicate, Show in Folder (the saved file, selected in the file manager), Close and Close Others.
- The update strip downloads the new version. Download fetches the file for the way Composa was installed and the processor it runs on (`.deb`, `.rpm`, AppImage, tarball, Windows installer or zip) into your Downloads folder, shows how far it got with Cancel, and checks it against the release's checksums before offering it. Show in Folder then selects it in the file manager, and Install hands it to its installer: on Windows Composa quits, asking about unsaved work, and starts the setup; a `.deb` or `.rpm` opens in your software installer, with the apt or dnf command to copy for a software centre that refuses it. A downloaded AppImage is made executable. Nothing is downloaded or started until you press the button, and Composa never replaces its own files.
- The status bar says what a copy put on the clipboard, such as "Copied 1920 × 1080 px".
- New Canvas presets for screens and social formats: 4K, 1440p and 1080p; iPhone, MacBook Pro and Studio Display; Instagram Square, Portrait and Story and a YouTube thumbnail, with the print sizes kept. The preset follows a typed size.

### Changed

- Undoing back to the state that was saved counts as saved again: the tab's dot goes, and closing asks nothing. Before, any undo marked the document as changed.
- A right-click on a tab opens its menu instead of switching to it.
- Hue/Saturation raises saturation as Photoshop does: +50 doubles it and +100 saturates any color fully. Before, +100 tripled it, so imported Photoshop layers came out too strong at small amounts and too weak near the top.
- Tools that come in groups are picked as in Photoshop. Press and hold a toolbar button, or right-click it, and its group opens beside it: each tool with its icon, name and key, and a dot at the current one. Click a tool there, or keep holding, slide onto one and let go. Marquee, Lasso, Magic, Brush and Eraser, Smear and Shape have groups, marked by a small triangle in the button's corner, and each button shows the tool last picked, so Smear and Shape now show their mode and shape too. The options bar no longer has boxes for choosing a variant; it names the tool in use instead. The keys work as before.

### Fixed

- After Help > Check for Updates with the automatic check turned off, the strip's Release notes and Skip this version did nothing.
- The `.deb` and `.rpm` told you to update through your package manager, which had never heard of Composa: they are downloaded from the releases page and installed by hand, so nothing would ever offer the next version. They now check for updates like the other downloads and open the release page; install the new file over the old one to upgrade. Builds that come from a repository can still be packaged as managed.

## [1.2.0] - 2026-09-27

An AI agent can drive Composa, a photo can become a painting, and the catch-up with Compositor 1.2.11 and 1.3.2.

### Added

- AI control. Composa hosts a Model Context Protocol server, so an AI agent (Claude Code, Claude Desktop or any other MCP client) can edit the open documents through the editor's own commands. Tick Help > Allow AI Control; it is off by default and remembered between launches. Every change an agent makes is one undoable step that appears in the window as it happens, and Ctrl+Z takes it back like anything else. The status bar says "AI connected" while an agent is attached. The macOS app instead watches its project folder for changes made by other programs; Composa has the agent talk to the editor.
- Fifty-two tools cover the editor: create, open, save and export documents; place image and SVG files as layers; add layers, text, shapes and lines; select, rename, hide, reorder, duplicate, delete, move, resize and rotate layers and set their opacity and blend mode; paint with the brush, eraser, blur, smudge, dodge and burn; apply every adjustment in place or as an adjustment layer and every filter but Camera Raw; make and modify selections with the marquee, lasso, wand, object and subject; undo; and render the canvas to see the result. The document list, a document's layers and its render are also resources an agent can read by URI.
- `composa --mcp` is the bridge an MCP client launches to reach the running application. It outlives Composa: while Composa is not running the tools are simply absent and a call says so, and each time Composa starts the tools appear again, so Composa can be started, quit and updated without touching the client. A request that arrives while the connection is still being set up waits for it instead of failing, and a request in flight when Composa quits gets an answer instead of hanging. Add `--launch` to the command and the bridge starts Composa when nothing answers.
- Painterly filter, in the Filter menu and as a tool: repaints a layer in brush strokes that follow the picture, the largest brush first and each smaller one only where the picture still differs, so a photo becomes a painting that is still recognizably the same photo. Four styles (impressionist, expressionist, colorist wash and pointillist), a brush size that fits itself to the picture, the number of brushes and how closely to follow the picture; the same seed paints the same strokes. The strokes are painted with the brush engine's falloff, in parallel by bands, so a 1000 by 1500 photo takes about two seconds.
- Tools for drawing by hand, so an agent's lines and colors come from the picture instead of from a guess: the render can carry a grid labelled in canvas coordinates to read positions from, or show one region at full size; the colors at points can be read, from the screen or from one layer under the agent's strokes; the picture's edges come back as polylines, longest first; and many brush strokes go in one call as one undoable step, so a painting is no longer capped by a round trip per stroke.
- Letters of a text layer can have their own colors: select some of the text while typing and pick a color from the Type bar's swatch or the foreground swatch, and only those letters take it. With nothing selected, or on a text layer that is not open for typing, the color goes on all of the text as before. New letters take the color of the letter before them, the swatch shows the color at the caret, and Fill still paints every letter. Project files that use this are format version 4.
- Saving writes in the background: the document as it is when you press Save goes to disk off the UI thread, so the tools stay usable while a large project encodes, and only that version counts as saved. The status bar names the file while it writes; closing waits for a save still writing.
- Camera Raw's Color Grading group sits directly under Color and opens with it.
- Hue/Saturation, Black & White and Color Balance sliders show their colors on the track. Hue shows the hue circle centred on the selected range's color (red to red when colorizing), Saturation runs from gray to the range's color or the tint, Lightness from black to white, each Black & White family from dark to light in its own hue, and Color Balance from each color to its opposite. Camera Raw's Temperature, Tint, Vibrance, Saturation, Glow Warmth, Color Mixer, Color Grading and Calibration sliders show theirs too.
- Every slider in a dialog can be reset: double-click it to type and a Reset button appears on its left, which puts it back to the value that changes nothing, or to a filter's default. Camera Raw's sliders reset to a fresh grade's values.
- Motion Blur's angle has a dial beside the field, as the shadow effects have. It is drawn as a line through the centre, since a blur runs along one, and turns the full circle: the angle now runs from -180 to 180 rather than -90 to 90, as Photoshop's does.
- Drag a number's label to change its value, as in Photoshop: the transform bar's X, Y, W, H and angle, the text size, tracking and leading, the object selection's edge offset, and the width, height and resolution in the New Canvas, Canvas Size and Image Size dialogs. Dragging moves in whole numbers, Alt makes it ten times finer, and typing still takes decimals.
- A layer mask can be painted anywhere on the canvas, past the layer's own pixels, with the brush, a gradient or a fill. The mask grows with its layer; new area starts as the mask's background, so a hide-all mask stays black and a reveal-all mask stays white.

### Fixed

- Bold and italic text rendered plain when the font family has no bold or italic face available to the renderer, as the default family did not. The renderer now substitutes a face and, failing that, synthesizes the weight and slant.
- A text layer renamed by hand took its text back as its name when it was restyled or recolored. The name now follows the text only until it is named by hand.
- Closing the window or a tab while typing text commits the text first, so the save prompt appears and the text is in what gets saved. Before, a document with nothing else changed closed without a word and the text was lost.

### Removed

- The importer for projects saved by the macOS app (`.comp` folders), and the File menu item that opened them. Composa's own `.cmps` project files are unaffected.

## [1.1.0] - 2026-09-25

Catches up with Compositor 1.2.7 to 1.2.10 and reworks every slider.

### Added

- Sliders in the options bar and in every dialog are now fields whose fill is the slider: drag to change, Alt-drag for ten times finer steps, double-click to type a value, and the wheel and arrow keys step by one.
- Shadow effects turn their light with a dial beside the angle field. The bright dot points at the light and the dim stub marks where the shadow falls.
- Shift squares a marquee held from the start when there is no selection to add to. While adding to one, letting Shift go and pressing it again squares the marquee, as in Photoshop.
- A selected text layer previews the Type bar's color picker as the color changes, without being opened for typing.
- Photoshop Large Document (`.psb`) files open through the same importer as `.psd`.
- Simple Photoshop text arrives as editable text: horizontal type layers keep their wording, font, size, color, alignment, tracking and leading. Vertical, sheared or unevenly scaled text still becomes pixels, and the import report says what was dropped.
- SVG files open and place as image layers, drawn by ImageMagick's SVG renderer. Opened, an SVG becomes a document at the size it declares; placed, it is drawn to fit the canvas, so a small icon still comes in sharp.
- A Photoshop file that would not fit in memory has its layers and masks cropped to the canvas instead of being refused; the import report lists every layer that was cut. A file that fits imports exactly as before.
- Text being typed previews the color picker's working color on the canvas, from the Type bar's swatch and from the foreground swatch alike. Cancel puts its own color back.
- With the Move tool, a double-click on text opens it for typing where you clicked.

### Changed

- A document's total raster now has its own budget, separate from the limit on any one layer: a quarter of the machine's memory, between 200 and 800 megapixels. One layer, canvas or export may be up to 200 megapixels (was 100). A print banner with dozens of large layers no longer fails to open against a limit meant for a single image.
- Marching ants around a detailed Magic Wand selection are drawn from a screen-resolution outline when zoomed out, so a selection with hundreds of thousands of edges no longer takes seconds per redraw.
- Clicking with the Type tool puts the first baseline at the pointer, as Photoshop does, so the letters rise from where you clicked instead of appearing a line lower.
- The color picker puts saturation and brightness in the square and hue on the strip, as Photoshop does. Starting from black, one click in the square finds a color; before, the strip held the brightness and stayed at zero.

### Fixed

- Changing a size, font or color in the Type bar for a text layer that was not being typed opened it for typing and moved the keyboard to the canvas, so the rest of what was typed in the field landed in the text. The layer is now restyled in place, and a run of changes undoes as one step.
- With an effect row highlighted in the Layers panel, Delete removed the effect even after a selection was drawn. Changing the selection now drops the highlight, so Delete clears the selected pixels.

## [1.0.0] - 2026-09-23

The first stable release, and the first for Windows.

### Added

- Windows builds: an installer that needs no administrator rights and a portable zip, for x64 and arm64. The installer adds a Start menu entry, makes Composa the program for `.cmps` projects and offers it under Open with for images without taking any over.
- HEIC, AVIF, TIFF and camera RAW open on Windows with nothing else installed: the Windows build carries ImageMagick, with its licence notices next to the executable. Linux builds keep using the distribution's ImageMagick.

### Changed

- On Windows, preferences are kept in `%APPDATA%\Composa` and crash-recovery copies in `%LOCALAPPDATA%\Composa`. Linux keeps its XDG locations unchanged.
- When ImageMagick is available but cannot read a file either, the error now gives ImageMagick's reason instead of suggesting to install it.

## [0.3.0] - 2026-09-23

Catches up with Compositor 1.2.3 to 1.2.6.

### Added

- Camera Raw Filter: Light, Color (with Auto white balance and an eyedropper on the panel's thumbnail), Effects (texture, clarity, dehaze, glow, vignette, grain), Curve, Color Mixer, Color Grading, Detail, Optics and Calibration, each group switchable off without clearing it, and a histogram of the graded layer.
- Finishing filters: Vignette in any color, which on an empty layer paints across the whole canvas; Bloom / Glow; Tonal Contrast.
- Gaussian Blur, Motion Blur and Add Noise as adjustment layers. Add Noise, as a layer and as a filter, offers a Gaussian distribution.
- The Inner Glow layer effect.
- Image > Trim… with a choice of transparent pixels or a corner's color, and which edges to trim.
- Copy and paste whole layers with nothing selected, folders and adjustments included, within a project or into another tab.
- A right-click menu on every layer row for the layer, its folder and its mask.
- Crop ratios 3:4 and 9:16, and a crop box that starts at the selection.
- Composa reports when a newer version is available, as a dismissable strip rather than a dialog. It never downloads or installs anything; the notice links to the release page. The check is one anonymous request a day, it can be turned off under Help, and builds installed from the `.deb` or `.rpm` never check at all because apt and dnf own updates for them.

### Changed

- Zoom In and Zoom Out step through fixed stops (12.5% to 1600%), anchored on the view's center.
- Duplicate Layer and Ctrl+J duplicate every selected layer as one step; several copies stack together above the topmost original and end up selected.
- Lens Correction keeps only Remove Distortion; the vignette has a filter of its own.
- Grain's Roughness adds smaller particles whose size follows Size instead of one-pixel noise.
- Project files are written as format version 3, which older builds cannot open when they hold the new adjustment layers or effect.

## [0.2.0] - 2026-09-23

The first release with downloadable packages. Composa has been buildable from source for a while; this is the first version you can simply install.

### Added

- Downloads for Linux on x86-64 and arm64, in four formats: an AppImage that runs on any distribution, a `.deb`, an `.rpm`, and a portable tarball with a per-user install script. Every release is published with a `sha256sums.txt`.
- The `.deb` and `.rpm` install a launcher, icons and the `.cmps` file type, and recommend ImageMagick rather than requiring it: it is needed only to open HEIC, AVIF, TIFF and camera RAW files.
- The version is shown in the About dialog. It is derived from the git tag, so a build can always be identified.
- An icon set covering the Linux hicolor sizes, Windows and macOS.
- AppStream metadata, so the application appears properly in GNOME Software and KDE Discover.

### Changed

- The project is now called **Composa**. It was Compositor for Linux, a name that no longer fits now that Windows and macOS builds are planned, and one that invited confusion with the macOS app it reimplements.
- Projects are saved as `.cmps` rather than `.compositor`. Existing files still open, because the reader looks at the archive manifest rather than the file extension.
- Preferences and crash-recovery files moved from `~/.config/compositor` and `~/.cache/compositor` to `~/.config/composa` and `~/.cache/composa`. Settings from before the rename are not carried over.

### Fixed

- Camera RAW and HEIC files could report a misleading error instead of saying that ImageMagick was missing. ImageMagick is now located once and asked to identify itself rather than trusted for its name.

[Unreleased]: https://github.com/dvdstelt/Composa/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/dvdstelt/Composa/releases/tag/v0.2.0
