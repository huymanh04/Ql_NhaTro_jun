# Hướng dẫn Debug Chat Real-time

## Vấn đề
Bên A nhắn tin cho bên B nhưng bên B không nhận được tin nhắn real-time.

## Các bước kiểm tra và sửa lỗi

### 1. Kiểm tra SignalR Connection

**Bước 1:** Mở file `test-signalr.html` trong browser
- Truy cập: `http://localhost:xxxx/test-signalr.html`
- Click "Connect SignalR" 
- Kiểm tra xem có kết nối thành công không

**Bước 2:** Kiểm tra Console trong Developer Tools
- Mở F12 → Console
- Xem có lỗi SignalR nào không
- Kiểm tra log messages

### 2. Kiểm tra Backend SignalR

**Bước 1:** Kiểm tra ChatHub
- File: `Hubs/ChatHub.cs`
- Đảm bảo có method `JoinUserGroup`
- Đảm bảo có method `SendMessage`

**Bước 2:** Kiểm tra Program.cs
- Đảm bảo có `builder.Services.AddSignalR()`
- Đảm bảo có `endpoints.MapHub<ChatHub>("/chatHub")`

### 3. Kiểm tra MessageController

**Bước 1:** Kiểm tra logging
- Mở file `Controllers/MessageController.cs`
- Tìm method `CreateMessage`
- Kiểm tra có log SignalR không

**Bước 2:** Test API endpoint
- Gửi POST request đến `/api/Message/add-message`
- Kiểm tra response có success không

### 4. Kiểm tra Frontend JavaScript

**Bước 1:** Mở chat trong 2 tab khác nhau
- Tab 1: User A
- Tab 2: User B

**Bước 2:** Kiểm tra Console
- Xem có log "SignalR connected successfully" không
- Xem có log "Joining user group" không
- Xem có log "Received real-time message" không

### 5. Debug Steps

**Bước 1:** Thêm debug buttons
- Trong chat, sẽ có nút "Debug SignalR" và "Test SignalR" (chỉ hiện ở localhost)
- Click để kiểm tra trạng thái

**Bước 2:** Test manual
```javascript
// Trong Console của browser
// Kiểm tra connection
console.log(chatbot.connection.state);

// Test gửi tin nhắn
chatbot.testSignalRConnection();
```

### 6. Các lỗi thường gặp

**Lỗi 1:** SignalR không kết nối
- Kiểm tra URL `/chatHub` có đúng không
- Kiểm tra CORS settings
- Kiểm tra firewall/network

**Lỗi 2:** User không join group
- Kiểm tra `currentUserId` có đúng không
- Kiểm tra method `JoinUserGroup` có được gọi không

**Lỗi 3:** Tin nhắn không được gửi
- Kiểm tra backend có gọi `_hubContext.Clients.Group()` không
- Kiểm tra user có trong group đúng không

**Lỗi 4:** Frontend không nhận tin nhắn
- Kiểm tra event listener `connection.on("ReceiveMessage")`
- Kiểm tra logic xử lý tin nhắn

### 7. Fallback Solution

Nếu SignalR không hoạt động, hệ thống sẽ:
- Tự động refresh tin nhắn mỗi 10 giây
- Load tin nhắn mới từ API
- Hiển thị tin nhắn với animation

### 8. Test Cases

**Test Case 1:** Real-time messaging
1. Mở chat trong 2 tab
2. Gửi tin nhắn từ tab A
3. Kiểm tra tab B có nhận được ngay không

**Test Case 2:** Fallback
1. Disconnect SignalR
2. Gửi tin nhắn
3. Kiểm tra có refresh sau 10s không

**Test Case 3:** Multiple users
1. Mở chat với 3 user khác nhau
2. Gửi tin nhắn
3. Kiểm tra chỉ đúng user nhận được

## Logs cần kiểm tra

### Backend Logs
```
[SignalR] Gửi tin nhắn đến người nhận: user_X
[SignalR] Gửi tin nhắn đến người gửi: user_Y
[SignalR] Đã gửi tin nhắn thành công qua SignalR
```

### Frontend Logs
```
SignalR connected successfully
Joining user group: user_X
=== RECEIVED REAL-TIME MESSAGE ===
Adding received message to UI - same conversation
```

## Contact Support

Nếu vẫn không giải quyết được, hãy:
1. Chụp screenshot console logs
2. Chụp screenshot network tab
3. Ghi lại các bước test đã thực hiện 