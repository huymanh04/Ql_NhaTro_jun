// Hoadon (Utility Bill) Management JavaScript
class HoadonManager {
    constructor() {
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.totalItems = 0;
        this.bills = [];
        this.filteredBills = [];
        this.isEditing = false;
        this.currentBillId = null;
        this.rooms = [];
        this.currentBillData = null;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadRooms();
        this.loadBills();
    }

    bindEvents() {
        // Add bill button
        const addBillBtn = document.getElementById('addBillBtn');
        if (addBillBtn) {
            addBillBtn.addEventListener('click', () => {
                this.openModal();
            });
        }

        // Modal events
        const closeModalBtn = document.getElementById('closeModalBtn');
        if (closeModalBtn) {
            closeModalBtn.addEventListener('click', () => {
                this.closeModal();
            });
        }

        const cancelModalBtn = document.getElementById('cancelModalBtn');
        if (cancelModalBtn) {
            cancelModalBtn.addEventListener('click', () => {
                this.closeModal();
            });
        }

        const saveBillBtn = document.getElementById('saveBillBtn');
        if (saveBillBtn) {
            saveBillBtn.addEventListener('click', () => {
                this.saveBill();
            });
        }

        // Delete modal events
        const closeDeleteModalBtn = document.getElementById('closeDeleteModalBtn');
        if (closeDeleteModalBtn) {
            closeDeleteModalBtn.addEventListener('click', () => {
                this.closeDeleteModal();
            });
        }

        const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');
        if (cancelDeleteBtn) {
            cancelDeleteBtn.addEventListener('click', () => {
                this.closeDeleteModal();
            });
        }

        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
        if (confirmDeleteBtn) {
            confirmDeleteBtn.addEventListener('click', () => {
                this.deleteBill();
            });
        }

        // Print modal events
        const closePrintModalBtn = document.getElementById('closePrintModalBtn');
        if (closePrintModalBtn) {
            closePrintModalBtn.addEventListener('click', () => {
                this.closePrintModal();
            });
        }

        const cancelPrintBtn = document.getElementById('cancelPrintBtn');
        if (cancelPrintBtn) {
            cancelPrintBtn.addEventListener('click', () => {
                this.closePrintModal();
            });
        }

        const printBillBtn = document.getElementById('printBillBtn');
        if (printBillBtn) {
            printBillBtn.addEventListener('click', () => {
                this.printBill();
            });
        }

        // Search and filters
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.filterBills();
            });
        }

        const roomFilter = document.getElementById('roomFilter');
        if (roomFilter) {
            roomFilter.addEventListener('change', () => {
                this.filterBills();
            });
        }

        const statusFilter = document.getElementById('statusFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', () => {
                this.filterBills();
            });
        }

        const clearFiltersBtn = document.getElementById('clearFiltersBtn');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.clearFilters();
            });
        }

        // Pagination
        const itemsPerPage = document.getElementById('itemsPerPage');
        if (itemsPerPage) {
            itemsPerPage.addEventListener('change', (e) => {
                this.itemsPerPage = parseInt(e.target.value);
                this.currentPage = 1;
                this.renderBills();
            });
        }

        const prevPageBtn = document.getElementById('prevPageBtn');
        if (prevPageBtn) {
            prevPageBtn.addEventListener('click', () => {
                if (this.currentPage > 1) {
                    this.currentPage--;
                    this.renderBills();
                }
            });
        }

        const nextPageBtn = document.getElementById('nextPageBtn');
        if (nextPageBtn) {
            nextPageBtn.addEventListener('click', () => {
                const maxPage = Math.ceil(this.filteredBills.length / this.itemsPerPage);
                if (this.currentPage < maxPage) {
                    this.currentPage++;
                    this.renderBills();
                }
            });
        }

        // Event delegation for action buttons
        document.addEventListener('click', (e) => {
            // Edit button
            if (e.target.closest('.btn-edit')) {
                const billId = e.target.closest('.btn-edit').getAttribute('data-bill-id');
                if (billId) {
                    this.editBill(parseInt(billId));
                }
            }
            
            // Print button
            if (e.target.closest('.btn-print')) {
                const billId = e.target.closest('.btn-print').getAttribute('data-bill-id');
                if (billId) {
                    this.printBillWithId(parseInt(billId));
                }
            }
            
            // Delete button
            if (e.target.closest('.btn-delete')) {
                const billId = e.target.closest('.btn-delete').getAttribute('data-bill-id');
                if (billId) {
                    this.confirmDelete(parseInt(billId));
                }
            }
        });

        // Close modals when clicking outside
        window.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal')) {
                this.closeModal();
                this.closeDeleteModal();
                this.closePrintModal();
            }
        });
    }

    async loadRooms() {
        try {
            this.showLoading();
            const response = await fetch('/api/UtilityBill/get-all-phong');
            const result = await response.json();
            
            if (result.success) {
                this.rooms = result.data;
                this.populateRoomFilters();
            } else {
                this.showAlert('Lỗi khi tải danh sách phòng: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error loading rooms:', error);
            this.showAlert('Lỗi khi tải danh sách phòng', 'error');
        } finally {
            this.hideLoading();
        }
    }

    async loadBills() {
        try {
            this.showLoading();
            const response = await fetch('/api/UtilityBill/get-all-hoa-don');
            const result = await response.json();
            
            if (result.success) {
                this.bills = result.data;
                this.filteredBills = [...this.bills];
                this.renderBills();
            } else {
                this.showAlert('Lỗi khi tải danh sách hóa đơn: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error loading bills:', error);
            this.showAlert('Lỗi khi tải danh sách hóa đơn', 'error');
        } finally {
            this.hideLoading();
        }
    }

    populateRoomFilters() {
        const roomFilter = document.getElementById('roomFilter');
        const maPhongSelect = document.getElementById('maPhong');
        
        if (!roomFilter || !maPhongSelect) return;
        
        // Clear existing options
        roomFilter.innerHTML = '<option value="">Tất cả phòng</option>';
        maPhongSelect.innerHTML = '<option value="">Chọn phòng</option>';
        
        this.rooms.forEach(room => {
            const option = document.createElement('option');
            option.value = room.maPhong;
            option.textContent = `${room.tenPhong} - ${this.formatCurrency(room.gia)}`;
            
            roomFilter.appendChild(option.cloneNode(true));
            maPhongSelect.appendChild(option);
        });
    }

    filterBills() {
        const searchInput = document.getElementById('searchInput');
        const roomFilter = document.getElementById('roomFilter');
        const statusFilter = document.getElementById('statusFilter');
        
        if (!searchInput || !roomFilter || !statusFilter) return;
        
        const searchTerm = searchInput.value.toLowerCase();
        const roomFilterValue = roomFilter.value;
        const statusFilterValue = statusFilter.value;

        this.filteredBills = this.bills.filter(bill => {
            const matchesSearch = 
                bill.maHoaDon.toString().includes(searchTerm) ||
                bill.tenPhong.toLowerCase().includes(searchTerm) ||
                `${bill.thang}/${bill.nam}`.includes(searchTerm);
            
            const matchesRoom = !roomFilterValue || bill.maPhong.toString() === roomFilterValue;
            const matchesStatus = !statusFilterValue || bill.daThanhToan.toString() === statusFilterValue;

            return matchesSearch && matchesRoom && matchesStatus;
        });

        this.currentPage = 1;
        this.renderBills();
    }

    clearFilters() {
        const searchInput = document.getElementById('searchInput');
        const roomFilter = document.getElementById('roomFilter');
        const statusFilter = document.getElementById('statusFilter');
        
        if (searchInput) searchInput.value = '';
        if (roomFilter) roomFilter.value = '';
        if (statusFilter) statusFilter.value = '';
        
        this.filterBills();
    }

    renderBills() {
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const currentBills = this.filteredBills.slice(startIndex, endIndex);

        this.renderTable(currentBills, startIndex);
        this.renderMobileCards(currentBills, startIndex);
        this.renderPagination();
    }

    renderTable(bills, startIndex) {
        const tbody = document.getElementById('billsTableBody');
        if (!tbody) return;
        
        tbody.innerHTML = '';

        if (bills.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="10" style="text-align: center; padding: 40px; color: #6b7280;">
                        <i class="fas fa-inbox" style="font-size: 48px; margin-bottom: 16px; display: block;"></i>
                        <p>Không có hóa đơn nào</p>
                    </td>
                </tr>
            `;
            return;
        }
       
        console.log(startIndex);
        let stt1 = 0;
        bills.forEach(bill => {
            const stt = (this.currentPage - 1) * this.itemsPerPage + (stt1++) + 1;
           
            const row = document.createElement('tr');
            row.innerHTML = `
                <td><strong>#${stt}</strong></td>
                <td>
                    <div class="room-info">
                        <div class="room-main">${bill.tenPhong}</div>
                        <div class="room-details">Phòng ${bill.maPhong}</div>
                        <div class="room-details">Giá ${this.formatCurrency(bill.giaphong)}</div>
                    </div>
                </td>
      
                <td>${bill.thang}/${bill.nam}</td>
                <td>${bill.soDien || 0} kWh</td>
                <td>${bill.soNuoc || 0} m³</td>
                <td>${this.formatCurrency(bill.phidv || 0)}</td>
                <td>${this.formatCurrency((bill.phiGiuXe || 0) * (bill.soxe || 0))} / ${bill.soxe || 0} xe</td>
                <td class="amount-info">${this.formatCurrency(bill.tongTien || 0)}</td>
                <td>
                    <span class="status-badge ${bill.daThanhToan ? 'status-paid' : 'status-unpaid'}">
                        ${bill.daThanhToan ? 'Đã TT' : 'Chưa TT'}
                    </span>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="btn btn-primary btn-edit" data-bill-id="${bill.maHoaDon}" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-success btn-print" data-bill-id="${bill.maHoaDon}" title="In">
                            <i class="fas fa-print"></i>
                        </button>
                        <button class="btn btn-danger btn-delete" data-bill-id="${bill.maHoaDon}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            `;
            tbody.appendChild(row);
        });
    }

    renderMobileCards(bills, startIndex) {
        const container = document.getElementById('mobileBillCards');
        if (!container) return;
        
        container.innerHTML = '';

        if (bills.length === 0) {
            container.innerHTML = `
                <div style="text-align: center; padding: 40px; color: #6b7280;">
                    <i class="fas fa-inbox" style="font-size: 48px; margin-bottom: 16px; display: block;"></i>
                    <p>Không có hóa đơn nào</p>
                </div>
            `;
            return;
        }
        let stt1 = 0;
        let m = this.itemsPerPage;
        bills.forEach(bill => {
            if (this.itemsPerPage != m) {
                stt1 = 0;
            }
            const stt = (this.currentPage - 1) * this.itemsPerPage + (stt1++) + 1;
            const card = document.createElement('div');
            card.className = 'mobile-card';
            card.innerHTML = `
                <div class="mobile-card-header">
                    <div class="mobile-card-title">#${stt} - ${bill.tenPhong}- Giá ${this.formatCurrency(bill.giaphong)}</div>
                    <span class="mobile-card-status ${bill.daThanhToan ? 'status-paid' : 'status-unpaid'}">
                        ${bill.daThanhToan ? 'Đã TT' : 'Chưa TT'}
                    </span>
                </div>
                <div class="mobile-card-content">
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Tháng/Năm</div>
                        <div class="mobile-card-value">${bill.thang}/${bill.nam}</div>
                    </div>
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Điện</div>
                        <div class="mobile-card-value">${bill.soDien || 0} kWh</div>
                    </div>
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Nước</div>
                        <div class="mobile-card-value">${bill.soNuoc || 0} m³</div>
                    </div>
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Phí DV</div>
                        <div class="mobile-card-value">${this.formatCurrency(bill.phidv || 0)}</div>
                    </div>
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Phí dữ xe</div>
                        <div class="mobile-card-value">${this.formatCurrency((bill.dongiaxe || 0)*(bill.soxe || 0))} / ${bill.soxe || 0} xe</div>
                    </div>
                    <div class="mobile-card-item">
                        <div class="mobile-card-label">Tổng tiền</div>
                        <div class="mobile-card-value amount-info">${this.formatCurrency(bill.tongTien || 0)}</div>
                    </div>
                </div>
                <div class="mobile-card-actions">
                    <button class="btn btn-primary btn-edit" data-bill-id="${bill.maHoaDon}">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-success btn-print" data-bill-id="${bill.maHoaDon}">
                        <i class="fas fa-print"></i> In
                    </button>
                    <button class="btn btn-danger btn-delete" data-bill-id="${bill.maHoaDon}">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </div>
            `;
            container.appendChild(card);
        });
    }

    renderPagination() {
        const totalPages = Math.ceil(this.filteredBills.length / this.itemsPerPage);
        const startItem = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endItem = Math.min(this.currentPage * this.itemsPerPage, this.filteredBills.length);

        // Update pagination info
        const paginationInfo = document.getElementById('paginationInfo');
        if (paginationInfo) {
            paginationInfo.textContent = `Hiển thị ${startItem} - ${endItem} của ${this.filteredBills.length} hóa đơn`;
        }

        // Update pagination controls
        const prevBtn = document.getElementById('prevPageBtn');
        const nextBtn = document.getElementById('nextPageBtn');
        const pageNumbers = document.getElementById('pageNumbers');

        if (prevBtn) prevBtn.disabled = this.currentPage === 1;
        if (nextBtn) nextBtn.disabled = this.currentPage === totalPages;

        // Render page numbers
        if (pageNumbers) {
            pageNumbers.innerHTML = '';
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
                pageBtn.addEventListener('click', () => {
                    this.currentPage = i;
                    this.renderBills();
                });
                pageNumbers.appendChild(pageBtn);
            }
        }
    }

    openModal(billId = null) {
        this.isEditing = billId !== null;
        this.currentBillId = billId;

        const modal = document.getElementById('billModal');
        const modalTitle = document.getElementById('modalTitleText');
        const saveBtnText = document.getElementById('saveBtnText');
        const statusGroup = document.getElementById('statusGroup');

        if (!modal || !modalTitle || !saveBtnText || !statusGroup) return;

        if (this.isEditing) {
            modalTitle.textContent = 'Sửa hóa đơn';
            saveBtnText.textContent = 'Cập nhật';
            statusGroup.style.display = 'block';
            this.loadBillData(billId);
        } else {
            modalTitle.textContent = 'Thêm hóa đơn mới';
            saveBtnText.textContent = 'Lưu';
            statusGroup.style.display = 'none';
            this.resetForm();
        }

        modal.classList.add('show');
    }

    closeModal() {
        const modal = document.getElementById('billModal');
        if (modal) {
            modal.classList.remove('show');
            this.resetForm();
        }
    }

    async loadBillData(billId) {
        try {
            this.showLoading();
            const response = await fetch(`/api/UtilityBill/get-hoa-don-chi-tiet/${billId}`);
            const result = await response.json();

            if (result.success) {
                const bill = result.data;
                const maPhong = document.getElementById('maPhong');
                const soDien = document.getElementById('soDien');
                const soNuoc = document.getElementById('soNuoc');
                const daThanhToan = document.getElementById('daThanhToan');
                
                if (maPhong) maPhong.value = bill.maPhong;
                if (soDien) soDien.value = bill.soDien;
                if (soNuoc) soNuoc.value = bill.soNuoc;
                if (daThanhToan) daThanhToan.value = bill.daThanhToan.toString();
            } else {
                this.showAlert('Lỗi khi tải thông tin hóa đơn: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error loading bill data:', error);
            this.showAlert('Lỗi khi tải thông tin hóa đơn', 'error');
        } finally {
            this.hideLoading();
        }
    }

    resetForm() {
        const form = document.getElementById('billForm');
        if (form) {
            form.reset();
            this.clearFormErrors();
        }
    }

    clearFormErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(element => {
            element.classList.remove('show');
            element.textContent = '';
        });
    }

    async saveBill() {
        const formData = this.getFormData();
        
        if (!this.validateForm(formData)) {
            return;
        }

        try {
            this.showLoading();
            const url = this.isEditing 
                ? `/api/UtilityBill/edit-hoadon-tien-ich/${this.currentBillId}`
                : '/api/UtilityBill/add-hoadon-tien-ich';
            
            const method = this.isEditing ? 'PUT' : 'POST';
            
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
                    this.isEditing ? 'Cập nhật hóa đơn thành công' : 'Thêm hóa đơn thành công', 
                    'success'
                );
                this.closeModal();
                this.loadBills();
            } else {
                this.showAlert('Lỗi: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error saving bill:', error);
            this.showAlert('Lỗi khi lưu hóa đơn', 'error');
        } finally {
            this.hideLoading();
        }
    }

    getFormData() {
        const maPhong = document.getElementById('maPhong');
        const soDien = document.getElementById('soDien');
        const soNuoc = document.getElementById('soNuoc');
        const note = document.getElementById('note');
        const daThanhToan = document.getElementById('daThanhToan');
        
        return {
            maPhong: maPhong ? parseInt(maPhong.value) : 0,
            soDien: soDien ? parseFloat(soDien.value) || 0 : 0,
            soNuoc: soNuoc ? parseFloat(soNuoc.value) || 0 : 0,
            note: note ? note.value : '',
            daThanhToan: this.isEditing && daThanhToan ? daThanhToan.value === 'true' : false
        };
    }

    validateForm(data) {
        this.clearFormErrors();
        let isValid = true;

        if (!data.maPhong) {
            this.showFieldError('maPhong', 'Vui lòng chọn phòng');
            isValid = false;
        }

        if (data.soDien < 0) {
            this.showFieldError('soDien', 'Số điện không được âm');
            isValid = false;
        }

        if (data.soNuoc < 0) {
            this.showFieldError('soNuoc', 'Số nước không được âm');
            isValid = false;
        }

        return isValid;
    }

    showFieldError(fieldName, message) {
        const errorElement = document.getElementById(fieldName + 'Error');
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    confirmDelete(billId) {
        this.currentBillId = billId;
        const deleteBillId = document.getElementById('deleteBillId');
        const deleteModal = document.getElementById('deleteModal');
        
        if (deleteBillId) deleteBillId.textContent = billId;
        if (deleteModal) deleteModal.classList.add('show');
    }

    closeDeleteModal() {
        const deleteModal = document.getElementById('deleteModal');
        if (deleteModal) {
            deleteModal.classList.remove('show');
            this.currentBillId = null;
        }
    }

    async deleteBill() {
        if (!this.currentBillId) return;

        try {
            this.showLoading();
            const response = await fetch(`/api/UtilityBill/delete-hoadon-tienich/${this.currentBillId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert('Xóa hóa đơn thành công', 'success');
                this.closeDeleteModal();
                this.loadBills();
            } else {
                this.showAlert('Lỗi: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error deleting bill:', error);
            this.showAlert('Lỗi khi xóa hóa đơn', 'error');
        } finally {
            this.hideLoading();
        }
    }

    editBill(billId) {
        this.openModal(billId);
    }

    async printBillWithId(billId) {
        try {
            this.showLoading();
            console.log('Đang tải dữ liệu hóa đơn cho ID:', billId);
            
            const response = await fetch(`/api/UtilityBill/print-hoa-don-beautiful/${billId}`);
            const result = await response.json();

            console.log('API Response:', result);

            if (result.success) {
                console.log('Dữ liệu hóa đơn nhận được:', result.data);
                this.showPrintModalBeautiful(result.data);
            } else {
                console.error('API trả về lỗi:', result.message);
                this.showAlert('Lỗi: ' + result.message, 'error');
            }
        } catch (error) {
            console.error('Error loading bill for print:', error);
            this.showAlert('Lỗi khi tải thông tin hóa đơn: ' + error.message, 'error');
        } finally {
            this.hideLoading();
        }
    }

    showPrintModalBeautiful(billData) {
        const modalBody = document.getElementById('printModalBody');
        const printModal = document.getElementById('printModal');
        const printBillBtn = document.getElementById('printBillBtn');
        
        if (!modalBody || !printModal) {
            console.error('Không tìm thấy modal elements');
            return;
        }
        
        // Lưu dữ liệu hóa đơn hiện tại
        this.currentBillData = billData;
        
        if (printBillBtn) printBillBtn.disabled = true; // Disable before rendering
        
        // Debug: Log dữ liệu hóa đơn
        console.log('Bill data for printing:', billData);
        
        modalBody.innerHTML = `
            <div class="bill-print-beautiful">
                <!-- Header -->
                <div class="bill-header-beautiful">
                    <div class="company-info">
                        <h1 class="company-name">Nhà trọ : ${billData.tenCongTy || 'NHÀ TRỌ ABC'}</h1>
                        <p class="company-address">Địa chỉ : ${billData.diaChiCongTy || '123 Đường ABC, Quận XYZ, TP.HCM'}</p>
                        <p class="company-contact">ĐT : ${billData.soDienThoaiCongTy || '0123 456 789'} | Email: ${billData.emailCongTy || 'info@nhatroabc.com'}</p>
                        <p class="company-website">website :${billData.website || 'www.nhatroabc.com'}</p>
                    </div>
                    <div class="bill-title-beautiful">
                        <h2>HÓA ĐƠN TIỆN ÍCH</h2>
                        <p class="bill-number">Số: ${billData.maHoaDon || 'N/A'}</p>
                        <p class="bill-date">Ngày: ${billData.ngayXuat || new Date().toLocaleDateString('vi-VN')}</p>
                    </div>
                </div>

                <!-- Customer Info -->
                <div class="customer-info-beautiful">
                    <div class="info-section">
                        <h3>THÔNG TIN PHÒNG</h3>
                        <div class="info-grid">
                            <div class="info-item">
                                <span class="label">Phòng:</span>
                                <span class="value">${billData.tenPhong || 'N/A'} (${billData.maPhong || 'N/A'})</span>
                            </div>
                            <div class="info-item">
                                <span class="label">Tháng/Năm:</span>
                                <span class="value">${billData.thangNam || 'N/A'}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Bill Details -->
                <div class="bill-details-beautiful">
                    <h3>CHI TIẾT HÓA ĐƠN</h3>
                    <table class="bill-table-beautiful">
                        <thead>
                            <tr>
                                <th>Mục</th>
                                <th>Số lượng</th>
                                <th>Đơn giá</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>Tiền phòng</td>
                                <td>1 tháng</td>
                                <td>${this.formatCurrency(billData.giaPhong || 0)}</td>
                                <td>${this.formatCurrency(billData.giaPhong || 0)}</td>
                            </tr>
                            <tr>
                                <td>Điện</td>
                                <td>${billData.soDien || 0} kWh</td>
                                <td>${this.formatCurrency(billData.donGiaDien || 0)}/kWh</td>
                                <td>${this.formatCurrency(billData.thanhTienDien || 0)}</td>
                            </tr>
                            <tr>
                                <td>Nước</td>
                                <td>${billData.soNuoc || 0} m³</td>
                                <td>${this.formatCurrency(billData.donGiaNuoc || 0)}/m³</td>
                                <td>${this.formatCurrency(billData.thanhTienNuoc || 0)}</td>
                            </tr>
                            <tr>
                                <td>Phí dịch vụ</td>
                                <td>1 tháng</td>
                                <td>${this.formatCurrency(billData.phidv || 0)}</td>
                                <td>${this.formatCurrency(billData.phidv || 0)}</td>
                            </tr>
                            <tr>
                                <td>Phí giữ xe</td>
                                <td>${billData.soxe || 0} xe</td>
                                <td>${this.formatCurrency((billData.phiGiuXe || 0) / (billData.soxe || 1))}/xe</td>
                                <td>${this.formatCurrency(billData.phiGiuXe || 0)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Total -->
                <div class="bill-total-beautiful">
                    <div class="total-row">
                        <span class="total-label">TỔNG CỘNG:</span>
                        <span class="total-amount">${this.formatCurrency(billData.tongTien || 0)}</span>
                    </div>
                    <div class="payment-status">
                        <span class="status-label">Trạng thái:</span>
                        <span class="status-value ${billData.daThanhToan === 'Đã thanh toán' ? 'paid' : 'unpaid'}">
                            ${billData.daThanhToan || 'Chưa thanh toán'}
                        </span>
                    </div>
                </div>

                <!-- Footer -->
                <div class="bill-footer-beautiful">
                    <div class="signature-section">
                        <div class="signature-box">
                            <p>Người lập hóa đơn</p>
                            <div class="signature-line" style="font-family: 'Brush Script MT', cursive; font-size: 24px; color: #000;">${billData.nguoilaphoadon.split('\r\n')[0]}</div>
                            <div style="font-family: 'Brush Script MT', cursive; font-size: 24px; color: #000;">${billData.nguoilaphoadon.split('\r\n')[1]}</div>
                        </div>
                        <div class="signature-box">
                            <p>Khách hàng</p>
                            <div class="signature-line"></div>
                        </div>
                    </div>
                    <div class="note-section">
                        <p><strong>Ghi chú:</strong></p>
                        <ul>
                            <li>Hóa đơn này có hiệu lực trong vòng 5 ngày</li>
                            <li>Vui lòng thanh toán đúng hạn để tránh phí phạt</li>
                            <li>Liên hệ: ${billData.soDienThoaiCongTy || '0123 456 789'} nếu có thắc mắc</li>
                        </ul>
                    </div>
                </div>
            </div>
        `;
        
        // Debug: Kiểm tra nội dung đã render
        console.log('Modal content rendered:', modalBody.innerHTML);
        
        if (printBillBtn) printBillBtn.disabled = false; // Enable after rendering
        printModal.classList.add('show');
    }

    printBill() {
        console.log('🖨️ Bắt đầu in hóa đơn...');
        
        // Kiểm tra xem có dữ liệu hóa đơn hiện tại không
        if (!this.currentBillData) {
            console.error('❌ Không có dữ liệu hóa đơn để in');
            alert('Không có dữ liệu hóa đơn để in. Vui lòng thử lại.');
            return;
        }
        
        console.log('📄 Sử dụng dữ liệu hóa đơn hiện tại:', this.currentBillData);
        
        // Debug: Kiểm tra nội dung trước khi in
        if (!this.debugPrintContent()) {
            alert('Không có nội dung hóa đơn để in. Vui lòng thử lại.');
            return;
        }
        
        const printContent = document.getElementById('printModalBody');
        
        const printWindow = window.open('', '_blank', 'width=800,height=600');
        if (!printWindow) {
            alert('Trình duyệt đã chặn popup. Vui lòng cho phép popup và thử lại.');
            return;
        }
        
        console.log('🔄 Đang tạo cửa sổ in...');
        
        printWindow.document.write(`
            <html>
                <head>
                    <title>Hóa đơn tiện ích - ${new Date().toLocaleDateString('vi-VN')}</title>
                    <style>
                        @media print {
                            body { margin: 0; }
                            .no-print { display: none; }
                        }
                        
                        body { 
                            font-family: 'Times New Roman', serif; 
                            margin: 20px; 
                            line-height: 1.4;
                            color: #333;
                        }
                        
                        .bill-print-beautiful {
                            max-width: 800px;
                            margin: 0 auto;
                            border: 2px solid #000;
                            padding: 20px;
                        }
                        
                        .bill-header-beautiful {
                            text-align: center;
                            border-bottom: 2px solid #000;
                            padding-bottom: 20px;
                            margin-bottom: 20px;
                        }
                        
                        .company-name {
                            font-size: 24px;
                            font-weight: bold;
                            margin: 0 0 10px 0;
                            color: #000;
                        }
                        
                        .company-address, .company-contact, .company-website {
                            margin: 5px 0;
                            font-size: 14px;
                        }
                        
                        .bill-title-beautiful h2 {
                            font-size: 20px;
                            font-weight: bold;
                            margin: 15px 0 10px 0;
                            text-transform: uppercase;
                        }
                        
                        .bill-number, .bill-date {
                            margin: 5px 0;
                            font-weight: bold;
                        }
                        
                        .customer-info-beautiful {
                            margin-bottom: 20px;
                        }
                        
                        .info-section {
                            margin-bottom: 15px;
                        }
                        
                        .info-section h3 {
                            font-size: 16px;
                            font-weight: bold;
                            margin-bottom: 10px;
                            border-bottom: 1px solid #000;
                            padding-bottom: 5px;
                        }
                        
                        .info-grid {
                            display: grid;
                            grid-template-columns: 1fr 1fr;
                            gap: 10px;
                        }
                        
                        .info-item {
                            display: flex;
                            justify-content: space-between;
                        }
                        
                        .label {
                            font-weight: bold;
                            min-width: 120px;
                        }
                        
                        .bill-details-beautiful h3 {
                            font-size: 16px;
                            font-weight: bold;
                            margin-bottom: 10px;
                            border-bottom: 1px solid #000;
                            padding-bottom: 5px;
                        }
                        
                        .bill-table-beautiful {
                            width: 100%;
                            border-collapse: collapse;
                            margin-bottom: 20px;
                        }
                        
                        .bill-table-beautiful th,
                        .bill-table-beautiful td {
                            border: 1px solid #000;
                            padding: 8px;
                            text-align: center;
                        }
                        
                        .bill-table-beautiful th {
                            background-color: #f0f0f0;
                            font-weight: bold;
                        }
                        
                        .bill-table-beautiful td:first-child {
                            text-align: left;
                        }
                        
                        .bill-total-beautiful {
                            text-align: right;
                            margin-bottom: 20px;
                        }
                        
                        .total-row {
                            font-size: 18px;
                            font-weight: bold;
                            margin-bottom: 10px;
                        }
                        
                        .total-amount {
                            font-size: 20px;
                            color: #000;
                        }
                        
                        .payment-status {
                            margin-top: 10px;
                        }
                        
                        .status-value.paid {
                            color: #28a745;
                            font-weight: bold;
                        }
                        
                        .status-value.unpaid {
                            color: #dc3545;
                            font-weight: bold;
                        }
                        
                        .bill-footer-beautiful {
                            margin-top: 30px;
                        }
                        
                        .signature-section {
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 20px;
                        }
                        
                        .signature-box {
                            text-align: center;
                            width: 45%;
                        }
                        
                        .signature-line {
                            border-top: 1px solid #000;
                            margin-top: 40px;
                        }
                        
                        .note-section {
                            border-top: 1px solid #000;
                            padding-top: 15px;
                        }
                        
                        .note-section ul {
                            margin: 10px 0;
                            padding-left: 20px;
                        }
                        
                        .note-section li {
                            margin-bottom: 5px;
                        }
                    </style>
                </head>
                <body>
                    ${printContent.innerHTML}
                </body>
            </html>
        `);
        
        printWindow.document.close();
        
        console.log('✅ Nội dung đã được ghi vào cửa sổ in');
        
        // Đợi nội dung load xong rồi mới in
        printWindow.onload = function() {
            console.log('📄 Cửa sổ in đã load xong');
            setTimeout(() => {
                console.log('🖨️ Bắt đầu in...');
                printWindow.focus();
                printWindow.print();
                // Đóng cửa sổ sau khi in xong
                setTimeout(() => {
                    console.log('🔒 Đóng cửa sổ in');
                    printWindow.close();
                }, 1000);
            }, 500);
        };
        
        // Fallback nếu onload không hoạt động
        setTimeout(() => {
            if (printWindow.document.readyState === 'complete') {
                console.log('🖨️ Fallback: Bắt đầu in...');
                printWindow.focus();
                printWindow.print();
                setTimeout(() => {
                    console.log('🔒 Fallback: Đóng cửa sổ in');
                    printWindow.close();
                }, 1000);
            }
        }, 1000);
    }

    closePrintModal() {
        const printModal = document.getElementById('printModal');
        if (printModal) {
            printModal.classList.remove('show');
        }
    }

    // Hàm debug để kiểm tra nội dung modal
    debugPrintContent() {
        const printContent = document.getElementById('printModalBody');
        if (!printContent) {
            console.error('❌ Không tìm thấy element printModalBody');
            return false;
        }
        
        console.log('📄 Nội dung modal hiện tại:');
        console.log('HTML length:', printContent.innerHTML.length);
        console.log('Text content:', printContent.textContent);
        console.log('Inner HTML:', printContent.innerHTML);
        
        if (!printContent.innerHTML || printContent.innerHTML.trim() === '') {
            console.error('❌ Nội dung modal trống!');
            return false;
        }
        
        // Kiểm tra xem có dữ liệu hóa đơn không
        const hasBillData = printContent.innerHTML.includes('HÓA ĐƠN TIỆN ÍCH') || 
                           printContent.innerHTML.includes('bill-print-beautiful');
        
        if (!hasBillData) {
            console.error('❌ Không tìm thấy dữ liệu hóa đơn trong modal!');
            return false;
        }
        
        console.log('✅ Nội dung modal hợp lệ');
        return true;
    }

    showAlert(message, type = 'info') {
        const alertContainer = document.getElementById('alertContainer');
        if (!alertContainer) return;
        
        const alert = document.createElement('div');
        alert.className = `alert alert-${type}`;
        alert.innerHTML = `
            <i class="fas fa-${this.getAlertIcon(type)}"></i>
            <span>${message}</span>
        `;

        alertContainer.appendChild(alert);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (alert.parentNode) {
                alert.parentNode.removeChild(alert);
            }
        }, 5000);
    }

    getAlertIcon(type) {
        switch (type) {
            case 'success': return 'check-circle';
            case 'error': return 'exclamation-circle';
            case 'warning': return 'exclamation-triangle';
            default: return 'info-circle';
        }
    }

    showLoading() {
        const loadingOverlay = document.getElementById('loadingOverlay');
        if (loadingOverlay) {
            loadingOverlay.style.display = 'flex';
        }
    }

    hideLoading() {
        const loadingOverlay = document.getElementById('loadingOverlay');
        if (loadingOverlay) {
            loadingOverlay.style.display = 'none';
        }
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    showMiniReceipt(billData) {
        const receipt = `
        <div class="receipt-mini">
            <div class="receipt-header">
                <div class="company">${billData.tenCongTy}</div>
                <div class="address">${billData.diaChiCongTy}</div>
                <div class="phone">ĐT: ${billData.soDienThoaiCongTy}</div>
            </div>
            <div class="receipt-title">PHIẾU THU TIỆN ÍCH</div>
            <div class="receipt-info">
                <div>Mã HĐ: <b>${billData.maHoaDon}</b></div>
                <div>Phòng: <b>${billData.tenPhong}</b></div>
                <div>Khách: <b>${billData.tenKhachHang}</b></div>
                <div>Ngày: ${billData.ngayXuat}</div>
            </div>
            <div class="receipt-table">
                <div class="row"><span>Tiền phòng</span><span>${this.formatCurrency(billData.giaPhong)}</span></div>
                <div class="row"><span>Điện</span><span>${billData.soDien} x ${this.formatCurrency(billData.donGiaDien)}</span></div>
                <div class="row"><span>Nước</span><span>${billData.soNuoc} x ${this.formatCurrency(billData.donGiaNuoc)}</span></div>
                <div class="row"><span>Phí DV</span><span>${this.formatCurrency(billData.phidv)}</span></div>
                <div class="row"><span>Phí giữ xe</span><span>${this.formatCurrency(billData.phiGiuXe)}</span></div>
                <div class="row total"><span>TỔNG CỘNG</span><span>${this.formatCurrency(billData.tongTien)}</span></div>
            </div>
            <div class="receipt-footer">
                <div>Trạng thái: <b>${billData.daThanhToan}</b></div>
                <div style="margin-top:10px">Cảm ơn quý khách!</div>
            </div>
        </div>
        `;
        let receiptDiv = document.getElementById('receipt-content');
        if (!receiptDiv) {
            receiptDiv = document.createElement('div');
            receiptDiv.id = 'receipt-content';
            receiptDiv.style.display = 'none';
            document.body.appendChild(receiptDiv);
        }
        receiptDiv.innerHTML = receipt;
    }

    printMiniReceipt() {
        const printContent = document.getElementById('receipt-content');
        if (!printContent) return;
        const printWindow = window.open('', '_blank', 'width=300,height=600');
        printWindow.document.write(`
            <html>
                <head>
                    <title>Phiếu thu tiện ích</title>
                    <style>
                        body { margin:0; padding:0; }
                        .receipt-mini {
                            width: 260px;
                            font-family: 'Courier New', monospace;
                            font-size: 13px;
                            color: #000;
                            background: #fff;
                            padding: 8px;
                        }
                        .receipt-header, .receipt-footer { text-align: center; }
                        .company { font-weight: bold; font-size: 15px; }
                        .receipt-title { text-align: center; font-weight: bold; margin: 8px 0; }
                        .receipt-info { margin-bottom: 8px; }
                        .receipt-table { border-top: 1px dashed #000; border-bottom: 1px dashed #000; margin-bottom: 8px; }
                        .receipt-table .row { display: flex; justify-content: space-between; padding: 2px 0; }
                        .receipt-table .total { font-weight: bold; border-top: 1px solid #000; margin-top: 4px; }
                    </style>
                </head>
                <body onload="window.print();window.close()">
                    ${printContent.innerHTML}
                </body>
            </html>
        `);
        printWindow.document.close();
    }
}

// Initialize the hoadon manager when the page loads
let hoadonManager;
document.addEventListener('DOMContentLoaded', () => {
    try {
        hoadonManager = new HoadonManager();
        // Make it globally accessible
        window.hoadonManager = hoadonManager;
        console.log('HoadonManager initialized successfully');
    } catch (error) {
        console.error('Error initializing HoadonManager:', error);
    }
});

// Add event delegation for mini receipt print button
document.addEventListener('click', (e) => {
    if (e.target.closest('.btn-mini-receipt')) {
        const billId = e.target.closest('.btn-mini-receipt').getAttribute('data-bill-id');
        if (billId) {
            // Fetch bill data and print mini receipt
            fetch(`/api/UtilityBill/print-hoa-don-beautiful/${billId}`)
                .then(res => res.json())
                .then(result => {
                    if (result.success) {
                        if (window.hoadonManager) {
                            window.hoadonManager.showMiniReceipt(result.data);
                            window.hoadonManager.printMiniReceipt();
                        }
                    } else {
                        alert('Lỗi: ' + result.message);
                    }
                });
        }
    }
}); 