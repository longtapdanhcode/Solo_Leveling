# Game Brief: Micro Arena Survivor

> Nguồn chính: `design/narrative/kich_ban_chuan.md`. Brief này không sửa nội dung kịch bản; các giá trị bổ sung ngoài kịch bản đã được chủ dự án duyệt (2026-11-02) và nằm ở mục "Giá trị ban đầu đã chốt".

**One-sentence pitch:** Chiến binh Kaelen chém, lướt né và chọn thẻ nâng cấp để sống sót qua 10 wave trong Đấu Trường Hư Vô, rồi hạ Chúa Tể Hư Vô.

## Core loop
- Di chuyển và quay mặt theo WASD; nhân vật tự động chém combo 3 đòn vào địch gần nhất, người chơi lo vị trí và lướt né để tiêu diệt quái trong wave.
- Tích nộ để tung Soul Burst (Q) dọn quái xung quanh.
- Hết wave, chọn 1 trong 3 thẻ nâng cấp ngẫu nhiên rồi sang wave tiếp theo.
- Vượt Wave 5 (Mini-Boss) và Wave 10 (Boss cuối) để thắng. Một lượt dài 8–12 phút.

## Player goal & fail state — what "working" looks like
- **Mục tiêu:** Hạ Void Warden ở Wave 10 để mở khóa tự do. Điểm thắng nằm ở cuối lượt, không có điểm số.
- **Thua:** HP về 0 thì dùng 1 lần hồi sinh. Mỗi lượt có sẵn 1 lần: hồi 50% HP, bất tử 2s, đẩy lùi quái xung quanh. HP về 0 lần nữa thì lượt kết thúc và chơi lại từ Wave 1, thẻ nâng cấp mất hết. Không có meta-progression.

## MVP — what must exist to be the game
1. Nhân vật Kaelen: di chuyển và quay theo WASD, tự động tấn công (combo 3 đòn, nhắm địch gần nhất), Dash 2 lượt tích trữ (Space), HP và hồi sinh.
2. Đấu trường và điều phối wave: 10 wave theo Bảng 1, gồm bẫy xoay ở Wave 8–9.
3. Ba loại quái: Crawler, Void Caster, Volatile Husk.
4. Chọn thẻ nâng cấp sau mỗi wave: 9 thẻ Thường/Hiếm/Huyền Thoại.
5. Soul Burst (thanh nộ + phím Q).
6. Hai boss: Gargoyle Bạo Quân (Wave 5) và Void Warden 2 giai đoạn (Wave 10), có telegraph vòng đỏ.
7. HUD và game feel: HP, Dash, WAVE xx/10 + tiến trình quái, hit-stop 0.06s, screen shake.

## Out of scope — not building this
- Meta-progression, mở khóa vĩnh viễn giữa các lượt.
- Multiplayer, nhiều đấu trường, hệ thống lưu game.
- Cốt truyện ngoài bối cảnh Kaelen bị giam ở Đấu Trường Hư Vô, không có cutscene hay thoại.
- Nền tảng ngoài PC (Windows/Mac).

## Build order
1. Nhân vật + auto-attack combo + Dash + hit-stop (kiểm chứng cảm giác chém tự động trước, đây là phần rủi ro nhất).
2. Crawler + wave director chạy Wave 1–2.
3. Thẻ nâng cấp + Soul Burst.
4. Void Caster, Volatile Husk, Wave 3–4 và 6–9 (kèm bẫy xoay).
5. Gargoyle (Wave 5) và Void Warden (Wave 10).
6. HUD, màn thắng/thua, hồi sinh và hoàn thiện.

---
**Who it's for / what they feel:** Người thích hack & slash, muốn một lượt 10 phút căng, đủ nhịp né và chém để thấy mình mạnh dần qua từng thẻ.

**Art & audio direction:** *[Kịch bản chưa nói; chưa quyết định.]*

**Reference game:** Vampire Survivors / Hades. Giữ vòng lặp wave + chọn thẻ và combat top-down, bỏ meta-progression.

## Giá trị ban đầu đã chốt (bổ sung ngoài kịch bản)
Chủ dự án duyệt 2026-11-02. **Đây là giá trị ban đầu**: cần cân bằng lại sau khi chơi thử. Mọi con số dưới đây (và các chỉ số trong kịch bản) **phải nằm trong config dữ liệu** (ví dụ ScriptableObject), không hardcode trong code.

| Mục | Giá trị ban đầu | Lý do |
|---|---|---|
| Hồi sinh | 1 lần mỗi lượt: hồi 50% HP, bất tử 2s, đẩy lùi quái xung quanh | Điều kiện thua do chủ dự án chọn |
| Auto-attack | Tầm phát hiện 3m; nhắm địch gần nhất, xoay về mục tiêu khi chém rồi trở lại hướng WASD; mỗi nhát 0.4s, nghỉ 0.3s sau nhát 3, lặp combo | Điều khiển đổi sang WASD + tự đánh (chủ dự án, 2026-11-02) |
| Dash | Chỉ phím Space; lướt theo hướng WASD, đứng yên thì lướt theo hướng đang nhìn | Không còn nhắm bằng chuột |
| Gargoyle Bạo Quân | 600 HP; đập búa 25 DMG, vòng đỏ bán kính 4m, báo trước 1s | Nằm giữa Void Caster (60) và Void Warden (2000) |
| Tích nộ Soul Burst | +5% mỗi quái diệt, giữ qua các wave | Đủ 100% sau 20 kill, tức khoảng 1 lần mỗi wave đầu |
| Chí mạng gốc | 5% tỉ lệ, sát thương ×1.5 | Để thẻ Cuồng Nộ Hư Vô (+50%) có nghĩa |
| Tỉ lệ ra thẻ | Thường 60% / Hiếm 30% / Huyền Thoại 10%; 3 thẻ mỗi lần chọn không trùng nhau | Huyền Thoại hiếm nhưng vẫn ra được trong lượt 9 lần chọn |
| Bẫy xoay | Cánh quét ở tâm, 1 vòng/6s, 15 DMG khi chạm, hoạt động từ Wave 8 | Thu hẹp góc di chuyển như kịch bản |
| Tên dự án | "Micro Arena Survivor" là tên làm việc; "Solo Leveling" trong kịch bản trùng IP có thật | Tránh rủi ro bản quyền khi phát hành |
