# Ql_NhaTro_jun

**Ql_NhaTro_jun** là hệ thống quản lý nhà trọ hiện đại, hỗ trợ chủ trọ và khách thuê quản lý hợp đồng, hóa đơn, đền bù, nhắn tin, và nhiều tính năng khác.

## 🚀 Tính năng nổi bật
- Quản lý phòng trọ, hợp đồng, hóa đơn, đền bù
- Quản lý người dùng: chủ trọ, khách thuê, admin
- Thống kê doanh thu, chi phí, số lượng phòng, khách hàng
- Nhắn tin giữa chủ trọ và khách thuê
- Giao diện hiện đại, responsive, dễ sử dụng
- Phân quyền rõ ràng, bảo mật

## 🗂️ Cấu trúc thư mục
```
Ql_NhaTro_jun/
├── Controllers/         # Các controller cho API và MVC
├── Models/              # Entity Framework models
├── Views/               # Razor views (giao diện)
├── wwwroot/
│   ├── css/             # File CSS giao diện
│   └── js/              # File JavaScript
├── Program.cs           # Entry point ASP.NET Core
├── appsettings.json     # Cấu hình kết nối DB, app
├── ...
```

## ⚙️ Yêu cầu hệ thống
- .NET 6.0 trở lên
- SQL Server (hoặc cấu hình lại connection string cho DB khác)
- Node.js (nếu muốn build lại frontend assets)

## 💻 Hướng dẫn cài đặt & chạy local
1. **Clone repo:**
   ```bash
   git clone https://github.com/yourusername/Ql_NhaTro_jun.git
   cd Ql_NhaTro_jun
   ```
2. **Cấu hình DB:**
   - Sửa `appsettings.json` cho đúng connection string SQL Server của bạn.
3. **Khởi tạo DB:**
   - Dùng Entity Framework migrations hoặc import script SQL nếu có.
4. **Chạy ứng dụng:**
   ```bash
   dotnet build
   dotnet run
   ```
   - Truy cập: http://localhost:5000 hoặc cổng hiển thị trên terminal.

## 📝 Đóng góp
- Fork repo, tạo branch mới, commit và gửi pull request.
- Mọi ý kiến, bug, tính năng mới vui lòng tạo issue trên GitHub.

## 📄 Bản quyền
- Dự án thuộc sở hữu của nhóm phát triển Ql_NhaTro_jun.
- Vui lòng không sử dụng cho mục đích thương mại khi chưa được phép.

---
**Liên hệ hỗ trợ:** manhcansa04@gmail.com 