# Story 005: Soul Burst (thanh nộ + phím Q)

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

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

- `Assets/Scripts/Gameplay/Player/SoulBurst.cs`
- `Assets/Scripts/Gameplay/Config/SoulBurstConfig.cs`
- `Assets/Tests/EditMode/SoulBurst/SoulBurstTests.cs`

---

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
