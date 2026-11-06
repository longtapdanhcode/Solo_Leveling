# Story 007 — HUD + game feel: evidence

Story: `production/epics/micro-arena-survivor/story-007-hud-game-feel.md`
Ngày chụp: 2026-11-05. Build: `Builds/macOS-dev/` (development) và `Builds/macOS/` (release), Unity 6000.6.3f1, 1280x720 cửa sổ.
Ảnh chụp bằng `ScreenCapture.CaptureScreenshot` trong bản build (`ScreenshotOnArg`), nên chỉ chứa cửa sổ game.
Stamp các file đã đổi: `production/qa/evidence/story-007-hud-game-feel/stamp.txt`.

## Ảnh

| Ảnh | Lệnh | Thấy trên màn hình |
|---|---|---|
| `story-007-hud-game-feel/01-hud-wave5-boss-bar-dev.png` | dev, `-arenaStartWave 5 -arenaInvulnerable` | HP 100/100, 2 chấm Dash sáng, NỘ 0/100; "WAVE 05/10", "CÒN LẠI 5/5" (boss + 4 quái phụ); thanh máu "GARGOYLE BẠO QUÂN"; overlay debug ở góc dưới trái |
| `story-007-hud-game-feel/02-hud-wave10-boss-bar-dev.png` | dev, `-arenaStartWave 10 -arenaInvulnerable` | "WAVE 10/10", "CÒN LẠI 1/1", thanh máu "CHÚA TỂ HƯ VÔ"; bẫy xoay màu đỏ |
| `story-007-hud-game-feel/03-victory.png` | dev, `-arenaForceEnd victory` | "CHIẾN THẮNG", nút "CHƠI LẠI", gợi ý "Enter / R hoặc bấm nút"; HUD bị làm tối phía sau |
| `story-007-hud-game-feel/04-defeat.png` | dev, `-arenaForceEnd defeat` | "THẤT BẠI", "Kaelen đã ngã xuống — không còn lượt hồi sinh.", nút "CHƠI LẠI" |
| `story-007-hud-game-feel/05-hud-release-no-debug-overlay.png` | release, không cờ | HUD đầy đủ, **không** có overlay debug, không có chữ "Development Build" |

Ghi chú khi chụp:
- Lần chụp màn thua đầu tiên (`--after 240` khung hình) chụp trước khi hết độ trễ 1 s nên chưa thấy màn; chụp lại với `--after 1500` thì màn hiện đúng.
- Ở ảnh 04, chấm Dash thứ hai tối: trong lúc chụp, cửa sổ game bị đưa lên trước để chạy (Run In Background tắt), nên phím gõ ở ứng dụng khác có thể đã lọt vào game. Đó là do cách chụp, không phải lỗi HUD.
- Tiếng Việt có dấu (Ạ, Ậ, Ế, Ắ, Ể, Ư, Ơ, ữ…) hiển thị đúng với Be Vietnam Pro (SIL OFL 1.1, `Assets/Fonts/BeVietnamPro/OFL.txt`).

## Kiểm tra tự động

- `tools/hud-feel-check/` — 89 PASS / 0 FAIL (`story-007-hud-game-feel/rule-check.txt`)
- Hồi quy: boss 89/89, soul burst 43/43, thẻ 64/64 (`story-007-hud-game-feel/regression-*.txt`)
- Smoke (mở project batchmode, biên dịch): exit 0, 0 `error CS`, 0 `warning CS`
- Build release + development: Succeeded, 0 lỗi

## Chưa kiểm (cần chơi)

Các mục 7.1–7.14 trong `production/qa/final-playtest-checklist.md`, đều NOT VERIFIED.

## Sign-off

Người ký (lead):
Ngày:
Kết quả lần chơi cuối:
