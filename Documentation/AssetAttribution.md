# Asset attribution and licensing

Verified from publisher sources on 12 October 2026. Acquisition manifest: `DownloadedAssets.json` (generated after downloads finish). Files under `Assets/ThirdParty` are third-party works; do not claim original authorship.

| Asset | Creator / source | License | Planned use |
|---|---|---|---|
| Classroom | Christophe Seux; https://www.blender.org/download/demo-files/ | CC0 (explicitly listed on Blender's official demo page) | Coherent furnished college environment |
| Classroom Unity mesh/texture conversion | lkewis; https://github.com/lkewis/HDRP-Classroom | Repository conversion is public-domain Unlicense; underlying Classroom is CC0 | FBX and textures only, rebuilt as URP materials |
| Korean Fire Extinguisher 01 | UM JOORIN; https://polyhaven.com/a/korean_fire_extinguisher_01 | CC0 | Interactive extinguisher, 2K textures |
| Fire Alarm | Slinc; https://polyhaven.com/a/fire_alarm | CC0 | Manual alarm pull station, 2K textures |

Poly Haven license: https://polyhaven.com/license. CC0 permits modification, use and redistribution, including commercial use; attribution is not required but is retained here and should appear in game credits. Asset access through its public API is permitted under https://polyhaven.com/our-api; clearly identify Poly Haven as the source.

Classroom conversion license: https://github.com/lkewis/HDRP-Classroom/blob/main/LICENSE. A local unmodified copy is stored in `SourceAssets/Classroom-conversion-LICENSE.txt`. Original classroom license was verified independently of GitHub. Import only the asset subtree, not third-party packages, HDRP configuration, demo scripts or baked reflection probes. The acquisition manifest pins the resolved Git tree revision for downloaded conversion files.

Alterations completed in Phase 1: 24 URP materials created and remapped; normal/color/data texture import configuration, 2K import caps, mipmaps and compression; source meshes assembled in an asset-review scene. Classroom mask R/G/A channels are used for metallic/AO/smoothness. Source image files remain unchanged. Equipment roughness-to-smoothness packing, final surface tuning, collision authoring, scene rearrangement, interactive component separation and English training overlays remain pending.

No paid Asset Store asset or account-bound download is required for this selection.

## Phase 3 additions — 16 October 2026

| Asset | Source / author | License and actual use |
|---|---|---|
| Particle Pack: flame_01, smoke_04, spark_01, circle_05 | Kenney Vleugels, https://kenney.nl/assets/particle-pack | CC0 1.0; four original 512px textures imported under Assets/ThirdParty/KenneyParticles, with the pack license. Used for flames, smoke, embers and water droplets. |
| Particle download mirror | https://github.com/Calinou/kenney-particle-pack | Mirror license checked separately at https://raw.githubusercontent.com/Calinou/kenney-particle-pack/master/LICENSE.txt; original author/publisher's CC0 independently verified. Only four textures and the license imported, no Godot code. |
| Liberation Sans | Included Unity TextMeshPro essentials | SIL Open Font License 1.1, bundled at Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt; used in the status HUD. |
| Detector housing, vents and LED; pendent sprinkler escutcheon, brass yoke, bulb and deflector; alarm sounder/beacon | Original project-authored mesh geometry | Created by BuildPhaseThree.cs and optimized by RefinePhaseThree.cs; no third-party model license. Not manufacturer-certified equipment representations. |
| Fire crackle, alarm signal, water spray loops | Original project-generated PCM recordings | Synthesized by BuildPhaseThree.cs; no third-party recording or attribution requirement. Alarm is a demonstration signal, not a certified acoustic appliance. |

Actual imported particle files, sizes, download URLs and SHA-256 hashes are recorded in Phase3ImportedAssets.json. CC0 redistribution and modification are allowed; voluntary Kenney attribution is retained. The original Kenney ZIP also downloaded successfully to SourceAssets/KenneyParticles after a slow request. Imported textures came from the verified mirror; the original ZIP is preserved as source reference. Runtime URP particle materials and import settings are project-authored; downloaded pixel data was not edited.

## Phase 4 additions — 16 October 2026

- Held body, safety pin, squeeze lever and nozzle meshes are derived from **Korean Fire Extinguisher 01 by UM JOORIN / Poly Haven**, CC0, https://polyhaven.com/a/korean_fire_extinguisher_01. Connected mesh parts were separated, the floor stand and stowed hose excluded from the held assembly, and original UVs/materials retained. The original downloaded FBX and textures remain unchanged. New derived mesh files are under `Assets/_FireSafety/Models/Extinguisher ...`.
- Deployed rubber hose geometry is original project-authored work. English training instructions appear in the existing Liberation Sans HUD; original Korean equipment labels remain visible.
- Extinguishing powder reuses Kenney's CC0 `smoke_04` texture with a project-authored material and particle configuration. No new third-party image download.
- `Extinguisher discharge - original.wav` and `Extinguisher pin - original.wav` are original synthesized PCM audio, generated by BuildPhaseFour.cs. No sampled third-party recording. The generated white UI pixel is original project content.

