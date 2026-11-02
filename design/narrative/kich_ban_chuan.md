**TÀI LIỆU THIẾT KẾ GAME**

*(GAME DESIGN DOCUMENT)*

# 1. THÔNG TIN DỰ ÁN

* **Tên dự án:** Solo Leveling (Micro Arena Survivor)
* **Thể loại:** 3D Top-Down Hack & Slash / Roguelite
* **Nền tảng mục tiêu:** PC (Windows / Mac)
* **Công cụ phát triển:** Unity Engine
* **Thời lượng 1 lượt chơi:** 8 – 12 phút (Tổng cộng 10 Waves)

# 2. Ý TƯỞNG CỐT LÕI & VÒNG LẶP CHƠI (CORE LOOP)

* **Bối cảnh:** Người chơi nhập vai chiến binh Kaelen bị giam cầm tại Đấu Trường Hư Vô, phải chiến đấu sinh tồn qua các đợt quái vật để mở khóa tự do.
* **Vòng lặp trận đấu:**

1. Xuất hiện tại trung tâm võ đài.
2. Di chuyển né tránh và tiêu diệt quái vật theo từng đợt (Wave).
3. Sau mỗi Wave, nhận 1 trong 3 thẻ bài nâng cấp sức mạnh ngẫu nhiên.
4. Vượt qua Wave 5 (Mini-Boss) và Wave 10 (Boss cuối) để chiến thắng.

# 3. CƠ CHẾ ĐIỀU KHIỂN & CHỈ SỐ NHÂN VẬT

## 3.1. Chỉ số cơ bản

* **Máu tối đa:** 100 HP
* **Tốc độ chạy:** 7 m/s
* **Sát thương cơ bản:** 20 DMG / đòn đánh
* **Số lượt lướt (Dash):** 2 lần tích trữ (thời gian hồi: 2 giây/lần)

## 3.2. Cụm phím điều khiển

* **Di chuyển (WASD):** Đi lại 8 hướng trên sàn đấu; nhân vật quay mặt theo hướng di chuyển.
* **Tấn công cơ bản (Tự động):** Khi có kẻ địch trong tầm (3m), nhân vật tự xoay về kẻ địch gần nhất và thực hiện chuỗi combo 3 đòn, lặp lại liên tục (mỗi nhát 0.4s, nghỉ 0.3s sau nhát 3):
  + **Nhát 1:** Chém ngang (20 DMG, đẩy lùi nhẹ).
  + **Nhát 2:** Chém chéo (25 DMG).
  + **Nhát 3:** Đâm thẳng (40 DMG, đẩy lùi xa 4m và làm choáng 0.3s).
* **Lướt né đòn (Space):** Lướt nhanh 5m theo hướng WASD (đứng yên thì lướt theo hướng đang nhìn). Có trạng thái bất tử (i-Frames) trong 0.18s đầu để né đòn hoặc lướt xuyên quái.
* **Chiêu Nộ - Soul Burst (Phím Q):** Khi thanh nộ tích lũy đủ 100%, dậm kiếm tạo sóng xung kích diện rộng gây 120 DMG và hất văng quái xung quanh.

# 4. CẤU TRÚC 10 ĐỢT QUÁI (WAVE PROGRESSION)

***Bảng 1. Cấu trúc các đợt quái***

| **Đợt (Wave)** | **Mục tiêu** | **Kẻ địch xuất hiện** | **Điểm lưu ý** |
| --- | --- | --- | --- |
| Wave 1 - 2 | Tiêu diệt 15 quái | Quái rệp cận chiến (Crawler) | Làm quen nhịp chém thường và lướt né đòn. |
| Wave 3 - 4 | Tiêu diệt 25 quái | Quái cận chiến + Phù thủy tầm xa (Void Caster) | Né đạn định hướng, ưu tiên tiêu diệt quái bắn xa. |
| Wave 5 | Diệt Mini-Boss | Gargoyle Bạo Quân + 4 quái phụ | Boss có đòn đập búa diện rộng (báo trước bằng vòng tròn đỏ). |
| Wave 6 - 7 | Sống sót 60s hoặc diệt 40 quái | Quái cận chiến + Quái tự sát (Volatile Husk) | Quái tự sát chạy rất nhanh và phát nổ khi áp sát. |
| Wave 8 - 9 | Tiêu diệt 50 quái | Xuất hiện hỗn hợp toàn bộ các loại quái | Bẫy xoay ở tâm sàn đấu kích hoạt, thu hẹp góc di chuyển. |
| Wave 10 | Tiêu diệt Boss cuối | Chúa Tể Hư Vô (Void Warden) | Trùm 2 giai đoạn: Chém kiếm quạt lớn, kéo hút và gọi mưa thiên thạch. |

