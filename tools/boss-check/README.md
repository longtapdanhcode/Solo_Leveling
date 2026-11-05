# Boss rule check (story 006)

`BossCheck.cs` is a batchmode rule check for the two bosses:

- `BossConfig` defaults (600 / 2000 HP, slam 25 DMG r 4 m 1 s, slash 30 x3, burst 40 r 4 m, meteors 20 DMG r 2 m,
  5 every 8 s, phase 2 at 50%, minimum telegraph 1 s, 3 s cooldown) and the generated `Assets/Data/BossConfig.asset`;
- telegraphs: the 1 s minimum is enforced in code, circle and fan containment (edge inclusive), the drawn mesh rim
  matches the hit-test shape, damage only to Kaelen inside the area **at resolve time**, nothing before it resolves;
- Gargoyle brain (slam, 25 DMG, 3 s cooldown), Void Warden brain (3 fans x 30, the decoy never moves a placed fan, pull
  wind-up >= 1 s with no pull, bounded pull toward the centre, dash outpaces it, burst 40 at the pull centre), phase 2
  latches once strictly below 50% (exactly 50% stays phase 1), meteors only in phase 2 (5 per 8 s), `Stop()` clears
  everything and nothing deals damage afterwards; no telegraph progress while `GamePause` holds a pause;
- components: real `GargoyleTyrant` / `VoidWarden` take damage from combo, Soul Burst, meteor-card and revive-shock packets
  but are never stunned or pushed; the Void Warden's death and a boss being destroyed (run reset / despawn) remove every
  telegraph and its drawing;
- waves (real `WaveConfig.asset`, real `WaveDirector.HandleEnemyDied`): Wave 5 = Gargoyle + 4 Crawler escorts, Wave 10 =
  Void Warden; Wave 5 does not end while an escort is alive after the Gargoyle died and ends with the last one (or with
  the Gargoyle, whichever dies last); the Void Warden's death gives Victory; `IsBoss` still identifies the boss.

It lives here, outside `Assets/`, so Unity never compiles it and it is never part of a build. The file is used
unchanged when copied back (namespace `MicroArena.Gameplay.Editor`; `Assets/Scripts/Gameplay/Editor/` compiles into
Assembly-CSharp-Editor, which already sees the game code). It reads the assets made by `ArenaSceneBuilder.Build`, so
run the scene builder first after deleting any of them.

Close the Unity editor first (batchmode cannot open a project that is already open). From the repo root:

```sh
# 1. Copy it into the editor folder
cp tools/boss-check/BossCheck.cs Assets/Scripts/Gameplay/Editor/BossCheck.cs

# 2. Run it (exit code 0 = all PASS, 1 = at least one FAIL)
"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit \
  -projectPath "$PWD" -logFile boss-check.log \
  -executeMethod MicroArena.Gameplay.Editor.BossCheck.Run
echo "exit $?"; grep -E "^(PASS|FAIL) |BossCheck:" boss-check.log

# 3. Remove the copy AND the .meta Unity generated for it, plus the log
rm -f Assets/Scripts/Gameplay/Editor/BossCheck.cs Assets/Scripts/Gameplay/Editor/BossCheck.cs.meta boss-check.log
git status --short Assets/Scripts/Gameplay/Editor/   # must not list BossCheck
```

When you change the check, edit the copy in this folder (or copy your edited version back here before step 3).
