# Unity Fire Safety Training System

A first-person, non-immersive **Windows desktop** simulation built with **Unity 6000.6.3f1 and C#**. Practise the PASS extinguisher technique or emergency evacuation using a keyboard and mouse. No VR headset, SteamVR or Oculus hardware is required.

## Features

- Two independent training modes with separate objectives and results.
- Extinguisher pickup, visible held model, safety pin, aiming, spraying, sweeping, limited capacity and progressive fire suppression.
- Fire and smoke effects, smoke-triggered alarms and heat-triggered sprinklers.
- Five explorable ground-floor classrooms, corridor, central exit and outdoor assembly point.
- One teacher and ten student NPCs evacuating through Unity AI Navigation.
- Two-storey decorative university facade, landscaping, buses, seating and separate sports practice courts.
- Fullscreen/windowed settings, resolution selection, Low/Medium/High graphics, volume and mouse sensitivity.

Basketball uses a 22 x 12 m practice layout; volleyball uses a 14 x 7 m practice layout. These are decorative, reduced-size facilities. The upper floor and compound gate are decorative; the green courtyard assembly marking is the evacuation destination.

## System requirements

These are the **tested specifications**, not independently measured minimum requirements:

| Component | Tested configuration |
|---|---|
| OS | Windows 11, 64-bit |
| CPU | Intel Core i5-1235U |
| Memory | 16 GB RAM |
| GPU | Intel Iris Xe integrated graphics, DirectX 11 |
| Input | Keyboard and mouse |
| Display | Native 1920 x 1200 tested; 1920 x 1080 selectable |
| Editor | Unity **6000.6.3f1** |
| Rendering | Universal Render Pipeline **17.6.0** |
| Navigation | Unity AI Navigation **2.0.14** |
| Input package | Unity Input System **1.20.0** |

Allow several GB of free storage for checkout, Unity's generated Library and builds. Internet is needed for installing Unity and restoring packages. Install **Windows Build Support (Mono)** for the matching Editor when building the Windows player.

Short fixed-view Windows candidate tests on this laptop at 1920 x 1200 averaged approximately **43–45 FPS on Medium**, **60 FPS on Low**, and **35–37 FPS on High** across the two modes. These classroom samples do not guarantee frame rate everywhere. Begin with Medium; reduce quality/resolution if needed.

## Clone and run in Unity

```powershell
git clone https://github.com/Levinnithin07/unity-fire-safety-training.git
cd unity-fire-safety-training
```

1. Install Unity Hub and **Unity 6000.6.3f1**.
2. Choose **Add project from disk** in Unity Hub and select **FireSafetyTraining**, inside this repository. Do not select the repository root.
3. Open with the exact Editor version and allow asset import and package restoration to finish. Dependencies are recorded in `Packages/manifest.json` and `Packages/packages-lock.json`.
4. Open **Assets/_FireSafety/Scenes/TrainingScene.unity**.
5. Press **Play**, select a training mode, read its briefing and choose **Start Simulation**.

Do not open in Unity 2022 or convert the rendering pipeline. Keep all `.meta` files: their GUIDs maintain references between scenes, scripts, materials and prefabs. Unity regenerates ignored Library/Temp/Logs/UserSettings folders locally.

The manifest also includes Unity Pipeline for optional Editor automation. Normal gameplay does not require a running CLI connection. If package restoration fails, inspect Package Manager and use the recorded Editor/package versions rather than deleting dependencies blindly.

## Controls

| Key/input | Action |
|---|---|
| WASD | Move |
| Mouse | Look |
| Left Shift | Sprint |
| Left Ctrl | Crouch |
| E | Interact / pick up extinguisher / operate exit door |
| P | Pull extinguisher safety pin |
| Hold left mouse | Spray extinguisher |
| Move mouse horizontally while spraying | Sweep across fire base |
| Esc | Pause/resume; main menu access |
| R | Retry current mode |
| Alt + Enter | Toggle fullscreen/windowed preference in Windows player |

Click the Editor Game view if it does not receive input. Menus release the cursor; gameplay captures it. **Play Maximized** enlarges the Editor view; standalone fullscreen is configured separately.

## Fire Extinguisher Training

Find the extinguisher in the passage beside the classroom doorway. Press **E** to pick it up and **P** to remove the pin. Aim at the **base of the flames on the burning chair writing surface** from approximately **1.4–4 m**. Use the aiming feedback, hold **left mouse** and sweep left/right.

