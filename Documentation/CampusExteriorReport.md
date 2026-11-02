# Campus exterior upgrade — 19 October 2026

The existing TrainingScene has been expanded visually into a two-storey, cream-and-terracotta academic block inspired by the supplied reference. It is the same Unity 6000.6.3f1 / URP project. The five ground-floor classrooms and the original main classroom remain in place. AssetReview is unchanged.

## Open and play

- Scene: `FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`.
- Select either mode on the main menu, then Start Simulation.
- WASD movement, mouse look, Shift sprint, Ctrl crouch; E interact, P pull extinguisher pin, hold left mouse to spray; Esc pause, R retry.
- The upper floor and main compound gate are decorative. Evacuation still ends at the green ground marking outside the central classroom exit, not at the campus gate.
- Backup: `FireSafetyTraining/PhaseBackups/CampusExterior-20261019/` contains the pre-upgrade project-owned content, Packages and ProjectSettings.

## Environment changes

- Decorative upper floor with a roof/parapet, terracotta colonnade, balcony railings, recessed windows and doors. No upstairs interiors, stairs, gameplay, NPCs or accessible navigation were added.
- A centered, text-only **SATHYABAMA UNIVERSITY** sign on the upper facade. No official logo or affiliation claim was introduced. The old School Safety Learning Centre exterior sign is hidden.
- The ground corridor is opened to the courtyard with low parapets/railings, column bays, a tiled floor, ceiling fittings and benches outside doorway clearance.
- Five real corridor-facing classroom window openings were cut into derived meshes. Frames, sills, transoms, glass and louvres were added. Source FBX files were not edited. Existing rear window geometry remains, with updated dark frame/glass finishes.
- Taller plaster boundary walls, regularly spaced piers, coping and grille sections; a centered double metal gate and marked paved entrance.
- Two CC0 yellow coaches parked in marked asphalt bays. They are environmental objects with colliders. They do not enter the evacuation route.
- Leafy trees, reused grass/shrubs, raised planting borders, benches and lanterns supplement the courtyard. A parking-side bench was moved to avoid intersecting a bus.
- The large upright assembly sign and its post are hidden. The green ground marking, original assembly trigger and destinations remain. Trigger center `(14.2, 1, -0.2)`, size `(4.8, 2, 4.8)`; navigation destination `(14.2, 0, -0.2)`.
- The previously requested removal of the computer/table blocking the classroom board is preserved. One computer remains on the original teacher desk.

## Files and assets

The only pre-existing project-owned files with changed contents compared with the checkpoint are listed in `CampusModifiedExistingFiles.json`. Core scene changes are `Assets/_FireSafety/Scenes/TrainingScene.unity` and `Assets/_FireSafety/SchoolExpansion/Navigation/SchoolNavMesh.asset`. Existing gameplay C# scripts are unchanged. The opt-in standalone QA helpers StandaloneExtinguisherChecks.cs and StandaloneVerification.cs were updated on 20 October to resume focus-loss pauses during background automation, record those pauses, and fail the verification process if any gameplay assertion fails. Normal play never instantiates this QA helper.

New generated assets are under `Assets/_FireSafety/CampusExterior/`: shared architectural meshes, derived window-opening meshes, URP materials, bus/tree/shrub prefabs and a packed leaf texture. New downloaded originals are under `Assets/ThirdParty/CampusExterior/Bus`, `plastered_wall_02`, `asphalt_02` and `floor_tiles_04`. Existing licensed tree/shrub sources under `Assets/ThirdParty/SchoolExpansion` are reused.

Authoring helpers in `AgentScripts/`: `BuildCampusExterior.cs`, `CampusWindows.cs`, `FurnishCampus.cs`, `ExportCampusTree.cs`, `PolishCampus.cs`, `RefineCampus.cs`, `FinalizeCampusGeometry.cs`, `OptimizeCampusRooms.cs`, `CampusFinalDetails.cs`. They are historical sequential authoring steps, outside Assets, not runtime components; do not rerun builders indiscriminately over an already-built scene.

Verification helpers: `AuditCampus.cs`, `TestCampusExtinguisher.cs`, `TestCampusEvacuation.cs`, `TestCampusSwitching.cs`, `TestCampusNPCs.cs`, `TestCampusExplore.cs`, `CampusPerformance.cs`, `CaptureCampus.cs`; standalone launch helper `Tools/VerifyCampusBuild.ps1`.

