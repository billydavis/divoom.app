# Divoom Manager

## TODOs

### Image Gallery — Lazy Loading & Scalability
The current implementation preloads all `BitmapImage` objects into memory upfront, which works fine up to ~200 images but degrades beyond that (slow startup, high memory).

When revisiting, the fix is two parts:
1. **Lazy load via `ItemsRepeater.ElementPrepared`** — load the image only when the element is about to be displayed. Use a `CancellationTokenSource` per element (cancel it in `ElementClearing`) to prevent the async race condition where a recycled element gets the wrong image source.
2. **Use thumbnails** — call `GetImageThumbnailAsync()` with an explicit 128px size request instead of `GetImageSourceAsync()`, so memory stays flat regardless of collection size.

Also consider a refresh mechanism (toolbar button or file system watcher) since new images can be added to the directory at any time.
