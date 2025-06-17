// Room Management JavaScript
class RoomManagement {
    constructor() {
        this.rooms = [];
        this.nhaTros = [];
        this.theLoais = [];
        this.filteredRooms = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.editingRoomId = null;
        this.selectedImages = [];
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadData();
    }

    bindEvents() {
        // Add room button
        document.getElementById('addRoomBtn').addEventListener('click', () => {
            this.openAddModal();
        });

        // Modal close buttons
        document.getElementById('closeModalBtn').addEventListener('click', () => {
            this.closeModal();
        });
        
        document.getElementById('cancelModalBtn').addEventListener('click', () => {
            this.closeModal();
        });

        // Save room button
        document.getElementById('saveRoomBtn').addEventListener('click', () => {
            this.saveRoom();
        });

        // Delete modal buttons
        document.getElementById('closeDeleteModalBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('cancelDeleteBtn').addEventListener('click', () => {
            this.closeDeleteModal();
        });
        
        document.getElementById('confirmDeleteBtn').addEventListener('click', () => {
            this.deleteRoom();
        });

        // Search and filters
        document.getElementById('searchInput').addEventListener('input', (e) => {
            this.filterRooms();
        });
        
        document.getElementById('nhaTroFilter').addEventListener('change', () => {
            this.filterRooms();
        });
        
        document.getElementById('theLoaiFilter').addEventListener('change', () => {
            this.filterRooms();
        });
        
        document.getElementById('trangThaiFilter').addEventListener('change', () => {
            this.filterRooms();
        });

        document.getElementById('clearFiltersBtn').addEventListener('click', () => {
            this.clearFilters();
        });

        // Items per page change
        document.getElementById('itemsPerPage').addEventListener('change', (e) => {
            console.log('=== ITEMS PER PAGE CHANGE EVENT ===');
            console.log('Old itemsPerPage:', this.itemsPerPage);
            console.log('New value from dropdown:', e.target.value);
            
            this.itemsPerPage = parseInt(e.target.value);
            this.currentPage = 1; // Reset to first page
            
            console.log('Updated itemsPerPage:', this.itemsPerPage);
            console.log('Current page reset to:', this.currentPage);
            
            this.renderTable();
            this.renderPagination();
        });

        // Image upload
        document.getElementById('images').addEventListener('change', (e) => {
            this.handleImageUpload(e);
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
            const totalPages = Math.ceil(this.filteredRooms.length / this.itemsPerPage);
            if (this.currentPage < totalPages) {
                this.currentPage++;
                this.renderTable();
                this.renderPagination();
            }
        });

