# Epic: Micro Arena Survivor

> **Source**: `design/game-brief.md` (tier `minimal`: no GDD, ADR, TR registry or control manifest)
> **Script**: `design/narrative/kich_ban_chuan.md`
> **Status**: Active

## Goal
Chiến binh Kaelen chém, lướt né và chọn thẻ nâng cấp để sống sót qua 10 wave trong Đấu Trường Hư Vô, rồi hạ Chúa Tể Hư Vô.

## Scope (brief MVP, in Build order)
1. Nhân vật Kaelen (WASD, auto-attack, Dash, HP, hồi sinh, hit-stop)
2. Ba loại quái
3. Đấu trường + 10 wave + bẫy xoay
4. Thẻ nâng cấp (9 thẻ)
5. Soul Burst
6. Hai boss
7. HUD + game feel

## Ground rules (apply to every story)
- Mọi chỉ số nằm trong ScriptableObject, không hardcode. Giá trị trong brief là giá trị ban đầu, cân bằng lại sau playtest.
- Giá trị kịch bản/brief chưa quy định: developer đặt tạm, đánh dấu `[PLACEHOLDER]` trong ScriptableObject, chủ dự án duyệt sau playtest.
- Auto-attack luôn nhắm địch gần nhất trong tầm 3m. Người chơi tự dùng Dash để tiếp cận Void Caster; kiểm tra lại khi playtest Wave 3–4.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | Kaelen — di chuyển, auto-attack, Dash, HP, hồi sinh | Logic | Ready | N/A (minimal) |
| 002 | Ba loại quái (Crawler, Void Caster, Volatile Husk) | Logic | Ready | N/A (minimal) |
| 003 | Đấu trường + điều phối 10 wave + bẫy xoay | Integration | Ready | N/A (minimal) |
| 004 | Thẻ nâng cấp (9 thẻ, 3 mức hiếm) | Logic | Ready | N/A (minimal) |
| 005 | Soul Burst (thanh nộ + phím Q) | Logic | Ready | N/A (minimal) |
| 006 | Hai boss (Gargoyle Bạo Quân, Void Warden) | Integration | Ready | N/A (minimal) |
| 007 | HUD + game feel | Visual/Feel | Ready | N/A (minimal) |