# 5. HỆ THỐNG QUÁI VẬT & ĐẶC TÍNH

## 5.1. Quái rệp cận chiến (Crawler)

* **Chỉ số:** 40 HP | 10 DMG | Tốc độ: Trung bình.
* **Hành vi:** Chạy thẳng đến áp sát người chơi, khựng lại 0.3s lấy đà rồi cắn.

## 5.2. Phù thủy hư vô (Void Caster)

* **Chỉ số:** 60 HP | 15 DMG | Tốc độ: Chậm.
* **Hành vi:** Đứng từ xa (khoảng cách 8m), bắn đạn năng lượng bay thẳng định kỳ mỗi 2.5s.

## 5.3. Quái tự sát (Volatile Husk)

* **Chỉ số:** 25 HP | 50 DMG (Nổ AOE) | Tốc độ: Rất nhanh.
* **Hành vi:** Lao thẳng vào người chơi. Khi cách 2m, quái phát sáng đỏ nhấp nháy trong 0.8s rồi tự nổ.

## 5.4. Trùm cuối (Void Warden)

* **Chỉ số:** 2000 HP.
* **Bộ kỹ năng:** Tam đoạn trảm diện rộng; Hút người chơi vào tâm rồi đâm kiếm nổ đất; Mưa thiên thạch ngẫu nhiên khi máu dưới 50%.

# 6. DANH SÁCH THẺ BÀI NÂNG CẤP (UPGRADE CARDS)

## 6.1. Thẻ Thường (Common)

* **Sức Mạnh Thô:** Tăng 15% sát thương tấn công cơ bản.
* **Giày Bão:** Tăng 12% tốc độ di chuyển.
* **Huyết Nhũ:** Hồi phục ngay 30 HP và tăng thêm 20 HP tối đa.

## 6.2. Thẻ Hiếm (Rare)

* **Vết Chém Hỏa Ngục:** Đòn đánh thường gây hiệu ứng thiêu đốt (gây 10 DMG/s trong 3s, cộng dồn 3 lần).
* **Lôi Chấn:** Đòn thứ 3 của combo phóng ra tia sét giật lan 3 mục tiêu xung quanh (25 DMG điện).
* **Dịch Chuyển Phân Thân:** Khi Dash để lại một ảo ảnh thu hút quái đánh trong 1.5 giây.

## 6.3. Thẻ Huyền Thoại (Legendary)

* **Vòng Xoáy Huyết Thần:** Tăng 30% bán kính vung kiếm. Tiêu diệt 1 quái hồi 4% máu tối đa.
* **Sao Băng Giáng Trần:** Sau cú Dash, một thiên thạch rơi ngay vào vị trí cũ gây 80 DMG diện rộng và làm choáng 1s.
* **Cuồng Nộ Hư Vô:** Khi HP xuống dưới 30%, sát thương tăng 100% và luôn nhận thêm 50% tỉ lệ chí mạng.

# 7. GIAO DIỆN & CẢM GIÁC CHIẾN ĐẤU (GAME FEEL)

## 7.1. Giao diện trong trận (HUD)

* **Góc trên bên trái:** Thanh máu (HP) và 2 icon hạt năng lượng biểu thị số lượt Dash.
* **Góc trên chính giữa:** Đợt quái hiện tại (WAVE 03/10) và thanh tiến trình đếm số quái còn lại.

## 7.2. Hiệu ứng đòn đánh (Juiciness)

* **Hit-stop (Khựng hình):** Dừng chuyển động 0.06s khi chém trúng quái để tạo độ nặng cho đòn chém.
* **Screen Shake:** Rung nhẹ màn hình khi tung nhát chém thứ 3 hoặc khi quái nổ.
* **Cảnh báo đòn đánh (Telegraph):** Hiển thị vòng tròn hoặc vệt quạt màu đỏ trên mặt sàn trước khi Boss ra chiêu nguy hiểm.