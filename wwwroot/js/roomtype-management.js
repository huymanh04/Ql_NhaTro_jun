class RoomTypeManagement {
    constructor() {
        this.roomTypes = [];
        this.filteredRoomTypes = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingRoomTypeId = null;
        this.deletingRoomTypeId = null;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadRoomTypes();
    }

    bindEvents() {
        // Add room type button
        document.getElementById('addRoomTypeBtn').addEventListener('click', () => this.showAddModal());
        
        // Modal close buttons
        document.getElementById('closeModalBtn').addEventListener('click', () => this.hideModal());
        document.getElementById('cancelModalBtn').addEventListener('click', () => this.hideModal());
        document.getElementById('closeDeleteModalBtn').addEventListener('click', () => this.hideDeleteModal());
        document.getElementById('cancelDeleteBtn').addEventListener('click', () => this.hideDeleteModal());
        
        // Save button
        document.getElementById('saveRoomTypeBtn').addEventListener('click', () => this.saveRoomType());
        
        // Delete confirmation
        document.getElementById('confirmDeleteBtn').addEventListener('click', () => this.confirmDelete());
        
        // Search
        document.getElementById('searchInput').addEventListener('input', () => this.applyFilters());
        document.getElementById('clearFiltersBtn').addEventListener('click', () => this.clearFilters());
        
        // Pagination
        document.getElementById('prevPageBtn').addEventListener('click', () => this.previousPage());
        document.getElementById('nextPageBtn').addEventListener('click', () => this.nextPage());
        
        // Modal backdrop click
        document.getElementById('roomTypeModal').addEventListener('click', (e) => {
            if (e.target === e.currentTarget) this.hideModal();
        });
        document.getElementById('deleteModal').addEventListener('click', (e) => {
            if (e.target === e.currentTarget) this.hideDeleteModal();
        });
        
        // Form validation
        document.getElementById('roomTypeForm').addEventListener('input', () => this.clearFieldErrors());
        
        // Image file handling
        document.getElementById('imageFile').addEventListener('change', (e) => this.handleImagePreview(e));
        document.getElementById('removeImageBtn').addEventListener('click', () => this.removeImagePreview());
    }

    async loadRoomTypes() {
        this.showLoading(true);
        try {
            const response = await fetch('/api/RoomType/get-type-room');
            const result = await response.json();
            
            if (response.ok && result.success) {
                this.roomTypes = result.data;
                this.filteredRoomTypes = [...this.roomTypes];
                this.renderRoomTypes();
                this.updatePagination();
            } else {
                throw new Error(result.message || 'Failed to load room types');
            }
        } catch (error) {
            console.error('Error loading room types:', error);
            this.showAlert('Lỗi khi tải danh sách loại phòng: ' + error.message, 'error');
        } finally {
            this.showLoading(false);
        }
    }

    renderRoomTypes() {
        this.renderTable();
        this.renderMobileCards();
    }

    renderTable() {
        const tbody = document.getElementById('roomTypesTableBody');
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageRoomTypes = this.filteredRoomTypes.slice(startIndex, endIndex);

        tbody.innerHTML = '';

        if (pageRoomTypes.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center" style="padding: 40px;">
                        <i class="fas fa-search" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px;"></i>
                        <p style="margin: 0; color: #6b7280;">Không tìm thấy loại phòng nào</p>
                    </td>
                </tr>
            `;
            return;
        }

        pageRoomTypes.forEach(roomType => {
            const row = document.createElement('tr');
            
            const imageHtml = roomType.imageBase64 
                ? `<img src="data:image/jpeg;base64,${roomType.imageBase64}" class="room-image" alt="${roomType.tenTheLoai}">`
                : `<div class="no-image">Không có ảnh</div>`;
            
            const description = roomType.moTa 
                ? (roomType.moTa.length > 50 ? roomType.moTa.substring(0, 50) + '...' : roomType.moTa)
                : 'Không có mô tả';
            
            const redirectUrl = roomType.redirectUrl 
                ? `<a href="${roomType.redirectUrl}" target="_blank" class="text-blue-600 hover:underline">${roomType.redirectUrl.length > 30 ? roomType.redirectUrl.substring(0, 30) + '...' : roomType.redirectUrl}</a>`
                : 'Không có';

            row.innerHTML = `
                <td><strong>#${roomType.maTheLoai}</strong></td>
                <td>${imageHtml}</td>
                <td><strong>${roomType.tenTheLoai}</strong></td>
                <td title="${roomType.moTa || ''}">${description}</td>
                <td><span class="badge">${roomType.soLuongPhong || 0} phòng</span></td>
                <td>${redirectUrl}</td>
                <td class="actions">
                    <button class="btn btn-edit" data-room-type-id="${roomType.maTheLoai}" data-action="edit">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-delete" data-room-type-id="${roomType.maTheLoai}" data-room-type-name="${roomType.tenTheLoai}" data-action="delete">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </td>
            `;
            
            // Add event listeners to buttons
            const editBtn = row.querySelector('[data-action="edit"]');
            const deleteBtn = row.querySelector('[data-action="delete"]');
            
            editBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showEditModal(roomType.maTheLoai);
            });
            
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showDeleteModal(roomType.maTheLoai, roomType.tenTheLoai);
            });
            
            tbody.appendChild(row);
        });
    }

    renderMobileCards() {
        const container = document.getElementById('mobileRoomTypeCards');
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageRoomTypes = this.filteredRoomTypes.slice(startIndex, endIndex);

        container.innerHTML = '';

        if (pageRoomTypes.length === 0) {
            container.innerHTML = `
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-search" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px;"></i>
                    <p style="margin: 0; color: #6b7280;">Không tìm thấy loại phòng nào</p>
                </div>
            `;
            return;
        }

        pageRoomTypes.forEach(roomType => {
            const card = document.createElement('div');
            card.className = 'roomtype-card';
            
            const imageHtml = roomType.imageBase64 
                ? `<img src="data:image/jpeg;base64,${roomType.imageBase64}" class="roomtype-card-image" alt="${roomType.tenTheLoai}">`
                : '';

            card.innerHTML = `
                <div class="roomtype-card-header">
                    <h3 class="roomtype-card-title">${roomType.tenTheLoai}</h3>
                    <span class="roomtype-card-id">#${roomType.maTheLoai}</span>
                </div>
                
                ${imageHtml}
                
                <div class="roomtype-card-info">
                    <div class="info-row">
                        <span class="info-label">Mô tả:</span>
                        <span class="info-value">${roomType.moTa || 'Không có mô tả'}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Số phòng:</span>
                        <span class="info-value">${roomType.soLuongPhong || 0} phòng</span>
                    </div>
                    ${roomType.redirectUrl ? `
                    <div class="info-row">
                        <span class="info-label">URL:</span>
                        <span class="info-value"><a href="${roomType.redirectUrl}" target="_blank" class="text-blue-600">${roomType.redirectUrl}</a></span>
                    </div>
                    ` : ''}
                </div>
                
                <div class="roomtype-card-actions">
                    <button class="btn btn-edit" data-room-type-id="${roomType.maTheLoai}" data-action="edit">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-delete" data-room-type-id="${roomType.maTheLoai}" data-room-type-name="${roomType.tenTheLoai}" data-action="delete">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </div>
            `;
            
            // Add event listeners to buttons
            const editBtn = card.querySelector('[data-action="edit"]');
            const deleteBtn = card.querySelector('[data-action="delete"]');
            
            editBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showEditModal(roomType.maTheLoai);
            });
            
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showDeleteModal(roomType.maTheLoai, roomType.tenTheLoai);
            });
            
            container.appendChild(card);
        });
    }

    applyFilters() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();

        this.filteredRoomTypes = this.roomTypes.filter(roomType => {
            const matchesSearch = !searchTerm || 
                roomType.tenTheLoai.toLowerCase().includes(searchTerm) ||
                (roomType.moTa && roomType.moTa.toLowerCase().includes(searchTerm));

            return matchesSearch;
        });

        this.currentPage = 1;
        this.renderRoomTypes();
        this.updatePagination();
    }

    clearFilters() {
        document.getElementById('searchInput').value = '';
        this.applyFilters();
    }

    showAddModal() {
        this.editingRoomTypeId = null;
        document.getElementById('modalTitleText').textContent = 'Thêm loại phòng';
        document.getElementById('saveBtnText').textContent = 'Thêm';
        this.clearForm();
        this.showModal();
    }

    showEditModal(roomTypeId) {
        const roomType = this.roomTypes.find(rt => rt.maTheLoai === roomTypeId);
        if (!roomType) return;

        this.editingRoomTypeId = roomTypeId;
        document.getElementById('modalTitleText').textContent = 'Sửa loại phòng';
        document.getElementById('saveBtnText').textContent = 'Cập nhật';
        
        // Populate form
        document.getElementById('tenTheLoai').value = roomType.tenTheLoai || '';
        document.getElementById('moTa').value = roomType.moTa || '';
        document.getElementById('redirectUrl').value = roomType.redirectUrl || '';
        
        // Show existing image if available
        if (roomType.imageBase64) {
            this.showImagePreview(`data:image/jpeg;base64,${roomType.imageBase64}`);
        }
        
        this.showModal();
    }

    showDeleteModal(roomTypeId, roomTypeName) {
        this.deletingRoomTypeId = roomTypeId;
        document.getElementById('deleteRoomTypeName').textContent = roomTypeName;
        this.showDeleteModalElement();
    }

    showModal() {
        const modal = document.getElementById('roomTypeModal');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
        
        // Scroll to top of page to ensure modal is fully visible
        setTimeout(() => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        }, 100);
        
        // Focus on first input after modal is shown and scroll is complete
        setTimeout(() => {
            document.getElementById('tenTheLoai').focus();
        }, 400);
    }

    hideModal() {
        const modal = document.getElementById('roomTypeModal');
        modal.classList.remove('show');
        document.body.style.overflow = '';
        
        this.clearForm();
        this.clearFieldErrors();
    }

    showDeleteModalElement() {
        const modal = document.getElementById('deleteModal');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
        
        // Scroll to top of page to ensure modal is fully visible
        setTimeout(() => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        }, 100);
    }

    hideDeleteModal() {
        const modal = document.getElementById('deleteModal');
        modal.classList.remove('show');
        document.body.style.overflow = '';
        
        this.deletingRoomTypeId = null;
    }

    clearForm() {
        document.getElementById('roomTypeForm').reset();
        this.removeImagePreview();
    }

    async saveRoomType() {
        if (!this.validateForm()) return;

        const formData = this.getFormData();
        const saveBtn = document.getElementById('saveRoomTypeBtn');
        const originalText = saveBtn.innerHTML;

        try {
            saveBtn.disabled = true;
            saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

            let response;
            if (this.editingRoomTypeId) {
                // Update
                response = await fetch(`/api/RoomType/edit-type-room/${this.editingRoomTypeId}`, {
                    method: 'PUT',
                    body: formData
                });
            } else {
                // Create
                response = await fetch('/api/RoomType/add-type-room', {
                    method: 'POST',
                    body: formData
                });
            }

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert(result.message || (this.editingRoomTypeId ? 'Cập nhật thành công!' : 'Thêm thành công!'), 'success');
                this.hideModal();
                await this.loadRoomTypes();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra', 'error');
            }
        } catch (error) {
            console.error('Error saving room type:', error);
            this.showAlert('Lỗi kết nối: ' + error.message, 'error');
        } finally {
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalText;
        }
    }

    async confirmDelete() {
        if (!this.deletingRoomTypeId) return;

        const deleteBtn = document.getElementById('confirmDeleteBtn');
        const originalText = deleteBtn.innerHTML;

        try {
            deleteBtn.disabled = true;
            deleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';

            const response = await fetch(`/api/RoomType/delete-type-room/${this.deletingRoomTypeId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert(result.message || 'Xóa thành công!', 'success');
                this.hideDeleteModal();
                await this.loadRoomTypes();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra khi xóa', 'error');
            }
        } catch (error) {
            console.error('Error deleting room type:', error);
            this.showAlert('Lỗi kết nối: ' + error.message, 'error');
        } finally {
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalText;
        }
    }

    getFormData() {
        const formData = new FormData();
        
        formData.append('tenTheLoai', document.getElementById('tenTheLoai').value.trim());
        formData.append('moTa', document.getElementById('moTa').value.trim());
        formData.append('redirectUrl', document.getElementById('redirectUrl').value.trim());
        
        const imageFile = document.getElementById('imageFile').files[0];
        if (imageFile) {
            formData.append('imageFile', imageFile);
        }
        
        return formData;
    }

    validateForm() {
        this.clearFieldErrors();
        let isValid = true;

        const tenTheLoai = document.getElementById('tenTheLoai').value.trim();
        const imageFile = document.getElementById('imageFile').files[0];
        const redirectUrl = document.getElementById('redirectUrl').value.trim();

        if (!tenTheLoai) {
            this.showFieldError('tenTheLoai', 'Tên loại phòng không được để trống');
            isValid = false;
        } else if (tenTheLoai.length > 100) {
            this.showFieldError('tenTheLoai', 'Tên loại phòng không được vượt quá 100 ký tự');
            isValid = false;
        }

        if (imageFile) {
            const maxSize = 5 * 1024 * 1024; // 5MB
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
            
            if (imageFile.size > maxSize) {
                this.showFieldError('imageFile', 'Kích thước ảnh không được vượt quá 5MB');
                isValid = false;
            } else if (!allowedTypes.includes(imageFile.type.toLowerCase())) {
                this.showFieldError('imageFile', 'Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF');
                isValid = false;
            }
        }

        if (redirectUrl && !this.isValidUrl(redirectUrl)) {
            this.showFieldError('redirectUrl', 'URL không hợp lệ');
            isValid = false;
        }

        return isValid;
    }

    isValidUrl(string) {
        try {
            new URL(string);
            return true;
        } catch (_) {
            return false;
        }
    }

    showFieldError(fieldName, message) {
        const field = document.getElementById(fieldName);
        const errorElement = document.getElementById(fieldName + 'Error');
        
        if (field && errorElement) {
            field.parentElement.classList.add('error');
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    clearFieldErrors() {
        document.querySelectorAll('.form-group.error').forEach(group => {
            group.classList.remove('error');
        });
        document.querySelectorAll('.error-message.show').forEach(error => {
            error.classList.remove('show');
            error.textContent = '';
        });
    }

    handleImagePreview(event) {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                this.showImagePreview(e.target.result);
            };
            reader.readAsDataURL(file);
        }
    }

    showImagePreview(src) {
        const preview = document.getElementById('imagePreview');
        const img = document.getElementById('previewImg');
        
        img.src = src;
        preview.style.display = 'block';
    }

    removeImagePreview() {
        const preview = document.getElementById('imagePreview');
        const img = document.getElementById('previewImg');
        const fileInput = document.getElementById('imageFile');
        
        preview.style.display = 'none';
        img.src = '';
        fileInput.value = '';
    }

    updatePagination() {
        const totalItems = this.filteredRoomTypes.length;
        const totalPages = Math.ceil(totalItems / this.itemsPerPage);
        const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endIndex = Math.min(this.currentPage * this.itemsPerPage, totalItems);

        // Update info text
        document.getElementById('paginationInfo').textContent = 
            `Hiển thị ${totalItems > 0 ? startIndex : 0} - ${endIndex} của ${totalItems} loại phòng`;

        // Update navigation buttons
        document.getElementById('prevPageBtn').disabled = this.currentPage <= 1;
        document.getElementById('nextPageBtn').disabled = this.currentPage >= totalPages;

        // Update page numbers
        this.renderPageNumbers(totalPages);
    }

    renderPageNumbers(totalPages) {
        const container = document.getElementById('pageNumbers');
        container.innerHTML = '';

        if (totalPages <= 1) return;

        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            const pageBtn = document.createElement('button');
            pageBtn.className = `page-number ${i === this.currentPage ? 'active' : ''}`;
            pageBtn.textContent = i;
            pageBtn.addEventListener('click', () => this.goToPage(i));
            container.appendChild(pageBtn);
        }
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.goToPage(this.currentPage - 1);
        }
    }

    nextPage() {
        const totalPages = Math.ceil(this.filteredRoomTypes.length / this.itemsPerPage);
        if (this.currentPage < totalPages) {
            this.goToPage(this.currentPage + 1);
        }
    }

    goToPage(page) {
        this.currentPage = page;
        this.renderRoomTypes();
        this.updatePagination();
    }

    showLoading(show) {
        document.getElementById('loadingOverlay').style.display = show ? 'flex' : 'none';
    }

    showAlert(message, type) {
        const container = document.getElementById('alertContainer');
        const alertId = 'alert-' + Date.now();
        
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type}">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-triangle' : 'info-circle'}"></i>
                ${message}
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', alertHtml);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            const alertEl = document.getElementById(alertId);
            if (alertEl) {
                alertEl.style.opacity = '0';
                alertEl.style.transform = 'translateY(-10px)';
                setTimeout(() => alertEl.remove(), 300);
            }
        }, 5000);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.roomTypeManagement = new RoomTypeManagement();
}); 