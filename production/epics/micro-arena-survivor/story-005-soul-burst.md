# Story 005: Soul Burst (thanh nộ + phím Q)

> **Epic**: micro-arena-survivor
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-05

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 5

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 3.2 và "Giá trị ban đầu đã chốt":*

- [ ] Thanh nộ chạy 0–100%; +5% mỗi quái diệt (theo sự kiện `EnemyDied`), giữ qua các wave, reset khi lượt kết thúc.
- [ ] Phím Q khi đủ 100%: dậm kiếm tạo sóng xung kích diện rộng, 120 DMG, hất văng quái xung quanh; thanh nộ về 0.
- [ ] Phím Q khi chưa đủ 100%: không có tác dụng.
- [ ] Sát thương đi qua đường xử lý sát thương chung của story 001.
- [ ] Giá trị (tốc độ tích nộ, 120 DMG, bán kính, lực hất) nằm trong ScriptableObject (`SoulBurstConfig`); không hardcode.
- [ ] Chạy game, tích đủ nộ và tung Q; giữ ảnh chụp trong `production/qa/evidence/story-005-soulburst.png`.

---

## Implementation Notes

- `[PLACEHOLDER]` (chưa quy định): bán kính sóng xung kích, lực hất văng, hiệu ứng lên boss (mặc định: nhận sát thương, không bị hất).
- Phát sự kiện thay đổi thanh nộ để HUD dùng (story 007).

---

## Out of Scope

- Story 007: hiển thị thanh nộ và hiệu ứng hình ảnh.

---

## Files

- `Assets/Scripts/Gameplay/Config/SoulBurstConfig.cs` — created (rage/kill 5, max 100, 120 DMG, radius 6 m, knockback 4 m, ring look)
- `Assets/Scripts/Gameplay/Player/SoulBurstModel.cs` — created (plain C# rage model)
- `Assets/Scripts/Gameplay/Player/SoulBurst.cs` — created (Q, gating, shockwave via IDamageable, boss no-knockback, ring, debug `-arenaAutoSoulBurst`)
- `Assets/Scripts/Gameplay/Waves/WaveDirector.cs` — modified (`IsBoss`)
- `Assets/Scripts/Gameplay/Waves/WaveDebugOverlay.cs` — modified (rage line, verification scaffolding)
- `Assets/Scripts/Gameplay/Editor/ArenaSceneBuilder.cs` — modified (SoulBurstConfig asset, ring material, SoulBurst on Kaelen)
- `Assets/Data/SoulBurstConfig.asset`, `Assets/Materials/SoulBurstRing.mat` — created
- `Assets/Scenes/Arena.unity` — regenerated
- `tools/soul-burst-check/SoulBurstCheck.cs`, `README.md` — created (rule check kept outside Assets/; copy in to run)
- No EditMode test (waived at qa.level minimal)

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `Assets/Tests/EditMode/SoulBurst/` (advisory ở `qa.level: minimal`); ảnh chụp trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `story-001-kaelen-core-combat.md`, `story-002-enemies.md`
- Unlocks: `story-007-hud-game-feel.md`

---

## Completion Notes

**Completed**: 2026-11-05
**Criteria**: 6/6 covered by automated checks; in-play half NOT VERIFIED (real Q key, boss at wave 5/10, Husk self-explosion in play, pause/Defeat/Victory, rage across waves) — listed in `production/qa/final-playtest-checklist.md` rows 5.1–5.12 for the owner's final playtest. Verdict was NOT ASSESSED; owner chose to close anyway.
**Deviations**:
- ADVISORY: Test Evidence section above still names the EditMode folder; tests are waived at qa.level minimal (rule check in `tools/soul-burst-check/`, 43/43 PASS).
- ADVISORY: `MakeTransparent` in `ArenaSceneBuilder` re-forces transparent on rebuild; ring material does not set alpha-channel blend; ring pauses during hit-stop.
- Decisions: radius 6 m, knockback 4 m, 120 flat DMG (no modifiers/crit), boss takes damage but no knockback, Q inactive while paused / not Playing / Kaelen dead, Kaelen unharmed; debug-only `-arenaAutoSoulBurst <s>` to capture the shockwave; rage line in `WaveDebugOverlay` (real HUD is story 007).
**Test Evidence**: waived at qa.level minimal; screenshots in `production/qa/evidence/story-005-soul-burst/` and `production/qa/evidence/story-005-soulburst.png`
**Reviewed by**: self, no independent review
