// Chatbot JavaScript đơn giản (không SignalR, không debug)
class Chatbot {
    constructor() {
        this.currentConversationId = null;
        this.currentRecipientId = null;
        this.currentRecipientName = null;
        this.currentUserId = null;
        this.currentUserRole = null;
        this.currentMaPhong = null;
        this.conversations = [];
        this.messages = [];
        this.isLoading = false;
        this.lastMessageCount = 0;
        this.pollingInterval = null;
        this.conversationPollingInterval = null;
        this.isFirstLoadMessages = true;
        this.isFirstLoadConversations = true;
        this.justSentMessage = false;
        this.lastNotifiedMessageId = null; // Lưu id hoặc timestamp tin nhắn cuối đã notify
        window._chatbot = this; // Gán biến toàn cục để debug
        this.init();
    }

    async init() {
        await this.getCurrentUserId();
        await this.getCurrentUserRole();
        this.loadConversations();
        this.bindEvents();
        this.setupPolling();
        this.setupConversationPolling();
        // 1. Yêu cầu quyền notification khi load trang (ép xin lại nếu denied)
        function requestNotificationPermission() {
            if ("Notification" in window) {
                Notification.requestPermission().then(function(permission) {
                    console.log('Notification permission:', permission);
                });
            }
        }
        requestNotificationPermission();
    }

    async getCurrentUserId() {
        try {
            const response = await fetch('/api/Auth/get-user-info');
            if (response.ok) {
                const data = await response.json();
                if (data.success && data.data && data.data.maNguoidung) {
                    this.currentUserId = data.data.maNguoidung;
                    localStorage.setItem('currentUserId', this.currentUserId);
                    return;
                }
            }
        } catch {}
        this.currentUserId = localStorage.getItem('currentUserId');
        if (!this.currentUserId) {
            this.showError('Không thể xác định người dùng. Vui lòng đăng nhập lại.');
        }
    }

    async getCurrentUserRole() {
        try {
            const response = await fetch('/api/Message/test-user');
            if (response.ok) {
                const data = await response.json();
                this.currentUserRole = data.userRole;
            }
        } catch {}
    }

    async loadConversations() {
        if (!this.currentUserId) return;
        try {
            this.isLoading = true;
            if (this.isFirstLoadConversations) {
                this.showLoadingConversations();
            }
            // Luôn dùng API conversations-for-allowed cho mọi vai trò
            const url = '/api/Message/conversations-for-allowed';
            const response = await fetch(url);
            if (!response.ok) throw new Error('Lỗi khi tải hội thoại');
            const data = await response.json();
            if (data.success) {
                this.conversations = data.data || [];
                this.renderConversations();
            } else {
                this.showError(data.message || 'Không thể tải danh sách hội thoại');
            }
        } catch (error) {
            
        } finally {
            this.isLoading = false;
            this.isFirstLoadConversations = false;
        }
    }

    showLoadingConversations() {
        const conversationList = document.getElementById('conversationList');
        conversationList.innerHTML = '<li class="loading">Đang tải hội thoại...</li>';
    }

    renderConversations() {
        const conversationList = document.getElementById('conversationList');
        if (this.conversations.length === 0) {
            conversationList.innerHTML = '<li class="loading">Chưa có hội thoại nào</li>';
            return;
        }
        conversationList.innerHTML = this.conversations.map(conv => {
            const recipientId = conv.recipientId !== undefined ? conv.recipientId : (conv.RecipientId !== undefined ? conv.RecipientId : 0);
            const recipientName = conv.recipientName !== undefined ? conv.recipientName : (conv.RecipientName !== undefined ? conv.RecipientName : 'Unknown');
            const lastMessage = conv.lastMessage || conv.LastMessage || 'Chưa có tin nhắn';
            const time = conv.lastMessageTime ? this.formatTime(conv.lastMessageTime) : (conv.LastMessageTime ? this.formatTime(conv.LastMessageTime) : '');
            const avatarText = this.getAvatarText(recipientName);
            const maHopDong = conv.maHopDong !== undefined ? conv.maHopDong : (conv.MaHopDong !== undefined ? conv.MaHopDong : 0);
            const maPhong = conv.maPhong !== undefined ? conv.maPhong : (conv.MaPhong !== undefined ? conv.MaPhong : 0);
            const roomName = conv.roomName || conv.RoomName || '';
            const motelName = conv.motelName || conv.MotelName || '';
            const unreadCount = conv.unreadCount || conv.UnreadCount || 0;
            // Tạo conversationId duy nhất cho mỗi hội thoại
            const conversationId = (maHopDong && maHopDong !== 0 && maHopDong !== '0') ? `contract-${maHopDong}` : `user-${recipientId}`;
            const isActive = this.currentConversationId === conversationId;
            return `
                <li class="conversation-item${isActive ? ' active' : ''}"
                    data-conversation-id="${conversationId}"
                    data-recipient-id="${recipientId}"
                    data-ma-phong="${maPhong}">
                    <div class="conversation-avatar">${avatarText}</div>
                    <div class="conversation-info">
                        <div class="conversation-name">${recipientName}</div>
                        <div class="conversation-last-message">${roomName} - ${motelName}</div>
                        <div class="conversation-time">${time}</div>
                    </div>
                    ${(!isActive && unreadCount > 0) ? `<div class="unread-badge">${unreadCount}</div>` : ''}
                </li>
            `;
        }).join('');
        this.bindConversationClicks();
        // Đảm bảo set lại class active cho hội thoại đang chọn
        if (this.currentConversationId) {
            const activeItem = document.querySelector(`.conversation-item[data-conversation-id="${this.currentConversationId}"]`);
            if (activeItem) activeItem.classList.add('active');
        }
    }