## Phase 5 additions — 17 October 2026

- **Hexagonal Concrete Paving**, Stephan Seeliger / Poly Haven: https://polyhaven.com/a/hexagonal_concrete_paving. Publisher license https://polyhaven.com/license verified before download: **CC0**, permits modification and redistribution. Powered by Poly Haven.
- Three actual 2K JPG maps (diffuse, OpenGL normal and roughness) are imported in `Assets/ThirdParty/PolyHavenPaving/`. Download URLs, byte sizes, SHA-256 and publisher MD5 verification are recorded in `Phase5ImportedAssets.json`; publisher metadata is in `SourceAssets/Phase5-Paving-files.json`. Original downloaded pixels remain unchanged. A derived metallic/smoothness PNG packs inverted roughness into alpha for URP; mipmaps and compressed imports are enabled.
- Courtyard paving geometry, perimeter masonry/railings, slatted benches, paired glazed exit doors, push bars, assembly sign and floor markings are original project-authored meshes. Existing CC0 Classroom wood, plaster and glass materials are reused. The Classroom source model and textures remain unchanged; only TrainingScene's original passage glazing instance is disabled to accommodate operable exit leaves in its two openings.
- Route smoke reuses Kenney's CC0 smoke texture; signs and evacuation HUD reuse Liberation Sans under the existing bundled OFL license.
- `Exit push bar - original.wav` and `Assembly arrival - original.wav` are original synthesized PCM audio. No third-party recordings were sampled.

## Phase 6 additions — 17 October 2026

- Menu, briefing, settings, pause and report layouts are original project-authored uGUI content. Typography reuses the bundled Liberation Sans font under the existing SIL Open Font License; no additional font or image download was required.
- Menu backgrounds show the existing licensed classroom/courtyard through the live camera. The existing third-party credits are included in the in-game Credits screen, with full URLs and alterations retained in this document.
- The short menu-click sound is an original synthesized sine tone generated by TrainingInterface at runtime, with no third-party recording.
- Low/Medium/High URP quality assets are project configuration derived from the existing compatible pipeline asset; shared third-party materials and textures remain unchanged.

## Phase 7 additions — 18 October 2026

- `Footstep 1 - original.wav`, `Footstep 2 - original.wav`, `Footstep 3 - original.wav` and `Extinguisher success - original.wav` are original synthesized mono PCM audio, generated by `AgentScripts/PolishPhaseSeven.cs`. No third-party recording was sampled and no new attribution is required.
- Particle quality presets reuse the existing licensed particle textures. Classroom geometry, original art, materials and lighting were preserved.

## School expansion — 18–19 October 2026

All listed third-party source files are actually present under `Assets/ThirdParty/SchoolExpansion/`. The original classroom art, textures, and equipment are reused. No paid or account-bound assets were introduced.

| Asset | Publisher/source | License | Actual integration |
|---|---|---|---|
| Animated Characters Pack | Quaternius, publisher upload: https://opengameart.org/content/animated-characters-pack ; https://quaternius.com/packs/ultimatedanimatedcharacter.html | CC0, explicitly stated on the publisher upload | Suit_Male teacher and four Casual character variants for ten students. Legacy idle/walk clips; project-authored URP material colors and scale normalization. Stylized background characters. |
| Furniture Kit | Kenney, https://kenney.nl/assets/furniture-kit | CC0; original License.txt retained | computerScreen, computerKeyboard, computerMouse FBX models, two complete desktop sets. URP materials and classroom placement are project-authored. |
| Quiver Tree 01 | Poly Haven, https://polyhaven.com/a/quiver_tree_01 | CC0 | Four ornamental courtyard trees, 1K maps; imported tree geometry is 150,124 triangles per instance. Shared mesh/materials with distance culling. |
| Shrub 01 | Poly Haven, https://polyhaven.com/a/shrub_01 | CC0 | Sixteen instances of the low-cost f variant, 2,676 triangles each. Derived RGBA texture combines original diffuse and alpha maps; original maps preserved. |
| Grass Bermuda 01 | Poly Haven, https://polyhaven.com/a/grass_bermuda_01 | CC0 | Ninety-six small medium_a grass tufts; shared source mesh/material and distance culling. |
| Leafy Grass | Charlotte Baglioni / Poly Haven, https://polyhaven.com/a/leafy_grass | CC0 | Three checksum-verified 1K maps downloaded; diffuse and normal maps used on planted courtyard beds. |

Poly Haven license and redistribution permission: https://polyhaven.com/license. CC0 permits copying, modification and redistribution; attribution is retained voluntarily. Character licensing was verified against the creator's own OpenGameArt upload, independently of other mirrors. Kenney's original furniture pack license is included alongside its imported models.

Source metadata, download manifests, archive listings and hashes are retained in `SourceAssets/SchoolExpansion/`. `LeafyGrassManifest.json` records the additional ground maps. The initially downloaded `tree_small_02` is **not used by the scene**, because its roughly two-million-triangle mesh exceeded the intended background-prop budget. It remains an unused source reference, not a runtime dependency.

