# Story 004: Thẻ nâng cấp (9 thẻ, 3 mức hiếm)

> **Epic**: micro-arena-survivor
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-05

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

- `Assets/Scripts/Gameplay/Combat/GamePause.cs` — created (sole writer of Time.timeScale: card-pick pause + hit-stop)
- `Assets/Scripts/Gameplay/Combat/HitStop.cs`, `HealthModel.cs`, `Health.cs` — modified
- `Assets/Scripts/Gameplay/Config/CardConfig.cs`, `CardPoolConfig.cs`, `CardEffectConfig.cs` — created
- `Assets/Scripts/Gameplay/Cards/ModifierStack.cs`, `CardPool.cs`, `CardEffects.cs`, `CardController.cs`, `ICardPickView.cs`, `BurnStatus.cs`, `ChainTargets.cs`, `ForcedCards.cs`, `CardEffectRunner.cs` — created
- `Assets/Scripts/Gameplay/Enemies/IEnemyTargetOverride.cs` — created; `EnemyBase.cs`, `Crawler.cs` — modified (decoy redirect)
- `Assets/Scripts/UI/CardPickScreen.cs` — created (UGUI, keys 1/2/3 + mouse + gamepad, no Find call)
- `Assets/Scripts/Gameplay/Player/AutoAttack.cs`, `PlayerController.cs`, `Dash.cs` — modified (modifiers, events, no move/dash while paused)
- `Assets/Scripts/Gameplay/Waves/WaveDirector.cs` — modified (SetAutoContinueCardPick, OnEnemyKilled, enemy target override)
- `Assets/Scripts/Gameplay/Editor/ArenaSceneBuilder.cs` — modified (9 card assets, CardEffectConfig, CardController + runner)
- `Assets/Data/Card_*.asset` (9), `CardPoolConfig.asset`, `CardEffectConfig.asset`; `Assets/Materials/Decoy.mat`, `MeteorMarker.mat` — created
- `Assets/Scenes/Arena.unity` — regenerated
- `tools/card-rule-check/CardRuleCheck.cs`, `README.md` — created (rule check kept outside Assets/; copy in to run)

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

---

## Completion Notes

**Completed**: 2026-11-05
**Criteria**: 8/8 passing (Common cards, card pick and reset: playtest `production/qa/evidence/story-004-upgrade-cards/playtest-m1.md`; Rare and Legendary cards: owner confirmed in play; Vòng Xoáy rule: `tools/card-rule-check/`, 64/64 PASS)
**Deviations**:
- ADVISORY: card-pick UI uses legacy `Text` + `LegacyRuntime.ttf` (deprecated; diacritics rely on OS fonts). Story 007: switch to a TextMeshPro font with Vietnamese glyphs.
- ADVISORY: UI strings are not localized (no localization system yet).
- ADVISORY: `GamePause` is static (reset on `SubsystemRegistration`) rather than injected.
- ADVISORY: no EditMode tests; the rule check lives in `tools/card-rule-check/` and is copied into `Assets/` to run (see its README).
- Decisions: Cuồng Nộ applies below 30% HP only (card text updated to match); repeated Rare/Legendary picks add their numbers; Lôi Chấn 4 m chain, Sao Băng 3 m / 0.4 s; debug-only `-arenaForceCards`.
**Test Evidence**: waived at qa.level minimal; screenshots in `production/qa/evidence/story-004-upgrade-cards/` and `production/qa/evidence/story-004-cards.png`
**Reviewed by**: self, no independent review
