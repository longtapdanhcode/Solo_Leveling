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

---

## Story 006 — Hai boss (`production/epics/micro-arena-survivor/story-006-bosses.md`)

Đã kiểm tự động: `tools/boss-check/` 89/89 PASS (`production/qa/evidence/story-006-bosses/rule-check.txt`); ảnh trong
`production/qa/evidence/story-006-bosses/` (vòng đập búa Gargoyle, vòng nổ đất và vệt quạt Void Warden, Wave 5 kết thúc khi
hết cả Gargoyle lẫn quái phụ).

Lệnh mở (development build; lượt chơi và mỗi lần chơi lại đều bắt đầu ở wave đã chọn):
- Wave 5: `open -n Builds/macOS-dev/MicroArenaSurvivor.app --args -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -arenaStartWave 5`
- Wave 10: `open -n Builds/macOS-dev/MicroArenaSurvivor.app --args -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -arenaStartWave 10`

Thêm `-arenaInvulnerable` để không chết (telegraph vẫn hiện, nhưng dòng `HP` không giảm, nên **bỏ cờ này** khi kiểm sát thương).

| # | Kiểm tra khi chơi | Trạng thái |
|---|---|---|
| 6.1 | Wave 5: Gargoyle đứng lại, vòng đỏ bán kính ≈4 m hiện ≈1 s rồi mới đập; đứng trong vòng mất đúng 25 HP. | NOT VERIFIED |
| 6.2 | Dash ra khỏi vòng đỏ trước khi nó nổ: không mất máu (sát thương chỉ trong vùng telegraph). | NOT VERIFIED |
| 6.3 | Hạ Gargoyle khi còn quái phụ: wave chưa kết thúc; diệt quái phụ cuối cùng thì hiện màn chọn thẻ. | NOT VERIFIED |
| 6.4 | Boss không bị đẩy lùi/choáng bởi nhát 3 của combo, Soul Burst (Q) và Sao Băng, nhưng vẫn mất máu. | NOT VERIFIED |
| 6.5 | Wave 10: tam đoạn trảm — 3 vệt quạt đỏ lần lượt, mỗi vệt trúng mất 30 HP; đứng ngoài vệt không mất máu. | NOT VERIFIED |
| 6.6 | Lực hút: vòng đỏ lớn báo trước ≥1 s rồi mới hút; Dash vẫn thoát được; sau đó vòng nổ đất 4 m ở tâm, trúng mất 40 HP. | NOT VERIFIED |
| 6.7 | Void Warden dưới 50% HP: bắt đầu mưa thiên thạch, mỗi 8 s 5 vòng đỏ 2 m, trúng mất 20 HP; không có trước 50%. | NOT VERIFIED |
| 6.8 | Hạ Void Warden: hiện Victory ngay, mọi vòng/vệt đỏ và thiên thạch đang chờ biến mất, HP không giảm sau đó. | NOT VERIFIED |
| 6.9 | Chơi lại sau Defeat ở Wave 5/10: không còn telegraph cũ trên sàn. | NOT VERIFIED |
| 6.10 | Phân Thân (nếu có thẻ): boss quay sang ảo ảnh, nhưng vòng/vệt đỏ đã hiện vẫn ở nguyên chỗ và vẫn nổ. | NOT VERIFIED |
| 6.11 | Cảm giác: telegraph dễ đọc, 1 s đủ để né, nhịp 3 s giữa các đòn hợp lý. | NOT VERIFIED |

Ghi chú khi máy chạy: boss đi xuyên vào thân Kaelen (không có va chạm thân, giống quái thường); vòng báo trước lực hút
cùng màu đỏ dù không gây sát thương. Thanh máu boss và screen shake là story 007.
