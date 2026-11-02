# Phase 6 — Menus, settings and evaluation

Completed 17 October 2026. Stopped before Phase 7. Unity 6000.6.3f1, URP 17.6.0 and Input System 1.20.0. Development and testing used the connected Unity CLI/Pipeline Editor bridge.

## Launch and controls

Open `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity` and press Play. The main menu appears. Choose **Start Training → Start Simulation** after reading the briefing. The saved scene is left open in Edit Mode, with Game view restored to 1920 × 1080.

| Input | Action |
|---|---|
| WASD / mouse | Move / look |
| Left Shift / Left Ctrl | Sprint / crouch |
| E | Pick up extinguisher or open exterior exit |
| P / held left mouse | Pull safety pin / discharge |
| Enter | Start the emergency during gameplay |
| V | Choose evacuation during an emergency |
| Esc | Pause / resume; cursor unlocks in menus |
| R | Restart the practice attempt |
| M | Toggle alarm audio |
| Alt + Enter | Toggle fullscreen preference; actual display change applies in a Windows build |

The briefing offers PASS + evacuation or evacuation practice as a learning focus. Both use the same classroom and emergency systems; the choice changes the instructions and report label rather than creating a separate environment. Exploring before pressing Enter remains possible. Suppression is optional, and unsafe conditions prompt evacuation. Reaching assembly opens the results screen after a short completion notification.

## Implemented

- Eight responsive uGUI pages: Main, Briefing, Instructions, Controls, Settings, Credits, Pause and Results. They use the existing Liberation Sans font, live classroom/courtyard background, orange accents, hover/selection states, short fades, click audio and dark results cards. One EventSystem handles mouse and keyboard navigation through the existing Input System actions asset.
- Main menu: Start Training, Instructions, Controls, Settings, Credits and Exit. Pause: Resume, Settings, Controls, Restart, Main Menu and Exit. Results: Retry Training, Main Menu and Exit. All callbacks are connected and were exercised.
- A single-scene session controller manages transitions without reloading the classroom. It freezes simulation time and audio while in menus, blocks gameplay inputs, controls cursor capture and hides the gameplay HUD. Esc now pauses, replacing the earlier cursor-release-only behavior. Losing application focus also requests pause.
- Restart and returning to the main menu clear emergency, extinguisher, evacuation and evaluation state. Retry returns to exploration. Results freeze a snapshot of the completed attempt; subsequent resets do not alter the saved snapshot.
- Settings: borderless/windowed display, resolution selection, Low/Medium/High graphics, master volume and mouse sensitivity. Apply previews the choices; Keep saves them. A 15-second unscaled timeout reverts unconfirmed settings even while gameplay is paused. Cancel restores confirmed values. Preferences use PlayerPrefs and reload at startup.
- First-launch display defaults to native-resolution FullScreenWindow. TrainingScene is the enabled build entry. Alt + Enter toggles the preference and is guarded against also submitting a menu button or starting a fire. Actual Screen.SetResolution calls are intentionally limited to the standalone player; the Editor clearly explains this.
- Evaluation records equipment pickup, pin removal, emergency start, correct aim/squeeze/sweep, suppression, alarm activation, evacuation start, exit crossing and assembly arrival with simulation timestamps. Results include objective completion, handling score, on-target discharge, safety score, evacuation duration, route changes, hazard exposure, individual PASS steps and personalized feedback.
- Completed reports are exported as timestamped JSON files under `Application.persistentDataPath/TrainingResults`. Save failures are reported on screen without breaking results. Existing JSON reports produced by automated verification may be present.

Main new components: TrainingSession, TrainingInterface, TrainingSettings and TrainingEvaluationManager. Existing player/fire/extinguisher/evacuation scripts received input gates for menus and pause; the emergency HUD now shows the learning focus and updated Esc behavior. The previous display-settings component was replaced by TrainingSettings; its source remains in the project and the pre-change scene/scripts are backed up.

## Evaluation rules

Scores are computed from the component telemetry, not preset outcomes. The formulas are also included in each JSON report.

| Metric | Calculation |
|---|---|
| Evacuation objectives | Four required milestones, 25 points each: emergency started, evacuation started, exterior crossed, assembly reached |
| Handling | Five actual flags (pin, aim, squeeze, sweep, suppression) at 12 points each, plus 40% of measured on-target discharge accuracy |
| Safety | 100 minus 5 per second of recorded hazard exposure, minus 35 for unsafe discharge; clamped to 0–100 |
| Navigation | 100 on completion within 90 seconds, then minus 0.5 per additional second to a minimum of 40; 0 if incomplete |
| Overall | Objectives 35%, safety 40%, navigation 15%, handling 10% |

If no discharge was attempted, handling is **not assessed**, its weight is excluded, and the remaining weights are normalized. This prevents penalizing a learner for choosing safe evacuation. The four evacuation objectives remain the required tasks for both learning focuses; optional PASS performance is evaluated separately. Completion status reflects reaching assembly, not an arbitrary score threshold.

