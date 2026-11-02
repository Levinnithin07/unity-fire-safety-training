# Phase 4 — Interactive extinguisher and PASS training

Completed and verified on 16 October 2026. Stopped before Phase 5.

## Project and recovery audit

- Unity 6000.6.3f1, URP 17.6.0, Input System 1.20.0. Live Editor operations used the Unity plugin's CLI/Pipeline bridge on localhost. The Editor initially was not reachable; opening the existing project restored the connection. No project recreation or pipeline conversion.
- Scene: `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`. Left open in Edit Mode, with no unsaved scene changes.
- The interrupted session had already saved FireExtinguisherController, ExtinguisherStatusUI, fire suppression/reset hooks, the held-model meshes, audio, particle material, overlay camera, and HUD references. The unfinished work was validation and documentation. This recovery verified that implementation rather than rebuilding it.
- An earlier captured cyan held-model appearance did not reproduce after reopening. Current captures show the original red/textured body, metal components, label and rubber hose correctly. No speculative material or pipeline replacement was made; the cause of the earlier transient appearance was not established.
- Final scene audit: zero missing scripts, zero unassigned serialized references in FireSafety components, zero invalid or unsupported materials. No incomplete prefab reference was found; the equipment is configured directly in TrainingScene.
- SHA-256 verification: all 204 protected files unchanged, including AssetReview and original source artwork. See `Phase4PreservationAfter.json`. Existing pre-Phase-4 scene/script backup remains under `FireSafetyTraining/PhaseBackups/Phase3`.

## Implemented behavior

- E pickup with 2.1 m interaction ray and wall obstruction checks. World equipment is hidden and its colliders disabled only after successful pickup; reset restores it.
- First-person held model derived from the existing licensed Poly Haven mesh, retaining UVs and materials. Separate pin and squeeze lever, deployed nozzle and flexible hose. The held assembly has no player-blocking colliders. A URP overlay camera keeps it visible close to surfaces.
- P removes the pin over 0.4 seconds with sound. The pin locks discharge until removed. LMB squeezes the animated lever and emits powder particles and spray audio.
- Base aiming checks both camera and nozzle line of sight, distance and height around the burning desktop. High aim, blocked aim and out-of-range aim cannot suppress fire. Green crosshair indicates a valid base target.
- Controlled horizontal movement in both directions earns sweep credit. Holding still gives very little suppression; rapid shaking is rejected. Correct sustained sweeping reduces the existing FireSource intensity and can extinguish a manageable fire.
- Twelve seconds of agent; misses also consume it. Empty equipment stops spraying. Escape/releasing mouse control stops discharge, and a recapture click must be released before it can spray.
- Live PASS states, agent bar, on-target discharge percentage, effective-sweep duration and success/unsafe/empty feedback. These are Phase 4 telemetry, not a final training score or results screen.
- Growing/spreading or smoky emergency conditions disable successful-suppression credit and direct the learner to leave. R resets both emergency and equipment state.

## How to inspect

1. Open TrainingScene and press Play. Click Game view to capture the mouse.
2. WASD moves; mouse looks; Left Shift sprints; Left Ctrl crouches. Esc releases the cursor, but does not pause time.
3. Go right from spawn through the front classroom doorway into the passage. Look down at the extinguisher beside the classroom wall; press E when prompted.
4. Press P. Return to the open front aisle near spawn, facing the nearest desk by the windows. Press Enter to start the fire.
5. Aim at the burning desktop, hold LMB and sweep slowly left/right across the base. Watch the green crosshair, PASS indicators and capacity bar.
6. R resets the scenario and returns the extinguisher to the passage. M toggles alarm audio.

Use Play Maximized to enlarge Game view. The future executable retains native-resolution borderless fullscreen settings. No executable was built or fullscreen-launch-tested during Phase 4.

## Tests actually performed

| Evidence file | Actual result |
|---|---|
| Phase4PlayModeTests.json | 29/29 passed: references, pickup wall/range limits, E/P/LMB bindings, visible held mesh, pin lock, lever, capacity, particles, audio output, cursor-release protection, base aim, high aim, obstacles, distance limits, static aim, fast shaking, controlled PASS suppression, unsafe fire, depletion and R reset. |
| Phase4LiveInput.json | Separate full PASS run through actual Update action bindings using temporary virtual keyboard/mouse devices. E, P, Enter, LMB and mouse delta completed all four PASS stages and suppressed the fire. 97.84% safe on-target discharge; 8.83/12 seconds agent remaining; 151 advancing frames across about 3.40 seconds of the recorded discharge/result interval. Setup positioning was automated. |
| Phase4EmergencyRegression.json | 26/26 passed: fire growth/spread, smoke accumulation, detector/alarm causality, local heat-triggered sprinkler, gradual suppression, no-fire baseline, particles/audio, mute and reset. Uses the existing deterministic simulation step plus real Play Mode/input checks. |
| Phase4MovementRegression.json | 24/24 passed: gravity, floor/wall/furniture collision, both doorways in both directions, crouch clearance, speeds, WASD/Shift/Ctrl/mouse bindings and cursor behavior. |
| Phase4HUD-1920.json / Phase4HUD-1280.json | All nine visible labels inside the screen and without text overflow at 1920x1080 and 1280x720. Long interaction-message variants also fit. Screenshots visually reviewed. |
| Phase4SceneAudit.json | Missing scripts 0, unassigned FireSafety references 0, invalid materials 0; 5 held meshes; total particle cap 1,218. |
| Phase4Performance.json | Intel Iris Xe, 1920x1080 Editor, fixed camera with growing fire, held model and continuous off-target powder spray: 345 frames/8.004 seconds = 43.10 average FPS; 117 peak particles. A short Editor sample, not a standalone or worst-case benchmark. |
| Phase4ConsoleAudit.json | Compilation failed: false; Console errors 0, warnings 0 after tests. |
| Phase4PreservationAfter.json | 204 protected files checked, zero changes. |

The initial single-call audio-output test read zero. Repeating the measurement over a short interval returned nonzero RMS (0.0539 in the final interaction suite; 0.0598 in the rendering sample). Source clips, playback, mute state and listener were also checked. This was corrected in the test measurement; audio assets were not replaced.

Visual evidence in `FireSafetyTraining/Assets/Screenshots`: `Phase4-Resume.png`, `Phase4-PASS-Success.png`, `Phase4-PASS-720.png`, `Phase4-Spray-1080.png`.

## Limits and remaining work

- Phase 4 has no known blocking interaction defect from these tests. Human review of the feel and appearance is the next gate. Hand/arm animation is not included; the extinguisher itself remains visible.
- Agent capacity, aiming tolerance, 1.4–4 m practice range, sweep speeds and smoke/fire thresholds are illustrative game parameters, not manufacturer specifications or real-world safe-distance rules. Follow certified instruction and the actual equipment label in real situations.
- No hazard-aware route guidance, assembly trigger, evacuation timer, final scoring, menus or Windows build was added. Phase 5 has not begun. The existing two doorways remain navigable; exit-route safety is not yet evaluated by the Phase 4 controller.
- Runtime checks used temporary virtual input devices; hardware keyboard/mouse feel still merits the user's review. Physical speaker listening was not independently verified; nonzero Unity audio output was measured.
- No manual repair or asset import is required to try Phase 4. No Git repository was created.
