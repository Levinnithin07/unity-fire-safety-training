# Phase 2 — playable environment inspection checkpoint

Completed 12 October 2026. Development stops here for user inspection.

## Exact scene

Unity asset path: `Assets/_FireSafety/Scenes/TrainingScene.unity`

Absolute path: `C:/Users/levin/Desktop/ARVR GPT/FireSafetyTraining/Assets/_FireSafety/Scenes/TrainingScene.unity`

Unity 6000.6.3f1 is open with TrainingScene active, saved and in Edit Mode. Press Play and click the Game view to explore. The Game view is set to 1920 x 1080. No manual repair or import step is required.

## Implemented

- Created a separate scene through Unity's live editor bridge, using the approved classroom model and its existing materials/textures.
- SHA-256 comparison confirms all 204 tracked files (AssetReview, third-party source files/metadata and shared imported material files/metadata) remain unchanged.
- Warmer, fixture-positioned interior lighting; cooler passage fills; daylight with a single shadowed light; 128px reflection probe updated on scene startup.
- Input System first-person CharacterController: gravity, grounded movement, normalized diagonal input, mouse-look with pitch limits, sprint, smooth camera height when crouching and overhead clearance before standing.
- 237 colliders, including static mesh collision for the classroom's walls, floor, furnishings, door leaves and windows; an invisible floor support volume; equipment collision and the player capsule.
- Player starts inside the classroom at `(0, 0.08, -1.95)` and settles to approximately y=0.025 due to controller skin width.
- Both door leaves opened further as scene overrides, with physical collision retained. Both openings lead to the existing adjoining passage and are traversable in both directions.
- Extinguisher stands upright beside the passage wall near a doorway; manual alarm pull station is mounted facing into the passage. Interaction is deliberately deferred.
- Restored one missing ceiling vent material assignment in TrainingScene only. AssetReview was not altered.
- Standalone settings: FullScreenWindow, native-resolution default, resizable window and 1920x1080 fallback. Runtime display component supports saved fullscreen/windowed preference and Alt+Enter in standalone. Editor Play Mode does not change the monitor's display mode.
- Separate runtime URP Medium profile: native render scale, 2x MSAA, 22m shadow distance, 2048 shadow map, two cascades, four additional lights per object, no additional-light shadows, SRP Batcher. Source art retains existing compressed 2K imports and mipmaps. Static objects are marked for batching.

## Controls

| Control | Action |
|---|---|
| W / A / S / D | Walk |
| Mouse | Look |
| Left Shift (hold) | Sprint |
| Left Ctrl (hold) | Crouch |
| Esc | Release cursor and suspend movement input |
| Left mouse button | Recapture cursor |
| Alt + Enter | Fullscreen/windowed toggle in the eventual standalone build; not active in the Editor |

Walk speed is 2.2m/s, sprint 3.5m/s and crouch 1.2m/s. Mouse sensitivity is exposed on Desktop Player in the Inspector; a later settings menu can call the provided persistent sensitivity setter. There is no pause/settings HUD in this phase.

## Verification performed

Live Unity bridge was used for scene construction, component assignment, compilation, Play Mode, camera capture and Console inspection.

All **24 automated Play Mode checks passed**. Evidence is saved in `Phase2PlayModeTests.json` and `Phase2TestExecution.json`:

- Ground contact and floor stability under gravity.
- Walk/sprint/crouch displacement: 0.660m / 1.050m / 0.360m over 0.3 simulated seconds.
- Obstruction/deflection by a student desk, obstruction by a teacher chair, front wall, window wall and passage glazing.
- Traversal from the classroom to passage through both front and rear doorways, and return through each doorway.
- Inability to stand under an overhead test obstruction; automatic return to standing once it is removed. The temporary obstruction is removed after the check.
- Real Input System bindings for all four movement keys, Ctrl crouch, Shift sprint, mouse-look, Esc release and click recapture. Temporary virtual keyboard/mouse devices were removed after testing. Background input routing was temporarily enabled only in the harness and restored afterward. The checks verified that Play Mode frames advanced.
- Supported, assigned materials on all renderers.

Final scene audit: zero missing materials, zero unsupported shaders and zero missing scripts; upright extinguisher height approximately 0.659m; correct build scene and fullscreen configuration. See `Phase2SceneAudit.json`.

Actual Unity renders were inspected at the player start and in the equipment passage. Final images are `Assets/Screenshots/Phase2-Training-1080p.png` and `Assets/Screenshots/Phase2-Equipment.png`. These are Unity camera captures from Play Mode, not generated mockups.

## Performance measurement

GPU reported by Unity: **Intel(R) Iris(R) Xe Graphics**.

12-second stationary **1920x1080 Unity Editor Game view** sample: 478 sampled frames, average sampled rate **39.87 FPS**, median frame time **24.88ms**, 95th percentile **28.98ms**. This is an editor-only stationary sample, not an executable benchmark or a guarantee for all viewpoints or future emergency effects. Original data: `Phase2EditorPerformance.json`.

An earlier small Game view (762x402) sample reached about 60 FPS; it is kept separately and is not used as the 1080p performance claim.

## Remaining limits and issues

- No standalone build was made in Phase 2. Native fullscreen launch, Alt+Enter, resolution switching and display preference persistence need actual Windows executable testing later. Their configuration and code exist, but they are not claimed as runtime-verified.
- Both classroom exits currently reach the adjoining passage. An exterior assembly area, hazard-aware route guidance and evacuation logic remain future work.
- Doors are intentionally fixed open for this exploration checkpoint; opening/closing interaction is not implemented.
- Equipment is positioned but cannot be picked up or activated yet. Fire, PASS, sprinklers, alarms, scoring and evacuation systems remain unimplemented, as requested.
- No lighting bake or occlusion bake was performed. Later effects may require additional GPU optimization. The environment remains around 624k source triangles.
- Phase 1's Windows Burst JIT policy warning remains recorded in the earlier report; no OS security configuration was changed. Phase 2 Console evidence is saved separately.

No known blocking movement or scene-rendering issue remains in the tested paths. This is the requested environment inspection checkpoint, not a claim that the complete training simulation is finished.
