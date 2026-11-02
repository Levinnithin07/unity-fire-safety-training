# School expansion verification

Updated 19 October 2026. Unity 6000.6.3f1, URP, Windows desktop, keyboard and mouse.

**19 October design revision:** At the user's request, removed Desktop computer 2 (monitor, keyboard and mouse) and its added Second computer desk from in front of the blackboard. One computer remains on the original teacher desk. Earlier two-computer counts and screenshots below describe the pre-revision expansion. TrainingScene and its saved navigation mesh were updated through the connected Editor. All eleven NPC spawn-to-assembly routes remained complete after rebaking. A Play Mode camera capture (`Assets/Screenshots/School-Board-Cleared.png`) visually confirmed the furniture removal. Original assets and the other classrooms were retained. Scene/navigation backup: `PhaseBackups/BoardClearance-20261019-155518`.

## Open the project

Open `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity` and press Play. The existing mode menu remains the entry point. The separate updated executable is `Builds/SchoolExpansion/FireSafetyTraining.exe`; keep its adjacent data folders and DLLs together. The earlier `Builds/Windows` release is preserved.

## Implemented environment

- Original classroom remains at its original position, with its original renderer transforms and material references. Fire, extinguisher, alarm, detector and sprinkler objects were not repositioned. AssetReview is unchanged.
- Four environment-only instances share `Assets/_FireSafety/SchoolExpansion/Prefabs/EnvironmentOnlyClassroom.prefab`. Their centers are z = -17.1, -8.55, +8.55 and +17.1 metres. The original main classroom is central.
- A continuous corridor connects all five rooms. Existing operable central exit leads to the original assembly location, enlarged to accommodate the occupants. Classroom signs, exit directions, roofline, glazing, corridor lighting and exterior details added.
- One teacher and ten students use licensed Quaternius models and idle/walk clips. These are deliberately simple stylized background characters, not photorealistic humans.
- Exactly two computer sets, each with monitor, keyboard and mouse. The second has a matching copy of the original teacher desk. Computers do not participate in training logic.
- Courtyard includes four ornamental trees, sixteen shrubs, ninety-six grass tufts, textured garden beds, benches, boundary masonry, lanterns and a marked assembly area.

## Runtime integration

`SchoolEvacuationController.cs` subscribes to two new TrainingSession events, `AttemptReset` and `SimulationStarted`. It resets all eleven occupants and starts their evacuation in either mode. `SchoolEvacuee.cs` handles staggered departure, NavMesh movement, walking animation, waiting for the central door, and unique assembly positions. It does not call any scoring/completion methods.

`ExitDoor.cs` adds a nearby NPC open request while retaining the existing E interaction. `TrainingSession.cs` keeps time running on completed result screens so background occupants can finish walking; player gameplay and scoring remain gated by Playing. Menus and pause still freeze the simulation.

The player has a non-carving NavMeshObstacle to support local NPC avoidance. NPCs have no physical capsule collider that could obstruct extinguisher rays or the player's controller. Navigation uses the existing agent type and a persisted expanded bake at `Assets/_FireSafety/SchoolExpansion/Navigation/SchoolNavMesh.asset`.

## Modified existing files

- `Assets/_FireSafety/Scenes/TrainingScene.unity`
- `Assets/_FireSafety/Scripts/TrainingSession.cs`
- `Assets/_FireSafety/Scripts/ExitDoor.cs`
- `Assets/_FireSafety/Scripts/TrainingSettings.cs` — bounded window-size retry after a reproduced Windows fullscreen-to-windowed transition issue. New Apply/Cancel requests replace any pending retry; retries stop once dimensions match or after five attempts.
- `Assets/Editor/ProjectBootstrap/AssetImportPolicy.cs` — enables Legacy animation importing only for the new character folder; existing art import policy remains unchanged.
- `Assets/_FireSafety/Verification/StandaloneVerification.cs` — opt-in display-only diagnostic and bounded resolution polling; never instantiated during normal play.

New runtime scripts, shared prefab, derived hallway mesh, navigation asset, landscaping materials and source assets are separate additions. Authoring and verification helpers are in AgentScripts. A pre-expansion checkpoint of _FireSafety, ProjectSettings and Packages is under `PhaseBackups/SchoolExpansion-20261018`.

## Verification evidence

- `SchoolSceneAudit.json`: five-room count, environment-only script scope, missing scripts/material checks, connected collision-checked paths from all five classrooms, two complete computer sets, original renderer transforms and material preservation.
- `SchoolPlayerExploration.json`: continuous virtual W+Shift input through the real CharacterController, from main spawn into each added room and then assembly. Five destinations reached; no player teleport during traversal.
- `SchoolNPCRegression-FireExtinguisher.json` and `SchoolNPCRegression-Evacuation.json`: post-desk-correction NPC movement, unique arrival positions, 100 ms swept torso-clearance samples, door waiting, animation, independent player completion and retry resets.
- `SchoolModeA.json`: 20 passed extinguisher checks, virtual E/P/left-click/mouse sweep through runtime input, with positioning fixtures for pickup and aiming. Fire intensity, flame size/emission, light and smoke decrease to zero; PASS, capacity, results and sprinkler exclusion verified.
- `SchoolModeB.json`: 15 passed evacuation checks. Smoke/heat simulation stepped in 50 ms increments to reach alarm/sprinkler conditions, followed by real W/E controller movement to assembly. Water particles/audio, suppression, guidance, completion and mode-specific scoring verified.
- `SchoolModeC.json`: 12 mode-switch/reset checks. Prepared-pin and deterministic suppression fixtures are explicitly identified in the report.
- `SchoolModeUI.json`: 16 menu, briefing, settings, pause and result-navigation checks; no tested text overflow at 1280x720.
- `SchoolFeedbackTests.json`: 20 movement, crouch/footstep, audio and quality-preset checks.

