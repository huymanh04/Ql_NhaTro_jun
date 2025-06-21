$(document).ready(function () {
    // --- STATE MANAGEMENT ---
    let allCompensations = [];
    let allContracts = [];
    let allMotels = [];
    let allRooms = [];
    let currentPage = 1;
    let itemsPerPage = 10;
    let currentEditId = null;

    // --- DOM ELEMENTS ---
    const $loadingOverlay = $('#loadingOverlay');
    const $tableBody = $('#tableBody');
    const $paginationInfo = $('#paginationInfo');
    const $pageNumbers = $('#pageNumbers');
    const $prevPageBtn = $('#prevPageBtn');
    const $nextPageBtn = $('#nextPageBtn');
    const $searchInput = $('#searchInput');
    const $contractFilter = $('#contractFilter');
    const $itemsPerPageSelect = $('#itemsPerPage');

    // Cascading dropdowns in modal
    const $motelSelect = $('#motelId');
    const $roomSelect = $('#roomId');
    const $contractSelect = $('#contractId');

    // Modals
    const $compensationModal = $('#compensationModal');
    const $deleteModal = $('#deleteModal');
    const $viewModal = $('#viewModal');
    const $modalTitleText = $('#modalTitleText');
    const $saveBtnText = $('#saveBtnText');
    const $compensationForm = $('#compensationForm');

    // --- DATA FETCHING ---
    function showLoading() { $loadingOverlay.show(); }
    function hideLoading() { $loadingOverlay.hide(); }

    async function fetchData() {
        showLoading();
        try {
            const [compensationsRes, contractsRes, motelsRes] = await Promise.all([
                fetch('/api/Denbu/get-denbu'),
                fetch('/api/Contract/get-contracts'),
                fetch('/api/Motel/get-list-motel')
            ]);

            const compensationsData = await compensationsRes.json();
            const contractsData = await contractsRes.json();
            const motelsData = await motelsRes.json();

            if (compensationsData.success) {
                allCompensations = compensationsData.data;
            } else {
                console.error("Failed to load compensations:", compensationsData.message);
            }

            if (contractsData.success) {
                allContracts = contractsData.data;
                populateContractFilters();
            } else {
                console.error("Failed to load contracts:", contractsData.message);
            }

            if (motelsData && Array.isArray(motelsData)) { // API returns an array directly
                allMotels = motelsData;
            } else if (motelsData.success) { // Or returns an ApiResponse object
                allMotels = motelsData.data;
            } else {
                console.error("Failed to load motels:", motelsData.message);
            }

            render();
        } catch (error) {
            console.error('Failed to fetch data:', error);
            showAlert('Không thể tải dữ liệu. Vui lòng thử lại.', 'danger');
        } finally {
            hideLoading();
        }
    }

    // --- RENDERING ---
    function render() {
        const filteredData = getFilteredData();
        renderTable(filteredData);
        renderMobileCards(filteredData);
        renderPagination(filteredData.length);
    }

    function getFilteredData() {
        const contractId = $contractFilter.val();

        return allCompensations.filter(item => {
            const contract = allContracts.find(c => c.contractId == item.maHopDong);
            const searchTerm = $searchInput.val().toLowerCase();

            const matchesSearch = searchTerm === '' ||
                item.noiDung.toLowerCase().includes(searchTerm) ||
                item.maHopDong.toString().includes(searchTerm) ||
                (contract?.room?.tenPhong?.toLowerCase()?.includes(searchTerm)) ||
                (contract?.room?.motel?.tenNhaTro?.toLowerCase()?.includes(searchTerm));

            const matchesContract = contractId === '' || item.maHopDong == contractId;

            return matchesSearch && matchesContract;
        });
    }

    function renderTable(data) {
        $tableBody.empty();
        if (data.length === 0) {
            $tableBody.html('<tr><td colspan="6" class="text-center">Không tìm thấy dữ liệu phù hợp.</td></tr>');
            return;
        }

        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const pageData = data.slice(startIndex, endIndex);

        pageData.forEach((item, index) => {
            const contract = allContracts.find(c => c.contractId == item.maHopDong);
            const roomName = contract?.room?.tenPhong;
            const motelName = contract?.room?.motel?.tenNhaTro;
            const roomInfo = roomName && motelName ? `${roomName} (${motelName})` : (roomName || 'Không xác định');

            const row = `
                <tr>
                    <td>${startIndex + index + 1}</td>
                    <td><strong>#${item.maHopDong}</strong><br><small>${roomInfo}</small></td>
                    <td>${item.noiDung}</td>
                    <td>${item.soTien.toLocaleString('vi-VN')} VNĐ</td>
                    <td>${new Date(item.ngayTao).toLocaleDateString('vi-VN')}</td>
                    <td>
                        <div class="btn-group">
                            <button class="btn btn-info btn-sm" onclick="window.viewItem(${item.maDenBu})" title="Xem"><i class="fas fa-eye"></i></button>
                            <button class="btn btn-primary btn-sm" onclick="window.editItem(${item.maDenBu})" title="Sửa"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-danger btn-sm" onclick="window.deleteItem(${item.maDenBu})" title="Xóa"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>
                </tr>
            `;
            $tableBody.append(row);
        });
    }

    function renderMobileCards(data) {
        const $mobileCardsContainer = $('#mobileCards');
        $mobileCardsContainer.empty();
        if (data.length === 0) {
            return; // Table already shows the 'no data' message
        }

        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const pageData = data.slice(startIndex, endIndex);

        pageData.forEach(item => {
            const contract = allContracts.find(c => c.contractId == item.maHopDong);
            const roomName = contract?.room?.tenPhong || 'N/A';
            const card = `
                <div class="compensation-card">
                    <div class="card-header">
                        <strong>HĐ #${item.maHopDong} - ${roomName}</strong>
                        <div class="card-actions">
                            <button class="btn btn-info btn-sm" onclick="window.viewItem(${item.maDenBu})" title="Xem"><i class="fas fa-eye"></i></button>
                            <button class="btn btn-primary btn-sm" onclick="window.editItem(${item.maDenBu})" title="Sửa"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-danger btn-sm" onclick="window.deleteItem(${item.maDenBu})" title="Xóa"><i class="fas fa-trash"></i></button>
                        </div>
                    </div>
                    <div class="card-body">
                        <p><strong>Nội dung:</strong> ${item.noiDung}</p>
                        <p><strong>Số tiền:</strong> ${item.soTien.toLocaleString('vi-VN')} VNĐ</p>
                        <p><strong>Ngày tạo:</strong> ${new Date(item.ngayTao).toLocaleDateString('vi-VN')}</p>
                    </div>
                </div>
            `;
            $mobileCardsContainer.append(card);
        });
    }

    function renderPagination(totalItems) {
        const totalPages = Math.ceil(totalItems / itemsPerPage);

        $paginationInfo.text(`Hiển thị ${totalItems > 0 ? (currentPage - 1) * itemsPerPage + 1 : 0} - ${Math.min(currentPage * itemsPerPage, totalItems)} của ${totalItems} mục`);

        $pageNumbers.empty();
        for (let i = 1; i <= totalPages; i++) {
            const btn = `<button class="btn btn-outline page-btn ${i === currentPage ? 'active' : ''}" data-page="${i}">${i}</button>`;
            $pageNumbers.append(btn);
        }

        $prevPageBtn.prop('disabled', currentPage === 1);
        $nextPageBtn.prop('disabled', currentPage === totalPages || totalPages === 0);
    }

    function populateContractFilters() {
        $contractFilter.empty().append('<option value="">Tất cả hợp đồng</option>');
        const modalSelect = $('#contractId').empty().append('<option value="">Chọn hợp đồng</option>');

        allContracts.forEach(c => {
            const room = c.room ? c.room.tenPhong : 'N/A';
            const option = `<option value="${c.contractId}">#${c.contractId} - ${room}</option>`;
            $contractFilter.append(option);
            modalSelect.append(option);
        });
    }

    // --- EVENT HANDLERS ---
    $searchInput.on('input', () => { currentPage = 1; render(); });
    $contractFilter.on('change', () => { currentPage = 1; render(); });
    $itemsPerPageSelect.on('change', function () {
        itemsPerPage = parseInt($(this).val());
        currentPage = 1;
        render();
    });

    $('#clearFiltersBtn').on('click', () => {
        $searchInput.val('');
        $contractFilter.val('');
        currentPage = 1;
        render();
    });

    $(document).on('click', '.page-btn', function () {
        currentPage = parseInt($(this).data('page'));
        render();
    });

    $prevPageBtn.on('click', () => { if (currentPage > 1) { currentPage--; render(); } });
    $nextPageBtn.on('click', () => {
        const totalPages = Math.ceil(getFilteredData().length / itemsPerPage);
        if (currentPage < totalPages) { currentPage++; render(); }
    });

    // --- CASCADING DROPDOWN LOGIC ---
    function populateMotelDropdown() {
        $motelSelect.empty().append('<option value="">Chọn nhà trọ</option>');
        allMotels.forEach(motel => {
            $motelSelect.append(`<option value="${motel.maNhaTro}">${motel.tenNhaTro}</option>`);
        });
    }

    async function updateRoomDropdown(motelId) {
        $roomSelect.prop('disabled', true).empty().append('<option value="">Đang tải...</option>');
        $contractSelect.prop('disabled', true).empty().append('<option value="">Vui lòng chọn phòng</option>');

        if (!motelId) {
            $roomSelect.empty().append('<option value="">Vui lòng chọn nhà trọ</option>');
            return;
        }

        try {
            const response = await fetch(`/api/Room/get-room-trong-by-nha-tro/${motelId}`);
            const result = await response.json();
            if (result.success && result.data.phong && result.data.phong.length > 0) {
                allRooms = result.data.phong;
                $roomSelect.empty().append('<option value="">Chọn phòng</option>');
                allRooms.forEach(room => {
                    $roomSelect.append(`<option value="${room.maPhong}">${room.tenPhong}</option>`);
                });
                $roomSelect.prop('disabled', false);
            } else {
                $roomSelect.empty().append('<option value="">Không có phòng nào đang thuê</option>');
            }
        } catch (error) {
            console.error('Failed to fetch rooms:', error);
            $roomSelect.empty().append('<option value="">Lỗi tải phòng</option>');
        }
    }

    async function updateContractDropdown(roomId) {
        $contractSelect.prop('disabled', true).empty().append('<option value="">Đang tải...</option>');

        if (!roomId) {
            $contractSelect.empty().append('<option value="">Vui lòng chọn phòng</option>');
            return;
        }

        try {
            const response = await fetch(`/api/Contract/get-contract-by-room-id/${roomId}`);
            const result = await response.json();
            if (result.success && result.data) {
                const contract = result.data;
                if (contract && !contract.isCompleted) {
                    $contractSelect.empty().append(`<option value="${contract.contractId}">Hợp đồng #${contract.contractId}</option>`);
                    $contractSelect.prop('disabled', false);
                } else {
                    $contractSelect.empty().append('<option value="">Không có hợp đồng hiệu lực</option>');
                }
            } else {
                $contractSelect.empty().append('<option value="">Không có hợp đồng cho phòng này</option>');
            }
        } catch (error) {
            console.error('Failed to fetch contract:', error);
            $contractSelect.empty().append('<option value="">Lỗi tải hợp đồng</option>');
        }
    }

    $motelSelect.on('change', function () {
        updateRoomDropdown($(this).val());
    });

    $roomSelect.on('change', function () {
        updateContractDropdown($(this).val());
    });

    // --- MODAL & CRUD ACTIONS ---
    function openModal(isEdit = false) {
        $compensationForm[0].reset();
        $('.error-message').text('');
        if (isEdit) {
            $modalTitleText.text('Sửa đền bù');
            $saveBtnText.text('Cập nhật');
        } else {
            $modalTitleText.text('Thêm đền bù');
            $saveBtnText.text('Lưu');
            currentEditId = null;
            // Populate motels when adding
            populateMotelDropdown();
            $roomSelect.prop('disabled', true).empty().append('<option value="">Chọn nhà trọ trước</option>');
            $contractSelect.prop('disabled', true).empty().append('<option value="">Chọn phòng trước</option>');
        }
        $compensationModal.fadeIn();
    }

    function closeModal($modal) {
        $modal.fadeOut();
    }

    $('#addBtn').on('click', () => openModal(false));
    $('#closeModalBtn, #cancelModalBtn').on('click', () => closeModal($compensationModal));

    window.editItem = async function (id) {
        const item = allCompensations.find(c => c.maDenBu == id);
        if (!item) return;

        const contract = allContracts.find(c => c.contractId == item.maHopDong);
        if (!contract || !contract.room || !contract.room.nhaTro) {
            showAlert('Không thể tải thông tin chi tiết cho hợp đồng này.', 'warning');
            return;
        }

        openModal(true);
        currentEditId = id;

        // Populate and set dropdowns sequentially
        populateMotelDropdown();
        $motelSelect.val(contract.room.nhaTro.maNhaTro);

        await updateRoomDropdown(contract.room.nhaTro.maNhaTro);
        $roomSelect.val(contract.room.maPhong);

        await updateContractDropdown(contract.room.maPhong);
        $contractSelect.val(item.maHopDong);

        // Set other fields
        $('#compensationId').val(item.maDenBu);
        $('#noiDung').val(item.noiDung);
        $('#soTien').val(item.soTien);
    };

    window.viewItem = function (id) {
        const item = allCompensations.find(c => c.maDenBu == id);
        if (item) {
            const contract = allContracts.find(c => c.contractId == item.maHopDong);
            const roomName = contract?.room?.tenPhong;
            const motelName = contract?.room?.motel?.tenNhaTro;
            const roomInfo = roomName && motelName ? `${roomName} (${motelName})` : (roomName || 'Không xác định');
            const detailsHtml = `
                <p><strong>Mã đền bù:</strong> ${item.maDenBu}</p>
                <p><strong>Hợp đồng:</strong> #${item.maHopDong} - ${roomInfo}</p>
                <p><strong>Ngày tạo:</strong> ${new Date(item.ngayTao).toLocaleDateString('vi-VN')}</p>
                <p><strong>Số tiền:</strong> ${item.soTien.toLocaleString('vi-VN')} VNĐ</p>
                <hr>
                <p><strong>Nội dung:</strong></p>
                <p>${item.noiDung}</p>
            `;
            $('#viewModalBody').html(detailsHtml);
            $viewModal.fadeIn();
        }
    };
    $('#closeViewModalBtn, #okViewModalBtn').on('click', () => closeModal($viewModal));

    window.deleteItem = function (id) {
        $('#deleteCompensationId').text(`#${id}`);
        $deleteModal.fadeIn();
        $('#confirmDeleteBtn').off('click').on('click', async function () {
            showLoading();
            try {
                const response = await fetch(`/api/Denbu/delete-denbu/${id}`, { method: 'DELETE' });
                const result = await response.json();
                if (result.success) {
                    showAlert('Xóa đền bù thành công.', 'success');
                    fetchData(); // Refetch all data
                } else {
                    showAlert(result.message || 'Có lỗi xảy ra khi xóa.', 'danger');
                }
            } catch (error) {
                showAlert('Không thể kết nối đến máy chủ.', 'danger');
            } finally {
                hideLoading();
                closeModal($deleteModal);
            }
        });
    };
    $('#closeDeleteModalBtn, #cancelDeleteBtn').on('click', () => closeModal($deleteModal));


    $('#saveBtn').on('click', async function () {
        // Simple validation
        let isValid = true;
        if (!$('#contractId').val()) {
            $('#contractIdError').text('Vui lòng chọn hợp đồng.'); isValid = false;
        } else { $('#contractIdError').text(''); }

        if (!$('#noiDung').val().trim()) {
            $('#noiDungError').text('Vui lòng nhập nội dung.'); isValid = false;
        } else { $('#noiDungError').text(''); }

        if (!$('#soTien').val() || $('#soTien').val() < 0) {
            $('#soTienError').text('Vui lòng nhập số tiền hợp lệ.'); isValid = false;
        } else { $('#soTienError').text(''); }

        if (!isValid) return;

        const data = {
            maHopDong: $('#contractId').val(),
            noiDung: $('#noiDung').val(),
            soTien: $('#soTien').val()
        };

        const isEdit = !!currentEditId;
        const url = isEdit ? `/api/Denbu/edit-denbu/${currentEditId}` : '/api/Denbu/add-denbu';
        const method = isEdit ? 'PUT' : 'POST';

        showLoading();
        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            const result = await response.json();

            if (result.success) {
                showAlert(isEdit ? 'Cập nhật thành công.' : 'Thêm mới thành công.', 'success');
                closeModal($compensationModal);
                fetchData(); // Refetch all data
            } else {
                // Try to parse error messages if they are in JSON format
                let errorMessage = 'Có lỗi xảy ra.';
                try {
                    const errorBody = await result.json();
                    if (errorBody && errorBody.errors) {
                        errorMessage = Object.values(errorBody.errors).flat().join(' ');
                    } else if (errorBody.message) {
                        errorMessage = errorBody.message;
                    }
                } catch (e) {
                    // Response was not json, use the original message
                    errorMessage = result.message || errorMessage;
                }
                showAlert(errorMessage, 'danger');
            }
        } catch (error) {
            showAlert('Không thể kết nối đến máy chủ.', 'danger');
        } finally {
            hideLoading();
        }
    });

    function showAlert(message, type = 'success') {
        const alert = $(`<div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`);
        $('#alertContainer').append(alert);
        setTimeout(() => alert.fadeOut('slow', () => alert.remove()), 5000);
    }

    // --- INITIALIZATION ---
    fetchData();
});