Download and mesh-processing helpers and provenance are under `Tools/` and `SourceAssets/CampusExterior/`. The asset links, creators, license terms and alterations are recorded in `AssetAttribution.md`. New bus and Poly Haven assets are CC0. Shared original classroom textures and meshes are retained.

## Tests actually performed

**84 connected-Editor checks passed**, with detailed evidence retained:

| Report | Checks | Method and coverage |
|---|---:|---|
| CampusSceneAudit.json | 18 | Five rooms; no gameplay in replicas; missing-script/material scan; routes from all five rooms; NPC references; decorative upper floor; exact university lettering; hidden old signs; unchanged trigger; two buses; serialized gameplay settings compared with pre-upgrade snapshot. Instance IDs normalized for comparison; this is supplemented by runtime interaction tests. |
| CampusModeA.json | 20 | Main-menu button events and virtual Input System E/P/LMB/mouse movement through runtime Update. Positioning fixture places the player near pickup/fire. Progressive intensity, flame size/emission/smoke reduction, PASS, capacity, zero sprinkler contribution, scoring, results and retry verified. |
| CampusModeB.json | 15 | Menu events; deterministic 50 ms simulation steps exercise smoke/heat thresholds; visible water and audio checked; then continuous CharacterController movement using virtual W/E and automatic yaw reaches the actual assembly trigger. No teleport during evacuation. |
| CampusModeC.json | 12 | Mode-switch/reset edge cases; wrong aim; depleted agent; high heat rejected in extinguisher mode; sprinklers restored in evacuation mode. Prepared-pin fixture used for the round-trip suppression test. |
| CampusNPCRegression-FireExtinguisher.json | 7 | All eleven occupants reached individual assembly positions in 15.83 seconds. Animated NavMesh movement; 100 ms torso sphere-cast samples detected zero wall intersections and no crossing of closed doors. No player completion caused by NPC arrival. |
| CampusNPCRegression-Evacuation.json | 7 | All eleven arrived in 15.95 seconds; same collision, animation, independence and retry checks. |
| CampusPlayerExploration.json | 5 | Continuous virtual W+Shift movement from the main spawn through all four additional classrooms, then assembly; automatic yaw follows NavMesh. No teleport. About 92 m total movement. |

The final NavMesh was rebuilt and saved with the existing agent type `-1372625422`; 1,158 navigation triangles. No upstairs navigation exists. The low-level canopy at about 3.05 m is a separate pre-existing surface; it is not an upper-floor route. The original player/NPC routes were retained. Final scene audit was repeated after the parking-bench/trim refinements; the minor refinements do not change the central exit/assembly approach.

The final pre-build Console sample reported no compilation failure and zero current Console errors/warnings. The plugin's historical warning counter was two, distinct from the current Console count. Command-script diagnostics during development were corrected before the successful runs above.

## Measured performance and optimization

On **Intel Iris Xe, Medium, 1920×1080**, two ten-second Editor Game-view samples measured:

| View | Average FPS | Median frame | 95th-percentile frame |
|---|---:|---:|---:|
| Main classroom, fire and departing NPCs | 34.83 | 29.53 ms | 32.37 ms |
| Courtyard facing the two-storey building | 36.22 | 27.58 ms | 30.68 ms |

These are short Editor samples, not a guarantee at every camera position, preset or thermal condition. Before detail reduction, the same courtyard view averaged 27.35 FPS. Full evidence: `CampusEditorPerformance.json`, `CampusEditorPerformance-BeforeOptimization.json`.

The four environment-only classrooms now keep all 231 renderers nearby and use 45 structural/window renderers at distance. Approaching/entering a room restores full detail; colliders remain in both levels. The main classroom is unchanged. Trees use shared derived LOD meshes (246,377 / 110,753 triangles instead of the two-million-triangle source), with distance culling. Architectural parts are batched by group/material. New exterior fill light has no realtime shadows and is restricted to exterior renderers. Most small details add no lights or runtime scripts. Materials use compressed, mipmapped 1K/2K imports.

## Actual scene screenshots

All six files are rendered by Unity from the saved scene at 1600×900, with the HUD omitted for documentation. They are not generated illustrations or edits to the reference.

1. `CampusScreenshots/01-Two-storey-academic-block.png`
2. `CampusScreenshots/02-University-name-board.png`
3. `CampusScreenshots/03-Corridor-and-classroom-window.png`
4. `CampusScreenshots/04-Campus-wall-and-main-gate.png`
5. `CampusScreenshots/05-School-bus-parking.png`
6. `CampusScreenshots/06-Assembly-without-upright-board.png`

