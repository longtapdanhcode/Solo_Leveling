# Story 003 — Playtest Wave 3–4

**Story**: `production/epics/micro-arena-survivor/story-003-arena-waves.md`
**Date**: 2026-11-04
**Tester**: project owner (manual play)
**Build**: macOS development build (Debug.isDebugBuild), Arena scene
**Question from the story**: can Kaelen reach a Void Caster (it keeps 8 m away; Kaelen's auto-attack reaches 3 m)?

## Result — Wave 3–4: PASS

- **Yes, Dash reaches the Void Caster.** The tester used Dash to close the gap.
- After all the Crawlers were dead, the Void Caster could be **pushed back into the wall** as normal and killed. It does not stay out of reach forever.
- The wave **did not get stuck**: the kill count reached its target and the wave ended.
- The run went into the **temporary card-pick step** (auto-continue until story 004), then into **Wave 4**.

## Other things seen in the same session (tester's report)

| Item | Result |
|---|---|
| Revive, then final death (launched with `-arenaStartWave 3`) | Pass: after the final death the run restarted at Wave 3, as the debug argument asks, with the revive reset correctly |
| Restart without arguments | Pass: back to Wave 1 with player state reset correctly |
| Wave 5 (Mini-Boss stand-in + 4 escorts) | Pass |
| Wave 6–7 (survive 60 s or kill 40) | Pass |
| Wave 8–9 (kill 50, rotating trap) | Pass |
| Wave 10 (Final Boss stand-in) | Pass |

## Screenshot

- `production/qa/evidence/story-003-waves.png` (2026-11-04 17:34, development build): the overlay reads `Wave 4/10 · Playing · Kill`, `Remaining 25/25`, `Last event: Wave 4 started`.
