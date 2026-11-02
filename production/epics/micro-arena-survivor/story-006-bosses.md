# Story 006: Hai boss (Gargoyle Bạo Quân, Void Warden)

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 6

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 4, 5.4 và "Giá trị ban đầu đã chốt":*

- [ ] **Gargoyle Bạo Quân** (Wave 5, kèm 4 quái phụ): 600 HP; đòn đập búa diện rộng 25 DMG, vòng tròn đỏ bán kính 4m hiện trước 1s rồi mới gây sát thương.
- [ ] Hạ Gargoyle (và hết quái phụ theo điều kiện wave) kết thúc Wave 5.
- [ ] **Void Warden** (Wave 10): 2000 HP, 2 giai đoạn. Giai đoạn 1: tam đoạn trảm diện rộng (vệt quạt đỏ báo trước) và hút người chơi vào tâm rồi đâm kiếm nổ đất (báo trước). Giai đoạn 2 khi HP dưới 50%: thêm mưa thiên thạch ngẫu nhiên (vòng đỏ báo trước mỗi điểm rơi).
- [ ] Mọi đòn nguy hiểm đều có telegraph (vòng tròn hoặc vệt quạt đỏ) trên sàn trước khi gây sát thương; sát thương chỉ tính trong vùng telegraph.
- [ ] Hạ Void Warden thì phát sự kiện thắng lượt.
- [ ] Chỉ số boss, thời gian báo trước, sát thương, ngưỡng giai đoạn nằm trong ScriptableObject (`BossConfig`); không hardcode.
- [ ] Chạy game gặp từng boss; giữ ảnh chụp trong `production/qa/evidence/story-006-bosses.png` (có telegraph hiển thị).

---

## Implementation Notes

- Telegraph tối thiểu (vòng tròn/vệt quạt đỏ) làm ở story này vì boss không chơi được nếu thiếu; story 007 chỉ tinh chỉnh hình ảnh.
- Ngưỡng 50% của Void Warden lấy từ kịch bản mục 5.4; mục 4 chỉ ghi "2 giai đoạn".
- `[PLACEHOLDER]` (chưa quy định): sát thương và cooldown các chiêu Void Warden, bán kính nổ đất, số thiên thạch/nhịp rơi, loại 4 quái phụ Wave 5, boss có miễn nhiễm choáng/đẩy lùi hay không.

---

## Out of Scope

- Story 007: HUD thanh máu boss (nếu có), hiệu ứng screen shake.

---

## Files

- `Assets/Scripts/Bosses/GargoyleTyrant.cs`
- `Assets/Scripts/Bosses/VoidWarden.cs`
- `Assets/Scripts/Bosses/Telegraph.cs`
- `Assets/Scripts/Config/BossConfig.cs`
- `Assets/Tests/PlayMode/Bosses/BossTests.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `Assets/Tests/PlayMode/Bosses/` OR playtest doc; ảnh chụp trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `story-001-kaelen-core-combat.md`, `story-002-enemies.md`, `story-003-arena-waves.md`
- Unlocks: `story-007-hud-game-feel.md`
