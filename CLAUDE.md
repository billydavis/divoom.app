# Divoom Manager

## Theme — Slate

The app uses the **Slate** theme with full dark/light support. All colors are defined as `ThemeDictionary` entries in `App.xaml` and should be referenced via `{ThemeResource}` — never hardcode these values inline.

### App.xaml resources

| Key | Dark | Light | Usage |
|-----|------|-------|-------|
| `AppChromeBrush` | `#0D1117` | `#FFFFFF` | Main content area background |
| `AppSidebarBrush` | `#161B22` | `#F6F8FA` | Side navigation pane background |
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

---

## TODOs

### Image Gallery — Lazy Loading & Scalability
The current implementation preloads all `BitmapImage` objects into memory upfront, which works fine up to ~200 images but degrades beyond that (slow startup, high memory).

When revisiting, the fix is two parts:
1. **Lazy load via `ItemsRepeater.ElementPrepared`** — load the image only when the element is about to be displayed. Use a `CancellationTokenSource` per element (cancel it in `ElementClearing`) to prevent the async race condition where a recycled element gets the wrong image source.
2. **Use thumbnails** — call `GetImageThumbnailAsync()` with an explicit 128px size request instead of `GetImageSourceAsync()`, so memory stays flat regardless of collection size.

Also consider a refresh mechanism (toolbar button or file system watcher) since new images can be added to the directory at any time.
