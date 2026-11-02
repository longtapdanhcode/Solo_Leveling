# Story 007: HUD + game feel

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 7

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). UI: UGUI 2.6.0 đã có trong `Packages/manifest.json`; UI Toolkit là lựa chọn khác (`unity-ui-specialist`).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 7 và brief MVP 7 (hit-stop 0.06s đã làm ở story 001):*

- [ ] HUD góc trên trái: thanh HP và 2 icon hạt năng lượng cho số lượt Dash (sáng/tối theo lượt còn lại, cập nhật khi hồi).
- [ ] HUD giữa trên: "WAVE xx/10" và thanh tiến trình số quái còn lại, cập nhật theo sự kiện của story 003.
- [ ] Chỉ báo thanh nộ để người chơi biết khi nào dùng được Q (**không có trong mục 7.1 của kịch bản — thêm theo nhu cầu chơi, chờ chủ dự án xác nhận**).
- [ ] Screen shake nhẹ khi tung nhát chém thứ 3 và khi quái nổ.
- [ ] Telegraph của boss (story 006) rõ ràng, nhất quán vòng tròn/vệt quạt đỏ.
- [ ] Màn thắng (hạ Void Warden) và màn thua (hết hồi sinh) có chọn chơi lại; hồi sinh có phản hồi hình ảnh.
- [ ] Giá trị cường độ/độ dài hiệu ứng nằm trong ScriptableObject (`FeelConfig`); không hardcode.
- [ ] Mỗi màn hình đã làm (HUD, thắng, thua) có ảnh chụp giữ lại trong `production/qa/evidence/` và file `story-007-evidence.md` có sign-off.

---

## Implementation Notes

- HUD đọc sự kiện từ các story 001, 003, 005; không giữ trạng thái game riêng.
- Screen shake không được làm lệch vị trí người chơi/gameplay (chỉ rung camera).

---

## Out of Scope

- Nhạc và âm thanh (brief chưa quyết định hướng audio).
- Menu chính, cài đặt.

---

## Files

- `Assets/Scripts/UI/HudController.cs`
- `Assets/Scripts/UI/EndScreens.cs`
- `Assets/Scripts/Gameplay/Feel/ScreenShake.cs`
- `Assets/Scripts/Gameplay/Config/FeelConfig.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Visual/Feel
**Required evidence**: ảnh chụp giữ lại trong `production/qa/evidence/` + sign-off `production/qa/evidence/story-007-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `story-001-kaelen-core-combat.md`, `story-003-arena-waves.md`, `story-004-upgrade-cards.md`, `story-005-soul-burst.md`, `story-006-bosses.md`
- Unlocks: None
