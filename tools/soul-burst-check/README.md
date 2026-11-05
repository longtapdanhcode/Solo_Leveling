# Soul Burst rule check (story 005)

`SoulBurstCheck.cs` is a batchmode rule check for the Soul Burst: the rage model (0 at start, +5 per kill, cap 100,
reset), `SoulBurstConfig` defaults (5, 100, 120, 6, 4), Q gating (below 100, card pick / `GamePause` pause, Defeat,
Victory, Kaelen dead: nothing happens and the rage is kept; hit-stop alone does not block), and an edit-mode physics part
that builds Kaelen (real `Health`), real `Crawler` enemies and a boss (via the real `WaveDirector.IsBoss`) and fires
`SoulBurst.TryActivate()`: enemies inside 6 m take exactly 120, the one at 7 m none, Kaelen's HP is unchanged, others are
pushed 4 m outward, the boss 0 m.

It lives here, outside `Assets/`, so Unity never compiles it and it is never part of a build. The file is used
unchanged when copied back (namespace `MicroArena.Gameplay.Editor`; `Assets/Scripts/Gameplay/Editor/` compiles into
Assembly-CSharp-Editor, which already sees the game code).

Close the Unity editor first (batchmode cannot open a project that is already open). From the repo root:

```sh
# 1. Copy it into the editor folder
cp tools/soul-burst-check/SoulBurstCheck.cs Assets/Scripts/Gameplay/Editor/SoulBurstCheck.cs

# 2. Run it (exit code 0 = all PASS, 1 = at least one FAIL)
"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit \
  -projectPath "$PWD" -logFile soul-burst-check.log \
  -executeMethod MicroArena.Gameplay.Editor.SoulBurstCheck.Run
echo "exit $?"; grep -E "^(PASS|FAIL) |SoulBurstCheck:" soul-burst-check.log

# 3. Remove the copy AND the .meta Unity generated for it, plus the log
rm -f Assets/Scripts/Gameplay/Editor/SoulBurstCheck.cs Assets/Scripts/Gameplay/Editor/SoulBurstCheck.cs.meta soul-burst-check.log
git status --short Assets/Scripts/Gameplay/Editor/   # must not list SoulBurstCheck
```

When you change the check, edit the copy in this folder (or copy your edited version back here before step 3).
