# Story 007: HUD + game feel

> **Epic**: micro-arena-survivor
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-06

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

- `Assets/Scripts/UI/HudController.cs` (new)
- `Assets/Scripts/UI/EndScreens.cs` (new)
- `Assets/Scripts/UI/UiKit.cs` (new)
- `Assets/Scripts/UI/CardPickScreen.cs` (TextMeshPro)
- `Assets/Scripts/Gameplay/Feel/ScreenShake.cs` (new)
- `Assets/Scripts/Gameplay/Feel/ShakeModel.cs` (new)
- `Assets/Scripts/Gameplay/Config/FeelConfig.cs` (new) + `Assets/Data/FeelConfig.asset`
- `Assets/Scripts/Gameplay/Config/BossConfig.cs` (telegraph colours, boss names)
- `Assets/Scripts/Gameplay/Config/WaveConfig.cs` + `Assets/Data/WaveConfig.asset` (removed DefeatRestartDelay)
- `Assets/Scripts/Gameplay/Arena/FollowCamera.cs` (shake offset)
- `Assets/Scripts/Gameplay/Bosses/{BossBase,GargoyleTyrant,VoidWarden,TelegraphView}.cs`
- `Assets/Scripts/Gameplay/Enemies/{EnemyBase,VolatileHusk}.cs` (events)
- `Assets/Scripts/Gameplay/Waves/{WaveDirector,WaveDebugOverlay}.cs`
- `Assets/Scripts/Gameplay/Editor/ArenaSceneBuilder.cs` (+ regenerated `Assets/Scenes/Arena.unity`, `Assets/Materials/SoulBurstRing.mat`)
- `Assets/Fonts/BeVietnamPro/` (TTF, OFL.txt, Regular/Bold SDF font assets), `Assets/TextMesh Pro/` (TMP Essential Resources)
- `tools/hud-feel-check/HudFeelCheck.cs`, `tools/hud-feel-check/README.md` (rule check; tests waived at qa.level minimal)

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

## Completion Notes

**Completed**: 2026-11-06
**Criteria**: 7 of 8 covered or deferred; the sign-off criterion is NOT MET. Seen on screen (retained, `production/qa/evidence/story-007-hud-game-feel/`): HUD with HP bar, 2 dash pips, rage bar, WAVE xx/10, remaining bar and boss bar on Waves 5 and 10; victory screen; defeat screen; release HUD without the debug overlay. Rule check `tools/hud-feel-check/` 89/89 PASS; boss 89/89, Soul Burst 43/43, cards 64/64 still PASS. DEFERRED — requires playtest: `production/qa/final-playtest-checklist.md` rows 7.1–7.14, all left NOT VERIFIED.
**Closed without play-testing and without sign-off**: the owner chose to close the story before playing it and before signing. Nobody has played this build by hand. The sign-off block in `production/qa/evidence/story-007-evidence.md` is blank, so the Visual/Feel gate (BLOCKING) is accepted as unmet, by the owner's decision.
**Enter and R not tested**: the rule check cannot simulate a key press outside play mode. Replay by mouse click at timeScale 0 is tested; replay by Enter or R is not, and no screenshot shows it.
**Deviations**:
- ADVISORY: Space is not a replay key (it is Dash).
- ADVISORY: the rage bar is top-left; revive feedback is a HUD flash and text only, no ring on Kaelen.
- ADVISORY (API review by `unity-specialist`): set `clearDynamicDataOnBuild` through its public setter, not `SerializedObject`; a redundant ShadowCaster disable in `MakeTransparent`; `MaterialPropertyBlock` on telegraphs bypasses the SRP Batcher; UI uses the default input actions, not `Assets/InputSystem_Actions.inputactions`.
- ADVISORY: the first defeat screenshot was taken before the 1 s `EndScreenDelay` and was re-captured; the dim second dash pip in `04-defeat.png` comes from the capture setup, not the HUD.
- Fixed in this story: `SoulBurstRing.mat` kept being rewritten because `ArenaSceneBuilder.MakeTransparent` wrote blend values by hand that URP recomputes on every load; the builder now calls URP's setup and the stable file was committed (8255753).
**Test Evidence**: waived at qa.level minimal; screenshots and rule-check output in `production/qa/evidence/story-007-hud-game-feel/`; sign-off `production/qa/evidence/story-007-evidence.md` left blank.
**Reviewed by**: self, no independent review (Unity API check by `unity-specialist`: nothing that breaks the build or a criterion; 4 advisories above)
