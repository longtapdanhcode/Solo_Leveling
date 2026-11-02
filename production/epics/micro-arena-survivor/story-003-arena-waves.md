# Story 003: Đấu trường + điều phối 10 wave + bẫy xoay

> **Epic**: micro-arena-survivor
> **Status**: Ready
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 2

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: NOT ASSESSED (no VERSION.md risk rating)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Play Mode không reload domain: reset static state khi bắt đầu/khởi động lại lượt.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*Từ kịch bản mục 2 và Bảng 1, "Giá trị ban đầu đã chốt":*

- [ ] Đấu trường 3D top-down; Kaelen xuất hiện ở trung tâm khi bắt đầu lượt.
- [ ] 10 wave định nghĩa bằng dữ liệu (`WaveConfig`), đúng Bảng 1: Wave 1–2 diệt 15 Crawler; Wave 3–4 diệt 25 (Crawler + Void Caster); Wave 5 Mini-Boss + 4 quái phụ; Wave 6–7 sống sót 60s **hoặc** diệt 40 (Crawler + Volatile Husk), đạt một trong hai là xong; Wave 8–9 diệt 50 (đủ loại); Wave 10 Boss cuối.
- [ ] Wave kết thúc đúng điều kiện của nó; phát sự kiện bắt đầu/kết thúc wave và tiến trình quái còn lại (cho HUD).
- [ ] Sau Wave 1–9 mở bước chọn thẻ (story 004) trước khi sang wave kế; sau Wave 10 là thắng.
- [ ] Bẫy xoay ở tâm sàn từ Wave 8: cánh quét 1 vòng/6s, 15 DMG khi chạm.
- [ ] Hồi sinh dùng hết rồi chết lần nữa: lượt kết thúc, bắt đầu lại từ Wave 1 với trạng thái người chơi và thẻ được reset.
- [ ] Mọi giá trị (số quái, thời gian, chu kỳ bẫy, sát thương bẫy) nằm trong ScriptableObject.
- [ ] Chạy một lượt từ Wave 1 đến ít nhất Wave 4; giữ ảnh chụp trong `production/qa/evidence/story-003-waves.png`.
- [ ] Ghi kết quả playtest Wave 3–4 (Void Caster có tiếp cận được bằng Dash không) vào `production/qa/evidence/story-003-wave34-notes.md`.

---

## Implementation Notes

- `[PLACEHOLDER]` (chưa quy định): kích thước đấu trường, tốc độ/nhịp spawn, vị trí spawn, loại 4 quái phụ ở Wave 5, số quái tối đa cùng lúc.
- Wave 5 và 10 gọi boss từ story 006; trước khi 006 xong dùng quái thay thế để thử luồng.
- Quản lý trạng thái lượt (đang chơi, chọn thẻ, thắng, thua) bằng state machine nhỏ.

---

## Out of Scope

- Story 004: giao diện/logic chọn thẻ.
- Story 006: hai boss.
- Story 007: HUD và màn thắng/thua.

---

## Files

- `Assets/Scripts/Gameplay/Waves/WaveDirector.cs`
- `Assets/Scripts/Gameplay/Waves/RunState.cs`
- `Assets/Scripts/Gameplay/Arena/RotatingTrap.cs`
- `Assets/Scripts/Gameplay/Config/WaveConfig.cs`
- `Assets/Scenes/Arena.unity`
- `Assets/Tests/PlayMode/Waves/WaveDirectorTests.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**: `Assets/Tests/PlayMode/Waves/` OR playtest doc; cộng ảnh chụp trong `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `story-001-kaelen-core-combat.md`, `story-002-enemies.md`
- Unlocks: `story-004-upgrade-cards.md`, `story-006-bosses.md`, `story-007-hud-game-feel.md`
