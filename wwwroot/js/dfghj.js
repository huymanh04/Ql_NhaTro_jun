// Banner Management JavaScript
class BannerManagement {
    constructor() {
        this.banners = [];
        this.filteredBanners = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingBannerId = null;
        this.selectedImage = null;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadData();
    }

    bindEvents() {
        // Add banner button
        document.getElementById('addBannerBtn').addEventListener('click', () => {
            this.openAddModal();
        });

        // Modal close buttons
        document.getElementById('closeModalBtn').addEventListener('click', () => {
            this.closeModal();
        });
        
        document.getElementById('cancelModalBtn').addEventListener('click', () => {
            this.closeModal();
        });

        // Save banner button
        document.getElementById('saveBannerBtn').addEventListener('click', () => {
            this.saveBanner();
        });

        // Delete modal buttons
        document.getElementById('closeDeleteModalBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('cancelDeleteBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('confirmDeleteBtn').addEventListener('click', () => {
            this.deleteBanner();
        });

        // Search and filters
        document.getElementById('searchInput').addEventListener('input', (e) => {
            this.filterBanners();
        });
        
        document.getElementById('statusFilter').addEventListener('change', () => {
            this.filterBanners();
        });

        document.getElementById('clearFiltersBtn').addEventListener('click', () => {
            this.clearFilters();
        });

        // Items per page change
        document.getElementById('itemsPerPage').addEventListener('change', (e) => {
            this.itemsPerPage = parseInt(e.target.value);
            this.currentPage = 1; // Reset to first page
            this.renderTable();
            this.renderPagination();
        });

        // Image upload
        document.getElementById('imageFile').addEventListener('change', (e) => {
            this.handleImageUpload(e);
        });

        // Content character counter
        document.getElementById('content').addEventListener('input', (e) => {
            this.updateCharCounter(e.target);
        });

        // Pagination
        document.getElementById('prevPageBtn').addEventListener('click', () => {
            if (this.currentPage > 1) {
                this.currentPage--;
                this.renderTable();
                this.renderPagination();
            }
        });

        document.getElementById('nextPageBtn').addEventListener('click', () => {
            const totalPages = Math.ceil(this.filteredBanners.length / this.itemsPerPage);
            if (this.currentPage < totalPages) {
                this.currentPage++;
                this.renderTable();
                this.renderPagination();
            }
        });

        // Modal click outside to close
        document.getElementById('bannerModal').addEventListener('click', (e) => {
            if (e.target.id === 'bannerModal') {
                this.closeModal();
            }
        });

        document.getElementById('deleteModal').addEventListener('click', (e) => {
            if (e.target.id === 'deleteModal') {
                this.closeDeleteModal();
            }
        });

        // Event delegation for dynamic buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-edit')) {
                e.preventDefault();
                const bannerId = parseInt(e.target.closest('.btn-edit').dataset.id);
                this.openEditModal(bannerId);
            }
            
            if (e.target.closest('.btn-delete')) {
                e.preventDefault();
                const bannerId = parseInt(e.target.closest('.btn-delete').dataset.id);
                this.openDeleteModal(bannerId);
            }
            
            if (e.target.closest('.page-number')) {
                e.preventDefault();
                const page = parseInt(e.target.closest('.page-number').dataset.page);
                this.currentPage = page;
                this.renderTable();
                this.renderPagination();
            }
            
            if (e.target.closest('.remove-btn')) {
                e.preventDefault();
                this.removeImage();
            }
        });
    }

    async loadData() {
        this.showLoading();
        try {
            await this.loadBanners();
            this.filterBanners();
        } catch (error) {
            console.error('Error loading data:', error);
            this.showAlert('Có lỗi xảy ra khi tải dữ liệu', 'error');
        } finally {
            this.hideLoading();
        }
    }

    async loadBanners() {
        try {
            const response = await fetch('/api/Banner/get-banner');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            
            if (result.success && Array.isArray(result.data)) {
                this.banners = result.data.map(banner => ({
                    id: banner.bannerId,
                    title: banner.title,
                    content: banner.content,
                    text: banner.text,
                    image: banner.imageBase64 ? `data:image/png;base64,${banner.imageBase64}` : null,
                    redirectUrl: banner.redirectUrl,
                    isActive: banner.isActive,
                    ngayTao: banner.createdAt
                }));
            } else {
                throw new Error(result.message || 'Không thể tải danh sách banner');
            }
        } catch (error) {
            console.error('Error loading banners:', error);
            // Show test data if API fails
            this.banners = [
                {
                    id: 1,
                    title: "Banner Test 1",
                    content: "Nội dung banner test 1",
                    text: "Văn bản test",
                    image: "https://via.placeholder.com/400x200",
                    redirectUrl: "https://example.com",
                    isActive: true,
                    ngayTao: "2024-01-01"
                },
                {
                    id: 2,
                    title: "Banner Test 2", 
                    content: "Nội dung banner test 2",
                    text: "Văn bản test 2",
                    image: "https://via.placeholder.com/400x200",
                    redirectUrl: "https://example2.com",
                    isActive: false,
                    ngayTao: "2024-01-02"
                }
            ];
        }
    }

    filterBanners() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
        const statusFilter = document.getElementById('statusFilter').value;

        this.filteredBanners = this.banners.filter(banner => {
            const matchesSearch = !searchTerm || 
                banner.title?.toLowerCase().includes(searchTerm) ||
                banner.content?.toLowerCase().includes(searchTerm) ||
                banner.text?.toLowerCase().includes(searchTerm);

            const matchesStatus = !statusFilter || 
                banner.isActive.toString() === statusFilter;

            return matchesSearch && matchesStatus;
        });

        this.currentPage = 1;
        this.renderTable();
        this.renderPagination();
    }

    clearFilters() {
        document.getElementById('searchInput').value = '';
        document.getElementById('statusFilter').value = '';
        this.filterBanners();
    }

    renderTable() {
        const tbody = document.getElementById('bannersTableBody');
        const mobileCards = document.getElementById('mobileBannerCards');
        
        if (this.filteredBanners.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="9" style="text-align: center; padding: 40px; color: #6b7280;">
                        <i class="fas fa-image" style="font-size: 48px; margin-bottom: 16px; opacity: 0.3;"></i>
                        <div>Không tìm thấy banner nào</div>
                    </td>
                </tr>
            `;
            mobileCards.innerHTML = '<div class="no-data">Không tìm thấy banner nào</div>';
            this.updatePaginationInfo();
            return;
        }

        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = Math.min(startIndex + this.itemsPerPage, this.filteredBanners.length);
        const bannersToShow = this.filteredBanners.slice(startIndex, endIndex);

        // Render table for desktop
        tbody.innerHTML = bannersToShow.map((banner, index) => {
            const actualIndex = startIndex + index + 1;
            const statusClass = banner.isActive ? 'status-active' : 'status-inactive';
            const statusText = banner.isActive ? 'Hoạt động' : 'Không hoạt động';
            
            return `
                <tr>
                    <td>${actualIndex}</td>
                    <td>
                        <div class="image-cell">
                            ${banner.image ? 
                                `<img src="${banner.image}" alt="${banner.title}" class="table-image">` : 
                                '<div class="no-image"><i class="fas fa-image"></i></div>'
                            }
                        </div>
                    </td>
                    <td>
                        <div class="title-cell">
                            <span class="title-text">${this.escapeHtml(banner.title || '')}</span>
                        </div>
                    </td>
                    <td>
                        <div class="content-cell">
                            <span class="content-text">${this.escapeHtml(banner.content || '')}</span>
                        </div>
                    </td>
                    <td>
                        <div class="text-cell">
                            <span class="text-text">${this.escapeHtml(banner.text || '')}</span>
                        </div>
                    </td>
                    <td>
                        <div class="url-cell">
                            ${banner.redirectUrl ? 
                                `<a href="${banner.redirectUrl}" target="_blank" class="url-link">
                                    <i class="fas fa-external-link-alt"></i> Link
                                </a>` : 
                                '<span class="no-url">Không có</span>'
                            }
                        </div>
                    </td>
                    <td>
                        <span class="status-badge ${statusClass}">${statusText}</span>
                    </td>
                    <td>${this.formatDate(banner.ngayTao)}</td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-edit" data-id="${banner.id}" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-delete" data-id="${banner.id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');

        // Render mobile cards
        mobileCards.innerHTML = bannersToShow.map(banner => {
            const statusClass = banner.isActive ? 'status-active' : 'status-inactive';
            const statusText = banner.isActive ? 'Hoạt động' : 'Không hoạt động';
            
            return `
                <div class="mobile-card">
                    <div class="card-header">
                        <div class="card-image">
                            ${banner.image ? 
                                `<img src="${banner.image}" alt="${banner.title}">` : 
                                '<div class="no-image"><i class="fas fa-image"></i></div>'
                            }
                        </div>
                        <div class="card-title">
                            <h3>${this.escapeHtml(banner.title || '')}</h3>
                            <span class="status-badge ${statusClass}">${statusText}</span>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="card-field">
                            <label>Nội dung:</label>
                            <span>${this.escapeHtml(banner.content || '')}</span>
                        </div>
                        ${banner.text ? `
                        <div class="card-field">
                            <label>Văn bản:</label>
                            <span>${this.escapeHtml(banner.text)}</span>
                        </div>
                        ` : ''}
                        ${banner.redirectUrl ? `
                        <div class="card-field">
                            <label>URL:</label>
                            <a href="${banner.redirectUrl}" target="_blank" class="url-link">
                                <i class="fas fa-external-link-alt"></i> Link
                            </a>
                        </div>
                        ` : ''}
                        <div class="card-field">
                            <label>Ngày tạo:</label>
                            <span>${this.formatDate(banner.ngayTao)}</span>
                        </div>
                    </div>
                    <div class="card-actions">
                        <button class="btn btn-edit" data-id="${banner.id}">
                            <i class="fas fa-edit"></i> Sửa
                        </button>
                        <button class="btn btn-delete" data-id="${banner.id}">
                            <i class="fas fa-trash"></i> Xóa
                        </button>
                    </div>
                </div>
            `;
        }).join('');

        this.updatePaginationInfo();
    }

    renderPagination() {
        const totalPages = Math.ceil(this.filteredBanners.length / this.itemsPerPage);
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');
        const pageNumbers = document.getElementById('pageNumbers');

        // Update prev/next buttons
        prevBtn.disabled = this.currentPage <= 1;
        nextBtn.disabled = this.currentPage >= totalPages;

        // Generate page numbers
        let paginationHTML = '';
        
        if (totalPages <= 7) {
            // Show all pages if total pages <= 7
            for (let i = 1; i <= totalPages; i++) {
                paginationHTML += this.createPageButton(i);
            }
        } else {
            // Show ellipsis pagination for more than 7 pages
            if (this.currentPage <= 4) {
                for (let i = 1; i <= 5; i++) {
                    paginationHTML += this.createPageButton(i);
                }
                paginationHTML += '<span class="ellipsis">...</span>';
                paginationHTML += this.createPageButton(totalPages);
            } else if (this.currentPage >= totalPages - 3) {
                paginationHTML += this.createPageButton(1);
                paginationHTML += '<span class="ellipsis">...</span>';
                for (let i = totalPages - 4; i <= totalPages; i++) {
                    paginationHTML += this.createPageButton(i);
                }
            } else {
                paginationHTML += this.createPageButton(1);
                paginationHTML += '<span class="ellipsis">...</span>';
                for (let i = this.currentPage - 1; i <= this.currentPage + 1; i++) {
                    paginationHTML += this.createPageButton(i);
                }
                paginationHTML += '<span class="ellipsis">...</span>';
                paginationHTML += this.createPageButton(totalPages);
            }
        }

        pageNumbers.innerHTML = paginationHTML;
    }

    createPageButton(page) {
        const isActive = page === this.currentPage;
        return `
            <button class="page-number ${isActive ? 'active' : ''}" data-page="${page}">
                ${page}
            </button>
        `;
    }

    updatePaginationInfo() {
        const start = this.filteredBanners.length === 0 ? 0 : (this.currentPage - 1) * this.itemsPerPage + 1;
        const end = Math.min(this.currentPage * this.itemsPerPage, this.filteredBanners.length);
        const total = this.filteredBanners.length;
        
        document.getElementById('paginationInfo').textContent = `Hiển thị ${start} - ${end} của ${total} banner`;
    }

    formatDate(dateString) {
        if (!dateString) return '';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('vi-VN');
        } catch {
            return dateString;
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    openAddModal() {
        this.editingBannerId = null;
        this.resetForm();
        document.getElementById('modalTitleText').textContent = 'Thêm banner';
        document.getElementById('saveBtnText').textContent = 'Lưu';
        document.getElementById('imageUpdateNote').style.display = 'none';
        this.showModal();
    }

    openEditModal(bannerId) {
        const banner = this.banners.find(b => b.id === bannerId);
        if (!banner) {
            this.showAlert('Không tìm thấy banner', 'error');
            return;
        }

        this.editingBannerId = bannerId;
        this.resetForm();
        
        // Fill form with banner data
        document.getElementById('title').value = banner.title || '';
        document.getElementById('content').value = banner.content || '';
        document.getElementById('text').value = banner.text || '';
        document.getElementById('redirectUrl').value = banner.redirectUrl || '';
        document.getElementById('isActive').value = banner.isActive.toString();
        
        // Update character counter
        this.updateCharCounter(document.getElementById('content'));
        
        // Show current image if exists
        if (banner.image) {
            this.showImagePreview(banner.image);
        }
        
        document.getElementById('modalTitleText').textContent = 'Chỉnh sửa banner';
        document.getElementById('saveBtnText').textContent = 'Cập nhật';
        document.getElementById('imageUpdateNote').style.display = 'block';
        
        this.showModal();
    }

    openDeleteModal(bannerId) {
        const banner = this.banners.find(b => b.id === bannerId);
        if (!banner) {
            this.showAlert('Không tìm thấy banner', 'error');
            return;
        }

        this.editingBannerId = bannerId;
        document.getElementById('deleteBannerTitle').textContent = banner.title || 'banner này';
        this.showDeleteModal();
    }

    showModal() {
        document.getElementById('bannerModal').style.display = 'flex';
        document.body.style.overflow = 'hidden';
        this.scrollToModal();
    }

    closeModal() {
        document.getElementById('bannerModal').style.display = 'none';
        document.body.style.overflow = '';
        this.resetForm();
    }

    showDeleteModal() {
        document.getElementById('deleteModal').style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }

    closeDeleteModal() {
        document.getElementById('deleteModal').style.display = 'none';
        document.body.style.overflow = '';
        this.editingBannerId = null;
    }

    scrollToModal() {
        setTimeout(() => {
            const modal = document.getElementById('bannerModal');
            if (modal) {
                modal.scrollTop = 0;
            }
        }, 100);
    }

    resetForm() {
        document.getElementById('bannerForm').reset();
        this.clearErrors();
        this.clearImagePreview();
        this.selectedImage = null;
        
        // Reset character counter
        const contentField = document.getElementById('content');
        this.updateCharCounter(contentField);
    }

    clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(element => {
            element.textContent = '';
        });
    }

    updateCharCounter(textarea) {
        const charCount = textarea.value.length;
        const counter = document.getElementById('contentCharCount');
        const maxLength = 200;
        
        counter.textContent = charCount;
        
        const counterContainer = counter.parentElement;
        if (charCount > maxLength) {
            counterContainer.classList.add('over-limit');
        } else {
            counterContainer.classList.remove('over-limit');
        }
    }

    handleImageUpload(event) {
        const file = event.target.files[0];
        if (!file) return;

        // Validate file type
        if (!file.type.startsWith('image/')) {
            this.showError('imageFileError', 'Vui lòng chọn file hình ảnh hợp lệ');
            event.target.value = '';
            return;
        }

        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            this.showError('imageFileError', 'Kích thước file không được vượt quá 5MB');
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

        // Clear error
        this.showError('imageFileError', '');
    }

    showImagePreview(imageSrc) {
        const container = document.getElementById('imagePreview');
        container.innerHTML = `
            <div class="image-preview-item">
                <img src="${imageSrc}" alt="Preview">
                <button type="button" class="remove-btn" title="Xóa ảnh">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        container.style.display = 'block';
    }

    removeImage() {
        this.selectedImage = null;
        this.clearImagePreview();
        document.getElementById('imageFile').value = '';
    }

    clearImagePreview() {
        const container = document.getElementById('imagePreview');
        container.innerHTML = '';
        container.style.display = 'none';
    }

    validateForm() {
        let isValid = true;
        this.clearErrors();

        // Validate title
        const title = document.getElementById('title').value.trim();
        if (!title) {
            this.showError('titleError', 'Vui lòng nhập tiêu đề banner');
            isValid = false;
        } else if (title.length > 255) {
            this.showError('titleError', 'Tiêu đề không được vượt quá 255 ký tự');
            isValid = false;
        }

        // Validate content
        const content = document.getElementById('content').value.trim();
        if (!content) {
            this.showError('contentError', 'Vui lòng nhập nội dung banner');
            isValid = false;
        } else if (content.length > 200) {
            this.showError('contentError', 'Nội dung không được vượt quá 200 ký tự');
            isValid = false;
        }

        // Validate status
        const isActive = document.getElementById('isActive').value;
        if (isActive === '') {
            this.showError('isActiveError', 'Vui lòng chọn trạng thái');
            isValid = false;
        }

        // Validate URL if provided
        const redirectUrl = document.getElementById('redirectUrl').value.trim();
        if (redirectUrl) {
            try {
                new URL(redirectUrl);
            } catch {
                this.showError('redirectUrlError', 'URL không hợp lệ');
                isValid = false;
            }
        }

        return isValid;
    }

    showError(elementId, message) {
        const errorElement = document.getElementById(elementId);
        if (errorElement) {
            errorElement.textContent = message;
        }
    }

    async saveBanner() {
        if (!this.validateForm()) {
            return;
        }

        this.showLoading();
        
        try {
            const formData = new FormData();
            formData.append('Title', document.getElementById('title').value.trim());
            formData.append('Content', document.getElementById('content').value.trim());
            formData.append('Text', document.getElementById('text').value.trim());
            formData.append('RedirectUrl', document.getElementById('redirectUrl').value.trim());
            formData.append('IsActive', document.getElementById('isActive').value);
            
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
            
            if (result.success) {
                this.showAlert(
                    this.editingBannerId ? 'Cập nhật banner thành công' : 'Thêm banner thành công', 
                    'success'
                );
                this.closeModal();
                await this.loadData();
            } else {
                throw new Error(result.message || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error saving banner:', error);
            this.showAlert('Có lỗi xảy ra khi lưu banner: ' + error.message, 'error');
        } finally {
            this.hideLoading();
        }
    }

    async deleteBanner() {
        if (!this.editingBannerId) return;

        this.showLoading();
        
        try {
            const response = await fetch(`/api/Banner/delete-banner/${this.editingBannerId}`, {
                method: 'DELETE'
            });

            const result = await response.json();
            
            if (result.success) {
                this.showAlert('Xóa banner thành công', 'success');
                this.closeDeleteModal();
                await this.loadData();
            } else {
                throw new Error(result.message || 'Có lỗi xảy ra');
            }
        } catch (error) {
            console.error('Error deleting banner:', error);
            this.showAlert('Có lỗi xảy ra khi xóa banner: ' + error.message, 'error');
        } finally {
            this.hideLoading();
        }
    }

    showLoading() {
        document.getElementById('loadingOverlay').style.display = 'flex';
    }

    hideLoading() {
        document.getElementById('loadingOverlay').style.display = 'none';
    }

    showAlert(message, type = 'info') {
        const alertContainer = document.getElementById('alertContainer');
        const alertId = 'alert-' + Date.now();
        
        const alertTypes = {
            success: { icon: 'check-circle', class: 'alert-success' },
            error: { icon: 'exclamation-circle', class: 'alert-error' },
            warning: { icon: 'exclamation-triangle', class: 'alert-warning' },
            info: { icon: 'info-circle', class: 'alert-info' }
        };
        
        const alert = alertTypes[type] || alertTypes.info;
        
        const alertHTML = `
            <div id="${alertId}" class="alert ${alert.class}">
                <i class="fas fa-${alert.icon}"></i>
                <span>${message}</span>
                <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        alertContainer.insertAdjacentHTML('beforeend', alertHTML);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            const alertElement = document.getElementById(alertId);
            if (alertElement) {
                alertElement.remove();
            }
        }, 5000);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM loaded, initializing BannerManagement...');
    
    // Check current path
    const currentPath = window.location.pathname.toLowerCase();
    console.log('Current Path:', currentPath);
    
    // Initialize only on banner page
    if (currentPath.includes('/nguoidungs/banner') || currentPath.includes('/admin/banner')) {
        console.log('✓ Exact path match:', currentPath);
        window.bannerManagement = new BannerManagement();
        console.log('BannerManagement initialized');
    } else {
        console.log('✗ Path does not match banner page');
    }
}); 