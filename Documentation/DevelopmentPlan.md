# Virtual Reality Fire Safety Training System

## Scope and decisions

Non-immersive Windows desktop training for keyboard and mouse. No headset or XR interaction packages. Local project, no version control, as requested. Work began 12 October 2026.

Project: `FireSafetyTraining/`. Unity 6000.6.3f1, Universal Render Pipeline 17.6.0. This installed editor supports the Unity Pipeline live bridge (requires Unity 6+), and its URP template provides Input System, AI Navigation and uGUI. Keep this pipeline throughout development. Windows Mono support is installed; validate standalone build capability in Phase 8. Editor selection is an installed-version compatibility decision, not a claim that this is the latest LTS.

Visual direction: a furnished college teaching room connected to a coherent circulation/evacuation area. Use Christophe Seux's Classroom as the environment foundation, retaining its detailed furniture, architecture and surface treatment. Extend only after checking scale, topology, material compatibility and performance. Do not substitute a cube-built final environment.

## Phases and evidence gates

1. **Setup and licensed assets.** Create project, establish live editor connection, acquire verified art, record licenses and hashes, import and inspect meshes/textures. Exit: correct version/pipeline, readable models, no compiler failures. Downloaded and imported are separate states.
2. **Environment and movement.** Adapt classroom and circulation space with two usable evacuation paths, reception/exit area and equipment. URP PBR materials, lighting, collisions, Input System character controller, crouch clearance and interaction raycast. Exit: inspect rendered screenshots and physically traverse every route; no missing shaders, no floor/wall penetration. Do not advance with an unacceptable environment.
3. **Emergency systems.** Combustible-object graph; bounded fire intensity and smoke accumulation; detector threshold driven by smoke; alarm circuit; independently heat-triggered sprinkler coverage. Exit: verify correct causal sequence and suppression response, including no-fire baseline.
4. **PASS extinguisher.** Visible held model and pin; limited agent; base-aim ray tests, obstruction tests, controlled sweep detection, particle/audio discharge and effective suppression telemetry. Exit: test locked pin, bad aim, empty capacity, good sweep and extinguishment.
5. **Evacuation.** Hazard-weighted route graph over walkable areas; directional guidance, route closures, fallback route, safe assembly trigger and timer. Exit: traverse both routes, prove rerouting, avoid rewarding unsafe firefighting.
6. **UI and evaluation.** Menu, briefing, instructions, controls, settings, HUD, pause, results, replay and credits. Responsive Canvas Scaler. Native-resolution borderless default, windowed/resolution options and saved preferences. Scores derive from timestamped gameplay events. Exit: exercise each button, settings persistence and scoring cases including evacuation without suppression when unsafe.
7. **Polish.** Spatial audio, restrained effects and transitions, texture compression, quality presets, batching and lighting optimization. Exit: visual/audio review and no new regressions.
8. **Build verification.** Windows x64 executable, clean compile/runtime logs, full fresh-launch walkthrough, resolution/fullscreen tests, measured frame-time sampling on this laptop. Exit: record results and limits; no invented FPS or unperformed tests.

## Runtime design

Component systems: TrainingManager, DesktopPlayerController, InteractionSystem, FireSource/FireManager, SmokeDetector, FireAlarmController, SprinklerController, FireExtinguisherController, EvacuationManager, NavigationManager, TrainingEvaluationManager, UIManager, AudioManager, SettingsManager.

Use explicit state transitions: Menu -> Briefing -> Exploration -> Emergency -> Response/Evacuation -> Results. Pause preserves previous state. Scenario reset clears all telemetry and transient hazards. Record objective completion, pin removal, spray duration, effective aim duration, sweep coverage, suppression, hazardous exposure, evacuation start and assembly arrival. Keep event data available in a local report.

Performance targets are goals pending measurement: 30–60 FPS, Medium tuned for Iris Xe, 2K prominent textures, smaller auxiliary maps, limited shadowed lights and particle budgets. Low/Medium/High settings must materially change rendering cost. Avoid HDRP and expensive volumetric simulation.

## Educational review

Verify final text against authoritative fire-safety guidance before gameplay implementation. Teach small/contained fire response only when trained, correct equipment is available and escape is clear; prioritize evacuation for heavy smoke, spreading fire, compromised egress or depleted equipment. Include: “This simulation supplements but does not replace certified fire safety training.” Numeric game thresholds are illustrative simulation parameters, not real-world safety limits.

## Known pending work

Phases 1–6 are implemented to their documented scope. Phase6Report.md records menus, pause, persistent settings, weighted evaluation and verification limits. Phase5Report.md retains evacuation/navigation evidence; Phase4Report.md retains the recovery audit and PASS checkpoint. Source Korean labels are preserved with English HUD instructions. Detector, sounder and heat-triggered sprinkler remain distinct working systems.

The Phase 6 checkpoint introduced the main menu, briefing and results flow in the single TrainingScene; the independent-mode update below supersedes its combined sequence. Esc pauses time and gameplay inputs. Settings use a timed confirmation and PlayerPrefs persistence. Evaluation captures actual task flags, PASS handling, hazard exposure and exit time, and exports a local JSON report. Safe evacuation without using the extinguisher remains a valid outcome.

Phase 7 polish is implemented and verified as recorded in Phase7Report.md. Phase 8 Windows build verification remains pending. The full training flow has been exercised in Editor Play Mode. Native display switching, Windows process exit and standalone performance still require executable verification; Editor tests do not establish those results.

## Independent modes update — 17 October 2026

The requested independent Fire Extinguisher and Emergency Evacuation modes supersede the combined timed sequence. Both use the existing TrainingScene. Implementation, mode policies, 61 passing checks and remaining Windows build work are recorded in IndependentModesReport.md. No additional art pipeline or environment rebuild was introduced.

## Phase 7 update — 18 October 2026

Added movement-triggered footstep variations, a once-only suppression success cue and graphics-linked particle limits. Retained the existing lighting, surface treatment, texture compression and batching configuration after inspection. All 83 current polish/workflow checks passed; the saved scene has no missing scripts, unassigned gameplay references, invalid materials or Console errors. AssetReview and all 204 protected art files are unchanged. See Phase7Report.md for exact evidence and automated-test limitations. Stopped before Phase 8; no standalone executable or FPS claim yet.

