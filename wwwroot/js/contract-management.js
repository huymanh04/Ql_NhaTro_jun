// Contract Management JavaScript
class ContractManagement {
    constructor() {
        this.contracts = [];
        this.rooms = [];
        this.tenants = [];
        this.filteredContracts = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingContractId = null;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadData();
    }

    bindEvents() {
        // Add contract button
        document.getElementById('addContractBtn').addEventListener('click', () => {
            this.openAddModal();
        });

        // Modal close buttons
        document.getElementById('closeModalBtn').addEventListener('click', () => {
            this.closeModal();
        });
        
        document.getElementById('cancelModalBtn').addEventListener('click', () => {
            this.closeModal();
        });

        // Save contract button
        document.getElementById('saveContractBtn').addEventListener('click', () => {
            this.saveContract();
        });

        // Delete modal buttons
        document.getElementById('closeDeleteModalBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('cancelDeleteBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('confirmDeleteBtn').addEventListener('click', () => {
            this.deleteContract();
        });

        // Search and filters
        document.getElementById('searchInput').addEventListener('input', (e) => {
            this.filterContracts();
        });
        
        document.getElementById('roomFilter').addEventListener('change', () => {
            this.filterContracts();
        });
        
        document.getElementById('statusFilter').addEventListener('change', () => {
            this.filterContracts();
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

        // Pagination
        document.getElementById('prevPageBtn').addEventListener('click', () => {
            if (this.currentPage > 1) {
                this.currentPage--;
                this.renderTable();
                this.renderPagination();
            }
        });

        document.getElementById('nextPageBtn').addEventListener('click', () => {
            const totalPages = Math.ceil(this.filteredContracts.length / this.itemsPerPage);
            if (this.currentPage < totalPages) {
                this.currentPage++;
                this.renderTable();
                this.renderPagination();
            }
        });

        // Modal click outside to close
        document.getElementById('contractModal').addEventListener('click', (e) => {
            if (e.target.id === 'contractModal') {
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
                const contractId = parseInt(e.target.closest('.btn-edit').dataset.id);
                this.openEditModal(contractId);
            }
            
            if (e.target.closest('.btn-delete')) {
                e.preventDefault();
                const contractId = parseInt(e.target.closest('.btn-delete').dataset.id);
                this.openDeleteModal(contractId);
            }
            
            if (e.target.closest('.page-number')) {
                e.preventDefault();
                const page = parseInt(e.target.closest('.page-number').dataset.page);
                this.currentPage = page;
                this.renderTable();
                this.renderPagination();
            }
        });

        // Date validation
        document.getElementById('startDate').addEventListener('change', () => {
            this.validateDates();
        });
        
        document.getElementById('endDate').addEventListener('change', () => {
            this.validateDates();
        });
    }

    async loadData() {
        this.showLoading();
        try {
            await Promise.all([
                this.loadContracts(),
                this.loadRooms()
            ]);
            this.filterContracts();
        } catch (error) {
            console.error('Error loading data:', error);
            this.showAlert('Có lỗi xảy ra khi tải dữ liệu', 'error');
        } finally {
            this.hideLoading();
        }
    }

    async loadContracts() {
        try {
            const response = await fetch('/api/Contract/get-contracts');
            const result = await response.json();
            
            if (result.success) {
                this.contracts = result.data || [];
            } else {
                throw new Error(result.message || 'Không thể tải danh sách hợp đồng');
            }
        } catch (error) {
            console.error('Error loading contracts:', error);
            throw error;
        }
    }

    async loadRooms() {
        try {
            const response = await fetch('/api/Room/get-all-room');
            const result = await response.json();
            
            if (result.success) {
                this.rooms = result.data.phong || [];
                this.populateRoomFilter(); // Populate filter dropdown
            } else {
                throw new Error('Không thể tải danh sách phòng');
            }
        } catch (error) {
            console.error('Error loading rooms:', error);
            this.rooms = [];
        }
    }

    populateRoomFilter() {
        const select = document.getElementById('roomFilter');
        if (select) {
            // Keep the first option (placeholder)
            const firstOption = select.querySelector('option');
            select.innerHTML = '';
            if (firstOption) {
                select.appendChild(firstOption);
            }
            
            // Show all rooms in filter
            this.rooms.forEach(room => {
                const option = document.createElement('option');
                option.value = room.maPhong;
                option.textContent = `${room.tenPhong} (${room.conTrong ? 'Còn trống' : 'Đã thuê'})`;
                select.appendChild(option);
            });
        }
    }

    async loadTenants() {
        try {
            const response = await fetch('/api/NguoiDung/get-all-customer');
            const result = await response.json();
            
            if (result.success) {
                this.tenants = result.data || [];
                this.populateTenantSelects();
            } else {
                throw new Error('Không thể tải danh sách khách thuê');
            }
        } catch (error) {
            console.error('Error loading tenants:', error);
            this.tenants = [];
        }
    }

    populateRoomSelects(currentRoomId = null) {
        const select = document.getElementById('roomId');
        if (select) {
            // Keep the first option (placeholder)
            const firstOption = select.querySelector('option');
            select.innerHTML = '';
            if (firstOption) {
                select.appendChild(firstOption);
            }
            
            // Show available rooms + current room (if editing)
            const roomsToShow = this.rooms.filter(room => {
                // Show available rooms OR the current room being edited
                return room.conTrong || (currentRoomId && room.maPhong === currentRoomId);
            });
            
            roomsToShow.forEach(room => {
                const option = document.createElement('option');
                option.value = room.maPhong;
                let statusText = room.conTrong ? 'Còn trống' : 'Đã thuê';
                // Add indicator for current room
                if (currentRoomId && room.maPhong === currentRoomId) {
                    statusText = 'Phòng hiện tại';
                }
                option.textContent = `${room.tenPhong} (${statusText})`;
                select.appendChild(option);
            });
        }
    }

    populateTenantSelects() {
        const select = document.getElementById('tenantIds');
        if (select) {
            select.innerHTML = '';
            
            this.tenants.forEach(tenant => {
                const option = document.createElement('option');
                option.value = tenant.maKhachThue;
                option.textContent = `${tenant.hoTen} - ${tenant.soDienThoai}`;
                select.appendChild(option);
            });
        }
    }

    async loadAvailableTenants() {
        try {
            const response = await fetch('/api/NguoiDung/get-available-tenants');
            const result = await response.json();
            
            if (result.success) {
                this.tenants = result.data || [];
                this.populateTenantSelects();
            } else {
                throw new Error('Không thể tải danh sách khách thuê khả dụng');
            }
        } catch (error) {
            console.error('Error loading available tenants:', error);
            this.tenants = [];
        }
    }

    filterContracts() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
        const roomFilter = document.getElementById('roomFilter').value;
        const statusFilter = document.getElementById('statusFilter').value;

        this.filteredContracts = this.contracts.filter(contract => {
            const roomName = contract.room ? contract.room.tenPhong : '';
            const motelName = contract.room && contract.room.nhaTro ? contract.room.nhaTro.tenNhaTro : '';
            const roomTypeName = contract.room && contract.room.theLoaiPhong ? contract.room.theLoaiPhong.tenTheLoai : '';
            
            const matchesSearch = contract.contractId.toString().includes(searchTerm) ||
                                roomName.toLowerCase().includes(searchTerm) ||
                                motelName.toLowerCase().includes(searchTerm) ||
                                roomTypeName.toLowerCase().includes(searchTerm);
            const matchesRoom = !roomFilter || contract.roomId.toString() === roomFilter;
            const matchesStatus = !statusFilter || contract.isCompleted.toString() === statusFilter;

            return matchesSearch && matchesRoom && matchesStatus;
        });

        this.currentPage = 1;
        this.renderTable();
        this.renderPagination();
    }

    clearFilters() {
        document.getElementById('searchInput').value = '';
        document.getElementById('roomFilter').value = '';
        document.getElementById('statusFilter').value = '';
        this.filterContracts();
    }

    renderTable() {
        const tbody = document.getElementById('contractsTableBody');
        const mobileCards = document.getElementById('mobileContractCards');
        
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageContracts = this.filteredContracts.slice(startIndex, endIndex);

        // Render desktop table
        tbody.innerHTML = pageContracts.map((contract, index) => {
            const roomInfo = contract.room ? contract.room : null;
            const roomDisplay = roomInfo ? 
                `${roomInfo.tenPhong}${roomInfo.nhaTro ? ` (${roomInfo.nhaTro.tenNhaTro})` : ''}` : 
                'N/A';
            
            const tenantsDisplay = contract.tenants && contract.tenants.length > 0 ?
                contract.tenants.map(t => t.hoTen).join(', ') :
                'Chưa có khách thuê';
            
            // Calculate STT (số thứ tự)
            const stt = (this.currentPage - 1) * this.itemsPerPage + index + 1;
            
            return `
                <tr>
                    <td><strong>${stt}</strong></td>
                    <td>
                        <div class="room-info">
                            <div class="room-main">${roomDisplay}</div>
                            ${roomInfo && roomInfo.theLoaiPhong ? 
                                `<div class="room-details">${roomInfo.theLoaiPhong.tenTheLoai}</div>` : 
                                ''}
                            ${roomInfo ? 
                                `<div class="room-details">${this.formatPrice(roomInfo.gia)}/tháng - ${roomInfo.dienTich}m²</div>` : 
                                ''}
                        </div>
                    </td>
                    <td><span class="date">${this.formatDate(contract.startDate)}</span></td>
                    <td><span class="date">${this.formatDate(contract.endDate)}</span></td>
                    <td>
                        <div class="people-vehicles">
                            <span><i class="fas fa-users"></i> ${contract.numberOfTenants} người</span>
                            <span><i class="fas fa-car"></i> ${contract.soxe || 0} xe</span>
                        </div>
                    </td>
                    <td><span class="price">${this.formatPrice(contract.depositAmount)}</span></td>
                    <td>
                        <div class="tenant-list" title="${tenantsDisplay}">
                            ${tenantsDisplay}
                        </div>
                    </td>
                    <td>
                        <span class="status-badge ${contract.isCompleted ? 'status-completed' : 'status-active'}">
                            ${contract.isCompleted ? 'Đã kết thúc' : 'Đang hiệu lực'}
                        </span>
                    </td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn-action btn-edit" data-id="${contract.contractId}">
                                <i class="fas fa-edit"></i> Sửa
                            </button>
                            <button class="btn-action btn-delete" data-id="${contract.contractId}">
                                <i class="fas fa-trash"></i> Xóa
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');

        // Render mobile cards
        mobileCards.innerHTML = pageContracts.map((contract, index) => {
            const roomInfo = contract.room ? contract.room : null;
            const roomDisplay = roomInfo ? 
                `${roomInfo.tenPhong}${roomInfo.nhaTro ? ` (${roomInfo.nhaTro.tenNhaTro})` : ''}` : 
                'N/A';
            
            const tenantsDisplay = contract.tenants && contract.tenants.length > 0 ?
                contract.tenants.map(t => t.hoTen).join(', ') :
                'Chưa có khách thuê';
            
            // Calculate STT (số thứ tự)
            const stt = (this.currentPage - 1) * this.itemsPerPage + index + 1;
            
            return `
                <div class="mobile-card">
                    <div class="mobile-card-header">
                        <div class="mobile-card-info">
                            <h3>Hợp đồng STT ${stt}</h3>
                            <p>Phòng: ${roomDisplay}</p>
                            ${roomInfo && roomInfo.theLoaiPhong ? 
                                `<p><small>Loại: ${roomInfo.theLoaiPhong.tenTheLoai}</small></p>` : 
                                ''}
                            ${roomInfo ? 
                                `<p><small>Giá: ${this.formatPrice(roomInfo.gia)}/tháng - ${roomInfo.dienTich}m²</small></p>` : 
                                ''}
                        </div>
                    </div>
                    <div class="mobile-card-details">
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Ngày bắt đầu</span>
                            <span class="mobile-detail-value date">${this.formatDate(contract.startDate)}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Ngày kết thúc</span>
                            <span class="mobile-detail-value date">${this.formatDate(contract.endDate)}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Số người ở</span>
                            <span class="mobile-detail-value">${contract.numberOfTenants} người</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Số xe</span>
                            <span class="mobile-detail-value">${contract.soxe || 0} xe</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Tiền đặt cọc</span>
                            <span class="mobile-detail-value price">${this.formatPrice(contract.depositAmount)}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Khách thuê</span>
                            <span class="mobile-detail-value">${tenantsDisplay}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Trạng thái</span>
                            <span class="status-badge ${contract.isCompleted ? 'status-completed' : 'status-active'}">
                                ${contract.isCompleted ? 'Đã kết thúc' : 'Đang hiệu lực'}
                            </span>
                        </div>
                    </div>
                    <div class="mobile-card-actions">
                        <button class="btn-action btn-edit" data-id="${contract.contractId}">
                            <i class="fas fa-edit"></i> Sửa
                        </button>
                        <button class="btn-action btn-delete" data-id="${contract.contractId}">
                            <i class="fas fa-trash"></i> Xóa
                        </button>
                    </div>
                </div>
            `;
        }).join('');

        this.updatePaginationInfo();
    }

    renderPagination() {
        const totalPages = Math.ceil(this.filteredContracts.length / this.itemsPerPage);
        const pageNumbers = document.getElementById('pageNumbers');
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');

        // Update button states
        prevBtn.disabled = this.currentPage === 1;
        nextBtn.disabled = this.currentPage === totalPages || totalPages === 0;

        // Generate page numbers
        let pagesHtml = '';
        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            pagesHtml += `
                <button class="page-number ${i === this.currentPage ? 'active' : ''}" data-page="${i}">
                    ${i}
                </button>
            `;
        }

        pageNumbers.innerHTML = pagesHtml;
    }

    updatePaginationInfo() {
        const totalItems = this.filteredContracts.length;
        const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endIndex = Math.min(startIndex + this.itemsPerPage - 1, totalItems);
        
        document.getElementById('paginationInfo').textContent = 
            `Hiển thị ${totalItems > 0 ? startIndex : 0} - ${totalItems > 0 ? endIndex : 0} của ${totalItems} hợp đồng`;
    }

    formatPrice(price) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    openAddModal() {
        this.editingContractId = null;
        document.getElementById('modalTitleText').textContent = 'Thêm hợp đồng';
        document.getElementById('saveBtnText').textContent = 'Thêm';
        document.getElementById('statusGroup').style.display = 'none';
        
        // Update tenant selection info message
        const tenantInfo = document.getElementById('tenantSelectionInfo');
        if (tenantInfo) {
            tenantInfo.textContent = '📋 Chỉ hiển thị khách hàng chưa thuê phòng (chưa có hợp đồng hiệu lực)';
            tenantInfo.style.color = '#0066cc';
        }
        
        this.resetForm();
        this.populateRoomSelects(); // Refresh room options to show only available rooms
        this.loadAvailableTenants(); // Load only available tenants for new contract
        this.showModal();
        this.scrollToModal();
    }

    async openEditModal(contractId) {
        try {
            const response = await fetch(`/api/Contract/get-contract/${contractId}`);
            const result = await response.json();
            
            if (!result.success) {
                this.showAlert('Không tìm thấy hợp đồng', 'error');
                return;
            }

            const contract = result.data;
            this.editingContractId = contractId;
            document.getElementById('modalTitleText').textContent = 'Sửa hợp đồng';
            document.getElementById('saveBtnText').textContent = 'Cập nhật';
            document.getElementById('statusGroup').style.display = 'block';
            
            // Update tenant selection info message
            const tenantInfo = document.getElementById('tenantSelectionInfo');
            if (tenantInfo) {
                tenantInfo.textContent = '📝 Hiển thị tất cả khách hàng (có thể thay đổi khách thuê)';
                tenantInfo.style.color = '#d4a574';
            }
            
            // Populate room selects with current room included
            this.populateRoomSelects(contract.roomId);
            
            // Load all tenants for editing (allow changing to any tenant)
            await this.loadTenants();
            
            // Fill form with contract data
            document.getElementById('roomId').value = contract.roomId;
            document.getElementById('numberOfTenants').value = contract.numberOfTenants;
            document.getElementById('startDate').value = this.formatDateForInput(contract.startDate);
            document.getElementById('endDate').value = this.formatDateForInput(contract.endDate);
            document.getElementById('soxe').value = contract.soxe || '';
            document.getElementById('depositAmount').value = contract.depositAmount;
            document.getElementById('isCompleted').value = contract.isCompleted.toString();
            
            // Set selected tenants
            if (contract.tenantIds && contract.tenantIds.length > 0) {
                const tenantSelect = document.getElementById('tenantIds');
                Array.from(tenantSelect.options).forEach(option => {
                    option.selected = contract.tenantIds.includes(parseInt(option.value));
                });
            }
            
            this.showModal();
            this.scrollToModal();
        } catch (error) {
            console.error('Error loading contract:', error);
            this.showAlert('Có lỗi xảy ra khi tải thông tin hợp đồng', 'error');
        }
    }

    openDeleteModal(contractId) {
        this.editingContractId = contractId;
        document.getElementById('deleteContractId').textContent = `HĐ${contractId}`;
        document.getElementById('deleteModal').classList.add('show');
    }

    showModal() {
        document.getElementById('contractModal').classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    closeModal() {
        document.getElementById('contractModal').classList.remove('show');
        document.body.style.overflow = '';
        this.resetForm();
    }

    closeDeleteModal() {
        document.getElementById('deleteModal').classList.remove('show');
        this.editingContractId = null;
    }

    scrollToModal() {
        setTimeout(() => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
            
            setTimeout(() => {
                const firstInput = document.querySelector('#contractModal select, #contractModal input');
                if (firstInput) {
                    firstInput.focus();
                }
            }, 400);
        }, 100);
    }

    resetForm() {
        document.getElementById('contractForm').reset();
        document.getElementById('statusGroup').style.display = 'none';
        this.clearErrors();
    }

    clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(el => {
            el.classList.remove('show');
            el.textContent = '';
        });
    }

    validateForm() {
        this.clearErrors();
        let isValid = true;

        const roomId = document.getElementById('roomId').value;
        const numberOfTenants = document.getElementById('numberOfTenants').value;
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;
        const depositAmount = document.getElementById('depositAmount').value;
        const tenantIds = Array.from(document.getElementById('tenantIds').selectedOptions).map(option => option.value);

        if (!roomId) {
            this.showError('roomIdError', 'Vui lòng chọn phòng');
            isValid = false;
        }

        if (!numberOfTenants || parseInt(numberOfTenants) <= 0) {
            this.showError('numberOfTenantsError', 'Vui lòng nhập số người ở hợp lệ');
            isValid = false;
        }

        if (!startDate) {
            this.showError('startDateError', 'Vui lòng chọn ngày bắt đầu');
            isValid = false;
        }

        if (!endDate) {
            this.showError('endDateError', 'Vui lòng chọn ngày kết thúc');
            isValid = false;
        }

        if (startDate && endDate && new Date(startDate) >= new Date(endDate)) {
            this.showError('endDateError', 'Ngày kết thúc phải sau ngày bắt đầu');
            isValid = false;
        }

        if (!depositAmount || parseFloat(depositAmount) < 0) {
            this.showError('depositAmountError', 'Vui lòng nhập tiền đặt cọc hợp lệ');
            isValid = false;
        }

        if (tenantIds.length === 0) {
            this.showError('tenantIdsError', 'Vui lòng chọn ít nhất một khách thuê');
            isValid = false;
        }

        return isValid;
    }

    validateDates() {
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;
        
        if (startDate && endDate && new Date(startDate) >= new Date(endDate)) {
            this.showError('endDateError', 'Ngày kết thúc phải sau ngày bắt đầu');
        } else {
            const errorElement = document.getElementById('endDateError');
            if (errorElement.textContent === 'Ngày kết thúc phải sau ngày bắt đầu') {
                errorElement.classList.remove('show');
                errorElement.textContent = '';
            }
        }
    }

    showError(elementId, message) {
        const errorElement = document.getElementById(elementId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    formatDateForInput(dateString) {
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    }

    async saveContract() {
        if (!this.validateForm()) {
            return;
        }

        const formData = {
            roomId: parseInt(document.getElementById('roomId').value),
            numberOfTenants: parseInt(document.getElementById('numberOfTenants').value),
            startDate: document.getElementById('startDate').value,
            endDate: document.getElementById('endDate').value,
            soxe: parseInt(document.getElementById('soxe').value) || 0,
            depositAmount: parseFloat(document.getElementById('depositAmount').value),
            tenantIds: Array.from(document.getElementById('tenantIds').selectedOptions).map(option => parseInt(option.value))
        };

        if (this.editingContractId) {
            formData.daKetThuc = document.getElementById('isCompleted').value === 'true';
        }

        this.showLoading();

        try {
            const url = this.editingContractId 
                ? `/api/Contract/edit-contract/${this.editingContractId}`
                : '/api/Contract/add-contract';
            
            const method = this.editingContractId ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert(
                    this.editingContractId ? 'Cập nhật hợp đồng thành công!' : 'Thêm hợp đồng thành công!',
                    'success'
                );
                this.closeModal();
                await this.loadData();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra', 'error');
            }
        } catch (error) {
            console.error('Error saving contract:', error);
            this.showAlert('Có lỗi xảy ra khi lưu hợp đồng', 'error');
        } finally {
            this.hideLoading();
        }
    }

    async deleteContract() {
        if (!this.editingContractId) return;

        this.showLoading();

        try {
            const response = await fetch(`/api/Contract/delete-contract/${this.editingContractId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert('Xóa hợp đồng thành công!', 'success');
                this.closeDeleteModal();
                await this.loadData();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra khi xóa hợp đồng', 'error');
            }
        } catch (error) {
            console.error('Error deleting contract:', error);
            this.showAlert('Có lỗi xảy ra khi xóa hợp đồng', 'error');
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
        
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type}">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                <span>${message}</span>
                <button class="alert-close" onclick="document.getElementById('${alertId}').remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        alertContainer.insertAdjacentHTML('beforeend', alertHtml);
        
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
    new ContractManagement();
}); 