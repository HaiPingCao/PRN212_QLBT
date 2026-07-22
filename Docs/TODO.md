Fix

- [x] Tải cài đặt mặc định lỗi (ServerHost đọc appsettings.json theo AppContext.BaseDirectory thay vì CWD)
- [x] Server config validate ip ngay tại thời gian lưu cài đặt (IPAddress.TryParse qua QLBT.Shared.NetworkValidation)
- [x] Client sv chưa có validate IP (áp dụng NetworkValidation cho cả 2 client)
