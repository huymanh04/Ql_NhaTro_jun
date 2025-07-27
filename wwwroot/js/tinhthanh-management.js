document.addEventListener('DOMContentLoaded', function() {
    let tinhData = [], filteredTinh = [], currentPage = 1, itemsPerPage = 10, searchValue = '';
    const loadingOverlay = document.getElementById('loadingOverlay');
    const loadingOverlayModal = document.getElementById('loadingOverlayModal');
    const alertContainer = document.getElementById('alertContainer');
    // Load data
    async function loadTinhThanh() {
        showLoading(true);
        const res = await fetch('/api/Location/Tinh-thanh');
        const data = await res.json();
        tinhData = data.success && data.data ? data.data : [];
        applyFilters();
        showLoading(false);
    }
    function applyFilters() {
        filteredTinh = tinhData.filter(t => t.tenTinh.toLowerCase().includes(searchValue.toLowerCase()));
        currentPage = 1;
        renderTable();
        renderPagination();
    }
    function renderTable() {
        const tbody = document.getElementById('tinhTableBody');
        tbody.innerHTML = '';
        const start = (currentPage - 1) * itemsPerPage;
        const pageData = filteredTinh.slice(start, start + itemsPerPage);
        pageData.forEach((t, i) => {
            tbody.innerHTML += `<tr>
                <td>${start + i + 1}</td>
                <td>${t.tenTinh}</td>
                <td>${t.soLuongKhuVuc}</td>
                <td>
                    <button class='btn btn-sm btn-warning btn-edit' data-id='${t.maTinh}' data-name='${t.tenTinh}'><i class='fas fa-edit'></i></button>
                    <button class='btn btn-sm btn-danger btn-delete' data-id='${t.maTinh}' data-name='${t.tenTinh}'><i class='fas fa-trash'></i></button>
                </td>
            </tr>`;
        });
        document.getElementById('paginationInfo').textContent = `Hiển thị ${filteredTinh.length ? start + 1 : 0} - ${Math.min(start + itemsPerPage, filteredTinh.length)} của ${filteredTinh.length} tỉnh thành`;
    }
    function renderPagination() {
        const totalPages = Math.ceil(filteredTinh.length / itemsPerPage);
        const pageNumbers = document.getElementById('pageNumbers');
        pageNumbers.innerHTML = '';
        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement('button');
            btn.className = `page-number ${i === currentPage ? 'active' : ''}`;
            btn.textContent = i;
            btn.onclick = () => { currentPage = i; renderTable(); renderPagination(); };
            pageNumbers.appendChild(btn);
        }
        document.getElementById('prevPageBtn').disabled = currentPage === 1;
        document.getElementById('nextPageBtn').disabled = currentPage === totalPages || totalPages === 0;
    }
    document.getElementById('searchTinh').oninput = function() {
        searchValue = this.value;
        applyFilters();
    };
    document.getElementById('clearFiltersBtn').onclick = function() {
        document.getElementById('searchTinh').value = '';
        searchValue = '';
        applyFilters();
    };
    document.getElementById('prevPageBtn').onclick = function() {
        if (currentPage > 1) { currentPage--; renderTable(); renderPagination(); }
    };
    document.getElementById('nextPageBtn').onclick = function() {
        const totalPages = Math.ceil(filteredTinh.length / itemsPerPage);
        if (currentPage < totalPages) { currentPage++; renderTable(); renderPagination(); }
    };
    // Modal logic
    const tinhModal = document.getElementById('tinhModal');
    const deleteModal = document.getElementById('deleteModal');
    let editingId = null;
    document.getElementById('btnAddTinh').onclick = function() {
        editingId = null;
        document.getElementById('tinhModalTitle').innerHTML = '<i class="fas fa-map-marker-alt"></i> Thêm tỉnh thành';
        document.getElementById('tenTinh').value = '';
        document.getElementById('tenTinhError').textContent = '';
        tinhModal.classList.add('show');
    };
    document.getElementById('cancelModalBtn').onclick = function() { tinhModal.classList.remove('show'); };
    document.getElementById('closeModalBtn').onclick = function() { tinhModal.classList.remove('show'); };
    document.getElementById('saveTinhBtn').onclick = async function() {
        const tenTinh = document.getElementById('tenTinh').value.trim();
        if (!tenTinh) {
            document.getElementById('tenTinhError').textContent = 'Tên tỉnh thành không được để trống';
            return;
        }
        document.getElementById('tenTinhError').textContent = '';
        showLoadingModal(true);
        let url = '/api/Location/add-tinh-thanh';
        let method = 'POST';
        if (editingId) {
            url = `/api/Location/edit-tinh-thanh/${editingId}`;
            method = 'PUT';
        }
        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tenTinh })
        });
        const data = await res.json();
        showLoadingModal(false);
        if (data.success) {
            tinhModal.classList.remove('show');
            showAlert('success', data.message);
            await loadTinhThanh();
        } else {
            showAlert('danger', data.message);
        }
    };
    // Edit
    document.getElementById('tinhTableBody').onclick = function(e) {
        const btn = e.target.closest('.btn-edit');
        if (btn) {
            editingId = btn.dataset.id;
            document.getElementById('tinhModalTitle').innerHTML = '<i class="fas fa-map-marker-alt"></i> Sửa tỉnh thành';
            document.getElementById('tenTinh').value = btn.dataset.name;
            document.getElementById('tenTinhError').textContent = '';
            tinhModal.classList.add('show');
        }
        const btnDel = e.target.closest('.btn-delete');
        if (btnDel) {
            document.getElementById('deleteTinhName').textContent = btnDel.dataset.name;
            deleteModal.classList.add('show');
            deleteModal.dataset.id = btnDel.dataset.id;
        }
    };
    document.getElementById('cancelDeleteBtn').onclick = function() { deleteModal.classList.remove('show'); };
    document.getElementById('closeDeleteModalBtn').onclick = function() { deleteModal.classList.remove('show'); };
    document.getElementById('confirmDeleteBtn').onclick = async function() {
        const id = deleteModal.dataset.id;
        if (!id) return;
        showLoadingModal(true);
        const res = await fetch(`/api/Location/delete-tinh-thanh/${id}`, { method: 'DELETE' });
        const data = await res.json();
        showLoadingModal(false);
        if (data.success) {
            deleteModal.classList.remove('show');
            showAlert('success', data.message);
            await loadTinhThanh();
        } else {
            showAlert('danger', data.message);
        }
    };
    function showAlert(type, message) {
        alertContainer.innerHTML = `<div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
        setTimeout(() => { alertContainer.innerHTML = ''; }, 3000);
    }
    function showLoading(show) { loadingOverlay.style.display = show ? 'flex' : 'none'; }
    function showLoadingModal(show) { document.getElementById('loadingOverlayModal').style.display = show ? 'flex' : 'none'; }
    loadTinhThanh();
}); 