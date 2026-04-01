<<<<<<< HEAD
# DataCenter Asset Exporter Mod (MelonLoader)

> ⚠️ **Important Notice (Ethics & Usage)**  
> This mod **does not** encourage or endorse the theft, unauthorized reuse, or distribution of code, assets, or any other content created by indie developers. It was developed strictly to support the **modding workflow** for the game *Data Center* (by Waseku) via MelonLoader. It is intended for modders who need a deeper understanding of the game's structure to create legitimate, transformative mods (e.g., custom assets, structural extensions) within a fair modding context.

---

## 📖 Overview

**DataCenter DHCP Switches** is a MelonLoader-based mod for the game *Data Center*. 

---

## ✨ Features

### Hotkeys


---

## 🛠 Technology Stack

- **.NET 6**
- **MelonLoader** (Modding Framework)
- **Unity IL2CPP Interop**
- **Unity Input System**

---

## 📋 Prerequisites

To build and use this mod, you need:


---

## 🏗 Build Instructions

Navigate to the project directory in your terminal and run:

---

## 🚀 Installation & Usage

1. Build the project following the instructions above (or download the compiled `.dll`).
2. Copy the `.dll` into the `Mods` folder located inside your *Data Center* game directory.
3. Launch the game.
4. Press the designated hotkeys () in-game to trigger the exports and UI logging.

---

## ⚙️ Runtime Behavior (Technical Details)

- **Scene Hierarchy Traversal:** The export relies on scene hierarchies (including inactive objects) to ensure it only captures "actually built/placed" content.
- **Robust UI Detection:** The <kbd>F9</kbd> UI path detection utilizes C# Reflection. This ensures that missing direct UI assembly references during the mod's compilation do not cause hard crashes at runtime.
- **Smart Filtering:** The `NotUsed` export process includes intelligent filtering to primarily export relevant asset candidates, ignoring obvious internal Unity or micro-assets.

---

## 📁 Project Structure


---

## 🤝 Contributing

Contributions are always welcome! If you'd like to help improve the tool, please follow this workflow:

### Workflow
1. **Fork** the repository.
2. Create a new **branch** (`feature/AddCoolThing` or `fix/ExportBug`).
3. Keep your changes small, focused, and well-documented.
4. Test your build locally inside the game.
5. Open a **Pull Request** with a clear and detailed description.

### Contribution Guidelines
- **Strictly No Piracy:** Do not submit PRs that facilitate copyright infringement, asset theft, or bypassing game protections. Exports and features must serve a legitimate modding purpose.
- **Maintain Architecture:** Adhere to the existing coding style and project architecture.
- **Minimal Dependencies:** Do not add unnecessary external dependencies or libraries.

---

## 🐛 Issues & Bug Reports

If you encounter a bug, please open an issue and include the following information to help us troubleshoot:
- **Game Version**
- **MelonLoader Version**
- **Mod Version / Commit Hash**
- **Exact Reproduction Steps**
- **Relevant Logs / Error Messages** (Attach your `MelonLoader/Latest.log`)

---

## 🔒 Security Policy

If you discover a security vulnerability or a critical exploit, **please do not report it publicly via GitHub Issues.** Instead, report it responsibly through a private channel by contacting the repository maintainer directly.

---

## 📜 Code of Conduct

- **Be Respectful:** Treat all community members with respect.
- **Be Constructive:** Keep feedback helpful and focused on the code/project.
- **Zero Tolerance:** No discriminatory, offensive, or toxic behavior will be tolerated. Repeated violations will result in bans from interacting with the repository.

---

## 📄 License

This project is licensed under the **MIT License**. See the `LICENSE.txt` file for full details.

---

## ⚠️ Disclaimer

This project is a community-driven modding tool. It is **unofficial**, not affiliated with, nor endorsed by Waseku or the developers of *Data Center*. Use it at your own risk.
=======
# DHCPSwitches

> **Gamified networking systems for _Data Center_ (Unity/IL2CPP, MelonLoader).**

---

## Overview

`DHCPSwitches` is a gameplay-focused mod project for **Data Center** that expands network management depth while keeping the experience practical and fun.

The project focuses on:

- Better in-game IP assignment UX
- DHCP scope management (`VLAN` / `Switch` / `Global`)
- VLAN-aware operations and management network concepts
- Patch-port labeling and topology clarity
- Shared server / multi-tenant gameplay concepts
- Progressive roadmap toward a semi-full gamified IPAM layer

---

## Current Status

- Roadmap-first planning is active.
- Main plan is documented in `ROADMAP.md`.
- Core mod code targets `net6.0` and MelonLoader + IL2CPP interop.

---

## Tech Stack

- **Game:** Data Center
- **Runtime:** MelonLoader (`0.7.2+` target)
- **Interop:** Il2CppInterop
- **Language:** C# / .NET 6
- **Patching:** Harmony

---

## Repository Structure

- `Main.cs` — Mod entry point and runtime hooks
- `DHCPManager.cs` — DHCP assignment flow and patches
- `IPAMOverlay.cs` — in-game monitoring/feedback overlay
- `ROADMAP.md` — phased implementation roadmap
- `.github/copilot-instructions.md` — project-specific guidance

---

## Getting Started (Development)

### 1) Requirements

- Windows + Steam version of **Data Center**
- .NET SDK 6.x
- Visual Studio 2022/2026 or compatible C# IDE
- MelonLoader installed for the target game

### 2) Clone

```bash
git clone https://github.com/mleem97/DataCenter_DHCPSwitches.git
cd DataCenter_DHCPSwitches
```

### 3) Configure Local References

`DHCPSwitches.csproj` references local game assemblies (MelonLoader and IL2CPP assemblies).  
Adjust `HintPath` entries to your local installation path if needed.

### 4) Build

```bash
dotnet build
```

### 5) Deploy

Copy the built mod DLL to your game `Mods` folder.

---

## Roadmap

Implementation planning is maintained in:

- [`ROADMAP.md`](./ROADMAP.md)

This includes release phases, epics, risks, testing strategy, and sprint-ready tasks.

---

## Contributing

Please read:

- [`CONTRIBUTING.md`](./CONTRIBUTING.md)
- [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)
- [`SECURITY.md`](./SECURITY.md)

---

## License

This project is licensed under the **MIT License**.  
See [`LICENSE`](./LICENSE) for full text.
>>>>>>> 7a29193 (docs(repo): add roadmap and community documentation)
