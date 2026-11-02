# Left-click extinguisher reliability fix — 17 October 2026

Applied to the existing Unity 6000.6.3f1 project. No phase expansion or scene rebuild.

## Changes

- Discharge uses the same elapsed-time ceiling as FireManager. Previously the extinguisher counted at most 50 ms per frame while fire growth could count 250 ms, weakening suppression at low frame rates and misclassifying sweep speed.
- Sweep detection uses a 0.75-degree reversal deadband. Small hand jitter no longer discards a valid stroke. Two substantial opposing strokes are still required for sweep credit.
- Correct base aiming reduces a manageable fire before sweep qualification; controlled sweeping is substantially stronger. Stationary aiming cannot earn complete PASS credit.
- Feedback distinguishes wrong aim height/range, obstructed view/nozzle, pin lock, cursor recapture and evacuation disabling discharge. Incorrect spraying uses an amber crosshair; valid aiming stays green.
- Shortened evacuation feedback and responsive prompt text sizing prevent clipping at smaller resolutions.
- Left mouse remains the spray control. Alarm/unsafe-condition evacuation and limited capacity remain enforced.

Changed scripts: FireExtinguisherController.cs, ExtinguisherStatusUI.cs and EvacuationUI.cs under Assets/_FireSafety/Scripts.

## Verified in connected Editor Play Mode

- 29/29 regression checks passed: actual Input System E/P/LMB bindings using virtual devices, pin animation, visible textured held model, discharge particles/audio/lever, capacity, cursor release/recapture, fresh click, aim height/range/obstruction, static aim without sweep credit, rapid shaking, full PASS suppression, unsafe spread and reset.
- A complete virtual E/P/Enter/LMB/mouse-delta attempt through runtime Update completed PASS, extinguished the fire by agent and initiated evacuation. Automated input, not a human mouse playthrough.
- Deterministic controlled sweeps at 60/20/10 Hz extinguished in approximately 1.43/1.45/1.50 simulated seconds. Before the fix the 10 Hz fixture failed after 10 seconds and counted only five seconds of agent use. These are frame-interval tests, not hardware FPS measurements.
- Sweeping with hand jitter completed PASS and extinguished in approximately 1.83 simulated seconds. Steady base aim extinguished in 5.08 seconds without sweep/PASS credit.
- Six additional checks passed: tiny jitter cannot earn sweep credit, clear base aim, specific nozzle obstruction feedback, smoke-driven alarm evacuation disables spray, correct HUD explanation, and no prompt overflow at 1280×720. Screenshot visually inspected.
- Compilation succeeded; final Console had zero errors and zero warnings.
- All 204 protected art/AssetReview hashes unchanged. TrainingScene bytes unchanged from the pre-fix backup.

Evidence: ExtinguisherReliability-Before.json, ExtinguisherReliability-After.json, ExtinguisherFixRegression.json, ExtinguisherFixLiveInput.json, ExtinguisherFixEdgeChecks.json and ExtinguisherFixPreservation.json. Fixtures: FireSafetyTraining/AgentScripts. Backup: FireSafetyTraining/PhaseBackups/Phase6-ExtinguisherFix.

The edge-check fixture initially did not advance background Game frames. Background execution was enabled for that fixture and checks rerun successfully. A genuine 720p message overflow discovered during testing was fixed before the final passing run.

## Trying the fix

Open Assets/_FireSafety/Scenes/TrainingScene.unity, Play, Start Training, Start Simulation. Pick up with E and pull the pin with P before pressing Enter to start the practice fire. Stand in the clear front aisle, aim at the burning desktop until the crosshair turns green, hold left mouse and sweep left/right. If the alarm has initiated evacuation, spraying is intentionally disabled: follow the route, or press R to restart practice. After returning from a menu or recapturing the cursor, release left mouse once before holding it again.

TrainingScene is left open in Edit Mode with the 1920×1080 Game view restored. Windows executable verification and actual Iris Xe performance measurement remain future build work.
