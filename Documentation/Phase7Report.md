# Phase 7 — final polish

Completed implementation and Editor verification on 18 October 2026. Windows executable verification remains Phase 8.

## Changes

- Added three original footstep variations. Sound follows actual collision-resolved walking distance, with stronger sprinting and quieter crouching. Standing still, holding movement against a wall, pausing and teleporting do not generate extra steps. No camera shake or movement physics changes.
- Added a short original success cue that plays once when extinguisher suppression completes. It does not play merely because the fire timer advances or the player opens a result screen. The existing evacuation arrival sound remains in place.
- Connected all 14 particle systems to the existing Low/Medium/High settings. Their combined maximum live-particle limits are **752 / 1,087 / 1,358**. These are configured caps, not measured active particle counts or FPS. Preview and Cancel apply and restore the limits without accumulating scaling. Fire intensity, heat, hazard smoke, suppression and score calculations remain independent of rendering quality.
- Reviewed the existing classroom lighting, surface materials, flame/spray appearance, menus and route feedback. Retained the existing visual treatment. The project already has compressed mipmapped imports for all 71 third-party textures (2K maximum), 231 static-batching-marked mesh renderers, and separate URP presets for resolution scale, antialiasing and shadows.
- Retained the corrected guidance to aim at the base of the flames on the chair writing surface. No new desktop model was inserted.

## Files

Runtime files under `FireSafetyTraining/`:

- **Added:** `Assets/_FireSafety/Scripts/TrainingFeedback.cs`.
- **Added:** `Assets/_FireSafety/Scripts/TrainingEffectsQuality.cs`.
- **Modified:** `Assets/_FireSafety/Scripts/TrainingSettings.cs` — applies the particle preset alongside the existing graphics settings.
- **Modified through the live Editor:** `Assets/_FireSafety/Scenes/TrainingScene.unity` — saved feedback references, two dedicated audio sources and particle budgets.
- **Added:** `Assets/_FireSafety/Audio/Footstep 1 - original.wav`, `Footstep 2 - original.wav`, `Footstep 3 - original.wav`, and `Extinguisher success - original.wav`, with Unity-generated metadata. All four are mono 22,050 Hz PCM, 0.24 seconds per step and 0.9 seconds for success, about 70 KiB total PCM data. Existing audio imports and mixers were not changed.

Authoring and verification helpers are in `FireSafetyTraining/AgentScripts/`: `PolishPhaseSeven.cs`, `TestPhaseSevenFeedback.cs`, `TestPhaseSevenExtinguisher.cs`, `TestPhaseSevenEvacuation.cs`, `TestPhaseSevenSwitching.cs`, `TestPhaseSevenUI.cs`, and `AuditPhaseSeven.cs`. They are outside Assets and are not included in the player. New screenshots are under `Assets/Screenshots/Phase7-*`.

Backup of the pre-polish TrainingScene and scripts: `FireSafetyTraining/PhaseBackups/Phase7/`.

## Verification actually performed

Unity 6000.6.3f1 / URP, connected Pipeline integration, Editor Play Mode. **83 assertions passed, zero failed**:

| Evidence file | Passed | Coverage |
|---|---:|---|
| `Phase7FeedbackTests.json` | 20 | Actual movement input, walking/sprint/crouch sounds, blocked movement, pause, teleport exclusion, valid mono audio data, graphics preview/cancel and physics independence |
| `Phase7ModeA.json` | 20 | Menu selection, virtual E/P/left mouse and mouse movement, full PASS, progressive flame/smoke reduction, extinguishment, once-only success cue, results, retry and sprinkler exclusion |
| `Phase7ModeB.json` | 15 | Smoke alarm, heat-triggered water/audio, water suppression, alternate route guidance, walking through the operable exit to assembly, independent evacuation results |
| `Phase7ModeC.json` | 12 | Mode round trip, reset, restored sprinklers, wrong aim, exhausted agent, successful PASS after switching and no timed automatic extinguisher completion |
| `Phase7ModeUI.json` | 16 | Both mode cards, briefing/back, instructions, controls, credits, settings cancel, pause/resume, result return and text fit at 1280×720 |

The extinguisher test recorded intensity falling from 0.45 through intermediate values to zero, with shrinking flame size and falling flame/smoke emission. Water contribution remained zero. The success audio source was playing after suppression and the cue counter stayed at one on the results screen.

The evacuation test advanced the existing emergency model in 50 ms simulation steps to reach detector/sprinkler thresholds, then used virtual W/E input and automatic yaw to physically traverse the route. It did not teleport to the exit. The extinguisher test used equipment/aim-position fixtures before testing actual Input System events. Menu tests invoked the real Button callbacks. These are automated Editor tests, not an unassisted human walkthrough.

Rendered images inspected: 1080p reduced flame/spray with corrected aiming text, active sprinkler water and route guidance, and 720p main menu. New audio clips were checked for nonzero PCM signal, channel count, boundaries and playback state; subjective loudspeaker/headphone listening remains a user review item.

`Phase7ModesSceneAudit.json`: clean saved TrainingScene, zero missing scripts, zero unassigned FireSafety component references, zero invalid materials, one EventSystem, persistent input bindings, and 211 navigation triangles. `Phase7Console.json`: no compilation failure, zero Console errors and zero warnings after testing.

`Phase7Preservation.json`: all **204 protected files** match their earlier SHA-256 hashes, including AssetReview and the original classroom art. No new third-party downloads or license obligations.

## Handoff and remaining work

TrainingScene is open in Edit Mode with the Game view restored to **1920×1080**. Press Play, choose either mode, then Start Simulation. Check the new walking/crouching sounds and extinguisher completion cue at a comfortable volume; settings retain the existing master-volume control.

Phase 8 remains: create the Windows executable, verify native-resolution/borderless/windowed switching and process exit in that executable, perform a fresh full walkthrough, and measure actual frame times on the laptop. This phase does not establish a standalone FPS result or certify real-world fire safety performance.
