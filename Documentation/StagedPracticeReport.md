# Extinguisher first, evacuation second

Implemented and verified on 17 October 2026 in Unity 6000.6.3f1.

## Current workflow

In the default **PASS + evacuation** mode:

1. Start Training > Start Simulation. Find the extinguisher, press E to pick it up, then P to remove its pin.
2. Return to the clear front aisle. Enter starts the fire only after the extinguisher is ready.
3. Practise for 10 seconds: aim at the burning desktop, hold left mouse and sweep left/right. A visible Stage 1 countdown shows the remaining time.
4. If the fire goes out early, discharge stops to preserve agent; the countdown continues. At its end, Stage 2 begins automatically with evacuation guidance, whether or not suppression succeeded.
5. Follow the route, open the exterior exit with E, reach Assembly Point A, and receive the results report.

V allows an immediate voluntary evacuation. Unsafe fire spread/heat/smoke conditions retain the existing automatic early evacuation protection. Alarm activation alone does not interrupt the bounded safe practice interval. In the separate evacuation-only mode, the alarm still triggers evacuation directly. This timing is a training sequence, not advice to delay evacuation during a real emergency.

Pause freezes the practice timer; restart resets it. Duration is exposed as `extinguisherPracticeSeconds` on EvacuationManager, currently saved as 10 seconds. Briefing text describes the current duration.

## Implementation

- EvacuationManager: practice interval, countdown, explicit transition evaluation and mode-specific alarm handling.
- FireManager: readiness gate for Enter in PASS mode.
- FireExtinguisherController: automatic discharge stop once the fire is suppressed, with countdown feedback.
- EmergencyStatusUI: preparation, practice countdown and evacuation stage labels.
- TrainingInterface and existing TrainingScene labels: updated briefing/instructions, no UI reconstruction.
- Existing classroom assets and AssetReview preserved; all 204 protected file hashes match the earlier baseline.

Pre-change scripts and scene are backed up in `FireSafetyTraining/PhaseBackups/Phase6-StagedPractice`.

## Verification

- **6/6 runtime input checks passed:** Enter waits for equipment and pin removal; PASS completes before evacuation; continued mouse holding after suppression preserves agent; pause freezes the countdown; evacuation starts at 10.00002 simulated seconds.
- A full PASS attempt used virtual keyboard/mouse events through the actual Input System bindings and runtime Update loop. All PASS steps completed, agent extinguished the fire and the timed transition started evacuation.
- Continued with virtual W/E input and route steering to the assembly point, without teleporting during evacuation. The Results screen opened and recorded completed objectives and PASS handling. This is automated gameplay verification, not a human playthrough.
- **8/8 transition checks passed:** before/at time limit, unsuccessful suppression timeout, voluntary early evacuation, unsafe spread, alarm during safe practice, evacuation-only readiness/alarm handling, and restart.
- Both changed briefing/instruction panels fit at **1280×720**, without text overflow. Briefing screenshot visually inspected. The Stage 1 countdown screenshot was also visually inspected at 1080p.
- Reopened Editor reported successful compilation and zero Console errors/warnings; final state is TrainingScene, clean, outside Play Mode, with 1920×1080 Game view restored.

Evidence in this directory: `StagedPracticeLiveInput.json`, `StagedPracticeWorkflow-PASS.json`, `StagedPracticeTransitions.json`, `StagedPracticeUI.json`, `StagedPracticePreservation.json`, and `StagedPracticeFinalConsole.json`. Test helpers are in `FireSafetyTraining/AgentScripts`.

Earlier extinguisher reports describe the previous immediate-evacuation sequence; this report supersedes that behavior. No Windows executable or hardware FPS benchmark was produced in this change. Later polish/build phases remain pending.
