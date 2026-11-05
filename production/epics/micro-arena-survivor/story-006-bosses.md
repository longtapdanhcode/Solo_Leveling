# Story 006: Hai boss (Gargoyle Bạo Quân, Void Warden)

> **Epic**: micro-arena-survivor
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-05

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

- `Assets/Scripts/Gameplay/Bosses/BossBase.cs`, `BossBrain.cs`, `GargoyleBrain.cs`, `GargoyleTyrant.cs`, `VoidWardenBrain.cs`, `VoidWarden.cs`, `Telegraph.cs`, `TelegraphView.cs` — created
- `Assets/Scripts/Gameplay/Config/BossConfig.cs` — created; `EnemyConfig.cs` (EnemyKind.Boss), `WaveConfig.cs` (boss prefab/config types) — modified
- `Assets/Scripts/Gameplay/Enemies/EnemyBase.cs` — modified (crowd-control immunity, OnDying hook, virtual HP/speed)
- `Assets/Scripts/Gameplay/Waves/WaveDirector.cs` — modified (boss spawn, Wave 5 ends when boss and escorts are dead)
- `Assets/Scripts/Gameplay/Editor/ArenaSceneBuilder.cs` — modified (bosses, BossConfig, telegraph material; stand-ins removed)
- `Assets/Data/BossConfig.asset`, `Assets/Prefabs/GargoyleTyrant.prefab`, `VoidWarden.prefab`, `Assets/Materials/BossTelegraph.mat`, `GargoyleTyrant.mat`, `VoidWarden.mat` — created
- `Assets/Data/EnemyConfig_{Mini,Final}BossStandIn.asset`, `Assets/Prefabs/{Mini,Final}BossStandIn.prefab`, `Assets/Materials/{Mini,Final}BossStandIn.mat` — deleted
- `Assets/Data/WaveConfig.asset`, `Assets/Scenes/Arena.unity` — regenerated
- `tools/boss-check/BossCheck.cs`, `README.md` — created (rule check kept outside Assets/; copy in to run)

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

---

## Completion Notes

**Completed**: 2026-11-05
**Criteria**: 7/7 closed. Seen on screen and rule-checked (`tools/boss-check/`, 89/89 PASS): Gargoyle slam circle, Void Warden burst circle and slash fan, Wave 5 ends only when the Gargoyle and all escorts are dead, BossConfig data-driven, telegraph-only damage, Void Warden death clears telegraphs. DEFERRED — requires playtest: all of `production/qa/final-playtest-checklist.md` rows 6.1–6.11, left NOT VERIFIED (not marked passed).
**Boss never played**: the owner chose to skip playtesting this story. No boss was fought by hand. The pull, the Dash escape, the meteor rain and phase 2 have no screenshot.
**To check when playing — boss fight length**: the combo does about 57 DPS, so Void Warden's 2000 HP could fall in roughly 35 s. Check whether that is long enough for both phases and the meteor rain to appear. If not, tune `BossConfig` (HP, cooldown) in story 007 or later.
**Deviations**:
- ADVISORY: the pull wind-up circle is red like damage telegraphs but deals 0 damage.
- ADVISORY: the hit test uses Kaelen's centre point (no body radius); bosses walk into Kaelen's body.
- ADVISORY: placeholder values chosen in `BossConfig` (fan 8 m / 90°, pull 3 m/s for 2 s with a 1.2 s wind-up, meteor warning 1.5 s, scatter ±16 m).
- ADVISORY: telegraph timers pause while Kaelen is down (inherited from `EnemyBase.Act`).
- ADVISORY: a new mesh per telegraph and `MaterialPropertyBlock` on URP Lit (small cost; revisit in story 007).
- ADVISORY: `Remaining` still reads 0/1 on Wave 5 while escorts are alive after the Gargoyle dies; the story 007 HUD may want an escort count.
- Story file's Files list names `Assets/Tests/PlayMode/Bosses/BossTests.cs`; not created (tests waived at `qa.level: minimal`). The rule check lives in `tools/boss-check/` and is copied into `Assets/` to run.
**Test Evidence**: waived at qa.level minimal; screenshots and rule-check output in `production/qa/evidence/story-006-bosses/` and `production/qa/evidence/story-006-bosses.png`
**Reviewed by**: self, no independent review (Unity API check by `unity-specialist`: no wrong or deprecated API; 3 suggestions applied)