## Scope and remaining limitations

No blocking gameplay issue was found in the tested routes. The reference informed proportions, colors, upper balcony, signage and gate; the result is a performance-conscious real-time environment, not a photorealistic reproduction of that image. Vegetation is lighter and the retained NPC art is stylized. Upper windows are decorative recesses, while ground-floor classroom windows are actual openings. Distant furniture transitions and fine railing/leaf aliasing can be visible at Medium settings. The gate is intentionally stationary and the upper floor inaccessible. Human review of the visual design and a longer walk-through is still useful; automated checks do not cover every possible movement or viewing angle.


## Windows build and verification follow-up — 20 October 2026

A Windows x64 release build is available at `Builds/CampusExterior/FireSafetyTraining.exe`. The first campus build succeeded in 47.46 seconds, with zero errors and three dependency warnings: automatic collision pre-baking in future Unity versions, Editor Pipeline intentionally absent in Player builds, and a deprecated TMP shader pragma. Current Unity still builds the required collision data. The subsequent QA rebuilds and final verification are recorded separately below.

Historical unsuccessful runs have been retained rather than overwritten:

- `CampusStandalone/`: the executable was launched inside the tool sandbox. Windows registry preference writes and cursor confinement were denied. This run cannot certify normal desktop behavior.
- `CampusStandaloneNormal/`: unrestricted execution removed those permission errors. All 15 evacuation assertions, eight display/error checks and the relaunch-persistence check passed. The extinguisher fixture paused after window focus was lost, so eight suppression/completion assertions failed. Its screenshot shows the Pause page and telemetry remains frozen at 2.9 simulated seconds while aim remains correct. No gameplay code change was needed for this: the opt-in QA fixture now resumes the normal pause, releases/represses LMB after recapture, and records focus/pause state. Performance automation also resumes focus-loss pauses. Aggregate verification now fails explicitly when either gameplay report has failing assertions.

### Final executable verification

The final build succeeded on 20 October in 23.35 seconds: zero errors, one warning that the Editor Pipeline integration is intentionally unavailable in the standalone player. Build size reported by Unity: 450,363,541 bytes. Full report: `CampusWindowsBuild.json`.

`CampusStandaloneFinal/` records the completed unrestricted Windows run. Both executable launches exited successfully. **All 44 assertions passed**: 20 extinguisher checks, 15 evacuation checks, eight display/runtime checks, and one settings-persistence check after executable relaunch. No runtime exceptions were recorded.

- E pickup, visible held extinguisher, pin gating, P removal, held LMB spraying and sweep input passed. Fire intensity progressively fell from approximately 0.45 to zero; particle size, emission, smoke and light responded. All PASS steps completed, a mode-specific result appeared, and retry reset capacity/progress. Water contributed nothing in this mode.
- Evacuation mode restored sprinkler availability after extinguisher mode. Smoke detection preceded heat-triggered water; visible particles, audio and water suppression were verified. Continuous player movement passed through the working exit into the original assembly trigger and completed evacuation without extinguisher use.
- Native borderless fullscreen launched at this laptop's detected **1920×1200** resolution. Windowed 1280×720 preview, Cancel, Alt+Enter in both directions, Keep, and persistence after relaunch passed. The verification helper restored the preferences captured before testing.
- These are automated menu events and virtual Input System events through runtime gameplay, with a positioning fixture for extinguisher equipment and deterministic emergency threshold steps. They are not a claim of manual keyboard/mouse playtesting.

Actual standalone performance on Intel Iris Xe at native 1920×1200:

| Preset | Render scale | Extinguisher average FPS | Evacuation average FPS |
|---|---:|---:|---:|
| Low | 0.70 | 60.00 | 57.01 |
| Medium | 0.85 | 43.79 | 36.35 |
| High | 1.00 | 33.49 | 29.44 |

Each measurement used a fixed classroom viewpoint, 2.5-second warmup and eight-second sample. All sampled frames had window focus. Evacuation hazard state was held after 33 simulated seconds for comparable load. These short samples do not measure an entire outdoor walk-through or sustained thermal performance; High can fall below 30 FPS. Medium is the balanced starting preset.

The updated `TrainingScene.unity` is open in Unity in Edit Mode, saved with no dirty scene state and no compilation in progress. The Scene view is framed toward the courtyard facade. The six final exterior screenshots and `output/CampusExterior-SixScreenshots.zip` remain available for inspection.
