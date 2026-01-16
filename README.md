# Ambinity

**Ambinity** is a cross-platform desktop application for **unifying and controlling ambient RGB lighting devices** through a single, extensible platform.

The name **Ambinity** comes from **Ambient + Unity**, reflecting the goal of bringing different ambient lighting devices together into one synchronized ecosystem.

Built with **.NET and Avalonia UI**, Ambinity runs on **Windows, macOS, and Linux**, with a strong focus on **performance**, **modular architecture**, and **real-time device synchronization**.

---

## ✨ Key Features

- Cross-platform desktop application (Windows / macOS / Linux)
- Unified control of multiple USB RGB lighting devices
- Device scanning, connection management, and firmware updates
- Highly customizable lighting effects with real-time playback
- Visual **effect editor** and **LED layout designer**
- Import / export system for sharing effects and device configurations
- Server-based effect distribution via public release workflow
- Lightweight bootstrap installer with on-demand downloads
- Automatic application update system
- Real-time synchronization of multiple devices by **color and spatial position**
- Extremely low background CPU usage (~**0.5%** during effect playback)

---

## 🧱 Project Architecture

Ambinity is organized as a **modular, multi-project solution**, separating UI, core logic, platform integration, and tooling to ensure maintainability and scalability.

```
Ambinity_Avalonia.sln
│
├─ Ambinity/              # Core Avalonia UI application
├─ AmbinityCore/          # Core logic, effect engine, synchronization
│
├─ Ambinity.Windows/      # Windows-specific platform integration
├─ Ambinity.MacOs/        # macOS-specific platform integration
├─ Ambinity.Linux/        # Linux-specific platform integration
│
├─ AmbinityServer/        # Backend / effect distribution logic
├─ Ambinity.Installer/    # Lightweight bootstrap installer
│
├─ Draw2D.Core/           # 2D rendering utilities
├─ Draw2DControlLibrary/  # Custom drawing controls
├─ Avalonia.Gif/          # GIF rendering support
├─ OpenRGB.NET/           # RGB device integration layer
├─ ScreenCapture.NET.*    # Screen capture modules
└─ Locales/               # Localization resources
```

---

## 🧠 Core Design Principles

- MVVM architecture
- Async / Await for responsive UI
- Platform-specific code isolation
- Performance-first background execution
- Long-term maintainability

---

## 🎨 Effects & Layout System

Users can design LED layouts, create effects, preview them in real time, synchronize multiple devices, and import/export configurations for sharing.

---

## 📦 Distribution & Updates

Ambinity uses a separate public release repository with a lightweight installer and built-in update system.

---

## 📚 Credits & Acknowledgements

- **Draw2D**
- **OpenRGB** – https://openrgb.org
- **Artemis RGB** – https://artemis-rgb.com
- **ScreenCapture.NET** – https://github.com/DarthAffe/ScreenCapture.NET

---

## 📄 License

MIT License
