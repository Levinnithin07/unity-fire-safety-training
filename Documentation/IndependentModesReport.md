# Two independent training modes — 17 October 2026

Implemented in the existing Unity 6000.6.3f1 / URP project. This replaces the earlier combined PASS-to-evacuation sequence and its 10-second timer. The classroom, equipment models, materials, lighting, player controller, settings and navigation mesh were retained.

## Launch and menu

Open `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity` and press Play. The main menu has two separate cards:

- **Fire Extinguisher Training:** orange card, PASS handling and fire suppression.
- **Emergency Evacuation Training:** teal card, safe exit guidance and assembly.

Each opens its own briefing. **Start Simulation** begins the selected mode automatically; Enter is no longer required. Briefing Back, pause Main Menu, results Main Menu and Retry Mode work. Existing settings, credits, controls and instructions remain available. Instructions and controls adapt to the selected mode.

## Fire Extinguisher Training

- A designated desk fire begins at intensity 0.45. It is held at a contained, manageable level until the learner applies agent; it does not grow, spread or burn itself out while the player finds equipment. The simplified smoke model remains active with a bounded accumulation level. Smoke alarms are optional and disabled for this controlled exercise.
- Find the extinguisher in the passage, pick up with **E**, remove the pin with **P**, return to the clear front aisle, aim at the burning desktop from 1.4–4m and hold **left mouse** while sweeping left/right.
- Existing camera/nozzle raycasts reject obstruction, wrong height and wrong range. The safety pin blocks discharge. Spraying consumes finite agent.
- Actual agent application lowers intensity. Flame size/emission, smoke emission, light and crackle respond to intensity. Flame emission and light stop at zero; particle remnants expire naturally.
- Sprinklers are unavailable in this mode: activation is guarded, water particles/audio are reset, and water suppression returns zero. Nothing is deleted.
- No evacuation manager update, route objective, timer transition or evacuation score applies. Successful suppression shows **Fire Successfully Extinguished** and then the extinguisher report. Exhausting agent without suppression shows an incomplete result with retry.
- Results include PASS flags, on-target accuracy, effective spray time, effective sweep time, capacity consumed, and elapsed time from mode start to suppression. Full PASS credit still requires a controlled sweep; stationary base aiming can suppress the fire without earning sweep credit.

## Emergency Evacuation Training

- A growing fire and evacuation guidance begin when the mode starts. Follow the green route and directional arrow. Smoke detection, alarms and heat-triggered sprinklers use their existing simulation thresholds.
- Sprinklers are explicitly restored on every mode start. Their particles/audio and local suppression remain functional.
- Extinguisher input and pickup are disabled; its installed world model remains visible. No PASS HUD, objectives or extinguisher scoring appear.
- Reach the exterior doors, press **E**, and proceed to **Assembly Point A**. Passing a doorway alone does not complete training. The evacuation timer stops at assembly; the separate report follows.
- Results record objective completion, total evacuation time, hazardous exposure, route changes, safety and navigation scores. Route changes indicate rerouting, not an invented route-following accuracy measure.
- A fast evacuation can finish before smoke/heat reach alarm or sprinkler thresholds. Learners are not required to wait near a fire to observe equipment activation.

## Architecture and resets

`TrainingSession` owns the `TrainingMode` enum (`FireExtinguisher`, `Evacuation`) and configures the required systems centrally. FireManager receives the controlled-fire configuration; SprinklerController owns an availability guard; FireExtinguisherController owns interaction availability; the evacuation component is enabled only for evacuation.

Every start/retry/switch explicitly resets fire, fuel, equipment, PASS flags, capacity, smoke, alarms, water, detector readings, player pose, exit door, navigation state, timers, objectives and evaluation. Explicit resets also cover components whose event subscriptions are disabled in the other mode.

## Scores

- Extinguisher: PASS/suppression task completion 50%, on-target discharge accuracy 30%, remaining agent after successful suppression 20%. The five task flags are pull, aim, squeeze, sweep and suppression. Remaining-agent credit is zero without suppression.
- Evacuation: objectives 40%, safety 40%, navigation 20%. Objectives are emergency started, evacuation started, exterior exit crossed and assembly reached. Safety loses five points per hazard-exposure second, clamped to 0–100. Navigation scores 100 up to 90 seconds, then loses 0.5 per second down to 40 on completion; incomplete navigation scores zero.
- Results derive from recorded gameplay state and events. Local JSON reports remain enabled.

## Actual verification

**61 automated checks passed in the connected Editor.**

| Suite | Checks | Evidence |
|---|---:|---|
| Extinguisher mode | 18/18 | `IndependentModeA.json` |
| Evacuation mode | 15/15 | `IndependentModeB.json` |
| Switching/reset/negative cases | 12/12 | `IndependentModeC.json` |
| UI navigation/layout | 16/16 | `IndependentModeUI.json` |

