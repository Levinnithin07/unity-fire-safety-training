# Phase 3 — fire and automatic emergency systems

Completed 16 October 2026. Stopped at the Phase 3 inspection checkpoint.

## Open and run

Unity version: **6000.6.3f1**, URP **17.6.0**, keyboard and mouse.

Scene: `Assets/_FireSafety/Scenes/TrainingScene.unity`

Absolute scene path: `C:/Users/levin/Desktop/ARVR GPT/FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`

TrainingScene is saved, open in the Editor, and left in Edit Mode. The Game view is restored to 1920 × 1080. Press Play, click the Game view, and press **Enter** to start. Look toward the desks beside the left-hand windows. The emergency does not begin merely by entering Play Mode.

| Input | Action |
|---|---|
| Enter | Start the emergency demonstration |
| R | Reset fire, smoke, detectors, alarm and sprinklers; retains player position |
| M | Toggle alarm audio for inspection; does not cancel the alarm or water |
| WASD / mouse | Walk / look |
| Left Shift / Left Ctrl | Sprint / crouch |
| Esc / left click | Release / recapture cursor; Esc does **not** pause the emergency |

No manual asset import, package installation or scene repair is required. Play Maximized can be selected before entering Play Mode to enlarge the game within Unity. A Windows executable is not part of this phase.

## Implemented

- Three predefined combustible desk locations, with progressive intensity, finite fuel, heat exposure and neighbouring-object ignition. The default sequence ignites the first desk, spreads to the second, and local sprinkler suppression prevents the third desk from igniting.
- Animated flame particles, rising smoke, embers, flickering local illumination and spatial crackle. Smoke plumes spread beneath the ceiling instead of passing through the roof. Fire intensity controls particle emission, light intensity and audio level.
- A simplified room smoke accumulator, passage leakage and two visibly modelled optical detectors. Detector readings include response lag and drive alarm activation when their threshold is reached. No timer directly activates detectors or sprinklers.
- Automatic alarm sounder with an emitting beacon lens, flashing red light and looping audio, connected to the detectors. The existing Poly Haven manual pull station remains in place; its manual interaction is deferred.
- Two independently heated pendent sprinkler heads with chrome escutcheons, brass yokes, red glass bulbs and deflectors. Each head has a thermal lag and a 68 °C demonstration threshold. An activated bulb disappears, water sprays, local audio plays, and fire intensity declines inside its 1.65m coverage radius. The cool east head remains off in the default scenario.
- Alarm and activated sprinkler stay latched after the fire cools. Only an explicit scenario reset clears them. Water does not instantly remove all fire or signal that the room is safe to re-enter.
- Compact uGUI/TMP status display with fire/alarm guidance, relative smoke index, elapsed simulation time, sprinkler count, controls and the educational-training notice. Nearby heat changes the guidance to a warning to move away.
- Input System actions for Enter/R/M. Existing movement/controller/display scripts and collisions retained.
- Four licensed CC0 particle textures, three original generated audio loops, detailed original equipment meshes, four equipment prefabs and six component scripts.

Runtime scripts: `FireSource.cs`, `FireManager.cs`, `SmokeDetector.cs`, `FireAlarmController.cs`, `SprinklerController.cs`, `EmergencyStatusUI.cs`, under `Assets/_FireSafety/Scripts/`.

## Asset sources and preservation

- Kenney Particle Pack: https://kenney.nl/assets/particle-pack — **CC0 1.0**. Four textures plus the included license are physically present under `Assets/ThirdParty/KenneyParticles/`. Downloaded from the independently verified CC0 mirror at https://github.com/Calinou/kenney-particle-pack after the original publisher download stalled; the original ZIP subsequently completed and is retained under SourceAssets. Exact files, URLs, sizes and hashes: `Phase3ImportedAssets.json`.
- Liberation Sans and SDF resources: Unity's included TextMeshPro essentials, **SIL OFL 1.1**; bundled license preserved.
- Ceiling equipment, alarm beacon/sounder geometry and audio loops are original project-authored assets, not downloaded manufacturer models or field recordings. The alarm waveform is a demonstration signal, not a certified appliance sound profile.
- SHA-256 verification of **204 original files: zero changes**, including AssetReview, original classroom/equipment source files and metadata, and shared imported materials. Before/after evidence: `Phase3PreservationBefore.json` and `Phase3PreservationAfter.json`.
- Pre-Phase-3 TrainingScene is backed up outside Assets at `FireSafetyTraining/PhaseBackups/Phase2/TrainingScene.unity`.

Full license notes: `AssetAttribution.md`.

## Tests performed

**26 emergency-system checks passed in Unity Play Mode**, with recorded evidence and a 120-second deterministic simulation timeline in `Phase3PlayModeTests.json`:

