# Final object placement corrections — 20 October 2026

Scene: `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`.

Only three existing scene objects were changed:

1. `Classroom - licensed source art/NoticeBoards/CorkBoard1` is inactive. Its attached drawings, pins and collision are removed from gameplay with it. The original source asset is retained. The window, frame, glazing and surrounding wall are unchanged.
2. `Phase 3 - emergency systems/Automatic alarm sounder and beacon` moved from `(3.51, 2.40, -0.70)` to `(3.4462, 2.40, 2.00)`.
3. `Safety equipment - future interaction/fire_alarm_2k` moved from `(3.48, 1.35, -0.70)` to `(3.4458, 1.35, 2.00)`.

Both devices face the corridor, on the solid classroom wall between the window and doorway. The sounder is 2.40 m above the original floor datum; the call-point center is 1.35 m (approximately 1.32 m above the corridor finish). Their original scale and orientation are retained. Rear faces sit approximately 1 mm in front of the wall surface, verified by raycasts.

The sounder keeps its existing FireAlarmController, AudioSource, beacon, lens and detector references. The call point has only model/rendering components and two MeshColliders in the existing project. It did not have a manual activation interaction; no new interaction was added as part of these placement-only corrections.

## Change protection

The original scene and metadata are backed up in `FireSafetyTraining/PhaseBackups/FinalPlacements-20261020/`. A comparison against this checkpoint confirms that the saved scene differs only in the one inactive-object override and the two device positions. Evidence: `FinalPlacementSceneChanges.diff` and `FinalPlacements.json`.

No gameplay scripts, lighting, furniture, windows, fire/extinguisher/sprinkler logic, NPCs, routes, menus, scoring or build settings were changed. `StandaloneVerification.cs` has one opt-in call added for the new `FinalPlacementChecks.cs` verification helper. This path requires the explicit `--training-placement-checks` QA argument; it does not run during normal gameplay. `StandaloneExtinguisherChecks.cs` now waits up to 500 ms for the completion cue emitted in LateUpdate before asserting its playback. The authoring helper `AgentScripts/FixFinalPlacements.cs` is outside Assets and is not a runtime component.

## Verification

The saved scene was opened in Play Mode. Frame advancement was observed, and the actual Game view was captured and visually inspected: `FinalPlacement-GameView.png`. The noticeboard no longer covers the main classroom window. The sounder and call point appear flush against solid wall, clearly separate from the window and doorway, with no visible clipping or floating.

The pre-build Console showed zero errors and zero warnings. The scene was saved, with no compilation in progress. The Windows build and executable checks are recorded below when complete.

The first executable run is retained in `FinalPlacementStandalone/`. All seven placement checks and 19 of 20 extinguisher assertions passed, with no runtime errors. The cue assertion ran between suppression in Update and sound playback in LateUpdate; the later assertion confirmed the cue played exactly once. The QA helper was corrected to wait briefly for this event, without changing gameplay or audio logic. This initial run stopped on that failed assertion and did not verify evacuation.

## Final build and results

The final Windows x64 build succeeded in 13.59 seconds with zero errors. The single remaining warning says the Editor Pipeline integration is unavailable in Player builds; it does not disable game functionality. The prior full build also reported existing future mesh collision pre-baking and deprecated TMP shader-pragma warnings. No unrelated asset/settings changes were made to suppress them.

Executable: `Builds/CampusExterior/FireSafetyTraining.exe`. Application name, build scene, target, fullscreen and resolution settings remain unchanged.

**All 52 final standalone assertions passed**, with zero recorded runtime exceptions:

| Report in FinalPlacementStandaloneFinal | Passed |
|---|---:|
| Placements.json | 8/8 |
| Extinguisher.json | 20/20 |
| Evacuation.json | 15/15 |
| Standalone.json | 8/8 |
| Relaunch.json | 1/1 |

Placement checks ran against the built scene, not the Editor. They confirmed the inactive board, both new coordinates, wall surfaces behind the devices, retained alarm/detector references and call-point MeshColliders. A smoke-threshold simulation in evacuation mode activated the relocated sounder and confirmed its AudioSource was playing.

The extinguisher workflow used virtual E/P/LMB/mouse input through runtime Update, with an equipment-positioning fixture. It verified progressive suppression, PASS, capacity, no sprinkler contribution, completion/results and retry. The evacuation workflow exercised smoke/heat thresholds, alarm-before-sprinkler activation, water effects and suppression, then continuous player movement through the working exit into the assembly trigger, without requiring extinguisher use. Display preview/cancel, Alt+Enter, saved settings and executable relaunch passed. Test preferences were restored.

`FinalPlacementStandaloneFinal/Corrected-Window-And-Alarms.png` is an actual screenshot from the rebuilt executable and was visually inspected. The final Unity Console check reported zero errors, no compilation failure, and one current Pipeline warning. TrainingScene remains open and saved in Edit Mode.

No blocking issue remains in the tested workflows. Manual call-point activation remains unimplemented, as it was before these fixes.