Extinguisher input used virtual E/P/left-button/mouse-delta events through the real Input System and runtime Update methods. A positioning fixture moved the player to equipment and the firing aisle; this was not a human playthrough. All PASS steps completed. Intensity decreased through intermediate values to zero with 0.45 total agent suppression. Flame size, emission and smoke emission decreased, and water contribution remained zero. Before/reduced/result screenshots were visually inspected.

Evacuation selected the real menu card and started without equipment. Its environmental simulation was advanced in 50ms steps to exercise thresholds: the alarm activated at approximately 17.20 simulated seconds and the sprinkler at 28.15 seconds. Water then reduced the first fire from approximately 0.792 to 0.675 over four simulated seconds. Water particles and active audio were verified. The rear route was selected as conditions changed. Actual CharacterController movement through virtual W/E and automatic yaw steering reached assembly without teleporting during the evacuation walk, then displayed the separate results. Because environmental time was advanced independently for the test, the recorded walking timer is not a real-time benchmark of the whole accelerated emergency.

Switching exercised extinguisher → evacuation → extinguisher → evacuation. Restored sprinklers actually activated. In extinguisher mode, even an injected high-heat fixture could not activate them. Two minutes of simulated waiting did not auto-complete or grow the controlled fire. Wrong aiming consumed agent without suppression. Full PASS worked after switching back. Exhausted agent produced an incomplete result instead of false success.

Menu cards, briefing Back, instructions/controls/credits Back, settings Cancel, pause/resume, results Main Menu and retry behavior were checked. Updated text fit at 1280×720 with no overflow. Main menu, both result screens, progressive fire and active-water screenshots were visually inspected. The shared fullscreen settings remain FullScreenWindow with native resolution; Editor Game view was restored to 1920×1080.

Final audit: zero missing scripts, zero unassigned project-component object references, zero invalid materials, one EventSystem, persistent Input System action references, 211 navigation triangles, no compilation failure, zero current Console errors/warnings. All 204 protected art/AssetReview hashes matched the prior baseline. See `IndependentModesSceneAudit.json`, `IndependentModesFinalConsole.json` and `IndependentModesPreservation.json`.

During development, the Editor briefly disconnected while recompiling. The menu save completed after a tool timeout and was inspected before continuing. One evacuation test report initially failed to serialize a Unity Vector3; its reporting helper was fixed and the suite rerun successfully. These were tooling/test issues, not unresolved gameplay failures.

## Modified project files

All runtime scripts are under `FireSafetyTraining/Assets/_FireSafety/Scripts/`:

- `TrainingSession.cs`: typed mode ownership, setup, resets and independent completion.
- `TrainingInterface.cs`: menu cards, separate instructions/controls/results and reusable in-place menu upgrade.
- `TrainingEvaluationManager.cs`: independent metrics, telemetry and scoring.
- `FireManager.cs`: controlled versus emergency simulation and sprinkler configuration.
- `FireSource.cs`: flame-size response as intensity falls.
- `FireExtinguisherController.cs`: mode availability, suppression timestamp and completion feedback.
- `SprinklerController.cs`: mode availability guard, activation/suppression blocking and reset.
- `EvacuationManager.cs`: removed timed combined sequence; respects disabled state and clears navigation.
- `NavigationManager.cs`: explicit route-state reset.
- `EmergencyStatusUI.cs`: separate objectives, status and controls.
- `ExtinguisherStatusUI.cs`: hides PASS HUD when equipment interaction is unavailable.
- `EvacuationUI.cs`: hides evacuation guidance outside its mode.

Saved scene: `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity` (existing menu edited in place). No prefab or source-art reconstruction. New verification helpers are in `FireSafetyTraining/AgentScripts/TestIndependent*.cs` and `AuditIndependentModes.cs`. Screenshots are in `FireSafetyTraining/Assets/Screenshots/Independent-*`. Documentation/README describe the current behavior.

Backup: `FireSafetyTraining/PhaseBackups/Phase6-IndependentModes/` contains the pre-change scripts and TrainingScene. Older phase reports describe historical checkpoints and are superseded by this report for mode behavior.

## Handoff and remaining limits

TrainingScene is saved, clean and open outside Play Mode. Both independent modes are implemented and verified as described. No unresolved gameplay issue was found in these checks.

A standalone Windows build, real fullscreen launch and hardware performance measurement were not performed in this change. They remain the later build/verification phase. For a personal input check, press Play, choose each mode and Start Simulation; use E/P/held left-click for extinguisher training and the green route/E at the exterior doors for evacuation. Use Esc → Main Menu to switch modes and R or Retry Mode to restart.
