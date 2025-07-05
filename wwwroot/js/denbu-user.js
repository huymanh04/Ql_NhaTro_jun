// Denbu User Management JavaScript
class DenbuUserManager {
    constructor() {
        this.allCompensations = [];
        this.filteredCompensations = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadCompensations();
    }

    bindEvents() {
        // Search input
        $('#searchInput').on('input', () => this.applyFilters());
        
        // Sort select
        $('#sortSelect').on('change', () => this.applyFilters());
        
        // Motel filter
        $('#motelFilter').on('change', () => this.applyFilters());
    }

    async loadCompensations() {
        try {
            this.showLoading();

            const response = await fetch('/api/Denbu/get-my-denbu');
            const result = await response.json();

            if (result.success) {
                this.allCompensations = result.data || [];
                this.updateStatistics();
                this.applyFilters();
            } else {
                this.showError(result.message || 'Có lỗi xảy ra khi tải dữ liệu');
            }
        } catch (error) {
            console.error('Error loading compensations:', error);
            this.showError('Không thể kết nối đến máy chủ');
        } finally {
            this.hideLoading();
        }
    }

    updateStatistics() {
        const total = this.allCompensations.length;
        const totalAmount = this.allCompensations.reduce((sum, item) => sum + item.soTien, 0);
        const thisMonth = this.allCompensations.filter(item => {
            const itemDate = new Date(item.ngayTao);
            const now = new Date();
            return itemDate.getMonth() === now.getMonth() && 
                   itemDate.getFullYear() === now.getFullYear();
        }).length;
        
        const latest = this.allCompensations.length > 0 ? 
            new Date(this.allCompensations[0].ngayTao).toLocaleDateString('vi-VN') : 'Chưa có';

        $('#totalCompensations').text(total);
        $('#totalAmount').text(totalAmount.toLocaleString('vi-VN') + ' VNĐ');
        $('#thisMonthCount').text(thisMonth);
        $('#latestDate').text(latest);
    }

    applyFilters() {
        const searchTerm = $('#searchInput').val().toLowerCase();
        const sortBy = $('#sortSelect').val();
        const motelFilter = $('#motelFilter').val();

        // Lọc dữ liệu
        this.filteredCompensations = this.allCompensations.filter(item => {
            const matchesSearch = !searchTerm || 
                item.noiDung.toLowerCase().includes(searchTerm) ||
                item.nhatro.toLowerCase().includes(searchTerm);
            const matchesMotel = !motelFilter || item.nhatro === motelFilter;
            
            return matchesSearch && matchesMotel;
        });

        // Sắp xếp
        switch(sortBy) {
            case 'date-desc':
                this.filteredCompensations.sort((a, b) => new Date(b.ngayTao) - new Date(a.ngayTao));
                break;
            case 'date-asc':
                this.filteredCompensations.sort((a, b) => new Date(a.ngayTao) - new Date(b.ngayTao));
                break;
            case 'amount-desc':
                this.filteredCompensations.sort((a, b) => b.soTien - a.soTien);
                break;
            case 'amount-asc':
                this.filteredCompensations.sort((a, b) => a.soTien - b.soTien);
                break;
        }

        // Cập nhật dropdown nhà trọ
        this.updateMotelFilter();
        
        // Hiển thị dữ liệu
        this.currentPage = 1;
        this.displayCompensations();
    }

    updateMotelFilter() {
        const motels = [...new Set(this.allCompensations.map(item => item.nhatro))];
        const currentValue = $('#motelFilter').val();
        
        let options = '<option value="">Tất cả nhà trọ</option>';
        motels.forEach(motel => {
            const selected = motel === currentValue ? 'selected' : '';
            options += `<option value="${motel}" ${selected}>${motel}</option>`;
        });
        
        $('#motelFilter').html(options);
    }

