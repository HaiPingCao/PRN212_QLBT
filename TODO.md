Thêm:

- [x] Thêm QL học kì
  -> DB thêm bảng hoc_ki
  -> DB sửa bảng bai_nop (Thêm điểm)
- [x] QL lớp lấy Kỳ học từ hoc_ki
- [x] Chấm điểm bài nộp
  - [x] Đánh 0 nếu không nộp và hết hạn (hiển thị 0 khi chưa nộp và đã quá hạn, chưa lưu tự động vào DB)
- [x] QLBT và QL Bài nộp mở file khi nhấn nút "Mở file"
- [x] Thêm nhiều sv vào lớp cùng lúc
- [x] Đăng nhập theo role (giáo viên / sinh viên)
- [x] Tách Client thành QLBT.Client.Teacher và QLBT.Client.Student, Server chỉ còn vai trò host + cài đặt

Fix

- [x] Tải cài đặt mặc định lỗi (ServerHost đọc appsettings.json theo AppContext.BaseDirectory thay vì CWD)
- [x] Server config validate ip ngay tại thời gian lưu cài đặt (IPAddress.TryParse qua QLBT.Shared.NetworkValidation)
- [x] Client sv chưa có validate IP (áp dụng NetworkValidation cho cả 2 client)
- [ ] Sort tự động khi chọn mục ở combo box

Chưa / sẽ ko thêm

- Quên mật khẩu
