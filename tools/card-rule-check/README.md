# Card rule check (story 004)

`CardRuleCheck.cs` is a batchmode rule check for the upgrade cards: modifier stacking and reset, the Vòng Xoáy rule
(swing radius +30%, detection stays 3 m), Cuồng Nộ below 30% HP, crit 0.05 + 0.50 = 0.55, burn stacks, chain targets,
the card draw, `-arenaForceCards` parsing and selection, and `GamePause`.

It lives here, outside `Assets/`, so Unity never compiles it and it is never part of a build. The file is used
unchanged when copied back (namespace `MicroArena.Gameplay.Editor`; `Assets/Scripts/Gameplay/Editor/` compiles into
Assembly-CSharp-Editor, which already sees the game code).

Close the Unity editor first (batchmode cannot open a project that is already open). From the repo root:

```sh
# 1. Copy it into the editor folder
cp tools/card-rule-check/CardRuleCheck.cs Assets/Scripts/Gameplay/Editor/CardRuleCheck.cs

# 2. Run it (exit code 0 = all PASS, 1 = at least one FAIL)
"/Applications/Unity/Hub/Editor/6000.6.3f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit \
  -projectPath "$PWD" -logFile card-rule-check.log \
  -executeMethod MicroArena.Gameplay.Editor.CardRuleCheck.Run
echo "exit $?"; grep -E "^(PASS|FAIL) |CardRuleCheck:" card-rule-check.log

# 3. Remove the copy AND the .meta Unity generated for it, plus the log
rm -f Assets/Scripts/Gameplay/Editor/CardRuleCheck.cs Assets/Scripts/Gameplay/Editor/CardRuleCheck.cs.meta card-rule-check.log
git status --short Assets/Scripts/Gameplay/Editor/   # must not list CardRuleCheck
```

When you change the check, edit the copy in this folder (or copy your edited version back here before step 3).
