# Trang Chatbot - Hệ thống Nhắn tin

## Tổng quan
Trang Chatbot cho phép khách thuê nhắn tin với chủ trọ của họ. Hệ thống được thiết kế với giao diện hiện đại, responsive và real-time.

## Tính năng chính

### 1. Giao diện người dùng
- **Sidebar hội thoại**: Hiển thị danh sách các cuộc trò chuyện với chủ trọ
- **Khung chat chính**: Hiển thị tin nhắn và cho phép gửi tin nhắn mới
- **Tìm kiếm**: Tìm kiếm hội thoại theo tên hoặc nội dung tin nhắn
- **Responsive**: Tương thích với mọi thiết bị

### 2. Chức năng nhắn tin
- **Gửi tin nhắn**: Gửi tin nhắn tới chủ trọ
- **Xem lịch sử**: Xem toàn bộ lịch sử tin nhắn
- **Real-time**: Tự động cập nhật tin nhắn mới
- **Hiển thị thời gian**: Hiển thị thời gian gửi tin nhắn

### 3. Bảo mật và phân quyền
- **Xác thực**: Chỉ người dùng đã đăng nhập mới có thể truy cập
- **Phân quyền**: Chỉ khách thuê (vai trò 0) mới có thể nhắn tin với chủ trọ
- **Kiểm tra hợp đồng**: Chỉ nhắn tin được với chủ trọ có hợp đồng

## Cấu trúc file

### Views
- `Views/Users/Chatbot.cshtml` - Giao diện chính của trang chatbot

### Controllers
- `Controllers/UsersController.cs` - Controller xử lý view Chatbot
- `Controllers/MessageController.cs` - API xử lý tin nhắn

### Styles
- `wwwroot/css/chatbot.css` - CSS cho giao diện chatbot

### Scripts
- `wwwroot/js/chatbot.js` - JavaScript xử lý logic chatbot

## API Endpoints

### 1. Lấy thông tin người dùng
```
GET /api/Auth/get-user-info
```

### 2. Lấy danh sách hội thoại (cho khách thuê)
```
GET /api/Message/conversations-for-tenant
```

### 3. Lấy tin nhắn giữa 2 người dùng
```
GET /api/Message/conversation-between?userId1={id1}&userId2={id2}
```

### 4. Gửi tin nhắn
```
POST /api/Message/add-message
Body: {
    "nguoiGuiID": int,
    "nguoiNhanID": int,
    "noiDung": string,
    "maHopDong": int
}
```

## Cách sử dụng

### 1. Truy cập trang
- Đăng nhập với tài khoản khách thuê
- Vào menu "Hòm thư" trong sidebar

### 2. Xem hội thoại
- Danh sách hội thoại hiển thị bên trái
- Mỗi hội thoại hiển thị: tên chủ trọ, tên phòng, nhà trọ
- Click vào hội thoại để xem tin nhắn

### 3. Gửi tin nhắn
- Chọn hội thoại muốn nhắn tin
- Nhập tin nhắn vào ô input
- Nhấn Enter hoặc click nút "Gửi"

### 4. Tìm kiếm
- Sử dụng ô tìm kiếm ở đầu sidebar
- Tìm theo tên chủ trọ hoặc nội dung tin nhắn

## Logic hoạt động

### 1. Xác định người dùng
- Lấy thông tin người dùng từ API `/api/Auth/get-user-info`
- Lưu vào localStorage để tái sử dụng

### 2. Lấy danh sách hội thoại
- Chỉ hiển thị hội thoại với chủ trọ có hợp đồng
- Sắp xếp theo thời gian tin nhắn cuối cùng
- Hiển thị thông tin phòng và nhà trọ

### 3. Gửi tin nhắn
- Kiểm tra quyền gửi tin nhắn
- Lưu tin nhắn vào database
- Cập nhật giao diện real-time

### 4. Auto refresh
- Tự động cập nhật danh sách hội thoại mỗi 30 giây
- Tự động cập nhật tin nhắn mỗi 10 giây

## Responsive Design

### Desktop
- Sidebar 300px, chat area chiếm phần còn lại
- Hiển thị đầy đủ thông tin hội thoại

### Mobile
- Sidebar chuyển thành header ngang
- Chat area chiếm toàn bộ màn hình
- Tối ưu cho màn hình nhỏ

## Bảo mật

### 1. Xác thực
- Kiểm tra đăng nhập trước khi truy cập
- Redirect về trang login nếu chưa đăng nhập

### 2. Phân quyền
- Chỉ khách thuê (vai trò 0) mới có thể truy cập
- Kiểm tra hợp đồng trước khi cho phép nhắn tin

### 3. Validation
- Kiểm tra dữ liệu đầu vào
- Escape HTML để tránh XSS
- Validate tin nhắn trước khi lưu

## Troubleshooting

### Lỗi thường gặp

1. **Không hiển thị hội thoại**
   - Kiểm tra đã đăng nhập chưa
   - Kiểm tra có hợp đồng với chủ trọ không
   - Kiểm tra console để xem lỗi API

2. **Không gửi được tin nhắn**
   - Kiểm tra đã chọn hội thoại chưa
   - Kiểm tra nội dung tin nhắn không rỗng
   - Kiểm tra quyền gửi tin nhắn

3. **Giao diện bị lỗi**
   - Kiểm tra file CSS và JS đã load đúng chưa
   - Clear cache browser
   - Kiểm tra console để xem lỗi JavaScript

### Debug
- Mở Developer Tools (F12)
- Xem tab Console để kiểm tra lỗi
- Xem tab Network để kiểm tra API calls
- Xem tab Elements để kiểm tra HTML structure

## Tương lai

### Tính năng có thể thêm
- Emoji picker
- Gửi file/hình ảnh
- Push notification
- Typing indicator
- Read receipts
- Group chat
- Voice messages

### Cải thiện hiệu suất
- WebSocket cho real-time tốt hơn
- Pagination cho tin nhắn cũ
- Lazy loading cho hội thoại
- Caching thông minh hơn 