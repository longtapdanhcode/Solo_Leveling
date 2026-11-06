# Micro Arena Survivor

Game hành động hack & slash góc nhìn từ trên xuống, làm bằng Unity 6.6.

Chiến binh **Kaelen** bị giam trong **Đấu Trường Hư Vô**. Để thoát ra, anh phải chém, lướt né và chọn thẻ nâng cấp để sống sót qua **10 wave** quái, hạ Mini-Boss **Gargoyle Bạo Quân** ở Wave 5 và **Chúa Tể Hư Vô (Void Warden)** ở Wave 10. Mỗi lượt dài khoảng 8–12 phút.

---

## Cách chơi

- Kaelen **tự động chém** địch gần nhất trong tầm 3m bằng combo 3 đòn. Việc của bạn là chọn vị trí và né đòn.
- Mỗi quái bị diệt cộng **nộ**. Đủ 100% thì tung **Soul Burst**: sóng xung kích 120 DMG hất văng quái xung quanh.
- Hết mỗi wave, chọn **1 trong 3 thẻ nâng cấp** ngẫu nhiên để mạnh dần lên.
- HP về 0 thì được **hồi sinh 1 lần** mỗi lượt (hồi 50% HP, bất tử 2s, đẩy lùi quái quanh mình). Gục lần nữa là thua: chơi lại từ Wave 1 và mất hết thẻ.

### Điều khiển

| Phím | Hành động |
|---|---|
| **W A S D** | Di chuyển 8 hướng, nhân vật quay theo hướng đi |
| *(tự động)* | Tấn công combo 3 đòn khi có địch trong tầm |
| **Space** | Lướt 5m (2 lượt tích trữ, hồi 2s/lượt), bất tử 0.18s đầu |
| **Q** | Soul Burst khi thanh nộ đầy |
| **1 / 2 / 3** hoặc chuột | Chọn thẻ nâng cấp |
| **Enter / R** hoặc nút *CHƠI LẠI* | Chơi lại sau khi thắng hoặc thua |

### Combo tự động

| Nhát | Đòn | Sát thương |
|---|---|---|
| 1 | Chém ngang | 20 DMG, đẩy lùi nhẹ |
| 2 | Chém chéo | 25 DMG |
| 3 | Đâm thẳng | 40 DMG, đẩy lùi 4m, choáng 0.3s |

---

## 10 wave

| Wave | Mục tiêu | Kẻ địch |
|---|---|---|
| 1–2 | Diệt 15 quái | Crawler |
| 3–4 | Diệt 25 quái | Crawler + Void Caster |
| 5 | Hạ Mini-Boss | **Gargoyle Bạo Quân** + 4 quái phụ |
| 6–7 | Sống sót 60s hoặc diệt 40 quái | Crawler + Volatile Husk |
| 8–9 | Diệt 50 quái | Tất cả các loại, kèm bẫy xoay ở tâm sàn |
| 10 | Hạ Boss cuối | **Void Warden** (2 giai đoạn) |

### Quái vật

- **Crawler**: 40 HP. Lao tới áp sát, khựng lại 0.3s lấy đà rồi cắn (10 DMG).
- **Void Caster**: 60 HP. Đứng xa 8m, cứ 2.5s bắn một viên đạn năng lượng (15 DMG).
- **Volatile Husk**: 25 HP, chạy rất nhanh. Khi cách 2m nó nhấp nháy đỏ 0.8s rồi phát nổ (50 DMG diện rộng).
- **Gargoyle Bạo Quân**: 600 HP. Đập búa 25 DMG trong vòng đỏ bán kính 4m, có báo trước 1s.
- **Void Warden**: 2000 HP. Tung tam đoạn trảm hình quạt, hút người chơi vào tâm rồi đâm kiếm nổ đất. Dưới 50% HP thì gọi thêm mưa thiên thạch.

Mọi đòn nguy hiểm của boss đều được báo trước bằng **vòng hoặc vệt đỏ trên sàn**. Lướt ra khỏi vùng đỏ là né được.

---

## Thẻ nâng cấp

Tỉ lệ ra thẻ: Thường 60%, Hiếm 30%, Huyền Thoại 10%. Mỗi lần chọn có 3 thẻ khác nhau.

| Độ hiếm | Thẻ | Hiệu ứng |
|---|---|---|
| Thường | Sức Mạnh Thô | +15% sát thương đòn thường |
| Thường | Giày Bão | +12% tốc độ di chuyển |
| Thường | Huyết Nhũ | Hồi 30 HP, +20 HP tối đa |
| Hiếm | Vết Chém Hỏa Ngục | Đòn thường gây thiêu đốt 10 DMG/s trong 3s, cộng dồn 3 lần |
| Hiếm | Lôi Chấn | Nhát 3 phóng tia sét lan sang 3 mục tiêu (25 DMG) |
| Hiếm | Dịch Chuyển Phân Thân | Lướt để lại ảo ảnh thu hút quái trong 1.5s |
| Huyền Thoại | Vòng Xoáy Huyết Thần | +30% bán kính vung kiếm, diệt quái hồi 4% HP tối đa |
| Huyền Thoại | Sao Băng Giáng Trần | Sau khi lướt, thiên thạch rơi vào chỗ cũ: 80 DMG diện rộng, choáng 1s |
| Huyền Thoại | Cuồng Nộ Hư Vô | Khi HP dưới 30%: +100% sát thương, +50% tỉ lệ chí mạng |

---

## Cảm giác chiến đấu

- **HUD**: thanh HP và 2 chấm Dash ở góc trên bên trái. Chính giữa phía trên hiện `WAVE xx/10` cùng số quái còn lại. Thanh nộ báo khi Soul Burst sẵn sàng.
- **Hit-stop**: khựng hình 0.06s khi chém trúng để đòn đánh có độ nặng.
- **Screen shake**: rung màn hình khi nhát chém thứ 3 trúng hoặc khi quái nổ.

---

## Chạy game

**Yêu cầu:** Unity **6000.6.3f1** (Unity 6.6), render pipeline URP.

1. Mở thư mục dự án bằng Unity Hub.
2. Mở scene `Assets/Scenes/Arena.unity` rồi nhấn **Play**.

**Build cho macOS:** File > Build Profiles > macOS > Build. Bản build chỉ chứa scene Arena, bundle identifier là `com.long.MicroArenaSurvivor`.

Các tham số dòng lệnh để debug (`-arenaStartWave`, `-arenaInvulnerable`, `-arenaForceCards`...) chỉ có tác dụng trong **Development Build**. Bản release bỏ qua chúng.

---

## Cấu trúc dự án

| Thư mục | Nội dung |
|---|---|
| `Assets/Scripts/Gameplay/` | Nhân vật, quái, boss, wave, thẻ nâng cấp, Soul Burst |
| `Assets/Scripts/UI/` | HUD, màn chọn thẻ, màn thắng/thua |
| `Assets/Data/` | Chỉ số game dạng ScriptableObject (thẻ, boss, wave...), không hardcode trong code |
| `Assets/Scenes/` | `Arena.unity` là scene chính của game |
| `design/` | Game brief và kịch bản gốc (`design/narrative/kich_ban_chuan.md`) |
| `production/` | Epic, story và tài liệu QA |

---

## Ghi chú

- Các chỉ số hiện tại là giá trị ban đầu, sẽ cân bằng lại sau khi chơi thử.
- Dự án được tổ chức theo template [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) (giấy phép MIT, xem `LICENSE`).
