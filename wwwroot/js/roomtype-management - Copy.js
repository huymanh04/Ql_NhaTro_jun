class BannerManagement {
    constructor() {
        this.banners = [];
        this.filteredBanners = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingBannerId = null;
        this.deletingBannerId = null;
        this.selectedImage = null;

        this.init();
    }

    init() {
        this.bindEvents();
        this.loadBanners();
    }

    bindEvents() {
        // Add banner button
        const addBtn = document.getElementById('addBannerBtn');
        if (addBtn) {
            addBtn.addEventListener('click', () => this.showAddModal());
        }

        // Modal buttons
        const closeModal = document.getElementById('closeModalBtn');
        const cancelModal = document.getElementById('cancelModalBtn');
        const saveBtn = document.getElementById('saveBannerBtn');

        if (closeModal) closeModal.addEventListener('click', () => this.hideModal());
        if (cancelModal) cancelModal.addEventListener('click', () => this.hideModal());
        if (saveBtn) saveBtn.addEventListener('click', () => this.saveBanner());

        // Delete modal buttons
        const closeDeleteModal = document.getElementById('closeDeleteModalBtn');
        const cancelDelete = document.getElementById('cancelDeleteBtn');
        const confirmDelete = document.getElementById('confirmDeleteBtn');

        if (closeDeleteModal) closeDeleteModal.addEventListener('click', () => this.hideDeleteModal());
        if (cancelDelete) cancelDelete.addEventListener('click', () => this.hideDeleteModal());
        if (confirmDelete) confirmDelete.addEventListener('click', () => this.confirmDelete());

        // Search and filters
        const searchInput = document.getElementById('searchInput');
        const statusFilter = document.getElementById('statusFilter');
        const clearFilters = document.getElementById('clearFiltersBtn');

        if (searchInput) searchInput.addEventListener('input', () => this.applyFilters());
        if (statusFilter) statusFilter.addEventListener('change', () => this.applyFilters());
        if (clearFilters) clearFilters.addEventListener('click', () => this.clearFilters());

        // Pagination
        const itemsPerPage = document.getElementById('itemsPerPage');
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');

        if (itemsPerPage) {
            itemsPerPage.addEventListener('change', (e) => {
                this.itemsPerPage = parseInt(e.target.value);
                this.currentPage = 1;
                this.renderBanners();
                this.updatePagination();
            });
        }

        if (prevBtn) prevBtn.addEventListener('click', () => this.previousPage());
        if (nextBtn) nextBtn.addEventListener('click', () => this.nextPage());

        // Modal backdrop clicks
        const bannerModal = document.getElementById('bannerModal');
        const deleteModal = document.getElementById('deleteModal');

        if (bannerModal) {
            bannerModal.addEventListener('click', (e) => {
                if (e.target.classList.contains('modal-backdrop')) {
                    this.hideModal();
                }
            });
        }

        if (deleteModal) {
            deleteModal.addEventListener('click', (e) => {
                if (e.target.classList.contains('modal-backdrop')) {
                    this.hideDeleteModal();
                }
            });
        }

        // Form handling
        const form = document.getElementById('bannerForm');
        if (form) {
            form.addEventListener('input', () => this.clearFieldErrors());
        }

        // Image handling
        const imageFile = document.getElementById('imageFile');
        if (imageFile) {
            imageFile.addEventListener('change', (e) => this.handleImagePreview(e));
        }

        // Content character counter
        const content = document.getElementById('content');
        if (content) {
            content.addEventListener('input', () => this.updateCharCounter());
        }

        // Remove image button (delegated event)
        document.addEventListener('click', (e) => {
            if (e.target.closest('#removeImageBtn')) {
                e.preventDefault();
                this.removeImagePreview();
            }
        });
    }

    async loadBanners() {
        this.showLoading(true);
        try {
            const response = await fetch('/api/Banner/get-banner');
            const result = await response.json();

            if (response.ok && result.success) {
                this.banners = result.data.map(banner => ({
                    id: banner.bannerId,
                    title: banner.title || '',
                    content: banner.content || '',
                    text: banner.text || '',
                    image: banner.imageBase64 ? `data:image/png;base64,${banner.imageBase64}` : null,
                    redirectUrl: banner.redirectUrl || '',
                    isActive: banner.isActive !== false,
                    createdAt: banner.createdAt ? new Date(banner.createdAt).toLocaleDateString('vi-VN') : ''
                }));
                this.filteredBanners = [...this.banners];
                this.renderBanners();
                this.updatePagination();
            } else {
                throw new Error(result.message || 'Không thể tải danh sách banner');
            }
        } catch (error) {
            console.error('Error loading banners:', error);

            // Use test data if API fails
            this.banners = [
                {
                    id: 1,
                    title: "Banner Test 1",
                    content: "Đây là nội dung test cho banner đầu tiên",
                    text: "Xem chi tiết",
                    image: null,
                    redirectUrl: "https://example.com",
                    isActive: true,
                    createdAt: new Date().toLocaleDateString('vi-VN')
                },
                {
                    id: 2,
                    title: "Banner Test 2",
                    content: "Đây là nội dung test cho banner thứ hai",
                    text: "Mua ngay",
                    image: null,
                    redirectUrl: "https://example2.com",
                    isActive: false,
                    createdAt: new Date().toLocaleDateString('vi-VN')
                }
            ];
            this.filteredBanners = [...this.banners];
            this.renderBanners();
            this.updatePagination();
            this.showAlert('Đang sử dụng dữ liệu test. Không thể kết nối API: ' + error.message, 'warning');
        } finally {
            this.showLoading(false);
        }
    }

    renderBanners() {
        this.renderTable();
        this.renderMobileCards();
    }

    renderTable() {
        const tbody = document.getElementById('bannersTableBody');
        if (!tbody) return;

        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageBanners = this.filteredBanners.slice(startIndex, endIndex);

        tbody.innerHTML = '';

        if (pageBanners.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center" style="padding: 40px;">
                        <i class="fas fa-images" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px; display: block;"></i>
                        <p style="margin: 0; color: #6b7280; font-size: 16px;">Không tìm thấy banner nào</p>
                    </td>
                </tr>
            `;
            return;
        }

        pageBanners.forEach((banner, index) => {
            const row = document.createElement('tr');
            const stt = startIndex + index + 1;

            const imageHtml = banner.image
                ? `<img src="${banner.image}" class="banner-image" alt="${banner.title}">`
                : `<div class="no-image"><i class="fas fa-image"></i></div>`;

            const contentDisplay = banner.content.length > 50
                ? banner.content.substring(0, 50) + '...'
                : banner.content || 'Không có nội dung';

            const statusHtml = banner.isActive
                ? '<span class="status-badge status-active">Hoạt động</span>'
                : '<span class="status-badge status-inactive">Không hoạt động</span>';

            const urlHtml = banner.redirectUrl
                ? `<a href="${banner.redirectUrl}" target="_blank" style="color: #667eea; text-decoration: none;"><i class="fas fa-external-link-alt"></i></a>`
                : '<span style="color: #9ca3af;">-</span>';

            row.innerHTML = `
                <td><strong>${stt}</strong></td>
                <td>${imageHtml}</td>
                <td><strong>${this.escapeHtml(banner.title)}</strong></td>
                <td title="${this.escapeHtml(banner.content)}">${this.escapeHtml(contentDisplay)}</td>
                <td>${statusHtml}</td>
                <td style="text-align: center;">${urlHtml}</td>
                <td>${banner.createdAt}</td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-action btn-edit" data-id="${banner.id}" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-delete" data-id="${banner.id}" data-name="${banner.title}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            `;

            // Add event listeners
            const editBtn = row.querySelector('.btn-edit');
            const deleteBtn = row.querySelector('.btn-delete');

            editBtn.addEventListener('click', () => this.showEditModal(banner.id));
            deleteBtn.addEventListener('click', () => this.showDeleteModal(banner.id, banner.title));

            tbody.appendChild(row);
        });
    }

    renderMobileCards() {
        const container = document.getElementById('mobileBannerCards');
        if (!container) return;

        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageBanners = this.filteredBanners.slice(startIndex, endIndex);

        container.innerHTML = '';

        if (pageBanners.length === 0) {
            container.innerHTML = `
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-images" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px;"></i>
                    <p style="margin: 0; color: #6b7280; font-size: 16px;">Không tìm thấy banner nào</p>
                </div>
            `;
            return;
        }

        pageBanners.forEach((banner, index) => {
            const stt = startIndex + index + 1;
            const card = document.createElement('div');
            card.className = 'mobile-card';

            const statusHtml = banner.isActive
                ? '<span class="status-badge status-active">Hoạt động</span>'
                : '<span class="status-badge status-inactive">Không hoạt động</span>';

            card.innerHTML = `
                <div class="mobile-card-header">
                    <h3 class="mobile-card-title">${this.escapeHtml(banner.title)}</h3>
                    <span class="mobile-card-id">#${stt}</span>
                </div>
                
                ${banner.image ? `<img src="${banner.image}" class="mobile-card-image" alt="${banner.title}">` : ''}
                
                <div class="mobile-card-field">
                    <label>Nội dung:</label>
                    <span>${this.escapeHtml(banner.content || 'Không có nội dung')}</span>
                </div>
                
                <div class="mobile-card-field">
                    <label>Trạng thái:</label>
                    ${statusHtml}
                </div>
                
                <div class="mobile-card-field">
                    <label>URL:</label>
                    ${banner.redirectUrl
                    ? `<a href="${banner.redirectUrl}" target="_blank" style="color: #667eea;">${banner.redirectUrl}</a>`
                    : '<span style="color: #9ca3af;">Không có</span>'
                }
                </div>
                
                <div class="mobile-card-field">
                    <label>Ngày tạo:</label>
                    <span>${banner.createdAt}</span>
                </div>
                
                <div class="mobile-card-actions">
                    <button class="btn btn-secondary btn-edit-mobile" data-id="${banner.id}">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-danger btn-delete-mobile" data-id="${banner.id}" data-name="${banner.title}">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </div>
            `;

            // Add event listeners
            const editBtn = card.querySelector('.btn-edit-mobile');
            const deleteBtn = card.querySelector('.btn-delete-mobile');

            editBtn.addEventListener('click', () => this.showEditModal(banner.id));
            deleteBtn.addEventListener('click', () => this.showDeleteModal(banner.id, banner.title));

            container.appendChild(card);
        });
    }

    applyFilters() {
        const searchTerm = document.getElementById('searchInput')?.value.toLowerCase().trim() || '';
        const statusFilter = document.getElementById('statusFilter')?.value || '';

        this.filteredBanners = this.banners.filter(banner => {
            const matchesSearch = !searchTerm ||
                banner.title.toLowerCase().includes(searchTerm) ||
                banner.content.toLowerCase().includes(searchTerm);

            const matchesStatus = !statusFilter ||
                banner.isActive.toString() === statusFilter;

            return matchesSearch && matchesStatus;
        });

        this.currentPage = 1;
        this.renderBanners();
        this.updatePagination();
    }

    clearFilters() {
        const searchInput = document.getElementById('searchInput');
        const statusFilter = document.getElementById('statusFilter');

        if (searchInput) searchInput.value = '';
        if (statusFilter) statusFilter.value = '';

        this.filteredBanners = [...this.banners];
        this.currentPage = 1;
        this.renderBanners();
        this.updatePagination();
    }

    showAddModal() {
        this.editingBannerId = null;
        this.clearForm();

        const modalTitle = document.getElementById('modalTitleText');
        const saveBtn = document.getElementById('saveBtnText');

        if (modalTitle) modalTitle.textContent = 'Thêm Banner';
        if (saveBtn) saveBtn.textContent = 'Lưu';

        this.showModal();
    }

    showEditModal(bannerId) {
        const banner = this.banners.find(b => b.id === bannerId);
        if (!banner) {
            this.showAlert('Không tìm thấy banner', 'error');
            return;
        }

        this.editingBannerId = bannerId;
        this.clearForm();

        // Fill form
        const title = document.getElementById('title');
        const content = document.getElementById('content');
        const text = document.getElementById('text');
        const redirectUrl = document.getElementById('redirectUrl');
        const isActive = document.getElementById('isActive');

        if (title) title.value = banner.title;
        if (content) content.value = banner.content;
        if (text) text.value = banner.text;
        if (redirectUrl) redirectUrl.value = banner.redirectUrl;
        if (isActive) isActive.value = banner.isActive.toString();

        this.updateCharCounter();

        // Show image if exists
        if (banner.image) {
            this.showImagePreview(banner.image);
        }

        const modalTitle = document.getElementById('modalTitleText');
        const saveBtn = document.getElementById('saveBtnText');

        if (modalTitle) modalTitle.textContent = 'Chỉnh sửa Banner';
        if (saveBtn) saveBtn.textContent = 'Cập nhật';

        this.showModal();
    }

    showDeleteModal(bannerId, bannerTitle) {
        this.deletingBannerId = bannerId;

        const nameElement = document.getElementById('deleteBannerName');
        if (nameElement) nameElement.textContent = bannerTitle;

        const modal = document.getElementById('deleteModal');
        if (modal) {
            modal.style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }
    }

    showModal() {
        const modal = document.getElementById('bannerModal');
        if (modal) {
            modal.style.display = 'flex';
            document.body.style.overflow = 'hidden';

            // Focus first input
            setTimeout(() => {
                const firstInput = modal.querySelector('#title');
                if (firstInput) firstInput.focus();
            }, 100);
        }
    }

    hideModal() {
        const modal = document.getElementById('bannerModal');
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
            this.clearForm();
            this.clearFieldErrors();
        }
    }

    hideDeleteModal() {
        const modal = document.getElementById('deleteModal');
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
            this.deletingBannerId = null;
        }
    }

    clearForm() {
        const form = document.getElementById('bannerForm');
        if (form) form.reset();

        this.selectedImage = null;
        this.removeImagePreview();
        this.updateCharCounter();
    }

    async saveBanner() {
        if (!this.validateForm()) return;

        const saveBtn = document.getElementById('saveBannerBtn');
        const originalText = saveBtn?.innerHTML || '';

        try {
            if (saveBtn) {
                saveBtn.disabled = true;
                saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
            }

            const formData = new FormData();

            const title = document.getElementById('title')?.value.trim() || '';
            const content = document.getElementById('content')?.value.trim() || '';
            const text = document.getElementById('text')?.value.trim() || '';
            const redirectUrl = document.getElementById('redirectUrl')?.value.trim() || '';
            const isActive = document.getElementById('isActive')?.value || 'true';

            formData.append('Title', title);
            formData.append('Content', content);
            formData.append('Text', text);
            formData.append('RedirectUrl', redirectUrl);
            formData.append('IsActive', isActive);

            if (this.selectedImage) {
                formData.append('ImageFile', this.selectedImage);
            }

            let url = '/api/Banner/add-banner';
            let method = 'POST';

            if (this.editingBannerId) {
                url = `/api/Banner/edit-banner/${this.editingBannerId}`;
                method = 'PUT';
            }

            const response = await fetch(url, {
                method: method,
                body: formData
            });

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert(
                    this.editingBannerId ? 'Cập nhật banner thành công!' : 'Thêm banner thành công!',
                    'success'
                );
                this.hideModal();
                await this.loadBanners();
            } else {
                throw new Error(result.message || 'Có lỗi xảy ra khi lưu banner');
            }
        } catch (error) {
            console.error('Error saving banner:', error);
            this.showAlert('Có lỗi xảy ra: ' + error.message, 'error');
        } finally {
            if (saveBtn) {
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalText;
            }
        }
    }

    async confirmDelete() {
        if (!this.deletingBannerId) return;

        const deleteBtn = document.getElementById('confirmDeleteBtn');
        const originalText = deleteBtn?.innerHTML || '';

        try {
            if (deleteBtn) {
                deleteBtn.disabled = true;
                deleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';
            }

            const response = await fetch(`/api/Banner/delete-banner/${this.deletingBannerId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert('Xóa banner thành công!', 'success');
                this.hideDeleteModal();
                await this.loadBanners();
            } else {
                throw new Error(result.message || 'Có lỗi xảy ra khi xóa banner');
            }
        } catch (error) {
            console.error('Error deleting banner:', error);
            this.showAlert('Có lỗi xảy ra: ' + error.message, 'error');
        } finally {
            if (deleteBtn) {
                deleteBtn.disabled = false;
                deleteBtn.innerHTML = originalText;
            }
        }
    }

    validateForm() {
        let isValid = true;
        this.clearFieldErrors();

        // Validate title
        const title = document.getElementById('title')?.value.trim() || '';
        if (!title) {
            this.showFieldError('titleError', 'Vui lòng nhập tên banner');
            isValid = false;
        } else if (title.length > 255) {
            this.showFieldError('titleError', 'Tên banner không được vượt quá 255 ký tự');
            isValid = false;
        }

        // Validate content length
        const content = document.getElementById('content')?.value.trim() || '';
        if (content.length > 200) {
            this.showFieldError('contentError', 'Nội dung không được vượt quá 200 ký tự');
            isValid = false;
        }

        // Validate status
        const isActive = document.getElementById('isActive')?.value;
        if (!isActive) {
            this.showFieldError('isActiveError', 'Vui lòng chọn trạng thái');
            isValid = false;
        }

        // Validate URL
        const redirectUrl = document.getElementById('redirectUrl')?.value.trim() || '';
        if (redirectUrl && !this.isValidUrl(redirectUrl)) {
            this.showFieldError('redirectUrlError', 'URL không hợp lệ');
            isValid = false;
        }

        // Validate image
        if (this.selectedImage && this.selectedImage.size > 5 * 1024 * 1024) {
            this.showFieldError('imageFileError', 'Kích thước ảnh không được vượt quá 5MB');
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

    showFieldError(errorId, message) {
        const errorElement = document.getElementById(errorId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    clearFieldErrors() {
        const errors = document.querySelectorAll('.error-message');
        errors.forEach(error => {
            error.textContent = '';
            error.classList.remove('show');
        });
    }

    handleImagePreview(event) {
        const file = event.target.files[0];
        if (!file) {
            this.removeImagePreview();
            return;
        }

        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowedTypes.includes(file.type)) {
            this.showAlert('Chỉ chấp nhận file ảnh (JPG, PNG, GIF)', 'error');
            event.target.value = '';
            return;
        }

        // Validate file size
        if (file.size > 5 * 1024 * 1024) {
            this.showAlert('Kích thước ảnh không được vượt quá 5MB', 'error');
            event.target.value = '';
            return;
        }

        this.selectedImage = file;

        // Show preview
        const reader = new FileReader();
        reader.onload = (e) => {
            this.showImagePreview(e.target.result);
        };
        reader.readAsDataURL(file);
    }

    showImagePreview(src) {
        const preview = document.getElementById('imagePreview');
        const img = document.getElementById('previewImg');

        if (preview && img) {
            img.src = src;
            preview.style.display = 'block';
        }
    }

    removeImagePreview() {
        const preview = document.getElementById('imagePreview');
        const img = document.getElementById('previewImg');
        const fileInput = document.getElementById('imageFile');

        if (preview) preview.style.display = 'none';
        if (img) img.src = '';
        if (fileInput) fileInput.value = '';

        this.selectedImage = null;
    }

    updateCharCounter() {
        const content = document.getElementById('content');
        const counter = document.getElementById('contentCounter');

        if (content && counter) {
            const length = content.value.length;
            counter.textContent = `${length}/200`;

            if (length > 200) {
                counter.style.color = '#ef4444';
            } else if (length > 150) {
                counter.style.color = '#f59e0b';
            } else {
                counter.style.color = '#6b7280';
            }
        }
    }

    updatePagination() {
        const totalItems = this.filteredBanners.length;
        const totalPages = Math.ceil(totalItems / this.itemsPerPage);

        // Update info
        const startItem = totalItems === 0 ? 0 : (this.currentPage - 1) * this.itemsPerPage + 1;
        const endItem = Math.min(this.currentPage * this.itemsPerPage, totalItems);

        const paginationInfo = document.getElementById('paginationInfo');
        if (paginationInfo) {
            paginationInfo.textContent = `Hiển thị ${startItem} - ${endItem} của ${totalItems} banners`;
        }

        // Update buttons
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');

        if (prevBtn) prevBtn.disabled = this.currentPage <= 1;
        if (nextBtn) nextBtn.disabled = this.currentPage >= totalPages;

        // Render page numbers
        this.renderPageNumbers(totalPages);
    }

    renderPageNumbers(totalPages) {
        const container = document.getElementById('pageNumbers');
        if (!container) return;

        container.innerHTML = '';

        if (totalPages <= 1) return;

        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            const button = document.createElement('button');
            button.className = `page-number ${i === this.currentPage ? 'active' : ''}`;
            button.textContent = i;
            button.addEventListener('click', () => this.goToPage(i));
            container.appendChild(button);
        }
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.goToPage(this.currentPage - 1);
        }
    }

    nextPage() {
        const totalPages = Math.ceil(this.filteredBanners.length / this.itemsPerPage);
        if (this.currentPage < totalPages) {
            this.goToPage(this.currentPage + 1);
        }
    }

    goToPage(page) {
        this.currentPage = page;
        this.renderBanners();
        this.updatePagination();

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    showLoading(show = true) {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.style.display = show ? 'flex' : 'none';
        }
    }

    showAlert(message, type = 'info') {
        const container = document.getElementById('alertContainer');
        if (!container) return;

        // Remove existing alerts
        container.innerHTML = '';

        const alert = document.createElement('div');
        alert.className = `alert alert-${type}`;

        const iconMap = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle'
        };

        alert.innerHTML = `
            <i class="${iconMap[type] || iconMap.info}"></i>
            <span>${message}</span>
            <button class="alert-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Auto hide after 5 seconds
        setTimeout(() => {
            if (alert.parentNode) {
                alert.remove();
            }
        }, 5000);

        // Close button
        alert.querySelector('.alert-close').addEventListener('click', () => {
            alert.remove();
        });

        container.appendChild(alert);
    }

    escapeHtml(unsafe) {
        if (!unsafe) return '';
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.bannerManagement = new BannerManagement();
}); 