    displayCompensations() {
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageData = this.filteredCompensations.slice(startIndex, endIndex);

        const tbody = $('#compensationTableBody');
        tbody.empty();

        if (pageData.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="5" class="text-center py-4">
                        <div class="text-muted">
                            <i class="fas fa-inbox fa-3x mb-3"></i>
                            <p class="mb-0">Không có dữ liệu đền bù nào</p>
                        </div>
                    </td>
                </tr>
            `);
        } else {
            pageData.forEach(item => {
                tbody.append(`
                    <tr>
                        <td>
                            <span class="badge bg-primary">#${item.maDenBu}</span>
                        </td>
                        <td>
                            <div class="fw-bold">${item.nhatro}</div>
                            <small class="text-muted">Hợp đồng #${item.maHopDong}</small>
                        </td>
                        <td>
                            <div class="text-truncate" style="max-width: 200px;" title="${item.noiDung}">
                                ${item.noiDung}
                            </div>
                        </td>
                        <td>
                            <span class="fw-bold text-success">
                                ${item.soTien.toLocaleString('vi-VN')} VNĐ
                            </span>
                        </td>
                        <td>
                            <div>${new Date(item.ngayTao).toLocaleDateString('vi-VN')}</div>
                            <small class="text-muted">${new Date(item.ngayTao).toLocaleTimeString('vi-VN')}</small>
                        </td>
                    </tr>
                `);
            });
        }

        // Cập nhật thông tin phân trang
        this.updatePagination();
    }

    updatePagination() {
        const totalPages = Math.ceil(this.filteredCompensations.length / this.itemsPerPage);
        const startItem = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endItem = Math.min(this.currentPage * this.itemsPerPage, this.filteredCompensations.length);

        $('#showingFrom').text(this.filteredCompensations.length > 0 ? startItem : 0);
        $('#showingTo').text(endItem);
        $('#totalItems').text(this.filteredCompensations.length);

        const pagination = $('#pagination');
        pagination.empty();

        if (totalPages <= 1) return;

        // Nút Previous
        const prevDisabled = this.currentPage === 1 ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${prevDisabled}">
                <a class="page-link" href="#" onclick="denbuManager.changePage(${this.currentPage - 1})">
                    <i class="fas fa-chevron-left"></i>
                </a>
            </li>
        `);

        // Các trang
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(totalPages, this.currentPage + 2);

        if (startPage > 1) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="denbuManager.changePage(1)">1</a>
                </li>
            `);
            if (startPage > 2) {
                pagination.append(`
                    <li class="page-item disabled">
                        <span class="page-link">...</span>
                    </li>
                `);
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            const active = i === this.currentPage ? 'active' : '';
            pagination.append(`
                <li class="page-item ${active}">
                    <a class="page-link" href="#" onclick="denbuManager.changePage(${i})">${i}</a>
                </li>
            `);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                pagination.append(`
                    <li class="page-item disabled">
                        <span class="page-link">...</span>
                    </li>
                `);
            }
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="denbuManager.changePage(${totalPages})">${totalPages}</a>
                </li>
            `);
        }

        // Nút Next
        const nextDisabled = this.currentPage === totalPages ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${nextDisabled}">
                <a class="page-link" href="#" onclick="denbuManager.changePage(${this.currentPage + 1})">
                    <i class="fas fa-chevron-right"></i>
                </a>
            </li>
        `);
    }

    changePage(page) {
        this.currentPage = page;
        this.displayCompensations();
    }

    resetFilters() {
        $('#searchInput').val('');
        $('#sortSelect').val('date-desc');
        $('#motelFilter').val('');
        this.applyFilters();
    }

    viewDetail(compensationId) {
        try {
            const compensation = this.allCompensations.find(item => item.maDenBu === compensationId);
            if (!compensation) {
                this.showError('Không tìm thấy thông tin đền bù');
                return;
            }

            const modalBody = $('#detailModalBody');
            modalBody.html(`
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Mã đền bù:</label>
                            <div class="fs-5">#${compensation.maDenBu}</div>
                        </div>
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Mã hợp đồng:</label>
                            <div class="fs-5">#${compensation.maHopDong}</div>
                        </div>
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Nhà trọ:</label>
                            <div class="fs-5">${compensation.nhatro}</div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Số tiền:</label>
                            <div class="fs-4 text-success fw-bold">
                                ${compensation.soTien.toLocaleString('vi-VN')} VNĐ
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Ngày tạo:</label>
                            <div class="fs-5">
                                ${new Date(compensation.ngayTao).toLocaleDateString('vi-VN')}
                            </div>
                            <small class="text-muted">
                                ${new Date(compensation.ngayTao).toLocaleTimeString('vi-VN')}
                            </small>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-12">
                        <div class="mb-3">
                            <label class="fw-bold text-muted">Nội dung:</label>
                            <div class="p-3 bg-light rounded">
                                ${compensation.noiDung}
                            </div>
                        </div>
                    </div>
                </div>
            `);

            $('#detailModal').modal('show');
        } catch (error) {
            console.error('Error viewing detail:', error);
            this.showError('Có lỗi xảy ra khi xem chi tiết');
        }
    }

    showLoading() {
        $('#compensationTableBody').html(`
            <tr>
                <td colspan="6" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Đang tải...</span>
                    </div>
                </td>
            </tr>
        `);
    }

    hideLoading() {
        // Loading sẽ được thay thế bởi displayCompensations()
    }

    showError(message) {
        $('#compensationTableBody').html(`
            <tr>
                <td colspan="6" class="text-center py-4">
                    <div class="text-danger">
                        <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                        <p class="mb-0">${message}</p>
                    </div>
                </td>
            </tr>
        `);
    }
}

// Global functions for onclick handlers
function resetFilters() {
    denbuManager.resetFilters();
}

function applyFilters() {
    denbuManager.applyFilters();
}

function changePage(page) {
    denbuManager.changePage(page);
}

function viewDetail(compensationId) {
    denbuManager.viewDetail(compensationId);
}

// Initialize when document is ready
let denbuManager;
$(document).ready(function() {
    denbuManager = new DenbuUserManager();
}); 