Effective discharge progressively reduces fire intensity, flame size/emission, smoke and lighting. Capacity decreases while spraying. **Sprinklers are disabled in this mode**. Extinguishing the fire opens PASS results; evacuation is not required. Exhausting capacity without suppression produces a retry result.

## Emergency Evacuation Training

Follow exit guidance through the classroom/corridor and central exterior exit to the **green outdoor assembly marking**. Press **E** when prompted at exit doors. The player must reach the assembly trigger to complete the mode.

Fire growth, smoke detection, alarms and heat-responsive sprinklers are enabled. Extinguisher interaction and PASS scoring are disabled. Leave promptly: waiting for alarm/sprinkler thresholds is not required. Teacher/student NPCs evacuate independently in both modes; their arrival does not complete the player's objectives.

Retry and switching modes reset equipment, fire, alarms, sprinklers, NPCs, objectives, timers and scoring. Results are saved as JSON under the application's `Application.persistentDataPath/TrainingResults` directory.

## Fullscreen and settings

With no saved preference, the Windows player defaults to borderless fullscreen at the detected display resolution. In Settings, choose fullscreen/windowed, resolution, graphics quality, volume and sensitivity. Select **Apply**, then **Keep** to save. Unconfirmed display previews revert automatically. Saved preferences take precedence on later launches.

## Build and run the Windows application

Generated executables are excluded from Git. Cloning this repository provides the complete Unity source, not a ready-made Windows player.

1. Install **Windows Build Support (Mono)** for Unity 6000.6.3f1 through Unity Hub.
2. Open **File > Build Profiles**, select **Windows**, and select **Intel 64-bit / x86_64**. Switch platform if necessary.
3. Keep **Assets/_FireSafety/Scenes/TrainingScene.unity** enabled as the startup scene. This is already recorded in the existing build scene configuration.
4. Preserve the existing Player Settings, application identity and fullscreen configuration.
5. Build into an empty folder, for example `../Builds/Windows/FireSafetyTraining.exe` relative to the Unity project.
6. Launch `FireSafetyTraining.exe`. Keep **the entire build output folder** together, including `FireSafetyTraining_Data`, `UnityPlayer.dll` and supporting runtime files. Copying only the EXE will not work.

The target computer does not need Unity installed to run a completed standalone build. Verify new builds before replacing a working output.

## Repository structure

| Folder | Contents |
|---|---|
| `FireSafetyTraining/Assets` | Scenes, scripts, prefabs, materials, UI, audio, effects and imported assets |
| `FireSafetyTraining/Packages` | Package manifest and lock file |
| `FireSafetyTraining/ProjectSettings` | Unity configuration and build scene settings |
| `FireSafetyTraining/AgentScripts` | Development and verification helpers, not required for normal play |
| `Documentation` | Development reports, educational sources, asset attribution and selected screenshots |
| `SourceAssets` | Acquisition metadata and licenses; duplicate archives/intermediate binaries ignored |
| `Tools` | Asset acquisition/conversion and verification scripts |

Generated caches, builds, machine-specific logs/preferences, temporary output, backups and installed tooling environments are ignored. Imported runtime assets and their `.meta` files are included.

## Verification and troubleshooting

Verification covered PASS interaction, progressive suppression, mode switching, smoke/heat activation, evacuation completion, NPC navigation, classroom accessibility, outdoor movement and display persistence. A later Windows candidate passed **73 checks in its own suite**. Older reports describe historical snapshots and different suites; their assertion counts should not be added together.

- **Pink/missing materials:** use the specified Unity/URP versions and wait for shader compilation. Do not migrate pipelines.
- **Controls do not respond:** click Game view and select Start Simulation after the briefing.
- **Extinguisher does not spray:** verify mode, pickup, pin removal, remaining capacity and gameplay cursor capture.
- **Fire does not reduce:** use effective range and aim at the flame base, following the aiming feedback.
- **Sprinklers inactive in extinguisher mode:** intentional; test them in evacuation mode.
- **Low performance:** select Low quality/lower resolution and close GPU-intensive applications.
- **Windows build unavailable:** install matching Windows Build Support and resolve Console errors.

## Asset credits and licensing

See **[AssetAttribution.md](Documentation/AssetAttribution.md)** for original creators, source links, licenses and modifications. Models, textures and fonts retain their original licenses, including CC0, Unlicense and SIL Open Font License. This project does not claim authorship of third-party assets.

No blanket open-source license is granted for original project code by this README. Public visibility alone does not change the ownership or license of included works. The university name is environmental text and does not imply official endorsement.

This simulation supplements, but does not replace, certified fire safety training.