    bindConversationClicks() {
        const conversationItems = document.querySelectorAll('.conversation-item');
        conversationItems.forEach(item => {
            item.addEventListener('click', () => {
                this.selectConversation(item);
            });
        });
    }

    selectConversation(item) {
        document.querySelectorAll('.conversation-item').forEach(i => i.classList.remove('active'));
        item.classList.add('active');
        const conversationId = item.getAttribute('data-conversation-id');
        const recipientId = item.getAttribute('data-recipient-id');
        const recipientName = item.querySelector('.conversation-name').textContent;
        const maPhong = item.getAttribute('data-ma-phong');
        this.currentConversationId = conversationId;
        this.currentRecipientId = recipientId;
        this.currentRecipientName = recipientName;
        this.currentMaPhong = maPhong;
        this.isFirstLoadMessages = true;
        // Tách maHopDong từ conversationId nếu có
        let maHopDong = 0;
        if (conversationId.startsWith('contract-')) {
            maHopDong = conversationId.replace('contract-', '');
        }
        this.loadMessages(maHopDong, recipientId, recipientName);
        this.setupPolling();
        this.enableChatInput();
    }

    enableChatInput() {
        const chatInput = document.getElementById('chatInput');
        const sendBtn = document.getElementById('sendBtn');
        if (chatInput && sendBtn) {
            chatInput.disabled = false;
            sendBtn.disabled = !chatInput.value.trim();
        }
    }

    disableChatInput() {
        const chatInput = document.getElementById('chatInput');
        const sendBtn = document.getElementById('sendBtn');
        if (chatInput && sendBtn) {
            chatInput.disabled = true;
            sendBtn.disabled = true;
        }
    }

    async loadMessages(conversationId, recipientId, recipientName) {
        if (!this.currentUserId) return;
        try {
            this.isLoading = true;
            if (this.isFirstLoadMessages) {
                this.showLoadingMessages();
            }
            this.currentRecipientId = recipientId;
            this.currentRecipientName = recipientName;
            this.currentMaPhong = 0;
            this.updateChatHeader(recipientName);
            let url = '';
            if (conversationId && conversationId > 0) {
                url = `/api/Message/conversation-by-contract?maHopDong=${conversationId}`;
            } else {
                url = `/api/Message/conversation-between?userId1=${this.currentUserId}&userId2=${recipientId}`;
            }
            const response = await fetch(url);
            if (!response.ok) throw new Error('Lỗi khi tải tin nhắn');
            const data = await response.json();
            if (data.success) {
                this.messages = data.data || [];
                this.renderMessages();
            } else {
                this.showError(data.message || 'Không thể tải tin nhắn');
            }
        } catch (error) {
          
        } finally {
            this.isLoading = false;
            this.isFirstLoadMessages = false;
        }
    }

    showLoadingMessages() {
        const chatMessages = document.getElementById('chatMessages');
        chatMessages.innerHTML = '<div class="loading">Đang tải tin nhắn...</div>';
    }

    updateChatHeader(recipientName) {
        const chatHeader = document.getElementById('chatHeader');
        if (chatHeader) chatHeader.textContent = recipientName;
    }

