# Story 001: Kaelen — di chuyển, auto-attack, Dash, HP, hồi sinh

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (+ hit-stop from Build order step 1)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Xem `docs/engine-reference/unity/` mục `6.3 → 6.6`: Play Mode không reload domain (reset static state), `EntityId` thay `InstanceID`, không dùng legacy `Input`.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ brief MVP 1, "Giá trị ban đầu đã chốt" và kịch bản mục 3:*

- [ ] WASD di chuyển 8 hướng, 7 m/s; nhân vật quay mặt theo hướng di chuyển; không có nhắm bằng chuột.
- [ ] Auto-attack: có địch trong tầm phát hiện 3m thì luôn chọn **địch gần nhất** (chọn lại khi có địch khác gần hơn), xoay về mục tiêu khi chém rồi quay lại hướng WASD; không có địch trong tầm thì không đánh.
- [ ] Combo lặp 3 nhát: nhát 1 20 DMG (đẩy lùi nhẹ), nhát 2 25 DMG, nhát 3 40 DMG (đẩy lùi 4m, choáng 0.3s); mỗi nhát 0.4s, nghỉ 0.3s sau nhát 3.
- [ ] Chí mạng gốc 5%, sát thương ×1.5.
- [ ] Dash bằng Space: 5m, 2 lượt tích trữ, hồi 2s/lần, bất tử 0.18s đầu, lướt xuyên quái; lướt theo hướng WASD, đứng yên thì theo hướng đang nhìn. Dash **không** tự lao về phía địch.
- [ ] HP tối đa 100; nhận sát thương từ nguồn bất kỳ qua một đường xử lý sát thương chung (dùng lại ở story 002, 003, 006).
- [ ] Hồi sinh 1 lần mỗi lượt khi HP về 0: hồi 50% HP, bất tử 2s, đẩy lùi quái xung quanh. HP về 0 lần nữa thì phát sự kiện kết thúc lượt.
- [ ] Hit-stop 0.06s khi chém trúng quái.
- [ ] Mọi chỉ số trên nằm trong ScriptableObject (`PlayerConfig`); không có số hardcode trong code.
- [ ] Chạy game, quan sát di chuyển + auto-attack + Dash; giữ ảnh chụp trong `production/qa/evidence/story-001-kaelen.png`.

---

## Implementation Notes

- Project có sẵn `Assets/InputSystem_Actions.inputactions` (Input System 1.20): đặt action Move, Dash (Space), SoulBurst (Q); bỏ action nhắm/tấn công bằng chuột.
- Tạo mẫu ScriptableObject cấu hình dùng chung cho các story sau (đường dẫn `Assets/Data/`).
- `[PLACEHOLDER]` (chưa quy định): combo trở về nhát 1 khi mất mục tiêu; lực đẩy lùi "nhẹ" của nhát 1; tầm chém hiệu dụng (khác tầm phát hiện 3m).
- Hit-stop không được làm Dash hoặc hồi sinh bị kẹt; khôi phục thời gian chắc chắn.

---

## Out of Scope

- Story 002: AI và chỉ số quái.
- Story 004: hiệu ứng thẻ lên đòn đánh/Dash.
- Story 005: Soul Burst (phím Q).
- Story 007: HUD, screen shake, màn thua.

---

## Files

- `Assets/Scripts/Player/PlayerController.cs`
- `Assets/Scripts/Player/AutoAttack.cs`
- `Assets/Scripts/Player/Dash.cs`
- `Assets/Scripts/Combat/Health.cs`
- `Assets/Scripts/Config/PlayerConfig.cs`
- `Assets/Tests/EditMode/Player/PlayerCombatTests.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

*`qa.level: minimal`: test tự động là advisory; ảnh chụp chạy thật không được miễn.*

**Story Type**: Logic
**Required evidence**:
- `Assets/Tests/EditMode/Player/` (chọn mục tiêu gần nhất, chu kỳ combo, Dash, hồi sinh)
- Ảnh chụp giữ lại trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: `production/epics/micro-arena-survivor/story-002-enemies.md`, `story-003-arena-waves.md`, `story-004-upgrade-cards.md`, `story-005-soul-burst.md`
