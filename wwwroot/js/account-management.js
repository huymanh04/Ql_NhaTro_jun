// Account Management JavaScript
class AccountManager {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 10;
        this.searchTerm = '';
        this.roleFilter = '';
        this.users = [];
        this.totalUsers = 0;
        this.totalPages = 0;
        this.currentUserId = null;
        this.currentUserForRole = null;
        this.currentUserForDelete = null;
        this.scrollPosition = 0;

        this.init();
    }

    init() {
        this.hideAllModals(); // Đảm bảo tất cả modal đều ẩn khi khởi tạo
        this.bindEvents();
        this.loadUsers();
    }

    bindEvents() {
        // Search input
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            let searchTimeout;
            searchInput.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    this.searchTerm = e.target.value;
                    this.currentPage = 1;
                    this.loadUsers();
                }, 500);
            });
        }

        // Role filter
        const roleFilter = document.getElementById('roleFilter');
        if (roleFilter) {
            roleFilter.addEventListener('change', (e) => {
                this.roleFilter = e.target.value;
                this.currentPage = 1;
                this.loadUsers();
            });
        }

        // Page size
        const pageSize = document.getElementById('pageSize');
        if (pageSize) {
            pageSize.addEventListener('change', (e) => {
                this.pageSize = parseInt(e.target.value);
                this.currentPage = 1;
                this.loadUsers();
            });
        }

        // Add user button
        const addUserBtn = document.getElementById('addUserBtn');
        if (addUserBtn) {
            addUserBtn.addEventListener('click', () => {
                this.openCreateModal();
            });
        }

        // Modal close buttons
        const closeUserModalBtn = document.getElementById('closeUserModalBtn');
        if (closeUserModalBtn) {
            closeUserModalBtn.addEventListener('click', () => {
                this.closeUserModal();
            });
        }

        const closeRoleModalBtn = document.getElementById('closeRoleModalBtn');
        if (closeRoleModalBtn) {
            closeRoleModalBtn.addEventListener('click', () => {
                this.closeRoleModal();
            });
        }

        const closeDeleteModalBtn = document.getElementById('closeDeleteModalBtn');
        if (closeDeleteModalBtn) {
            closeDeleteModalBtn.addEventListener('click', () => {
                this.closeDeleteModal();
            });
        }

        // Modal cancel buttons
        const cancelUserModalBtn = document.getElementById('cancelUserModalBtn');
        if (cancelUserModalBtn) {
            cancelUserModalBtn.addEventListener('click', () => {
                this.closeUserModal();
            });
        }

        const cancelRoleModalBtn = document.getElementById('cancelRoleModalBtn');
        if (cancelRoleModalBtn) {
            cancelRoleModalBtn.addEventListener('click', () => {
                this.closeRoleModal();
            });
        }

        const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');
        if (cancelDeleteBtn) {
            cancelDeleteBtn.addEventListener('click', () => {
                this.closeDeleteModal();
            });
        }

        // Save and confirm buttons
        const saveUserBtn = document.getElementById('saveUserBtn');
        if (saveUserBtn) {
            saveUserBtn.addEventListener('click', () => {
                this.saveUser();
            });
        }

        const confirmRoleChangeBtn = document.getElementById('confirmRoleChangeBtn');
        if (confirmRoleChangeBtn) {
            confirmRoleChangeBtn.addEventListener('click', () => {
                this.confirmRoleChange();
            });
        }

        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
        if (confirmDeleteBtn) {
            confirmDeleteBtn.addEventListener('click', () => {
                this.confirmDelete();
            });
        }

        // Pagination buttons
        const prevPageBtn = document.getElementById('prevPageBtn');
        if (prevPageBtn) {
            prevPageBtn.addEventListener('click', () => {
                if (this.currentPage > 1) {
                    this.goToPage(this.currentPage - 1);
                }
            });
        }

        const nextPageBtn = document.getElementById('nextPageBtn');
        if (nextPageBtn) {
            nextPageBtn.addEventListener('click', () => {
                if (this.currentPage < this.totalPages) {
                    this.goToPage(this.currentPage + 1);
                }
            });
        }

        // Items per page
        const itemsPerPage = document.getElementById('itemsPerPage');
        if (itemsPerPage) {
            itemsPerPage.addEventListener('change', (e) => {
                this.pageSize = parseInt(e.target.value);
                this.currentPage = 1;
                this.loadUsers();
            });
        }

        // Clear filters
        const clearFiltersBtn = document.getElementById('clearFiltersBtn');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.searchTerm = '';
                this.roleFilter = '';
                this.currentPage = 1;
                
                const searchInput = document.getElementById('searchInput');
                const roleFilter = document.getElementById('roleFilter');
                
                if (searchInput) searchInput.value = '';
                if (roleFilter) roleFilter.value = '';
                
                this.loadUsers();
            });
        }

        // Form submission
        const userForm = document.getElementById('userForm');
        if (userForm) {
            userForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.saveUser();
            });
        }

        // Modal close on overlay click
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal')) {
                this.closeAllModals();
            }
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeAllModals();
            }
        });

        // Event delegation for action buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-role')) {
                const button = e.target.closest('.btn-role');
                const userId = button.getAttribute('data-user-id');
                const userName = button.getAttribute('data-user-name');
                const userRole = button.getAttribute('data-user-role');

                if (userId && userName) {
                    this.openRoleModal(parseInt(userId), userName, userRole);
                }
            } else if (e.target.closest('.btn-delete')) {
                const button = e.target.closest('.btn-delete');
                const userId = button.getAttribute('data-user-id');
                const userName = button.getAttribute('data-user-name');

                if (userId && userName) {
                    this.openDeleteModal(parseInt(userId), userName);
                }
            }
        });
    }

    async loadUsers() {
        this.showLoading(true);

        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                search: this.searchTerm
            });

            if (this.roleFilter) {
                params.append('roleFilter', this.roleFilter);
            }

            const response = await fetch(`/api/Admin/Users?${params}`);
            const data = await response.json();

            if (data.success) {
                this.users = data.data.users;
                this.totalUsers = data.data.totalUsers;
                this.totalPages = data.data.totalPages;
                this.currentPage = data.data.currentPage;

                this.renderUsers();
                this.renderPagination();
            } else {
                this.showToast('error', 'Lỗi', data.message || 'Không thể tải danh sách người dùng');
            }
        } catch (error) {
            console.error('Error loading users:', error);
            this.showToast('error', 'Lỗi', 'Không thể kết nối đến server');
        } finally {
            this.showLoading(false);
        }
    }

    renderUsers() {
        const tbody = document.getElementById('usersTableBody');
        const tableContainer = document.querySelector('.table-container');

        if (!tbody || !tableContainer) return;

        if (this.users.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" style="text-align: center; padding: 40px 20px; color: #64748b;">
                        <i class="fas fa-users" style="font-size: 3rem; margin-bottom: 16px; display: block; opacity: 0.5;"></i>
                        <p style="margin: 0; font-size: 1.1rem;">Không có tài khoản nào</p>
                        <p style="margin: 8px 0 0 0; font-size: 0.9rem; opacity: 0.7;">Hãy tạo tài khoản mới hoặc thử lại với bộ lọc khác</p>
                    </td>
                </tr>
            `;
            this.clearMobileCards();
            return;
        }

        // Calculate the starting index for STT
        const startIndex = (this.currentPage - 1) * this.pageSize;

        // Render table for desktop
        tbody.innerHTML = this.users.map((user, index) => {
            // Calculate STT (số thứ tự)
            const stt = startIndex + index + 1;

            return `
                <tr>
                    <td data-label="STT"><strong>${stt}</strong></td>
                    <td data-label="Họ tên">${this.escapeHtml(user.hoTen)}</td>
                    <td data-label="Email">${this.escapeHtml(user.email)}</td>
                    <td data-label="Số điện thoại">${this.escapeHtml(user.soDienThoai)}</td>
                    <td data-label="Vai trò">
                        <span class="role-badge ${user.vaiTro === '2' ? 'admin' : user.vaiTro === '1' ? 'manager' : 'customer'}">
                            <i class="fas ${user.vaiTro === '2' ? 'fa-crown' : user.vaiTro === '1' ? 'fa-user-tie' : 'fa-user'}"></i>
                            ${user.vaiTroText}
                        </span>
                    </td>
                    <td data-label="Trạng thái">
                        <span class="status-badge active">
                            <i class="fas fa-circle"></i>
                            Hoạt động
                        </span>
                    </td>
                    <td data-label="Thao tác">
                        <div class="action-buttons">
                            <button class="btn-action btn-edit" onclick="accountManager.openEditModal(${user.maNguoiDung})" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn-action btn-role" data-user-id="${user.maNguoiDung}" data-user-name="${this.escapeHtml(user.hoTen)}" data-user-role="${user.vaiTro}" title="Thay đổi quyền">
                                <i class="fas fa-user-cog"></i>
                            </button>
                            <button class="btn-action btn-delete" data-user-id="${user.maNguoiDung}" data-user-name="${this.escapeHtml(user.hoTen)}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');

        // Render cards for mobile
        this.renderMobileCards();
    }

    renderMobileCards() {
        const mobileContainer = document.getElementById('mobileUserCards');
        if (!mobileContainer) return;

        // Calculate the starting index for STT
        const startIndex = (this.currentPage - 1) * this.pageSize;

        mobileContainer.innerHTML = this.users.map((user, index) => {
            const initials = user.hoTen.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);

            // Calculate STT (số thứ tự)
            const stt = startIndex + index + 1;

            return `
                <div class="user-card">
                    <div class="user-card-header">
                        <div class="user-card-avatar">${initials}</div>
                        <div class="user-card-info">
                            <div class="user-card-name">${this.escapeHtml(user.hoTen)}</div>
                            <div class="user-card-email">${this.escapeHtml(user.email)}</div>
                            <span class="role-badge ${user.vaiTro === '2' ? 'admin' : user.vaiTro === '1' ? 'manager' : 'customer'}">
                                <i class="fas ${user.vaiTro === '2' ? 'fa-crown' : user.vaiTro === '1' ? 'fa-user-tie' : 'fa-user'}"></i>
                                ${user.vaiTroText}
                            </span>
                        </div>
                    </div>
                    <div class="user-card-details">
                        <div class="user-card-detail">
                            <div class="user-card-detail-label">STT</div>
                            <div class="user-card-detail-value">${stt}</div>
                        </div>
                        <div class="user-card-detail">
                            <div class="user-card-detail-label">Số điện thoại</div>
                            <div class="user-card-detail-value">${this.escapeHtml(user.soDienThoai)}</div>
                        </div>
                        <div class="user-card-detail">
                            <div class="user-card-detail-label">Trạng thái</div>
                            <div class="user-card-detail-value">
                                <span class="status-badge active">
                                    <i class="fas fa-circle"></i>
                                    Hoạt động
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="user-card-actions">
                        <button class="btn-action btn-edit" onclick="accountManager.openEditModal(${user.maNguoiDung})" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-role" data-user-id="${user.maNguoiDung}" data-user-name="${this.escapeHtml(user.hoTen)}" data-user-role="${user.vaiTro}" title="Thay đổi quyền">
                            <i class="fas fa-user-cog"></i>
                        </button>
                        <button class="btn-action btn-delete" data-user-id="${user.maNguoiDung}" data-user-name="${this.escapeHtml(user.hoTen)}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            `;
        }).join('');
    }

    clearMobileCards() {
        const mobileContainer = document.getElementById('mobileUserCards');
        if (mobileContainer) {
            mobileContainer.innerHTML = '';
        }
    }

    renderPagination() {
        const paginationInfo = document.getElementById('paginationInfo');
        const prevPageBtn = document.getElementById('prevPageBtn');
        const nextPageBtn = document.getElementById('nextPageBtn');
        const pageNumbers = document.getElementById('pageNumbers');

        if (!paginationInfo || !prevPageBtn || !nextPageBtn || !pageNumbers) return;

        // Update info
        const start = (this.currentPage - 1) * this.pageSize + 1;
        const end = Math.min(this.currentPage * this.pageSize, this.totalUsers);
        paginationInfo.textContent = `Hiển thị ${start} - ${end} của ${this.totalUsers} tài khoản`;

        // Update prev/next buttons
        prevPageBtn.disabled = this.currentPage === 1;
        nextPageBtn.disabled = this.currentPage === this.totalPages;

        // Generate page numbers
        let pageNumbersHTML = '';
        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        if (startPage > 1) {
            pageNumbersHTML += `<button class="btn btn-outline" onclick="accountManager.goToPage(1)">1</button>`;
            if (startPage > 2) {
                pageNumbersHTML += `<span class="pagination-ellipsis">...</span>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            pageNumbersHTML += `
                <button class="btn btn-outline ${i === this.currentPage ? 'active' : ''}" 
                        onclick="accountManager.goToPage(${i})">
                    ${i}
                </button>
            `;
        }

        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) {
                pageNumbersHTML += `<span class="pagination-ellipsis">...</span>`;
            }
            pageNumbersHTML += `<button class="btn btn-outline" onclick="accountManager.goToPage(${this.totalPages})">${this.totalPages}</button>`;
        }

        pageNumbers.innerHTML = pageNumbersHTML;
    }

    goToPage(page) {
        if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
            this.currentPage = page;
            this.loadUsers();
        }
    }

    openCreateModal() {
        this.currentUserId = null;
        this.resetForm();

        const modalTitleText = document.getElementById('modalTitleText');
        const passwordRequired = document.getElementById('passwordRequired');
        const passwordHint = document.getElementById('passwordHint');
        const matKhau = document.getElementById('matKhau');
        const saveBtnText = document.getElementById('saveBtnText');

        if (modalTitleText) modalTitleText.textContent = 'Tạo tài khoản mới';
        if (passwordRequired) passwordRequired.style.display = 'inline';
        if (passwordHint) passwordHint.style.display = 'none';
        if (matKhau) matKhau.required = true;
        if (saveBtnText) saveBtnText.textContent = 'Lưu';

        this.showModal('userModal');

        // Force show footer after modal is shown
        setTimeout(() => {
            const modal = document.getElementById('userModal');
            const footer = modal.querySelector('.modal-footer');
            if (footer) {
                footer.style.display = 'flex';
                footer.style.visibility = 'visible';
                footer.style.opacity = '1';
                footer.style.position = 'relative';
                footer.style.zIndex = '1000';
            }
        }, 100);
    }

    async openEditModal(userId) {
        if (!userId) {
            this.showToast('error', 'Lỗi', 'Không thể xác định người dùng cần chỉnh sửa');
            return;
        }

        this.currentUserId = userId;

        // Find user data
        const user = this.users.find(u => u.maNguoiDung === userId);
        if (!user) {
            this.showToast('error', 'Lỗi', 'Không tìm thấy thông tin người dùng');
            return;
        }

        // Fill form with validation
        const hoTenElement = document.getElementById('hoTen');
        const emailElement = document.getElementById('email');
        const soDienThoaiElement = document.getElementById('soDienThoai');
        const vaiTroElement = document.getElementById('vaiTro');
        const matKhauElement = document.getElementById('matKhau');
        const modalTitleText = document.getElementById('modalTitleText');
        const passwordRequiredElement = document.getElementById('passwordRequired');
        const passwordHintElement = document.getElementById('passwordHint');
        const saveBtnText = document.getElementById('saveBtnText');

        if (hoTenElement) hoTenElement.value = user.hoTen || '';
        if (emailElement) emailElement.value = user.email || '';
        if (soDienThoaiElement) soDienThoaiElement.value = user.soDienThoai || '';
        if (vaiTroElement) vaiTroElement.value = user.vaiTro || '0';
        if (matKhauElement) {
            matKhauElement.value = '';
            matKhauElement.required = false;
        }

        if (modalTitleText) modalTitleText.textContent = 'Chỉnh sửa tài khoản';
        if (passwordRequiredElement) passwordRequiredElement.style.display = 'none';
        if (passwordHintElement) passwordHintElement.style.display = 'block';
        if (saveBtnText) saveBtnText.textContent = 'Cập nhật';

        this.showModal('userModal');

        // Force show footer after modal is shown
        setTimeout(() => {
            const modal = document.getElementById('userModal');
            const footer = modal.querySelector('.modal-footer');
            if (footer) {
                footer.style.display = 'flex';
                footer.style.visibility = 'visible';
                footer.style.opacity = '1';
                footer.style.position = 'relative';
                footer.style.zIndex = '1000';
            }
        }, 100);
    }

    openRoleModal(userId, userName, currentRole) {
        console.log('openRoleModal called with:', { userId, userName, currentRole });

        if (!userId || !userName) {
            console.error('Missing userId or userName:', { userId, userName });
            this.showToast('error', 'Lỗi', 'Không thể xác định người dùng cần thay đổi quyền');
            return;
        }

        this.currentUserForRole = userId;
        console.log('Set currentUserForRole to:', this.currentUserForRole);

        const roleUserNameElement = document.getElementById('roleUserName');
        const newRoleElement = document.getElementById('newRole');

        if (roleUserNameElement) {
            roleUserNameElement.textContent = userName;
        }
        if (newRoleElement) {
            newRoleElement.value = currentRole || '0';
        }

        console.log('About to show roleModal');
        this.showModal('roleModal');
        console.log('showModal called for roleModal');
    }

    openDeleteModal(userId, userName) {
        console.log('openDeleteModal called with:', { userId, userName });

        if (!userId || !userName) {
            console.error('Missing userId or userName:', { userId, userName });
            this.showToast('error', 'Lỗi', 'Không thể xác định người dùng cần xóa');
            return;
        }

        this.currentUserForDelete = userId;
        console.log('Set currentUserForDelete to:', this.currentUserForDelete);

        const deleteUserNameElement = document.getElementById('deleteUserName');
        if (deleteUserNameElement) {
            deleteUserNameElement.textContent = userName;
        }
        this.showModal('deleteModal');
    }

    async saveUser() {
        if (!this.validateForm()) return;

        const formData = {
            hoTen: document.getElementById('hoTen').value.trim(),
            email: document.getElementById('email').value.trim(),
            soDienThoai: document.getElementById('soDienThoai').value.trim(),
            matKhau: document.getElementById('matKhau').value,
            vaiTro: document.getElementById('vaiTro').value
        };

        const saveButton = document.getElementById('saveUserBtn');
        const originalText = saveButton.innerHTML;
        saveButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
        saveButton.disabled = true;

        try {
            let response;
            if (this.currentUserId) {
                // Update user
                response = await fetch(`/api/Admin/UpdateUser/${this.currentUserId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                });
            } else {
                // Create user
                response = await fetch('/api/Admin/CreateUser', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                });
            }

            const data = await response.json();

            if (data.success) {
                this.showToast('success', 'Thành công', data.message);
                this.closeUserModal();
                this.loadUsers();
            } else {
                this.showToast('error', 'Lỗi', data.message || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error saving user:', error);
            this.showToast('error', 'Lỗi', 'Không thể kết nối đến server');
        } finally {
            saveButton.innerHTML = originalText;
            saveButton.disabled = false;
        }
    }

    async confirmRoleChange() {
        console.log('confirmRoleChange called, currentUserForRole:', this.currentUserForRole);

        if (!this.currentUserForRole) {
            console.error('No currentUserForRole set');
            this.showToast('error', 'Lỗi', 'Không có người dùng nào được chọn để thay đổi quyền');
            this.closeRoleModal();
            return;
        }

        const newRoleElement = document.getElementById('newRole');
        if (!newRoleElement) {
            this.showToast('error', 'Lỗi', 'Không thể xác định vai trò mới');
            return;
        }

        const newRole = newRoleElement.value;
        if (!newRole && newRole !== '0') {
            this.showToast('error', 'Lỗi', 'Vui lòng chọn vai trò');
            return;
        }

        const confirmButton = document.querySelector('#roleModal .btn-primary');
        if (confirmButton) {
            confirmButton.disabled = true;
            confirmButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang cập nhật...';
        }

        try {
            // Find the user to get current data
            const user = this.users.find(u => u.maNguoiDung === this.currentUserForRole);
            if (!user) {
                throw new Error('Không tìm thấy thông tin người dùng');
            }

            // Update user with new role (giữ nguyên các trường khác)
            const updateData = {
                hoTen: user.hoTen,
                email: user.email,
                soDienThoai: user.soDienThoai,
                vaiTro: newRole,
                matKhau: '' // Không đổi mật khẩu
            };

            const response = await fetch(`/api/Admin/UpdateUser/${this.currentUserForRole}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.success) {
                this.showToast('success', 'Thành công', 'Đã cập nhật quyền người dùng thành công');
                this.closeRoleModal();
                this.loadUsers();
            } else {
                this.showToast('error', 'Lỗi', data.message || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error changing role:', error);
            this.showToast('error', 'Lỗi', 'Không thể kết nối đến server');
        } finally {
            if (confirmButton) {
                confirmButton.disabled = false;
                confirmButton.innerHTML = '<i class="fas fa-check"></i> Xác nhận';
            }
        }
    }

    async confirmDelete() {
        console.log('confirmDelete called, currentUserForDelete:', this.currentUserForDelete);

        if (!this.currentUserForDelete) {
            console.error('No currentUserForDelete set');
            this.showToast('error', 'Lỗi', 'Không có người dùng nào được chọn để xóa');
            this.closeDeleteModal();
            return;
        }

        const deleteButton = document.querySelector('#deleteModal .btn-danger');
        if (deleteButton) {
            deleteButton.disabled = true;
            deleteButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';
        }

        try {
            const response = await fetch(`/api/Admin/DeleteUser/${this.currentUserForDelete}`, {
                method: 'DELETE'
            });

            const data = await response.json();

            if (data.success) {
                this.showToast('success', 'Thành công', data.message);
                this.closeDeleteModal();
                this.loadUsers();
            } else {
                this.showToast('error', 'Lỗi', data.message || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            this.showToast('error', 'Lỗi', 'Không thể kết nối đến server');
        } finally {
            if (deleteButton) {
                deleteButton.disabled = false;
                deleteButton.innerHTML = '<i class="fas fa-trash"></i> Xóa';
            }
        }
    }

    validateForm() {
        this.clearErrors();
        let isValid = true;

        const hoTen = document.getElementById('hoTen').value.trim();
        const email = document.getElementById('email').value.trim();
        const soDienThoai = document.getElementById('soDienThoai').value.trim();
        const matKhau = document.getElementById('matKhau').value;
        const vaiTro = document.getElementById('vaiTro').value;

        if (!hoTen) {
            this.showError('hoTenError', 'Vui lòng nhập họ tên');
            isValid = false;
        }

        if (!email) {
            this.showError('emailError', 'Vui lòng nhập email');
            isValid = false;
        } else if (!this.isValidEmail(email)) {
            this.showError('emailError', 'Email không hợp lệ');
            isValid = false;
        }

        if (!soDienThoai) {
            this.showError('soDienThoaiError', 'Vui lòng nhập số điện thoại');
            isValid = false;
        } else if (!this.isValidPhone(soDienThoai)) {
            this.showError('soDienThoaiError', 'Số điện thoại không hợp lệ');
            isValid = false;
        }

        if (!this.currentUserId && !matKhau) {
            this.showError('matKhauError', 'Vui lòng nhập mật khẩu');
            isValid = false;
        } else if (matKhau && matKhau.length < 6) {
            this.showError('matKhauError', 'Mật khẩu phải có ít nhất 6 ký tự');
            isValid = false;
        }

        if (!vaiTro) {
            this.showError('vaiTroError', 'Vui lòng chọn vai trò');
            isValid = false;
        }

        return isValid;
    }

    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    isValidPhone(phone) {
        const phoneRegex = /^[0-9]{10,11}$/;
        return phoneRegex.test(phone);
    }

    showError(elementId, message) {
        const errorElement = document.getElementById(elementId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(element => {
            element.classList.remove('show');
            element.textContent = '';
        });
    }

    resetForm() {
        const form = document.getElementById('userForm');
        if (form) {
            form.reset();
        }
        this.clearErrors();
    }

    showModal(modalId) {
        console.log('showModal called for:', modalId);

        // Hide only other modals, not the one we're showing
        const allModals = ['userModal', 'roleModal', 'deleteModal'];
        allModals.forEach(id => {
            if (id !== modalId) {
                const modal = document.getElementById(id);
                if (modal) {
                    modal.classList.remove('show');
                }
            }
        });

        const modal = document.getElementById(modalId);
        console.log('Modal element found:', modal);
        
        if (modal) {
            // Save current scroll position
            this.scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

            // Show the modal using CSS classes
            modal.classList.add('show');
            console.log('Added show class to modal');

            // Prevent background scroll
            document.body.classList.add('modal-open');
            document.body.style.overflow = 'hidden';
            document.body.style.position = 'fixed';
            document.body.style.top = `-${this.scrollPosition}px`;
            document.body.style.width = '100%';

            // Force modal to center and show footer
            setTimeout(() => {
                const modalContent = modal.querySelector('.modal-content');
                const modalFooter = modal.querySelector('.modal-footer');

                if (modalContent) {
                    modalContent.scrollTop = 0;
                }

                if (modalFooter) {
                    modalFooter.style.display = 'flex';
                    modalFooter.style.visibility = 'visible';
                    modalFooter.style.opacity = '1';
                    modalFooter.style.position = 'relative';
                    modalFooter.style.zIndex = '1000';
                }
                
                console.log('Modal should be visible now');
            }, 50);
        } else {
            console.error('Modal element not found:', modalId);
        }
    }

    hideModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('show');

            // Restore scroll position
            document.body.classList.remove('modal-open');
            document.body.style.overflow = '';
            document.body.style.position = '';
            document.body.style.top = '';
            document.body.style.width = '';

            // Restore scroll position
            if (this.scrollPosition !== undefined) {
                window.scrollTo(0, this.scrollPosition);
                this.scrollPosition = undefined;
            }
        }
    }

    closeUserModal() {
        this.hideModal('userModal');
        this.currentUserId = null;
    }

    closeRoleModal() {
        console.log('closeRoleModal called, resetting currentUserForRole from:', this.currentUserForRole);
        this.hideModal('roleModal');
        this.currentUserForRole = null;

        // Reset confirm button
        const confirmButton = document.querySelector('#roleModal .btn-primary');
        if (confirmButton) {
            confirmButton.disabled = false;
            confirmButton.innerHTML = '<i class="fas fa-check"></i> Xác nhận';
        }

        // Clear user name and reset role
        const roleUserNameElement = document.getElementById('roleUserName');
        const newRoleElement = document.getElementById('newRole');

        if (roleUserNameElement) {
            roleUserNameElement.textContent = '';
        }
        if (newRoleElement) {
            newRoleElement.value = '0';
        }
    }

    closeDeleteModal() {
        this.hideModal('deleteModal');
        this.currentUserForDelete = null;

        // Reset delete button
        const deleteButton = document.querySelector('#deleteModal .btn-danger');
        if (deleteButton) {
            deleteButton.disabled = false;
            deleteButton.innerHTML = '<i class="fas fa-trash"></i> Xóa';
        }

        // Clear user name
        const deleteUserNameElement = document.getElementById('deleteUserName');
        if (deleteUserNameElement) {
            deleteUserNameElement.textContent = '';
        }
    }

    closeAllModals(resetVariables = true) {
        this.closeUserModal();
        this.closeRoleModal();
        this.closeDeleteModal();
    }

    hideAllModals(resetVariables = true) {
        console.log('hideAllModals called, resetVariables:', resetVariables);
        // Force hide all modals on init
        const modals = ['userModal', 'roleModal', 'deleteModal'];
        modals.forEach(modalId => {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.classList.remove('show');
            }
        });

        // Reset body state
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.width = '';

        // Only reset variables if explicitly requested (like on init)
        if (resetVariables) {
            this.currentUserId = null;
            this.currentUserForRole = null;
            this.currentUserForDelete = null;
        }
    }

    showLoading(show) {
        const loadingOverlay = document.getElementById('loadingOverlay');
        if (loadingOverlay) {
            loadingOverlay.style.display = show ? 'flex' : 'none';
        }
    }

    refreshData() {
        this.loadUsers();
        this.showToast('info', 'Thông báo', 'Đã làm mới dữ liệu');
    }

    togglePassword() {
        const passwordInput = document.getElementById('matKhau');
        const passwordIcon = document.getElementById('passwordIcon');

        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            passwordIcon.classList.remove('fa-eye');
            passwordIcon.classList.add('fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            passwordIcon.classList.remove('fa-eye-slash');
            passwordIcon.classList.add('fa-eye');
        }
    }

    showToast(type, title, message) {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) return;

        const toastId = 'toast-' + Date.now();
        const iconMap = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            warning: 'fa-exclamation-triangle',
            info: 'fa-info-circle'
        };

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast ${type}`;
        toast.innerHTML = `
            <div class="toast-icon">
                <i class="fas ${iconMap[type]}"></i>
            </div>
            <div class="toast-content">
                <div class="toast-title">${title}</div>
                <div class="toast-message">${message}</div>
            </div>
            <button class="toast-close" onclick="accountManager.closeToast('${toastId}')">
                <i class="fas fa-times"></i>
            </button>
        `;

        toastContainer.appendChild(toast);

        // Auto remove after 5 seconds
        setTimeout(() => {
            this.closeToast(toastId);
        }, 5000);
    }

    closeToast(toastId) {
        const toast = document.getElementById(toastId);
        if (toast) {
            toast.style.animation = 'toastSlideOut 0.3s ease-in forwards';
            setTimeout(() => {
                toast.remove();
            }, 300);
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Global functions for onclick handlers
function openCreateModal() {
    accountManager.openCreateModal();
}

function closeUserModal() {
    accountManager.closeUserModal();
}

function closeRoleModal() {
    accountManager.closeRoleModal();
}

function closeDeleteModal() {
    accountManager.closeDeleteModal();
}

function saveUser() {
    accountManager.saveUser();
}

function confirmRoleChange() {
    accountManager.confirmRoleChange();
}

function confirmDelete() {
    accountManager.confirmDelete();
}

function refreshData() {
    accountManager.refreshData();
}

function togglePassword() {
    accountManager.togglePassword();
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Force hide all modals immediately
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        modal.classList.remove('show');
        modal.style.display = 'none';
        modal.style.opacity = '0';
        modal.style.visibility = 'hidden';
    });

    // Reset body state
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';

    // Initialize account manager
    window.accountManager = new AccountManager();
});

// Add CSS for toast slide out animation
const style = document.createElement('style');
style.textContent = `
    @keyframes toastSlideOut {
        from {
            opacity: 1;
            transform: translateX(0);
        }
        to {
            opacity: 0;
            transform: translateX(100%);
        }
    }
`;