        // Modal click outside to close
        document.getElementById('roomModal').addEventListener('click', (e) => {
            if (e.target.id === 'roomModal') {
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
                const roomId = parseInt(e.target.closest('.btn-edit').dataset.id);
                this.openEditModal(roomId);
            }
            
            if (e.target.closest('.btn-delete')) {
                e.preventDefault();
                const roomId = parseInt(e.target.closest('.btn-delete').dataset.id);
                this.openDeleteModal(roomId);
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
                const index = parseInt(e.target.closest('.remove-btn').dataset.index);
                this.removeImage(index);
            }
        });
    }

    async loadData() {
        this.showLoading();
        try {
            await Promise.all([
                this.loadRooms(),
                this.loadNhaTros(),
                this.loadTheLoais()
            ]);
            this.filterRooms();
        } catch (error) {
            console.error('Error loading data:', error);
            this.showAlert('Có lỗi xảy ra khi tải dữ liệu', 'error');
        } finally {
            this.hideLoading();
        }
    }

    async loadRooms() {
        try {
            const response = await fetch('/api/Room/get-all-room');
            const result = await response.json();
            
            if (result.success) {
                this.rooms = result.data.phong || [];
                this.roomImages = result.data.hinhAnh || [];
            } else {
                throw new Error(result.message || 'Không thể tải danh sách phòng');
            }
        } catch (error) {
            console.error('Error loading rooms:', error);
            throw error;
        }
    }

    async loadNhaTros() {
        try {
            const response = await fetch('/api/Motel/get-list-motel');
            const result = await response.json();
            
            if (Array.isArray(result)) {
                this.nhaTros = result.map(item => ({
                    maNhaTro: item.maNhaTro,
                    tenNhaTro: item.tenNhaTro
                }));
                this.populateNhaTroSelects();
            } else {
                throw new Error('Không thể tải danh sách nhà trọ');
            }
        } catch (error) {
            console.error('Error loading nha tros:', error);
            this.nhaTros = [];
        }
    }

    async loadTheLoais() {
        try {
            const response = await fetch('/api/RoomType/get-type-room');
            const result = await response.json();
            
            if (result.success) {
                this.theLoais = result.data || [];
                this.populateTheLoaiSelects();
            } else {
                throw new Error(result.message || 'Không thể tải danh sách loại phòng');
            }
        } catch (error) {
            console.error('Error loading room types:', error);
            this.theLoais = [];
        }
    }

    populateNhaTroSelects() {
        const selects = ['maNhaTro', 'nhaTroFilter'];
        
        selects.forEach(selectId => {
            const select = document.getElementById(selectId);
            if (select) {
                // Keep the first option (placeholder)
                const firstOption = select.querySelector('option');
                select.innerHTML = '';
                if (firstOption) {
                    select.appendChild(firstOption);
                }
                
                this.nhaTros.forEach(nhaTro => {
                    const option = document.createElement('option');
                    option.value = nhaTro.maNhaTro;
                    option.textContent = nhaTro.tenNhaTro;
                    select.appendChild(option);
                });
            }
        });
    }

    populateTheLoaiSelects() {
        const selects = ['maTheLoai', 'theLoaiFilter'];
        
        selects.forEach(selectId => {
            const select = document.getElementById(selectId);
            if (select) {
                // Keep the first option (placeholder)
                const firstOption = select.querySelector('option');
                select.innerHTML = '';
                if (firstOption) {
                    select.appendChild(firstOption);
                }
                
                this.theLoais.forEach(theLoai => {
                    const option = document.createElement('option');
                    option.value = theLoai.maTheLoai;
                    option.textContent = theLoai.tenTheLoai;
                    select.appendChild(option);
                });
            }
        });
    }

    filterRooms() {
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
        const nhaTroFilter = document.getElementById('nhaTroFilter').value;
        const theLoaiFilter = document.getElementById('theLoaiFilter').value;
        const trangThaiFilter = document.getElementById('trangThaiFilter').value;

        this.filteredRooms = this.rooms.filter(room => {
            const matchesSearch = room.tenPhong.toLowerCase().includes(searchTerm);
            const matchesNhaTro = !nhaTroFilter || room.maNhaTro.toString() === nhaTroFilter;
            const matchesTheLoai = !theLoaiFilter || room.maTheLoai.toString() === theLoaiFilter;
            const matchesTrangThai = !trangThaiFilter || room.conTrong.toString() === trangThaiFilter;

            return matchesSearch && matchesNhaTro && matchesTheLoai && matchesTrangThai;
        });

        this.currentPage = 1;
        this.renderTable();
        this.renderPagination();
    }

    clearFilters() {
        document.getElementById('searchInput').value = '';
        document.getElementById('nhaTroFilter').value = '';
        document.getElementById('theLoaiFilter').value = '';
        document.getElementById('trangThaiFilter').value = '';
        this.filterRooms();
    }

    renderTable() {
        const tbody = document.getElementById('roomsTableBody');
        const mobileCards = document.getElementById('mobileRoomCards');
        
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageRooms = this.filteredRooms.slice(startIndex, endIndex);

        // Clear any existing content first
        tbody.innerHTML = '';
        mobileCards.innerHTML = '';

        // Debug logging (temporary - can be removed later)
        console.log('=== ROOM PAGINATION DEBUG ===');
        console.log('Current page:', this.currentPage);
        console.log('Items per page:', this.itemsPerPage);
        console.log('Total filtered rooms:', this.filteredRooms.length);
        console.log('Start index:', startIndex);
        console.log('End index:', endIndex);
        console.log('Page rooms length (should be ≤ itemsPerPage):', pageRooms.length);

        // Render desktop table
        tbody.innerHTML = pageRooms.map((room, index) => {
            const nhaTro = this.nhaTros.find(nt => nt.maNhaTro === room.maNhaTro);
            const theLoai = this.theLoais.find(tl => tl.maTheLoai === room.maTheLoai);
            const roomImage = this.roomImages?.find(img => img.maPhong === room.maPhong && img.isMain);
            
            // Calculate STT (số thứ tự)
            const stt = (this.currentPage - 1) * this.itemsPerPage + index + 1;
            
            return `
                <tr>
                    <td><strong>${stt}</strong></td>
                    <td>
                        ${roomImage ? 
                            `<img src="data:image/jpeg;base64,${roomImage.duongDanHinhBase64}" alt="${room.tenPhong}" class="room-image">` :
                            `<div class="no-image">Không có ảnh</div>`
                        }
                    </td>
                    <td><strong>${room.tenPhong}</strong></td>
                    <td>${nhaTro ? nhaTro.tenNhaTro : 'N/A'}</td>
                    <td>${theLoai ? theLoai.tenTheLoai : 'N/A'}</td>
                    <td><span class="price">${this.formatPrice(room.gia)}</span></td>
                    <td><span class="area">${room.dienTich} m²</span></td>
                    <td>
                        <span class="status-badge ${room.conTrong ? 'status-available' : 'status-occupied'}">
                            ${room.conTrong ? 'Còn trống' : 'Đã thuê'}
                        </span>
                    </td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn-action btn-edit" data-id="${room.maPhong}">
                                <i class="fas fa-edit"></i> Sửa
                            </button>
                            <button class="btn-action btn-delete" data-id="${room.maPhong}">
                                <i class="fas fa-trash"></i> Xóa
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');

        // Render mobile cards
        mobileCards.innerHTML = pageRooms.map((room, index) => {
            const nhaTro = this.nhaTros.find(nt => nt.maNhaTro === room.maNhaTro);
            const theLoai = this.theLoais.find(tl => tl.maTheLoai === room.maTheLoai);
            const roomImage = this.roomImages?.find(img => img.maPhong === room.maPhong && img.isMain);
            
            // Calculate STT (số thứ tự)
            const stt = (this.currentPage - 1) * this.itemsPerPage + index + 1;
            
            return `
                <div class="mobile-card">
                    <div class="mobile-card-header">
                        ${roomImage ? 
                            `<img src="data:image/jpeg;base64,${roomImage.duongDanHinhBase64}" alt="${room.tenPhong}" class="mobile-card-image">` :
                            `<div class="no-image" style="width: 80px; height: 80px;">Không có ảnh</div>`
                        }
                        <div class="mobile-card-info">
                            <h3>${room.tenPhong}</h3>
                            <p>STT: ${stt}</p>
                        </div>
                    </div>
                    <div class="mobile-card-details">
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Nhà trọ</span>
                            <span class="mobile-detail-value">${nhaTro ? nhaTro.tenNhaTro : 'N/A'}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Loại phòng</span>
                            <span class="mobile-detail-value">${theLoai ? theLoai.tenTheLoai : 'N/A'}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Giá thuê</span>
                            <span class="mobile-detail-value price">${this.formatPrice(room.gia)}</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Diện tích</span>
                            <span class="mobile-detail-value">${room.dienTich} m²</span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Trạng thái</span>
                            <span class="status-badge ${room.conTrong ? 'status-available' : 'status-occupied'}">
                                ${room.conTrong ? 'Còn trống' : 'Đã thuê'}
                            </span>
                        </div>
                        <div class="mobile-detail-item">
                            <span class="mobile-detail-label">Mô tả</span>
                            <span class="mobile-detail-value">${room.moTa || 'Không có mô tả'}</span>
                        </div>
                    </div>
                    <div class="mobile-card-actions">
                        <button class="btn-action btn-edit" data-id="${room.maPhong}">
                            <i class="fas fa-edit"></i> Sửa
                        </button>
                        <button class="btn-action btn-delete" data-id="${room.maPhong}">
                            <i class="fas fa-trash"></i> Xóa
                        </button>
                    </div>
                </div>
            `;
        }).join('');

        console.log('✅ RENDERED:');
        console.log('  - Table rows:', tbody.children.length);
        console.log('  - Mobile cards:', mobileCards.children.length);
        console.log('  - Both should equal pageRooms.length:', pageRooms.length);

        this.updatePaginationInfo();
    }

    renderPagination() {
        const totalPages = Math.ceil(this.filteredRooms.length / this.itemsPerPage);
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
        const totalItems = this.filteredRooms.length;
        const startIndex = (this.currentPage - 1) * this.itemsPerPage + 1;
        const endIndex = Math.min(startIndex + this.itemsPerPage - 1, totalItems);
        
        document.getElementById('paginationInfo').textContent = 
            `Hiển thị ${totalItems > 0 ? startIndex : 0} - ${totalItems > 0 ? endIndex : 0} của ${totalItems} phòng trọ`;
    }

    formatPrice(price) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    }

    openAddModal() {
        this.editingRoomId = null;
        this.selectedImages = [];
        document.getElementById('modalTitleText').textContent = 'Thêm phòng trọ';
        document.getElementById('saveBtnText').textContent = 'Thêm';
        document.getElementById('imageUpdateNote').style.display = 'none';
        
        // Enable all fields for add mode
        document.getElementById('maNhaTro').disabled = false;
        document.getElementById('maTheLoai').disabled = false;
        
        // Reset visual styling
        document.getElementById('maNhaTro').style.backgroundColor = '';
        document.getElementById('maTheLoai').style.backgroundColor = '';
        
        this.resetForm();
        this.showModal();
        this.scrollToModal();
    }

    openEditModal(roomId) {
        const room = this.rooms.find(r => r.maPhong === roomId);
        if (!room) {
            this.showAlert('Không tìm thấy phòng trọ', 'error');
            return;
        }

        this.editingRoomId = roomId;
        this.selectedImages = [];
        document.getElementById('modalTitleText').textContent = 'Sửa phòng trọ';
        document.getElementById('saveBtnText').textContent = 'Cập nhật';
        document.getElementById('imageUpdateNote').style.display = 'block';
        
        // Fill form with room data
        document.getElementById('tenPhong').value = room.tenPhong;
        document.getElementById('maNhaTro').value = room.maNhaTro;
        document.getElementById('maTheLoai').value = room.maTheLoai;
        document.getElementById('gia').value = room.gia;
        document.getElementById('dienTich').value = room.dienTich;
        document.getElementById('conTrong').value = room.conTrong.toString();
        document.getElementById('moTa').value = room.moTa || '';
        
        // Disable maNhaTro and maTheLoai fields during edit
        document.getElementById('maNhaTro').disabled = true;
        document.getElementById('maTheLoai').disabled = true;
        
        // Add visual indication that these fields are read-only
        document.getElementById('maNhaTro').style.backgroundColor = '#f3f4f6';
        document.getElementById('maTheLoai').style.backgroundColor = '#f3f4f6';
        
        this.showModal();
        this.scrollToModal();
    }

    openDeleteModal(roomId) {
        const room = this.rooms.find(r => r.maPhong === roomId);
        if (!room) {
            this.showAlert('Không tìm thấy phòng trọ', 'error');
            return;
        }

        this.editingRoomId = roomId;
        document.getElementById('deleteRoomName').textContent = room.tenPhong;
        document.getElementById('deleteModal').classList.add('show');
    }

    showModal() {
        document.getElementById('roomModal').classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    closeModal() {
        document.getElementById('roomModal').classList.remove('show');
        document.body.style.overflow = '';
        this.resetForm();
    }

    closeDeleteModal() {
        document.getElementById('deleteModal').classList.remove('show');
        this.editingRoomId = null;
    }

    scrollToModal() {
        setTimeout(() => {
            // Auto scroll to top for better UX
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
            
            // Focus on first input after scroll
            setTimeout(() => {
                const firstInput = document.querySelector('#roomModal input:not([type="file"])');
                if (firstInput) {
                    firstInput.focus();
                }
            }, 400);
        }, 100);
    }

    resetForm() {
        document.getElementById('roomForm').reset();
        document.getElementById('imageUpdateNote').style.display = 'none';
        
        // Reset field states
        document.getElementById('maNhaTro').disabled = false;
        document.getElementById('maTheLoai').disabled = false;
        document.getElementById('maNhaTro').style.backgroundColor = '';
        document.getElementById('maTheLoai').style.backgroundColor = '';
        
        this.clearErrors();
        this.clearImagePreview();
        this.selectedImages = [];
    }

    clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(el => {
            el.classList.remove('show');
            el.textContent = '';
        });
    }

    handleImageUpload(event) {
        const files = Array.from(event.target.files);
        const maxSize = 5 * 1024 * 1024; // 5MB
        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
        
        let validFiles = [];
        let errors = [];

        files.forEach(file => {
            if (!allowedTypes.includes(file.type)) {
                errors.push(`${file.name}: Định dạng không được hỗ trợ`);
                return;
            }
            
            if (file.size > maxSize) {
                errors.push(`${file.name}: Kích thước quá lớn (tối đa 5MB)`);
                return;
            }
            
            validFiles.push(file);
        });

        if (errors.length > 0) {
            this.showError('imagesError', errors.join('\n'));
            return;
        }

        this.selectedImages = validFiles;
        this.showImagePreview();
    }

    showImagePreview() {
        const previewContainer = document.getElementById('imagePreview');
        
        if (this.selectedImages.length === 0) {
            previewContainer.style.display = 'none';
            return;
        }

        previewContainer.style.display = 'block';
        previewContainer.innerHTML = '';

        this.selectedImages.forEach((file, index) => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const previewItem = document.createElement('div');
                previewItem.className = 'image-preview-item';
                previewItem.innerHTML = `
                    <img src="${e.target.result}" alt="Preview ${index + 1}">
                    <button type="button" class="remove-btn" data-index="${index}">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                previewContainer.appendChild(previewItem);
            };
            reader.readAsDataURL(file);
        });
    }

    removeImage(index) {
        this.selectedImages.splice(index, 1);
        this.showImagePreview();
        
        // Update file input
        const fileInput = document.getElementById('images');
        const dt = new DataTransfer();
        this.selectedImages.forEach(file => dt.items.add(file));
        fileInput.files = dt.files;
    }

    clearImagePreview() {
        const previewContainer = document.getElementById('imagePreview');
        previewContainer.style.display = 'none';
        previewContainer.innerHTML = '';
    }

    validateForm() {
        this.clearErrors();
        let isValid = true;

        const tenPhong = document.getElementById('tenPhong').value.trim();
        const maNhaTro = document.getElementById('maNhaTro').value;
        const maTheLoai = document.getElementById('maTheLoai').value;
        const gia = document.getElementById('gia').value;
        const dienTich = document.getElementById('dienTich').value;
        const conTrong = document.getElementById('conTrong').value;

        if (!tenPhong) {
            this.showError('tenPhongError', 'Vui lòng nhập tên phòng');
            isValid = false;
        }

        // Only validate maNhaTro and maTheLoai for new rooms (add operation)
        if (!this.editingRoomId) {
            if (!maNhaTro) {
                this.showError('maNhaTroError', 'Vui lòng chọn nhà trọ');
                isValid = false;
            }

            if (!maTheLoai) {
                this.showError('maTheLoaiError', 'Vui lòng chọn loại phòng');
                isValid = false;
            }
        }

        if (!gia || parseFloat(gia) <= 0) {
            this.showError('giaError', 'Vui lòng nhập giá thuê hợp lệ');
            isValid = false;
        }

        if (!dienTich || parseFloat(dienTich) <= 0) {
            this.showError('dienTichError', 'Vui lòng nhập diện tích hợp lệ');
            isValid = false;
        }

        if (conTrong === '') {
            this.showError('conTrongError', 'Vui lòng chọn trạng thái');
            isValid = false;
        }

        return isValid;
    }

    showError(elementId, message) {
        const errorElement = document.getElementById(elementId);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.classList.add('show');
        }
    }

    async saveRoom() {
        if (!this.validateForm()) {
            return;
        }

        const formData = new FormData();
        
        // Common fields for both add and edit
        formData.append('tenPhong', document.getElementById('tenPhong').value.trim());
        formData.append('gia', document.getElementById('gia').value);
        formData.append('dienTich', document.getElementById('dienTich').value);
        formData.append('conTrong', document.getElementById('conTrong').value === 'true');
        formData.append('moTa', document.getElementById('moTa').value.trim());

        // Only add maNhaTro and maTheLoai for new rooms (add operation)
        if (!this.editingRoomId) {
            formData.append('maNhaTro', document.getElementById('maNhaTro').value);
            formData.append('maTheLoai', document.getElementById('maTheLoai').value);
        }

        // Handle images - different logic for add vs edit
        if (!this.editingRoomId) {
            // For add operation: include all selected images
            this.selectedImages.forEach((file, index) => {
                formData.append('images', file);
            });
        } else {
            // For edit operation: only include images if user selected new ones
            if (this.selectedImages && this.selectedImages.length > 0) {
                this.selectedImages.forEach((file, index) => {
                    formData.append('images', file);
                });
            }
            // If no new images selected, don't append images field at all
            // The controller will handle this case and keep existing images
        }

        this.showLoading();

        try {
            const url = this.editingRoomId 
                ? `/api/Room/edit-room/${this.editingRoomId}`
                : '/api/Room/add-room';
            
            const method = this.editingRoomId ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                body: formData
            });

            if (!response.ok) {
                const responseText = await response.text();
                console.error('Response error:', responseText);
                throw new Error(`HTTP ${response.status}: ${responseText}`);
            }

            const result = await response.json();

            if (result.success) {
                this.showAlert(
                    this.editingRoomId ? 'Cập nhật phòng trọ thành công!' : 'Thêm phòng trọ thành công!',
                    'success'
                );
                this.closeModal();
                await this.loadRooms();
                this.filterRooms();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra', 'error');
            }
        } catch (error) {
            console.error('Error saving room:', error);
            this.showAlert(`Có lỗi xảy ra khi lưu phòng trọ: ${error.message}`, 'error');
        } finally {
            this.hideLoading();
        }
    }

    async deleteRoom() {
        if (!this.editingRoomId) return;

        this.showLoading();

        try {
            const response = await fetch(`/api/Room/delete-room/${this.editingRoomId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert('Xóa phòng trọ thành công!', 'success');
                this.closeDeleteModal();
                await this.loadRooms();
                this.filterRooms();
            } else {
                this.showAlert(result.message || 'Có lỗi xảy ra khi xóa phòng trọ', 'error');
            }
        } catch (error) {
            console.error('Error deleting room:', error);
            this.showAlert('Có lỗi xảy ra khi xóa phòng trọ', 'error');
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
    new RoomManagement();
}); 