    renderMessages() {
        const chatMessages = document.getElementById('chatMessages');
        if (this.messages.length === 0) {
            chatMessages.innerHTML = '<div class="empty-chat">Chưa có tin nhắn nào</div>';
            this.enableChatInput(); // Cho phép nhập/gửi tin nhắn kể cả khi chưa có tin nhắn nào
            return;
        }
        chatMessages.innerHTML = this.messages.map(message => {
            const isSent = message.nguoiGuiID == this.currentUserId || message.NguoiGuiID == this.currentUserId;
            const senderName = isSent ? 'Bạn' : (message.tenNguoiGui || message.TenNguoiGui || this.currentRecipientName || 'Người khác');
            const avatarText = this.getAvatarText(senderName);
            const time = this.formatTime(message.thoiGianGui || message.ThoiGianGui);
            return `
                <div class="message ${isSent ? 'sent' : 'received'}">
                    <div class="message-avatar">${avatarText}</div>
                    <div class="message-content">
                        <div class="message-sender">${senderName}</div>
                        <div class="message-text">${this.escapeHtml(message.noiDung || message.NoiDung)}</div>
                        <div class="message-time">${time}</div>
                    </div>
                </div>
            `;
        }).join('');
        if (this.isFirstLoadMessages || this.justSentMessage) {
            this.scrollToBottom();
            this.justSentMessage = false;
        }
        this.enableChatInput();
        // --- Notify khi có tin nhắn mới từ người khác, chỉ 1 lần ---
        if (this.messages.length > 0) {
            const lastMsg = this.messages[this.messages.length - 1];
            const isSent = lastMsg.nguoiGuiID == this.currentUserId || lastMsg.NguoiGuiID == this.currentUserId;
            // Ưu tiên dùng id, nếu không có thì dùng thoiGianGui
            const msgUnique = lastMsg.id || lastMsg.ID || lastMsg.thoiGianGui || lastMsg.ThoiGianGui;
            if (!isSent && msgUnique !== this.lastNotifiedMessageId) {
                const senderName = lastMsg.tenNguoiGui || lastMsg.TenNguoiGui || this.currentRecipientName || 'Người khác';
                const msgText = lastMsg.noiDung || lastMsg.NoiDung || '';
                showDesktopNotification('Tin nhắn mới', `Từ ${senderName}: ${msgText}`);
                this.lastNotifiedMessageId = msgUnique;
            }
        }
    }

    scrollToBottom() {
        const chatMessages = document.getElementById('chatMessages');
        if (chatMessages) {
            chatMessages.scrollTo({
                top: chatMessages.scrollHeight,
                behavior: 'smooth'
            });
        }
    }

