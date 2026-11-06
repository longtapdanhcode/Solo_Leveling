# HUD + game feel rule check (story 007)

`HudFeelCheck.cs` is a batchmode rule check for story 007:

- **Assets and scene**
  - The Be Vietnam Pro Regular/Bold TMP font assets:
    - every glyph of `ẮẰẲẴẶắằẳẵặĐđƠơƯưỆệ` and of the whole builder set is already present (`HasCharacters`, no add);
    - the assets are Dynamic, keep their source `.ttf` for player builds, have `ClearDynamicDataOnBuild` off, and store the atlas and material as sub-assets;
    - the material uses a `TextMeshPro/` shader.
  - TMP Essential Resources are imported, and `FeelConfig.asset` exists.
  - In `Arena.unity`, the HUD, end screens, camera `ScreenShake` and card pick are present and wired to the fonts and `FeelConfig`.
  - There is no legacy `UnityEngine.UI.Text`, and `WaveDirector` has no auto-restart field.
- **Pure logic**
  - `ShakeModel`: linear fade, a weaker shake does not cut a stronger one short, `Reset` zeroes the offset.
  - `WaveDebugOverlay` is gated on `Debug.isDebugBuild` and drawn bottom-left.
  - `BossConfig` telegraph colours (red for damage, purple for the 0-damage warning). Each `TelegraphView` of a real Void Warden uses those colours: slash fans red, pull wind-up purple.
- **HUD (real Kaelen prefab, `WaveDirector`, `SoulBurst`; `Awake`/`OnEnable` invoked by reflection)**
  - HP bar and label, including the low-HP colour from `FeelConfig`.
  - Dash pips bright/dim, and a pip brightening on recharge.
  - Rage label and the Q-ready text.
  - The revive flash and message.
  - `WAVE xx/10` and the remaining bar, including Wave 5:
    - boss plus escorts, escorts not yet spawned counted while the Gargoyle lives;
    - 4/5 (not 0/1) when the Gargoyle dies with 4 escorts alive;
    - 0/5 when the wave ends.
  - The boss bar shows on Waves 5 and 10, follows hits, and hides on death or despawn.
  - The HUD shows event values rather than source values, and its runtime fields are UI objects only.
- **Screen shake**
  - Triggers only on a 3rd slash that hits and on a husk's `OnExploded`, at the `FeelConfig` amplitudes.
  - Moves only the camera; Kaelen is never moved.
  - Stops on a `GamePause` pause and on a run reset.
- **End screens**
  - The Void Warden's death schedules victory. Revive spent plus lethal damage gives defeat, with no auto-restart.
  - Each screen pauses the game. Clicking the button, or `Replay()` (what Enter/R call), at `timeScale 0` restarts at Wave 1 and releases the pause.
  - The replay keys exclude Space (Dash).
  - Every runtime TMP text (HUD, end screens, a card pick showing all 9 cards) uses Be Vietnam Pro with all its glyphs present.

Limitation: edit mode routes input to editor updates, so `wasPressedThisFrame` never fires there. The check confirms in the source that `Update` maps the replay keys to `Replay()`, then calls `Replay()` itself. The real key press is seen in a player build.

It lives here, outside `Assets/`, so Unity never compiles it and it is never part of a build. It is used unchanged when copied back
(namespace `MicroArena.Gameplay.Editor`). Run `ArenaSceneBuilder.Build` first if any generated asset was deleted.

Close the Unity editor first. From the repo root:

```sh
cp tools/hud-feel-check/HudFeelCheck.cs Assets/Scripts/Gameplay/Editor/HudFeelCheck.cs
"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit \
  -projectPath "$PWD" -logFile hud-feel-check.log \
  -executeMethod MicroArena.Gameplay.Editor.HudFeelCheck.Run
echo "exit $?"; grep -E "^(PASS|FAIL) |^HudFeelCheck: " hud-feel-check.log
rm -f Assets/Scripts/Gameplay/Editor/HudFeelCheck.cs Assets/Scripts/Gameplay/Editor/HudFeelCheck.cs.meta hud-feel-check.log
git status --short Assets/Scripts/Gameplay/Editor/   # must not list HudFeelCheck
```
