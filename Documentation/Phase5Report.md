# Phase 5 — Evacuation and assembly

Completed 17 October 2026. Stopped before Phase 6. Unity 6000.6.3f1, URP 17.6.0, Input System 1.20.0 and AI Navigation 2.0.14. Live Unity Editor operations used the connected Unity CLI/Pipeline bridge. No version-control repository was created.

## Open and play

Scene: `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`.

The saved scene is open in Edit Mode; Game view is restored to 1920 × 1080. Press Play and click Game view. Use WASD/mouse, Left Shift to sprint and Left Ctrl to crouch. Esc releases the cursor; it does not pause simulation. Click to recapture.

1. Press Enter to start the emergency. V voluntarily starts evacuation during an emergency. Alarm activation, unsafe fire conditions or successful extinguisher suppression also starts evacuation automatically.
2. Follow the green floor line and directional route card. A smoky or obstructed front classroom doorway causes guidance to select the rear doorway when that route is clear.
3. In the passage, approach either glazed courtyard exit leaf, look at its leaf or push bar and press E. The paired leaves open outwards. They have physical collisions and an interaction range of 2.3 m.
4. Walk into the marked Assembly Point A outside. The completion message, sound and frozen evacuation time confirm arrival. Stay outside and report to the person in charge.
5. R resets the emergency and evacuation, closes the exterior doors, restores the extinguisher and returns the player to the classroom spawn after an evacuation attempt. M retains the alarm-audio inspection toggle.

Existing PASS controls remain E pickup, P pull pin and left mouse spray. Collect the extinguisher before starting a manageable fire if practising PASS. Once evacuation begins, further discharge stops and the HUD prioritizes leaving.

## Implemented

- Preserved the classroom, furniture, materials, lighting and player controller. The two existing classroom openings provide front and rear paths into the passage.
- Added operable glazed exterior leaves fitted into the passage's two existing full-height glazed openings, with frames, panic bars, kick plates, exit signs and door audio. Push labels move with their leaves. The original central wall pier remains. The original glazing scene instance is disabled only in TrainingScene; its source mesh is unchanged.
- Added a bounded outdoor assembly court with 2K PBR concrete paving, benches, guard rails, floor markings, sign and an assembly trigger volume. The camera now uses the existing skybox for an outdoor sky; interior ambient settings remain intact.
- Baked a 211-triangle NavMesh from physical colliders, with a 0.25 m radius, 1.8 m tall trainee profile, 0.15 m step and 0.055 m voxel size. Data is saved as `Assets/_FireSafety/Scenes/TrainingEvacuationNavMesh.asset`.
- Added smoke/heat/obstruction-aware path queries. Front and rear doorway areas are excluded when hazardous, and safe routes are weighted by smoke. Paths are checked for physical obstructions. No autonomous NavMeshAgent drives the player.
- Added localized doorway smoke, red/green route signs, route-change feedback, turn guidance, distance, floor trail, no-clear-route warning and elapsed evacuation time. Both doorway smoke systems are limited to 70 particles each; total configured scene particle capacity is 1,358.
- Added actual evacuation state and telemetry: start reason/time, elapsed time, route changes, hazardous exposure, exit crossing, assembly completion and a one-shot completion event. These are available for later evaluation; no score or results screen was added.
- Reset clears evacuation state, audio, doors and hazards. Completion only occurs during an active emergency evacuation after exterior crossing and entering the assembly zone under the illustrative local heat limit.

New components: ExitDoor, RouteHazard, NavigationManager, EvacuationManager and EvacuationUI. Existing DesktopPlayerController gained a reset-pose method; FireExtinguisherController and EmergencyStatusUI now respond to evacuation state.

## Actual verification

