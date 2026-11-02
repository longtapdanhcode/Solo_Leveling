# Story 002: Ba loại quái (Crawler, Void Caster, Volatile Husk)

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 3

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Xem `docs/engine-reference/unity/` mục `6.3 → 6.6`.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 5.1–5.3:*

- [ ] **Crawler**: 40 HP, 10 DMG, tốc độ trung bình; chạy thẳng đến người chơi, khựng 0.3s lấy đà rồi cắn.
- [ ] **Void Caster**: 60 HP, 15 DMG, tốc độ chậm; giữ khoảng cách ~8m, bắn đạn năng lượng bay thẳng mỗi 2.5s; đạn bay theo hướng chốt lúc bắn nên né được.
- [ ] **Volatile Husk**: 25 HP, 50 DMG nổ AOE, tốc độ rất nhanh; cách người chơi 2m thì phát sáng đỏ nhấp nháy 0.8s rồi tự nổ.
- [ ] Cả ba nhận sát thương, đẩy lùi, choáng từ đường xử lý chung của story 001 và gây sát thương cho Kaelen qua đường đó.
- [ ] Khi chết phát sự kiện `EnemyDied` (loại quái, vị trí) cho wave, nộ, thẻ dùng.
- [ ] Mọi chỉ số nằm trong ScriptableObject (`EnemyConfig` mỗi loại); không hardcode.
- [ ] Chạy game, quan sát từng loại quái hành động; giữ ảnh chụp trong `production/qa/evidence/story-002-enemies.png`.

---

## Implementation Notes

- Auto-attack chỉ với 3m, Void Caster đứng ở ~8m: người chơi phải tự Dash để tiếp cận. **Kiểm tra lại khi playtest Wave 3–4**; không thêm gap-closer tự động.
- `[PLACEHOLDER]` (chưa quy định): tốc độ chạy (trung bình/chậm/rất nhanh), tốc độ đạn, bán kính nổ Husk, Husk bị giết trong lúc nhấp nháy có nổ hay không (mặc định: không nổ), khoảng giữ của Crawler lúc cắn.
- Dùng NavMesh hoặc steering đơn giản; tránh lấy đối tượng bằng `FindObjectOfType` mỗi frame.

---

## Out of Scope

- Story 003: spawn và điều phối wave.
- Story 006: Gargoyle Bạo Quân, Void Warden.

---

## Files

- `Assets/Scripts/Enemies/Crawler.cs`
- `Assets/Scripts/Enemies/VoidCaster.cs`
- `Assets/Scripts/Enemies/VolatileHusk.cs`
- `Assets/Scripts/Enemies/EnemyBase.cs`
- `Assets/Scripts/Config/EnemyConfig.cs`
- `Assets/Tests/EditMode/Enemies/EnemyBehaviourTests.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- `Assets/Tests/EditMode/Enemies/` (advisory ở `qa.level: minimal`)
- Ảnh chụp giữ lại trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/micro-arena-survivor/story-001-kaelen-core-combat.md`
- Unlocks: `story-003-arena-waves.md`, `story-005-soul-burst.md`, `story-006-bosses.md`
