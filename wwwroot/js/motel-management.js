class MotelManagement {
    constructor() {
        this.motels = [];
        this.filteredMotels = [];
        this.provinces = [];
        this.areas = [];
        this.owners = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingMotelId = null;
        this.deletingMotelId = null;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadInitialData();
    }

    bindEvents() {
        // Add motel button
        document.getElementById('addMotelBtn').addEventListener('click', () => this.showAddModal());
        
        // Modal close buttons
        document.getElementById('closeModalBtn').addEventListener('click', () => this.hideModal());
        document.getElementById('cancelModalBtn').addEventListener('click', () => this.hideModal());
        document.getElementById('closeDeleteModalBtn').addEventListener('click', () => this.hideDeleteModal());
        document.getElementById('cancelDeleteBtn').addEventListener('click', () => this.hideDeleteModal());
        
        // Save button
        document.getElementById('saveMotelBtn').addEventListener('click', () => this.saveMotel());
        
        // Delete confirmation
        document.getElementById('confirmDeleteBtn').addEventListener('click', () => this.confirmDelete());
        
        // Search and filters
        document.getElementById('searchInput').addEventListener('input', () => this.applyFilters());
        document.getElementById('provinceFilter').addEventListener('change', () => this.applyFilters());
        document.getElementById('areaFilter').addEventListener('change', () => this.applyFilters());
        document.getElementById('clearFiltersBtn').addEventListener('click', () => this.clearFilters());
        
        // Province change in form modal to load areas
        document.getElementById('maTinh').addEventListener('change', (e) => this.loadAreasByProvince(e.target.value));
        
        // Pagination
        document.getElementById('prevPageBtn').addEventListener('click', () => this.previousPage());
        document.getElementById('nextPageBtn').addEventListener('click', () => this.nextPage());
        
        // Modal backdrop click
        document.getElementById('motelModal').addEventListener('click', (e) => {
            if (e.target === e.currentTarget) this.hideModal();
        });
        document.getElementById('deleteModal').addEventListener('click', (e) => {
            if (e.target === e.currentTarget) this.hideDeleteModal();
        });
        
        // Form validation
        document.getElementById('motelForm').addEventListener('input', () => this.clearFieldErrors());
    }

    async loadInitialData() {
        this.showLoading(true);
        try {
            await Promise.all([
                this.loadMotels(),
                this.loadProvinces(),
                this.loadAreas(),
                this.loadOwners()
            ]);
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showAlert('Lỗi khi tải dữ liệu ban đầu', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    async loadMotels() {
        try {
            const response = await fetch('/api/Motel/get-list-motel');
            if (response.ok) {
                this.motels = await response.json();
                this.filteredMotels = [...this.motels];
                this.renderMotels();
                this.updatePagination();
            } else {
                throw new Error('Failed to load motels');
            }
        } catch (error) {
            console.error('Error loading motels:', error);
            this.showAlert('Lỗi khi tải danh sách nhà trọ', 'error');
        }
    }

    async loadProvinces() {
        try {
            const response = await fetch('/api/Location/Tinh-thanh');
            const result = await response.json();
            if (response.ok && result.success) {
                this.provinces = result.data.map(p => ({
                    maTinh: p.maTinh,
                    tenTinh: p.tenTinh
                }));
                this.populateProvinceDropdowns();
            }
        } catch (error) {
            console.error('Error loading provinces:', error);
        }
    }

    async loadAreas() {
        try {
            // Get all areas without pagination (set a large pageSize)
            const response = await fetch('/api/Location/get-khu-vuc?pageSize=1000');
            const result = await response.json();
            if (response.ok && result.success) {
                this.areas = result.data.map(a => ({
                    maKhuVuc: a.maKhuVuc,
                    tenKhuVuc: a.tenKhuVuc,
                    maTinh: a.maTinh
                }));
                this.populateAreaDropdowns();
            }
        } catch (error) {
            console.error('Error loading areas:', error);
        }
    }

    async loadOwners() {
        try {
            const response = await fetch('/api/Motel/get-owners');
            const result = await response.json();
            if (response.ok && result.success) {
                this.owners = result.data;
                this.populateOwnerDropdown();
            }
        } catch (error) {
            console.error('Error loading owners:', error);
        }
    }

    populateProvinceDropdowns() {
        const formSelect = document.getElementById('maTinh');
        const filterSelect = document.getElementById('provinceFilter');
        
        // Clear existing options
        formSelect.innerHTML = '<option value="">Chọn tỉnh/thành</option>';
        filterSelect.innerHTML = '<option value="">Tất cả tỉnh/thành</option>';
        
        this.provinces.forEach(province => {
            const formOption = new Option(province.tenTinh, province.maTinh);
            const filterOption = new Option(province.tenTinh, province.maTinh);
            formSelect.add(formOption);
            filterSelect.add(filterOption);
        });
    }

    populateAreaDropdowns() {
        const formSelect = document.getElementById('maKhuVuc');
        const filterSelect = document.getElementById('areaFilter');
        
        // Clear existing options
        formSelect.innerHTML = '<option value="">Chọn khu vực</option>';
        filterSelect.innerHTML = '<option value="">Tất cả khu vực</option>';
        
        this.areas.forEach(area => {
            const formOption = new Option(area.tenKhuVuc, area.maKhuVuc);
            const filterOption = new Option(area.tenKhuVuc, area.maKhuVuc);
            formSelect.add(formOption);
            filterSelect.add(filterOption);
        });
    }

    populateAreaDropdowns(filteredAreas = null) {
        console.log('=== DEBUG populateAreaDropdowns ===');
        console.log('filteredAreas parameter:', filteredAreas);
        
        const formSelect = document.getElementById('maKhuVuc');
        const filterSelect = document.getElementById('areaFilter');
        const areasToShow = filteredAreas || this.areas;
        
        console.log('areasToShow:', areasToShow);
        
        // Clear form dropdown always
        formSelect.innerHTML = '<option value="">Chọn khu vực</option>';
        
        // Only clear filter dropdown if we're showing all areas
        if (!filteredAreas) {
            filterSelect.innerHTML = '<option value="">Tất cả khu vực</option>';
        }
        
        areasToShow.forEach(area => {
            console.log('Adding area to dropdown:', area);
            const formOption = new Option(area.tenKhuVuc, area.maKhuVuc);
            formSelect.add(formOption);
            
            // Only add to filter dropdown if we're showing all areas
            if (!filteredAreas) {
                const filterOption = new Option(area.tenKhuVuc, area.maKhuVuc);
                filterSelect.add(filterOption);
            }
        });
        
        console.log('Form dropdown final options count:', formSelect.options.length);
    }

    populateOwnerDropdown() {
        const select = document.getElementById('maChuTro');
        select.innerHTML = '<option value="">Chọn chủ trọ</option>';
        
        this.owners.forEach(owner => {
            const option = new Option(owner.hoTen, owner.maNguoiDung);
            select.add(option);
        });
    }

    renderMotels() {
        this.renderTable();
        this.renderMobileCards();
    }

    renderTable() {
        const tbody = document.getElementById('motelsTableBody');
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageMotels = this.filteredMotels.slice(startIndex, endIndex);

        tbody.innerHTML = '';

        if (pageMotels.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center" style="padding: 40px;">
                        <i class="fas fa-search" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px;"></i>
                        <p style="margin: 0; color: #6b7280;">Không tìm thấy nhà trọ nào</p>
                    </td>
                </tr>
            `;
            return;
        }

        pageMotels.forEach(motel => {
            const row = document.createElement('tr');
            
            const provinceName = this.getProvinceName(motel.maTinh);
            const areaName = this.getAreaName(motel.maKhuVuc);
            const ownerName = this.getOwnerName(motel.maChuTro);
            const createdDate = this.formatDate(motel.ngayTao);

            row.innerHTML = `
                <td><strong>#${motel.maNhaTro}</strong></td>
                <td><strong>${motel.tenNhaTro}</strong></td>
                <td>${motel.diaChi || ''}</td>
                <td>${provinceName}</td>
                <td>${areaName}</td>
                <td>${ownerName}</td>
                <td>${createdDate}</td>
                <td class="actions">
                    <button class="btn btn-edit" onclick="motelManagement.showEditModal(${motel.maNhaTro})">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-delete" onclick="motelManagement.showDeleteModal(${motel.maNhaTro}, '${motel.tenNhaTro}')">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }

    renderMobileCards() {
        const container = document.getElementById('mobileMotelCards');
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageMotels = this.filteredMotels.slice(startIndex, endIndex);

        container.innerHTML = '';

        if (pageMotels.length === 0) {
            container.innerHTML = `
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-search" style="font-size: 48px; color: #d1d5db; margin-bottom: 16px;"></i>
                    <p style="margin: 0; color: #6b7280;">Không tìm thấy nhà trọ nào</p>
                </div>
            `;
            return;
        }

        pageMotels.forEach(motel => {
            const provinceName = this.getProvinceName(motel.maTinh);
            const areaName = this.getAreaName(motel.maKhuVuc);
            const ownerName = this.getOwnerName(motel.maChuTro);
            const createdDate = this.formatDate(motel.ngayTao);

            const card = document.createElement('div');
            card.className = 'motel-card';
            card.innerHTML = `
                <div class="motel-card-header">
                    <h3 class="motel-card-title">${motel.tenNhaTro}</h3>
                    <span class="motel-card-id">#${motel.maNhaTro}</span>
                </div>
                
                <div class="motel-card-info">
                    <div class="info-row">
                        <span class="info-label">Địa chỉ:</span>
                        <span class="info-value">${motel.diaChi || 'Chưa có'}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Tỉnh/Thành:</span>
                        <span class="info-value">${provinceName}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Khu vực:</span>
                        <span class="info-value">${areaName}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Chủ trọ:</span>
                        <span class="info-value">${ownerName}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Ngày tạo:</span>
                        <span class="info-value">${createdDate}</span>
                    </div>
                </div>
                
                <div class="motel-card-actions">
                    <button class="btn btn-edit" onclick="motelManagement.showEditModal(${motel.maNhaTro})">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-delete" onclick="motelManagement.showDeleteModal(${motel.maNhaTro}, '${motel.tenNhaTro}')">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </div>
            `;
            container.appendChild(card);
        });
    }

    applyFilters() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
        const provinceFilter = document.getElementById('provinceFilter').value;
        const areaFilter = document.getElementById('areaFilter').value;

        this.filteredMotels = this.motels.filter(motel => {
            const matchesSearch = !searchTerm || 
                motel.tenNhaTro.toLowerCase().includes(searchTerm) ||
                (motel.diaChi && motel.diaChi.toLowerCase().includes(searchTerm));
            
            const matchesProvince = !provinceFilter || motel.maTinh.toString() === provinceFilter;
            const matchesArea = !areaFilter || motel.maKhuVuc.toString() === areaFilter;

            return matchesSearch && matchesProvince && matchesArea;
        });

        this.currentPage = 1;
        this.renderMotels();
        this.updatePagination();
    }

    clearFilters() {
        document.getElementById('searchInput').value = '';
        document.getElementById('provinceFilter').value = '';
        document.getElementById('areaFilter').value = '';
        this.applyFilters();
    }

    showAddModal() {
        this.editingMotelId = null;
        document.getElementById('modalTitleText').textContent = 'Thêm nhà trọ';
        document.getElementById('saveBtnText').textContent = 'Thêm';
        this.clearForm();
        this.populateAreaDropdowns();
        this.showModal();
    }

    showEditModal(motelId) {
        const motel = this.motels.find(m => m.maNhaTro === motelId);
        if (!motel) return;

        this.editingMotelId = motelId;
        document.getElementById('modalTitleText').textContent = 'Sửa nhà trọ';
        document.getElementById('saveBtnText').textContent = 'Cập nhật';
        
        // First load areas for the province
        if (motel.maTinh) {
            this.loadAreasByProvince(motel.maTinh);
        } else {
            this.populateAreaDropdowns();
        }
        
        // Then populate form values
        document.getElementById('tenNhaTro').value = motel.tenNhaTro || '';
        document.getElementById('diaChi').value = motel.diaChi || '';
        document.getElementById('maTinh').value = motel.maTinh || '';
        document.getElementById('maChuTro').value = motel.maChuTro || '';
        document.getElementById('moTa').value = motel.moTa || '';
        
        // Set area value after areas are loaded
        setTimeout(() => {
            document.getElementById('maKhuVuc').value = motel.maKhuVuc || '';
        }, 100);
        
        this.showModal();
    }

    showDeleteModal(motelId, motelName) {
        this.deletingMotelId = motelId;
        document.getElementById('deleteMotelName').textContent = motelName;
        this.showDeleteModalElement();
    }

    showModal() {
        const modal = document.getElementById('motelModal');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
        document.getElementById('tenNhaTro').focus();
    }

    hideModal() {
        const modal = document.getElementById('motelModal');
        modal.classList.remove('show');
        document.body.style.overflow = '';
        this.clearForm();
        this.clearFieldErrors();
    }

    showDeleteModalElement() {
        const modal = document.getElementById('deleteModal');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    hideDeleteModal() {
        const modal = document.getElementById('deleteModal');
        modal.classList.remove('show');
        document.body.style.overflow = '';
        this.deletingMotelId = null;
    }

    clearForm() {
        document.getElementById('motelForm').reset();
        // Reset area dropdown to show all areas
        this.populateAreaDropdowns();
    }

    async saveMotel() {
        if (!this.validateForm()) return;

        const formData = this.getFormData();
        const saveBtn = document.getElementById('saveMotelBtn');
        const originalText = saveBtn.innerHTML;

        try {
            saveBtn.disabled = true;
            saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';

            let response;
            if (this.editingMotelId) {
                // Update
                response = await fetch(`/api/Motel/edit-motel/${this.editingMotelId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });
            } else {
                // Create
                response = await fetch('/api/Motel/add-motel', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });
            }

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert(result.message || (this.editingMotelId ? 'Cập nhật thành công!' : 'Thêm thành công!'), 'success');
                this.hideModal();
                await this.loadMotels();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra', 'error');
            }
        } catch (error) {
            console.error('Error saving motel:', error);
            this.showAlert('Lỗi kết nối: ' + error.message, 'error');
        } finally {
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalText;
        }
    }

    async confirmDelete() {
        if (!this.deletingMotelId) return;

        const deleteBtn = document.getElementById('confirmDeleteBtn');
        const originalText = deleteBtn.innerHTML;

        try {
            deleteBtn.disabled = true;
            deleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';

            const response = await fetch(`/api/Motel/delete-motel/${this.deletingMotelId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (response.ok && result.success) {
                this.showAlert(result.message || 'Xóa thành công!', 'success');
                this.hideDeleteModal();
                await this.loadMotels();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra khi xóa', 'error');
            }
        } catch (error) {
            console.error('Error deleting motel:', error);
            this.showAlert('Lỗi kết nối: ' + error.message, 'error');
        } finally {
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalText;
        }
    }

    getFormData() {
        return {
            tenNhaTro: document.getElementById('tenNhaTro').value.trim(),
            diaChi: document.getElementById('diaChi').value.trim(),
            maTinh: parseInt(document.getElementById('maTinh').value),
            maKhuVuc: parseInt(document.getElementById('maKhuVuc').value),
            maChuTro: parseInt(document.getElementById('maChuTro').value),
            moTa: document.getElementById('moTa').value.trim()
        };
    }

    validateForm() {
        this.clearFieldErrors();
        let isValid = true;

        const tenNhaTro = document.getElementById('tenNhaTro').value.trim();
        const diaChi = document.getElementById('diaChi').value.trim();
        const maTinh = document.getElementById('maTinh').value;
        const maKhuVuc = document.getElementById('maKhuVuc').value;
        const maChuTro = document.getElementById('maChuTro').value;

        if (!tenNhaTro) {
            this.showFieldError('tenNhaTro', 'Tên nhà trọ không được để trống');
            isValid = false;
        }

        if (!diaChi) {
            this.showFieldError('diaChi', 'Địa chỉ không được để trống');
            isValid = false;
        }

        if (!maTinh) {
            this.showFieldError('maTinh', 'Vui lòng chọn tỉnh/thành');
            isValid = false;
        }

        if (!maKhuVuc) {
            this.showFieldError('maKhuVuc', 'Vui lòng chọn khu vực');
            isValid = false;
        }

        if (!maChuTro) {
            this.showFieldError('maChuTro', 'Vui lòng chọn chủ trọ');
            isValid = false;
        }

        return isValid;
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

    updatePagination() {
        const totalItems = this.filteredMotels.length;
        const totalPages = Math.ceil(totalItems / this.itemsPerPage);
        const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endIndex = Math.min(this.currentPage * this.itemsPerPage, totalItems);

        // Update info text
        document.getElementById('paginationInfo').textContent = 
            `Hiển thị ${totalItems > 0 ? startIndex : 0} - ${endIndex} của ${totalItems} nhà trọ`;

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
        const totalPages = Math.ceil(this.filteredMotels.length / this.itemsPerPage);
        if (this.currentPage < totalPages) {
            this.goToPage(this.currentPage + 1);
        }
    }

    goToPage(page) {
        this.currentPage = page;
        this.renderMotels();
        this.updatePagination();
    }

    // Helper methods
    getProvinceName(maTinh) {
        const province = this.provinces.find(p => p.maTinh === maTinh);
        return province ? province.tenTinh : 'Không xác định';
    }

    getAreaName(maKhuVuc) {
        const area = this.areas.find(a => a.maKhuVuc === maKhuVuc);
        return area ? area.tenKhuVuc : 'Không xác định';
    }

    getOwnerName(maChuTro) {
        const owner = this.owners.find(o => o.maNguoiDung === maChuTro);
        return owner ? owner.hoTen : 'Không xác định';
    }

    formatDate(dateString) {
        if (!dateString) return 'Không có';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
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

    loadAreasByProvince(provinceId) {
        console.log('=== DEBUG loadAreasByProvince ===');
        console.log('Selected provinceId:', provinceId, typeof provinceId);
        console.log('All areas:', this.areas);
        
        if (!provinceId) {
            console.log('No province selected, showing all areas');
            this.populateAreaDropdowns();
            return;
        }
        
        // Filter areas by selected province - convert both to number for comparison
        const filteredAreas = this.areas.filter(area => {
            const areaProvince = parseInt(area.maTinh);
            const selectedProvince = parseInt(provinceId);
            const match = areaProvince === selectedProvince;
            console.log(`Area "${area.tenKhuVuc}" (maTinh: ${area.maTinh}) matches province ${provinceId}: ${match}`);
            return match;
        });
        
        console.log('Filtered areas result:', filteredAreas);
        this.populateAreaDropdowns(filteredAreas);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.motelManagement = new MotelManagement();
}); 