The environment-only classroom prefab reuses the original licensed art. A separate derived hallway wall mesh removes passage end caps; the original imported mesh is unchanged. New corridor connections, fascia, courtyard boundaries, garden borders, signs, lighting placement and assembly markings are project-authored. These additions reuse existing licensed plaster, paving and metal materials where applicable. Liberation Sans remains under its bundled OFL license.

## Campus exterior upgrade — 19 October 2026

The TrainingScene now uses the following additional licensed content. These entries supersede the earlier note that tree_small_02 was unused. The previously removed computer in front of the board remains removed; one computer set remains on the original teacher desk.

| Asset | Creator and source | License | Actual use and modifications |
|---|---|---|---|
| 3D Bus | ajanhallinta — https://opengameart.org/content/3d-bus | CC0 | BUS.fbx imported under Assets/ThirdParty/CampusExterior/Bus. Two stationary coaches, normalized to 10.5 m, yellow URP materials and text-only university lettering. Original README retained. No official university logo. |
| Plastered Wall 02 | Charlotte Baglioni / Poly Haven — https://polyhaven.com/a/plastered_wall_02 | CC0 | 2K diffuse, OpenGL normal and roughness maps imported. Diffuse/normal used on cream and terracotta facade materials; smoothness is artist-set. |
| Asphalt 02 | Rob Tuytel / Poly Haven — https://polyhaven.com/a/asphalt_02 | CC0 | 1K diffuse, normal and roughness maps imported. Diffuse/normal used on parking and entrance paving. |
| Floor Tiles 04 | Rob Tuytel / Poly Haven — https://polyhaven.com/a/floor_tiles_04 | CC0 | 2K diffuse, normal and roughness maps imported. Diffuse/normal used on corridor flooring. |
| Tree Small 02 | Rico Cilliers / Poly Haven — https://polyhaven.com/a/tree_small_02 | CC0 | Existing downloaded model now used as four leafy trees. Derived LOD meshes simplify wood geometry and sample complete leaf islands, retaining source leaf UVs. Leaves enlarged about island centres to retain canopy coverage. LOD0 246,377 triangles, LOD1 110,753 versus original 2,062,487. Source FBX/maps unchanged. Derived RGBA packs source opacity into base-map alpha. |
| Shrub 01 | Poly Haven — https://polyhaven.com/a/shrub_01 | CC0 | Existing source reused for facade planters and courtyard planting. Additional instances and variant choices reuse existing compatible URP materials and maps. |

Poly Haven's license is https://polyhaven.com/license. CC0 permits reuse, modification and redistribution; these credits are retained voluntarily. License/source pages, API metadata, ZIP archives and download hash manifests are saved under SourceAssets/CampusExterior. New imports are actually present under Assets/ThirdParty/CampusExterior. Trees, shrub and grass originals remain under Assets/ThirdParty/SchoolExpansion.

Architecture, railing/grille assemblies, wall caps, lamp housings, window frames, bus parking markings and sign layouts are project-authored. Derived classroom wall meshes introduce real window openings without changing source FBX files. Original interior textures remain intact. Liberation Sans lettering retains its existing SIL Open Font License.

The authoring-only mesh reduction tool fast-simplification 0.2.0 (https://github.com/pyvista/fast-simplification) is MIT licensed. Installed locally in Tools/CampusMeshTools with its license metadata; it is not included in Assets or the game runtime. It was used for mesh processing, not image generation. All screenshots are actual Unity camera captures.

## Campus recreation upgrade — 20 October 2026

| Asset | Publisher and exact source | License | Integration and alterations |
|---|---|---|---|
| Wall basketball hoop sports equipment | Innerscene — https://www.innerscene.com/tools/library/3d-parts/wall-basketball-hoop-sports-equipment-7cfb56b9 | CC0, as stated on the publisher's asset page | Two instances; 6,592 triangles each. Original GLB retained. Meshes converted to Unity coordinates and grouped by material; transparent URP glass. Project-authored ground supports and safety padding. |
| Volleyball net sports equipment | Innerscene — https://www.innerscene.com/tools/library/3d-parts/volleyball-net-sports-equipment-3ddaf116 | CC0, as stated on the publisher's asset page | One instance; 3,812 triangles. Original GLB retained. Width scaled to 77.5% to suit the 7 m practice court; original vertical dimensions retained. Project-authored post padding. |

Source GLBs, original material textures and conversion intermediates exist under Assets/ThirdParty/CampusSports. Source links, licensing records, hashes and geometry counts are in SourceAssets/CampusSports/ImportedAssets.json. Derived Unity mesh/material/prefab assets are under Assets/_FireSafety/CampusSports. CC0 permits copying, modification and redistribution; credit is retained voluntarily.

The court surfaces and markings, padded supports, simple backstop grille, pergola, bicycle stands and bins are project-authored. They reuse existing licensed asphalt, stone, wood and metal materials. Existing benches, trees and garden plants are reused. No new runtime package, sports gameplay or ball physics was introduced. Court sizes are intentionally reduced practice layouts: basketball 22 x 12 m; volleyball 14 x 7 m. They are not regulation competition courts.