- Safe initial exploration; actual Enter binding starts fire; particles and audio sources run.
- No fire produces no alarm/sprinkler activation even after 180 simulated seconds.
- Fire grows, smoke accumulates, detector remains idle below its threshold.
- Smoke threshold triggers the connected alarm; adjacent desk ignition is recorded.
- Alarm occurs before heat activation; only the nearby head activates.
- Gradual suppression, out-of-coverage exclusion and eventual local flame suppression.
- Alarm and water remain latched after cooling.
- Smoke without heat does not activate sprinklers; local heat can activate one head without a smoke alarm.
- Actual water particles, alarm source playback and ceiling-bounded smoke.
- Actual M and R bindings; reset clears effects/audio and restores the bulb.
- No missing materials, unsupported shaders or missing scripts.

**24 Phase 2 movement/collision regression checks also passed** in the modified scene: `Phase3MovementRegression.json`. These cover floor/wall/desk/chair collision, both doorway routes in both directions, walk/sprint/crouch, overhead clearance, real WASD/mouse bindings, cursor release and recapture.

**Separate real-time run:** the normal runtime Update loop advanced 36.15 simulated seconds during a 36-second observation. Alarm activated at **17.20s**; the second desk ignited at approximately **24.35s** in the deterministic sequence; sprinkler A activated at **28.15s**, at a reported simulated element temperature of 68.11 °C. Two fires and one operating head were observed. Alarm output samples were nonzero (maximum sampled RMS 0.0298). Evidence: `Phase3LiveObservation.json`.

The CLI request for that 36-second observation hit its default 30-second transport timeout; the Unity async operation continued, completed, and wrote its result file. Completion and final state were subsequently verified. Re-running the longer observation should use CLI `--timeout 60` as well as the run_script timeout parameter. The timeout is retained in `Phase3LiveExecution.json` rather than represented as a successful synchronous tool response.

**Feedback audit:** actual beacon-on and beacon-off frames observed; fire and water source output samples were also nonzero (maximum sampled RMS 0.0226 and 0.0111). `Phase3FeedbackAudit.json`. These verify Unity audio output; speaker volume and subjective sound quality were not assessed through a physical microphone.

**Visual/UI checks:** actual Game view captures inspected at 1920×1080 and 1280×720. All four text labels were inside the screen and reported no overflow at both sizes. Evidence: `Phase3HUD-1920.json`, `Phase3HUD-1280.json`. Final screenshots:

- `Assets/Screenshots/Phase3-Emergency-1080p.png`
- `Assets/Screenshots/Phase3-Emergency-720p.png`

**Final scene audit:** 3 fire sources, 2 detectors correctly linked to the alarm, 2 sprinklers, 237 colliders, correct original player spawn, zero missing materials/scripts and zero unsupported shaders. Fullscreen settings remain FullScreenWindow/native resolution. `Phase3SceneAudit.json`.

**Console:** final clean Play Mode audit reports **0 errors, 0 warnings**, no compile failure. `Phase3ConsolePlayMode.json`. Earlier bridge-operation timeouts and a legacy TMP font metadata warning were preserved in `Phase3ConsoleHistory.json`. The font metadata was rewritten through Unity's serialization API to the supported version 4, and the final audit was taken after a fresh Play Mode entry.

## Performance and limits

Intel Iris Xe, 1920×1080 Unity Editor Game view, 36-second instrumented emergency run: **41.93 FPS** average sampled rate, **23.75ms** median frame time, **27.48ms** 95th percentile, 1,510 samples. Peak observed live particles: **319**. Total configured cap across all particle systems: **1,088**. The existing URP Medium profile is retained; added fire/beacon lights cast no shadows. Detector vent geometry was combined to reduce renderer overhead.

This is a stationary Editor observation, not a standalone benchmark or a guarantee across every viewpoint. The final flame color correction changed tint only, after this measurement; particle budgets and simulation behaviour were unchanged.

Smoke density and temperature are illustrative model values, not calibrated measurements or tenability predictions. Propagation uses predefined connections, not CFD. Smoke particle rendering represents local plumes; the normalized room accumulator drives detector state. Sprinkler behaviour is inspired by independent heat activation, but timing, placement, coverage and suppression are demonstration parameters, not a building-system design. Reference: https://www.usfa.fema.gov/prevention/home-fires/prepare-for-fire/home-fire-sprinklers/ . The status UI instructs evacuation and explicitly avoids treating water activation or flame suppression as permission to remain or re-enter.

## Remaining work

- PASS training, extinguisher pickup/operation and aiming/sweeping: Phase 4, not implemented.
- Interactive evacuation, hazard-aware guidance and safe assembly point: Phase 5, not implemented. The existing two doorways remain physically usable.
- Full menus/settings, evaluation/scoring and replay workflow: later phases.
- Windows executable and real standalone fullscreen behaviour: later build phase.
- Existing doors remain fixed open, and the manual pull station remains non-interactive.

No known blocking Phase 3 issue remains in the tested scene. Development stops here for user inspection.