These are automated Editor integration checks, not a claim that every possible human movement or camera angle was manually tested.

## Performance and rendering

`SchoolEditorPerformance.json` records two 10-second 1920x1080 Medium samples on Intel Iris Xe. Classroom fire/NPC evacuation averaged 38.98 FPS (median 25.61 ms, p95 27.23 ms). Courtyard averaged 46.98 FPS (median 20.89 ms, p95 22.06 ms). Editor measurements include Editor overhead and are not a standalone benchmark. Shared meshes/materials, compressed mipmapped textures, modest grass geometry, non-shadowing fill lights and vegetation distance culling limit the added cost.

Actual rendered inspection images are in `Assets/Screenshots/School-*`. The first support-desk omission was found in a close-up, fixed, and included in the Windows build. No original classroom art was removed to hide the issue.

## Licensing

See `Documentation/AssetAttribution.md` and `SourceAssets/SchoolExpansion/` for original sources, CC0 licenses, download hashes and alterations. The unused high-poly initial tree download remains a source reference and is not referenced by TrainingScene.

## Review controls

WASD move; mouse look; Shift sprint; Ctrl crouch; E interact/pick up; P pull pin; hold left mouse to spray and sweep; Esc pause; R retry. Choose either mode from the main menu. In extinguisher mode, sprinklers remain disabled. In evacuation mode, no extinguisher use is required.

For visual review, visit both corridor wings, inspect the two teacher-area computers, then follow the central exit to assembly. NPCs should leave in both modes and gather outdoors without completing the player's objective.

## Build and Console notes

The release build reported Succeeded and produced the separate SchoolExpansion executable. Its first build report includes two Editor plugin request-timeout entries because verification requests were sent while Unity was busy building; these are tool transport failures, not C# compilation errors. Three build warnings concern automatic collider pre-baking in future Unity versions, the Editor integration being intentionally absent from player builds, and a deprecated TextMeshPro shader pragma. Current Unity still builds the collision data. These dependency warnings remain documented rather than being hidden or addressed by unrelated package changes.

After the build, both post-desk NPC tests passed all seven checks per mode. A fresh Console check after those Play Mode runs reported zero errors, zero warnings, and no compilation failure. The updated scene is left open in Edit Mode.

## Final Windows verification

The final rebuild (`SchoolWindowsBuild-Final.json`) succeeded in 12.56 seconds with **zero errors and one warning**, producing 401,474,053 bytes. The warning states that the Editor Pipeline integration is disabled in Player builds; gameplay does not require it. The prior full rebuild emitted the additional collision pre-baking and TMP warnings documented above; incremental rebuilding did not emit them again. The earlier build's transport errors are retained in its historical report and did not recur in the final build.

`SchoolStandalone/Extinguisher.json` passed all 20 checks and `SchoolStandalone/Evacuation.json` passed all 15. These ran in the release Windows executable with real rendering, automated positioning/simulation fixtures and virtual Input System events. The earlier 115 Editor checks also passed. Eleven NPCs reached their distinct outdoor destinations in both post-desk regression runs, without completing the player's objective.

Standalone fixed-view performance on Intel Iris Xe at native **1920x1200** used 2.5 seconds warmup and 8 seconds sampling per mode/preset:

| Preset | Extinguisher FPS | Evacuation FPS |
|---|---:|---:|
| Low | 60.00 | 60.00 |
| Medium | 42.20 | 43.16 |
| High | 34.36 | 33.11 |

These are short scene samples, not guarantees for every position or thermal condition. Medium is the balanced default; Low provides more headroom.

Fresh native borderless startup, main menu, display cancel, Alt+Enter in both directions, stored settings and executable relaunch were verified. The original standalone run recorded a failed 1280x720 windowed-preview check. A second diagnostic launch reproduced it (`SchoolDisplayConfirm/DisplayOnly.json`): mode changed to Windowed while dimensions remained 1920x1200 throughout the extra four-second polling window. TrainingSettings now retries the pending window size at 350 ms intervals, up to five attempts, and stops when dimensions match. Apply/Cancel replace pending requests. Two fresh launches of the fixed build (`SchoolDisplayFixed1/DisplayOnly.json` and `SchoolDisplayFixed2/DisplayOnly.json`) passed all six checks each, with exact 1280x720 dimensions and zero runtime errors. User preferences were restored after each run. The earlier failure reports remain as historical evidence.

No known blocking gameplay issue remains in the tested routes and workflows. Remaining limitations are stylized NPC art, the documented dependency warnings, and the scope of automated rather than exhaustive human play testing. If a display preview ever looks wrong, Cancel or the 15-second automatic revert returns to the previous settings. Unity's resolution API applies changes after the frame finishes: [Screen.SetResolution documentation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Screen.SetResolution.html).
