# Divoom Manager

## Theme — Slate

The app uses the **Slate** theme with full dark/light support. All colors are defined as `ThemeDictionary` entries in `App.xaml` and should be referenced via `{ThemeResource}` — never hardcode these values inline.

### App.xaml resources

| Key | Dark | Light | Usage |
|-----|------|-------|-------|
| `AppChromeBrush` | `#161B22` | `#FFFFFF` | Main content area background |
| `AppSidebarBrush` | `#21262D` | `#F6F8FA` | Side navigation pane background |
| `AppTitleBarBrush` | `#21262D` | `#EAEEF2` | Title bar / window chrome background |
| `AppAccentBrush` | `#58A6FF` | `#0969DA` | Selected nav indicator, interactive highlights |
| `AppNavHoverBrush` | `#18FFFFFF` | `#0F000000` | Nav button hover background |
| `AppNavPressedBrush` | `#0CFFFFFF` | `#07000000` | Nav button pressed background |
| `AppNavSelectedBrush` | `#14FFFFFF` | `#0A000000` | Nav button selected background |
| `AppIconDarkVisibility` | `Visible` | `Collapsed` | Show/hide the dark-variant title bar icon |
| `AppIconLightVisibility` | `Collapsed` | `Visible` | Show/hide the light-variant title bar icon |

### Caption button colors

Set programmatically in `MainWindow.xaml.cs → UpdateTitleBarButtonColors()`, called on init and on `ActualThemeChanged`. Uses `AppWindow.TitleBar` color properties — keep these in sync with `AppTitleBarBrush` if the title bar background ever changes.

| Property | Dark | Light |
|----------|------|-------|
| Foreground | `#E6EDF3` | `#1F2328` |
| Hover background | `#2D333B` | `#D1D6DC` |
| Pressed background | `#252D37` | `#C4C9CF` |
| Inactive foreground | 40% opacity of foreground | same |

### Title bar icons

Two PNG exports at 48×48 px, generated from SVG source via `convert-icons.mjs` (`node convert-icons.mjs` from repo root):
- `Assets/AppIcon-dark.png` — shown in dark mode (light-colored icon)
- `Assets/AppIcon-light.png` — shown in light mode (dark-colored icon)

### Text colors

Do not define custom text brushes — use WinUI's built-in `TextFillColorPrimaryBrush` and `TextFillColorSecondaryBrush`, which automatically adapt to the current theme.

### App.xaml card brushes

Three additional brushes used in device cards — also defined per-theme:

| Key | Dark | Light | Usage |
|-----|------|-------|-------|
| `AppCardBrush` | `#21262D` | `#FFFFFF` | Device card background |
| `AppCardBorderBrush` | `#30363D` | `#D0D7DE` | Card border, device icon frame/body stroke |
| `AppCardImageBrush` | `#2D333B` | `#F6F8FA` | Background well behind device icons |

---

## Device Icons

Device cards in `DivoomButton.xaml` use XAML vector icons — **not PNG images**. They are theme-aware `Canvas`-in-`Viewbox` drawings defined as `DataTemplate` resources in `App.xaml` and selected at runtime by `HardwareIconSelector` (`HardwareIconSelector.cs`).

| Hardware ID | Device | Template key |
|-------------|--------|-------------|
| 92 | Pixoo 64 | `DeviceIconPixoo64` |
| 400 | Times Gate | `DeviceIconTimesGate` |
| (other) | Default | `DeviceIconDefault` |

**To add a new device:** add a `DataTemplate` in `App.xaml` and a new `case` in `HardwareIconSelector.SelectTemplateCore`. All icon brushes must use `{ThemeResource}` — never hardcode colors. The icon canvas coordinate space is arbitrary; a `Viewbox` scales it to fit the 62×62 `ContentControl` in the card.

---

## Image Loading — Aspect Ratio Rules

Images in the gallery (`ImageViewerPage`) are loaded via `ImageFileInfo.GetImageSourceAsync(int decodePixelWidth)` with `decodePixelWidth = 128`. The `Image` control uses `Stretch="UniformToFill"` inside a fixed 128×128 tile to crop-fill without letterboxing.

**Do not switch to `GetImageThumbnailAsync()` for the gallery tiles.** `ThumbnailMode.PicturesView` returns thumbnails that Windows pre-letterboxes internally, which overrides the `UniformToFill` stretch and produces blank padding. Always load the source image (with a `DecodePixelWidth` cap) and let the `Image` control handle the crop.

`BitmapImage.SetSource()` starts an async decode but returns immediately — always use `await SetSourceAsync()` instead, or the stream will be disposed before decoding completes, resulting in silent blank tiles.

---

## TODOs

### Image Gallery — Lazy Loading & Scalability
The current implementation preloads all `BitmapImage` objects into memory upfront, which works fine up to ~200 images but degrades beyond that (slow startup, high memory).

When revisiting, the fix is two parts:
1. **Lazy load via `ItemsRepeater.ElementPrepared`** — load the image only when the element is about to be displayed. Use a `CancellationTokenSource` per element (cancel it in `ElementClearing`) to prevent the async race condition where a recycled element gets the wrong image source.
2. **Use thumbnails** — call `GetImageThumbnailAsync()` with an explicit 128px size request instead of `GetImageSourceAsync()`, so memory stays flat regardless of collection size.

A toolbar Reload button is already implemented (`ReloadButton_Click` → `ReloadAsync()`). A file system watcher could further automate this.
