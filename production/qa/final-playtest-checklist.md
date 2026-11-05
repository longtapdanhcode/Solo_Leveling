# Final Playtest Checklist

Những tiêu chí chỉ kiểm được bằng cách chơi thật. Mỗi mục ghi trạng thái **NOT VERIFIED** cho đến khi chủ dự án tự
chơi và đánh dấu. Bằng chứng tự động (rule check, ảnh chụp) ghi kèm để biết phần nào đã được máy kiểm.

Bản build để chơi: `Builds/macOS-dev/MicroArenaSurvivor.app` (development build; bỏ qua trong git).
Lệnh mở: `open -n Builds/macOS-dev/MicroArenaSurvivor.app --args -screen-width 1280 -screen-height 720 -screen-fullscreen 0`
(thêm `-arenaInvulnerable` nếu muốn không chết; KHÔNG dùng `-arenaAutoSoulBurst` khi kiểm tra phím Q thật).

---

## Story 005 — Soul Burst (`production/epics/micro-arena-survivor/story-005-soul-burst.md`)

Đã kiểm tự động: `tools/soul-burst-check/` 43/43 PASS (`production/qa/evidence/story-005-soul-burst/rule-check.txt`);
ảnh trong `production/qa/evidence/story-005-soul-burst/` (vòng sóng xung kích, "last burst: hit 2", nộ giữ ở màn chọn thẻ).

| # | Kiểm tra khi chơi | Trạng thái |
|---|---|---|
| 5.1 | Nhấn **Q** trên bàn phím thật khi dòng `Rage` < 100: không có gì xảy ra, nộ giữ nguyên. | NOT VERIFIED |
| 5.2 | Diệt 20 quái → `Rage 100/100`; nhấn **Q**: thấy vòng tím lan ra quanh Kaelen, quái gần bị hất ra ngoài, nộ về 0. | NOT VERIFIED |
| 5.3 | Vùng tác động khớp với vòng: quái trong vòng (≈6 m) mất máu/chết, quái ngoài vòng không bị gì. | NOT VERIFIED |
| 5.4 | Khoảng hất ≈4 m nhìn hợp lý (cảm giác, không đo). | NOT VERIFIED |
| 5.5 | Kaelen không mất máu khi tự tung Q (xem dòng `HP`). | NOT VERIFIED |
| 5.6 | Wave 5 hoặc 10: tung Q cạnh boss → boss mất máu nhưng **không** bị đẩy lùi. | NOT VERIFIED |
| 5.7 | Volatile Husk tự nổ (không bị chém chết) vẫn cộng +5 nộ. | NOT VERIFIED |
| 5.8 | Nhấn Q trong màn chọn thẻ: không có tác dụng, nộ giữ nguyên. | NOT VERIFIED |
| 5.9 | Nộ giữ nguyên khi sang wave tiếp theo (sau khi chọn thẻ). | NOT VERIFIED |
| 5.10 | Nhấn Q ở màn Defeat (trước khi tự chơi lại) và sau Victory (Wave 10): không có tác dụng. | NOT VERIFIED |
| 5.11 | Sau khi lượt kết thúc và chơi lại từ Wave 1: nộ về 0. | NOT VERIFIED |
| 5.12 | Tung Q ngay trong lúc hit-stop (vừa chém trúng): vẫn kích hoạt được, không bị nuốt phím. | NOT VERIFIED |

Ghi chú: thanh nộ hiện chỉ là dòng chữ debug (`WaveDebugOverlay`); HUD thật và hiệu ứng đẹp là story 007.