These weights and time/exposure thresholds are educational design choices, not certified competency or real-world exposure limits. The simulation notice and the earlier educational references remain applicable.

## Graphics settings

| Preset | Render scale | Main shadow map | Shadow distance | MSAA |
|---|---:|---:|---:|---:|
| Low | 0.70 | 512 | 12 m | Off |
| Medium | 0.85 | 1024 | 20 m | 2× |
| High | 1.00 | 2048 | 30 m | 2× |

Low also uses a one-level texture mip reduction and lower LOD bias. Medium is the initial graphics preference. All three use compatible URP assets derived from the existing setup. This configuration is not a new measured FPS claim; final Iris Xe standalone profiling remains pending.

## Actual test evidence

| Verification | Result | Evidence file |
|---|---|---|
| Menu flow, mouse click, callbacks, settings and pause | 27/27 passed | Phase6UITests.json |
| Scoring fixtures for safe, incomplete, poor handling and unsafe attempts | 9/9 passed | Phase6ScoringTests.json |
| Alt + Enter preference and input-conflict checks | 4/4 passed | Phase6ShortcutTests.json |
| Main, Pause and Results Exit buttons | All three invoked quit and stopped Editor Play Mode; saved preferences reloaded | Phase6-ExitGame.json, Phase6-PauseExit.json, Phase6-ResultsExit.json |
| Fire/smoke/detector/alarm/sprinkler regression | 26/26 passed | Phase6EmergencyRegression.json |
| Movement, collision, mouse look and updated Escape behavior | 24/24 passed | Phase6MovementRegression.json |
| Actual E/P/Enter/LMB/mouse-delta PASS sequence | Passed, all handling steps and suppression recorded | Phase6PASSIntegration.json |
| Real-time evacuation without discharge → results | Passed; latest exit timer 8.834 s, handling not assessed | Phase6Workflow-Evacuation.json |
| PASS → evacuation → results | Passed; handling assessed at 100%, all five flags true | Phase6Workflow-PASS.json |
| All eight page bounds/text overflow at 1080p and 720p | Passed, including longest feedback fixture | Phase6Layout-1920.json, Phase6Layout-1280.json |
| Final Console | 0 errors, 0 warnings, compilation not failed | Phase6ConsoleAudit.json |
| Saved scene | No missing scripts, unassigned gameplay references or unsupported materials; one EventSystem; all UI actions resolve to the saved asset | Phase6SceneAudit.json |
| Protected originals | 204/204 SHA-256 matches | Phase6PreservationAfter.json |

The input tests include a virtual mouse click through the real UI Input System module, real keyboard bindings and pointer-click dispatch through the actual button components. Other checks directly exercise shared simulation functions. Exit tests execute the real Editor branch of Quit; they do not establish that a built Windows process exits correctly.

The two complete walkthroughs move the CharacterController through ordinary Update with virtual input and automated yaw steering along the path. No teleport is used during the evacuation walk. The PASS test positions the player at the equipment/fire for controlled interaction verification, then uses actual input bindings. The PASS report's 31.66-second evacuation timer includes the tool interval between suppression and the separate walking test; it is not a human speed benchmark. Scoring fixtures inject controlled component values, are explicitly labelled as fixtures, and do not save user reports. A poor-performance fixture produced 71.35 overall rather than 100, confirming the score responds to the measured inputs.

Visually inspected screenshots include `Assets/Screenshots/Phase6-Main.png`, `Phase6-Settings-720.png`, and `Phase6-Results-Evacuation.png`. The latest evacuation screenshot includes the final dark report cards and unassessed PASS wording. `Phase6-Results-PASS.png` records the earlier successful PASS report before those presentation refinements.

Problems found and fixed during this phase: an Input System button-property typo; default UI references lacking a saved actions asset; Keep values reverting when leaving Settings; and Alt + Enter also submitting the highlighted menu button. The final audit checks scene-embedded action references by their resolved saved actions asset, rather than incorrectly requiring each reference to be a separate asset file. Input continued working after the user reopened the Editor.

## Preservation and remaining work

The approved classroom art, materials, textures and AssetReview remain unchanged. Phase 5 navigation data still has 211 triangles. Backups before this phase are in `FireSafetyTraining/PhaseBackups/Phase5/`, including TrainingScene and its scripts. No version-control repository was created. No additional third-party download was needed; AssetAttribution.md includes the UI/font/audio credits.

No manual import or repair is required. Inspect TrainingScene through the new main menu. Phase 7 final polish and Phase 8 Windows build verification have not started. No executable was built in Phase 6, and native resolution changes, borderless/windowed switching, Windows process exit, a fresh executable launch and standalone hardware performance remain unverified. Editor resolution and preference tests do not substitute for those checks.
