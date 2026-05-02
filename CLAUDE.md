# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Divoom Manager is a WinUI 3 / Windows App SDK 1.6 desktop app (C# / .NET 8) for managing Divoom pixel art devices. It supports device discovery, image browsing, and AI-powered image generation.

---

## Build & Run

**Primary workflow**: Open `divoom.app.sln` in Visual Studio 2022 (17.13+) and run. Two launch profiles are defined in `Properties/launchSettings.json`:

- `divoom.app (Unpackaged)` — direct run, default for development
- `divoom.app (Package)` — MSIX-packaged deployment

**Supported platforms**: x86, x64, ARM64

**Icon generation** (only needed when SVG source changes):
```
node convert-icons.mjs
```
Requires `sharp` (`npm install`). Outputs `Assets/AppIcon-dark.png` and `Assets/AppIcon-light.png` at 48×48 px.

**No test projects exist.**

---

## Architecture

### Project Structure

Single-project solution (`divoom.app`). Key areas:

- `App.xaml(.cs)` — app lifecycle (minimal), theme `ResourceDictionary` entries, device icon `DataTemplate` resources
- `MainWindow.xaml(.cs)` — window chrome, custom title bar, SplitView shell, page cache and navigation routing
- `Pages/` — five full-page views (Devices, ImageViewer, CreateImage, Settings, ComingSoon)
- `Controls/` — `SideMenu`, `SideMenuButton`, `DivoomButton` (device card)
- `Services/` — stateless/static helpers for state, settings, device persistence, and AI image generation
- `Models/` — `DeviceViewModel`, `NavigationChangeEvent`
- `Converters/` — `HardwareToNameConverter`, `HardwareIconSelector` (DataTemplateSelector)
- `Utilities/` — `ImageFileInfo` (async image loading wrapper)

**External dependency**: `Divoom.net.dll` — referenced from a sibling repo at `..\..\divoom.net\...`. This library handles the Divoom UDP/HTTP protocol (device discovery, commands).

### Navigation

`SideMenu` fires a `NavigationChangeEvent`. `MainWindow.SideMenu_OnNavigationChange()` handles it by swapping `ContentFrame.Content` to a page from a `Dictionary<string, Page> _pages` cache. Pages are instantiated once and reused — there is no `Frame` navigation stack.

### State & Persistence

| Class | Pattern | Storage |
|-------|---------|---------|
| `AppState` | Static, event-driven | In-memory only; fires `SelectionChanged` when device/channel changes |
| `AppSettings` | Static wrapper | `ApplicationData.Current.LocalSettings` (key-value) |
| `DeviceStore` | Static | JSON file in local app folder |

**Default image storage folder**: `%LocalAppData%\DivoomManager\Images` — created via `System.IO.Directory.CreateDirectory` (not `ApplicationData.Current.LocalFolder`, which would put it under the MSIX package sandbox GUID). The folder resolution helper `GetDefaultStorageFolderAsync()` is a private static method duplicated in each XAML code-behind that needs it (`ImageViewerPage`, `SettingsPage`, `CreateImagePage`) — WinRT async operations are only awaitable within XAML partial class contexts in this project.

### Image Generation (CreateImagePage)

Four AI providers implement `IImageGenerationProvider` (DisplayName, SettingsKey, `GenerateAsync`):
- `DallEProvider` — OpenAI DALL-E 3
- `GeminiProvider` — Google Gemini
- `DeepAiProvider` — DeepAI
- `StabilityProvider` — Stability AI

API keys and per-provider prompt instructions are stored in `AppSettings` (LocalSettings). A global system prompt can also be set. To add a provider, implement the interface and register it in `CreateImagePage`.

### Code Style

- **Pattern**: Code-behind with light MVVM (no framework, no DI container). Pages use `ObservableCollection` and `INotifyPropertyChanged` directly.
- **Nullable**: Enabled (`Nullable=true` in .csproj). Use `!` annotations only when null is genuinely impossible.
- **Bindings**: Prefer `x:Bind` (compile-time) over `{Binding}` (runtime). Use `Mode=OneWay` / `Mode=TwoWay` explicitly.

---

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
