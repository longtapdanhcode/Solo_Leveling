# Story 004: Thẻ nâng cấp (9 thẻ, 3 mức hiếm)

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 4

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Xem `docs/engine-reference/unity/` mục `6.3 → 6.6`.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 6 và "Giá trị ban đầu đã chốt":*

- [ ] Sau mỗi wave 1–9 hiện 3 thẻ ngẫu nhiên không trùng nhau; tỉ lệ Thường 60% / Hiếm 30% / Huyền Thoại 10%; chọn 1 thẻ thì áp dụng ngay và sang wave kế.
- [ ] Thường: **Sức Mạnh Thô** +15% sát thương cơ bản; **Giày Bão** +12% tốc độ di chuyển; **Huyết Nhũ** hồi 30 HP và +20 HP tối đa.
- [ ] Hiếm: **Vết Chém Hỏa Ngục** thiêu 10 DMG/s trong 3s, cộng dồn tối đa 3 lần; **Lôi Chấn** nhát 3 phóng sét lan 3 mục tiêu xung quanh, 25 DMG điện; **Dịch Chuyển Phân Thân** Dash để lại ảo ảnh thu hút quái 1.5s.
- [ ] Huyền Thoại: **Vòng Xoáy Huyết Thần** +30% bán kính **vung kiếm** và hồi 4% HP tối đa mỗi quái diệt; **Sao Băng Giáng Trần** sau Dash một thiên thạch rơi vào vị trí cũ, 80 DMG diện rộng, choáng 1s; **Cuồng Nộ Hư Vô** khi HP dưới 30% thì sát thương +100% và +50% tỉ lệ chí mạng.
- [ ] **Vòng Xoáy Huyết Thần chỉ tăng bán kính chém, không tăng tầm phát hiện 3m** của auto-attack (có kiểm thử).
- [ ] Thẻ nhận được giữ suốt lượt và mất khi lượt kết thúc/chơi lại.
- [ ] Toàn bộ thẻ và hệ số là dữ liệu ScriptableObject (`CardConfig`, bảng tỉ lệ ra thẻ); không hardcode.
- [ ] Màn chọn thẻ hiển thị tên, mô tả, mức hiếm; chụp ảnh giữ trong `production/qa/evidence/story-004-cards.png`.

---

## Implementation Notes

- Thẻ sửa chỉ số qua lớp modifier lên `PlayerConfig` của story 001 (không sửa ScriptableObject gốc lúc chạy).
- `[PLACEHOLDER]` (chưa quy định): thẻ có được nhận lặp không (mặc định: cho phép, cộng dồn), tầm lan và số mục tiêu của Lôi Chấn ngoài "3 mục tiêu", bán kính nổ Sao Băng, "bán kính vung kiếm" gốc.
- Cuồng Nộ Hư Vô dùng chí mạng gốc 5% ×1.5 từ story 001.

---

## Out of Scope

- Story 005: Soul Burst.
- Story 007: tinh chỉnh hình ảnh màn chọn thẻ.

---

## Files

- `Assets/Scripts/Cards/CardPool.cs`
- `Assets/Scripts/Cards/CardEffects.cs`
- `Assets/Scripts/Cards/CardPickScreen.cs`
- `Assets/Scripts/Config/CardConfig.cs`
- `Assets/Tests/EditMode/Cards/CardTests.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**: `Assets/Tests/EditMode/Cards/` (advisory ở `qa.level: minimal`); ảnh chụp màn chọn thẻ trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `story-001-kaelen-core-combat.md`, `story-003-arena-waves.md`
- Unlocks: `story-007-hud-game-feel.md`
