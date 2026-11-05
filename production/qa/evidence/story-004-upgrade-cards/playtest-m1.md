# Story 004 — Playtest mốc 1 (màn chọn thẻ + 3 thẻ Thường)

**Story**: `production/epics/micro-arena-survivor/story-004-upgrade-cards.md`
**Date**: 2026-11-05
**Tester**: project owner (manual play)
**Build**: macOS release build, Arena scene

## Result: PASS

Tester's report: "playtest xong rồi, ổn hết". No issues reported.

Scope played: card-pick screen after waves (keys 1/2/3 and mouse), Sức Mạnh Thô, Giày Bão, Huyết Nhũ,
pause during the pick (including hit-stop ending while the screen is open), modifier reset on restart.

## Screenshot

- `production/qa/evidence/story-004-upgrade-cards/01-card-pick.png`

## Decisions taken for milestone 2 (same day)

- Cuồng Nộ Hư Vô: +100% damage and +50 percentage points crit apply only while HP < 30%.
- Repeated Rare/Legendary picks add their numbers; counts (3 chain targets, 3 burn stacks) stay fixed.
- Placeholders accepted: Lôi Chấn chains to the 3 nearest others within 4 m of the struck target; Sao Băng lands
  0.4 s after the dash, 3 m radius; Hỏa Ngục up to 3 burn stacks per enemy, each slash adds one and refreshes 3 s;
  Phân Thân decoy draws enemies for 1.5 s and takes no damage.
