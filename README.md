# Divoom Manager

A Windows desktop app for managing [Divoom](https://www.divoom.com/) pixel art devices — browse your image library, generate AI artwork, and push it to your device in one click.

Built with WinUI 3 / Windows App SDK and C# / .NET 8.

---

## Features

- **Device discovery** — automatically finds Divoom devices on your local network
- **Image gallery** — browse, upload, and manage your pixel art library
- **Send to device** — push any image directly to a connected Pixoo or Times Gate
- **AI image generation** — create new pixel art using four AI providers:
  - OpenAI DALL-E 3
  - Google Imagen 4
  - Stability AI
  - DeepAI
- **Multi-screen support** — target individual panels on Times Gate devices
- **Dark and light mode** — full theme support via the Slate design system

---

## Supported Devices

| Device | Resolution |
|--------|-----------|
| Pixoo 64 | 64 × 64 |
| Times Gate | 128 × 128 (5 panels) |

---

## Requirements

- Windows 10 version 1809 or later (Windows 11 recommended)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) version 17.13 or later
  - Workload: **.NET desktop development**
  - Workload: **Windows application development**
- [Windows App SDK 1.6](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- .NET 8 SDK

---

## Building from Source

This app depends on a companion library in a sibling repository. Clone both into the same parent folder:

```
git clone https://github.com/billydavis/divoom.net
git clone https://github.com/billydavis/divoom.app
```

Your folder structure should look like this:

```
parent/
├── divoom.net/
│   └── divoom.net/
│       └── divoom.net.csproj
└── divoom.app/
    └── divoom.app.sln
```

Open `divoom.app/divoom.app.sln` in Visual Studio 2022 and run the **`divoom.app (Unpackaged)`** launch profile.

---

## AI Image Generation

To generate images you need API keys for the providers you want to use. Go to **Settings** in the app to enter them.

| Provider | Where to get a key |
|----------|--------------------|
| OpenAI DALL-E 3 | [platform.openai.com](https://platform.openai.com/api-keys) |
| Google Imagen 4 | [aistudio.google.com](https://aistudio.google.com/app/apikey) |
| Stability AI | [platform.stability.ai](https://platform.stability.ai/account/keys) |
| DeepAI | [deepai.org](https://deepai.org/dashboard) |

Generated images are saved to `%LocalAppData%\DivoomManager\Images` at 512 × 512 px and automatically appear in your gallery.

---

## Image Library

Images are stored in `%LocalAppData%\DivoomManager\Images` by default. You can add extra folders in **Settings**.

Supported formats for upload: PNG, JPG, JPEG, GIF. Images must be square (width = height) and no larger than 1024 × 1024 px.

---

## Dependencies

- [Divoom.net](https://github.com/billydavis/divoom.net) — companion library for the Divoom HTTP/UDP protocol
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK)
- [WinUI 3](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

---

## License

MIT