| Verification | Result | Evidence |
|---|---|---|
| Evacuation Play Mode checks | 25/25 passed | Phase5PlayModeTests.json |
| Earlier fire, smoke, detector, alarm and sprinkler regression | 26/26 passed | Phase5EmergencyRegression.json |
| Player movement and collision regression | 24/24 passed | Phase5MovementRegression.json |
| Extinguisher/PASS regression | 29/29 passed | Phase5ExtinguisherRegression.json |
| Full PASS input sequence through Update, then automatic evacuation | Passed | Phase5PASSIntegration.json |
| Real-time front route to assembly | Passed; 6.624 s evacuation time | Phase5LiveFront.json |
| Real-time rear route with front smoke hazard | Passed; 8.346 s evacuation time | Phase5LiveRear.json |
| HUD bounds/text overflow at 1920 × 1080 and 1280 × 720 | Passed, including longest no-route warning | Phase5HUD-1920.json, Phase5HUD-1280.json |
| Saved scene audit | No missing scripts, null gameplay references or unsupported materials; valid initial path | Phase5SceneAudit.json |
| Final Console | 0 errors, 0 warnings; compilation not failed | Phase5ConsoleAudit.json |
| Protected source files | 204/204 identical SHA-256 values | Phase5PreservationAfter.json |

The 25 evacuation checks cover V/E/R bindings, no false exploration completion, closed-door collision and range, door opening, both physical routes, smoke-based rerouting, physical obstruction rerouting, no-clear-route fallback and recovery, hazard exposure, automatic alarm/unsafe-fire transitions, discharge cancellation, timer freezing, one-time completion and courtyard boundaries.

Deterministic tests advance simulation and CharacterController motion in controlled steps. The two real-time walkthroughs instead used virtual W/V/E Input System devices through normal Update, with automated yaw steering along navigation corners. No player teleport was used during those route traversals. Hazard conditions for the rear walkthrough were prepared by advancing the fire simulation. These are automated Editor tests, not a claim of an independently performed human playthrough.

Completion audio was verified with nonzero output RMS (front 0.0103, rear 0.0116). Screenshots were visually reviewed for smoke, route signs, doors, exterior materials and completion UI. Final screenshots include `Assets/Screenshots/Phase5-Exit-Front.png`, `Phase5-Assembly-Front.png` and `Phase5-Assembly-720.png`. `Phase5-Reroute.png` records the smoke reroute. Rear screenshots were taken before the final push-label attachment and skybox refinements; they record the successful route, not the latest visual state.

During implementation, a NavMeshPath initialization on the wrong thread and a non-readable texture conversion were found and fixed. Final Play Mode runs and scene audit passed after those repairs. The Editor was reopened during this phase; the saved navigation data and scene remained functional, as verified by the later real-time walkthroughs.

## Assets and preservation

Imported Poly Haven's **Hexagonal Concrete Paving** by **Stephan Seeliger**, CC0: https://polyhaven.com/a/hexagonal_concrete_paving. License: https://polyhaven.com/license. Diffuse, normal and roughness 2K JPGs exist under `Assets/ThirdParty/PolyHavenPaving/`, with publisher MD5 matches and SHA-256 records in Phase5ImportedAssets.json. A derived smoothness map is used by URP. See AssetAttribution.md for the full credits and original generated geometry/audio. Powered by Poly Haven.

AssetReview and all protected prior art files are unchanged. Pre-Phase-5 scene and scripts are preserved in `FireSafetyTraining/PhaseBackups/Phase4/`. Existing earlier backups remain intact. Scene changes were made through Unity Editor APIs; original scene YAML was not hand-edited.

## Limits and next work

- The two alternative classroom routes converge in a shared passage and courtyard. This is not a claim of two fully independent building egress networks or building-code certification.
- Smoke transport and hazard thresholds are simplified educational parameters, not CFD or measured survival limits. The HUD identifies smoke as a relative simulation index. EducationalSources.md documents the values and authoritative guidance.
- Completion remains latched until R. The instruction is to stay outside; the player is not physically restrained from moving or re-entering afterward. Later evaluation can use the recorded events.
- No new Windows build or standalone fullscreen test was performed in Phase 5. Existing borderless launch configuration remains. Game-view resolution checks do not verify executable fullscreen behavior.
- No new hardware performance benchmark is claimed. Existing graphics settings were retained, the navigation mesh is small and particles are bounded. Final standalone Iris Xe profiling remains Phase 8 work.
- Phase 6 menus, settings, briefing, scoring/results and persistent preferences remain incomplete. Phase 7 polish and Phase 8 build verification remain pending.

No manual import or repair is required. The remaining action is the user's Phase 5 environment/gameplay review in TrainingScene before proceeding.