    bindEvents() {
        const chatInput = document.getElementById('chatInput');
        const sendBtn = document.getElementById('sendBtn');
        if (chatInput && sendBtn) {
            sendBtn.addEventListener('click', () => this.sendMessage());
            chatInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.sendMessage();
                }
            });
            chatInput.addEventListener('input', () => {
                sendBtn.disabled = !chatInput.value.trim();
            });
            sendBtn.disabled = !chatInput.value.trim();
        }
        // Thêm sự kiện tìm kiếm hội thoại
        const searchInput = document.getElementById('searchConversation');
        if (searchInput) {
            searchInput.addEventListener('input', () => {
                this.filterConversations(searchInput.value.trim());
            });
        }
    }

    async sendMessage() {
        const chatInput = document.getElementById('chatInput');
        const sendBtn = document.getElementById('sendBtn');
        const message = chatInput.value.trim();
        if (!message || !this.currentRecipientId || !this.currentUserId) return;
        sendBtn.disabled = true;
        try {
            const requestBody = {
                nguoiGuiID: this.currentUserId,
                nguoiNhanID: this.currentRecipientId,
                noiDung: message
            };
            if (this.currentConversationId && this.currentConversationId > 0) {
                requestBody.maHopDong = this.currentConversationId;
            }
            if (this.currentMaPhong && this.currentMaPhong > 0) {
                requestBody.maPhong = this.currentMaPhong;
            }
            const response = await fetch('/api/Message/add-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestBody)
            });
            if (!response.ok) throw new Error('Lỗi khi gửi tin nhắn');
            const data = await response.json();
            if (data.success) {
                chatInput.value = '';
                sendBtn.disabled = true;
                this.isFirstLoadMessages = false;
                this.justSentMessage = true;
                this.scrollToBottom();
                this.loadMessages(this.currentConversationId, this.currentRecipientId, this.currentRecipientName);
            } else {
               
            }
        } catch (error) {
           
        }
    }

    setupPolling() {
        if (this.pollingInterval) clearInterval(this.pollingInterval);
        this.pollingInterval = setInterval(() => {
            if (this.currentConversationId && this.currentRecipientId) {
                this.isFirstLoadMessages = false;
                let maHopDong = 0;
                let recipientId = this.currentRecipientId;
                if (this.currentConversationId.startsWith('contract-')) {
                    maHopDong = this.currentConversationId.replace('contract-', '');
                } else if (this.currentConversationId.startsWith('user-')) {
                    recipientId = this.currentConversationId.replace('user-', '');
                }
                console.log('Polling loadMessages:', { currentConversationId: this.currentConversationId, maHopDong, recipientId, currentRecipientName: this.currentRecipientName });
                this.loadMessages(maHopDong, recipientId, this.currentRecipientName);
            }
        }, 3000);
    }

    setupConversationPolling() {
        if (this.conversationPollingInterval) clearInterval(this.conversationPollingInterval);
        this.conversationPollingInterval = setInterval(() => {
            this.loadConversations();
        }, 3000);
    }

    formatTime(timestamp) {
        if (!timestamp) return '';
        const date = new Date(timestamp);
        const now = new Date();
        const diffInHours = (now - date) / (1000 * 60 * 60);
        if (diffInHours < 24) {
            return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        } else if (diffInHours < 48) {
            return 'Hôm qua';
        } else {
            return date.toLocaleDateString('vi-VN');
        }
    }

    getAvatarText(name) {
        if (!name) return '?';
        return name.charAt(0).toUpperCase();
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showError(message) {
        alert('Lỗi: ' + message);
    }

    filterConversations(keyword) {
        if (!keyword) {
            this.renderConversations();
            return;
        }
        const lowerKeyword = keyword.toLowerCase();
        const filtered = this.conversations.filter(conv => {
            const recipientName = conv.recipientName || conv.RecipientName || '';
            return recipientName.toLowerCase().includes(lowerKeyword);
        });
        this.renderConversationsWithList(filtered);
    }

    renderConversationsWithList(list) {
        const conversationList = document.getElementById('conversationList');
        if (list.length === 0) {
            conversationList.innerHTML = '<li class="loading">Không tìm thấy hội thoại nào</li>';
            return;
        }
        conversationList.innerHTML = list.map(conv => {
            const recipientId = conv.recipientId !== undefined ? conv.recipientId : (conv.RecipientId !== undefined ? conv.RecipientId : 0);
            const recipientName = conv.recipientName !== undefined ? conv.recipientName : (conv.RecipientName !== undefined ? conv.RecipientName : 'Unknown');
            const lastMessage = conv.lastMessage || conv.LastMessage || 'Chưa có tin nhắn';
            const time = conv.lastMessageTime ? this.formatTime(conv.lastMessageTime) : (conv.LastMessageTime ? this.formatTime(conv.LastMessageTime) : '');
            const avatarText = this.getAvatarText(recipientName);
            const maHopDong = conv.maHopDong !== undefined ? conv.maHopDong : (conv.MaHopDong !== undefined ? conv.MaHopDong : 0);
            const maPhong = conv.maPhong !== undefined ? conv.maPhong : (conv.MaPhong !== undefined ? conv.MaPhong : 0);
            const roomName = conv.roomName || conv.RoomName || '';
            const motelName = conv.motelName || conv.MotelName || '';
            const unreadCount = conv.unreadCount || conv.UnreadCount || 0;
            const conversationId = (maHopDong && maHopDong !== 0 && maHopDong !== '0') ? `contract-${maHopDong}` : `user-${recipientId}`;
            const isActive = this.currentConversationId === conversationId;
            return `
                <li class="conversation-item${isActive ? ' active' : ''}"
                    data-conversation-id="${conversationId}"
                    data-recipient-id="${recipientId}"
                    data-ma-phong="${maPhong}">
                    <div class="conversation-avatar">${avatarText}</div>
                    <div class="conversation-info">
                        <div class="conversation-name">${recipientName}</div>
                        <div class="conversation-last-message">${roomName} - ${motelName}</div>
                        <div class="conversation-time">${time}</div>
                    </div>
                    ${(!isActive && unreadCount > 0) ? `<div class="unread-badge">${unreadCount}</div>` : ''}
                </li>
            `;
        }).join('');
        this.bindConversationClicks();
        if (this.currentConversationId) {
            const activeItem = document.querySelector(`.conversation-item[data-conversation-id="${this.currentConversationId}"]`);
            if (activeItem) activeItem.classList.add('active');
        }
    }
}

// 2. Hàm show notify
function showDesktopNotification(title, body) {
    console.log('Gọi notify:', title, body, Notification.permission);
    if ("Notification" in window && Notification.permission === "granted") {
        new Notification(title, { body: body, icon: '/favicon.ico' });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new Chatbot();
}); 