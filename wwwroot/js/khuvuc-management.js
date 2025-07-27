document.addEventListener('DOMContentLoaded', function() {
    let khuVucData = [], filteredKhuVuc = [], currentPage = 1, itemsPerPage = 10, searchValue = '', filterTinh = '';
    const loadingOverlay = document.getElementById('loadingOverlayKV');
    const loadingOverlayModal = document.getElementById('loadingOverlayModal');
    const alertContainer = document.getElementById('alertContainer');
    // Load tỉnh cho filter và form
    async function loadTinhOptions(selectedId) {
        const res = await fetch('/api/Location/Tinh-thanh');
        const data = await res.json();
        const select = document.getElementById('maTinh');
        select.innerHTML = '<option value="">-- Chọn tỉnh thành --</option>';
        if (data.success && data.data) {
            data.data.forEach(t => {
                select.innerHTML += `<option value="${t.maTinh}" ${selectedId == t.maTinh ? 'selected' : ''}>${t.tenTinh}</option>`;
            });
        }
        // Filter select
        const filter = document.getElementById('filterTinh');
        filter.innerHTML = '<option value="">-- Lọc theo tỉnh thành --</option>';
        if (data.success && data.data) {
            data.data.forEach(t => {
                filter.innerHTML += `<option value="${t.maTinh}">${t.tenTinh}</option>`;
            });
        }
    }
    // Load data
    async function loadKhuVuc() {
        showLoading(true);
        let url = '/api/Location/get-khu-vuc?pageSize=1000';
        const res = await fetch(url);
        const data = await res.json();
        khuVucData = data.success && data.data ? data.data : [];
        applyFilters();
        showLoading(false);
    }
    function applyFilters() {
        filteredKhuVuc = khuVucData.filter(k =>
            k.tenKhuVuc.toLowerCase().includes(searchValue.toLowerCase()) &&
            (!filterTinh || k.maTinh == filterTinh)
        );
        currentPage = 1;
        renderTable();
        renderPagination();
    }
    function renderTable() {
        const tbody = document.getElementById('khuVucTableBody');
        tbody.innerHTML = '';
        const start = (currentPage - 1) * itemsPerPage;
        const pageData = filteredKhuVuc.slice(start, start + itemsPerPage);
        pageData.forEach((k, i) => {
            tbody.innerHTML += `<tr>
                <td>${start + i + 1}</td>
                <td>${k.tenKhuVuc}</td>
                <td>${k.maTinh}</td>
                <td>
                    <button class='btn btn-sm btn-warning btn-edit' data-id='${k.maKhuVuc}' data-name='${k.tenKhuVuc}' data-matinh='${k.maTinh}'><i class='fas fa-edit'></i></button>
                    <button class='btn btn-sm btn-danger btn-delete' data-id='${k.maKhuVuc}' data-name='${k.tenKhuVuc}'><i class='fas fa-trash'></i></button>
                </td>
            </tr>`;
        });
        document.getElementById('paginationInfo').textContent = `Hiển thị ${filteredKhuVuc.length ? start + 1 : 0} - ${Math.min(start + itemsPerPage, filteredKhuVuc.length)} của ${filteredKhuVuc.length} khu vực`;
    }
    function renderPagination() {
        const totalPages = Math.ceil(filteredKhuVuc.length / itemsPerPage);
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
    document.getElementById('searchKhuVuc').oninput = function() {
        searchValue = this.value;
        applyFilters();
    };
    document.getElementById('filterTinh').onchange = function() {
        filterTinh = this.value;
        applyFilters();
    };
    document.getElementById('clearFiltersBtn').onclick = function() {
        document.getElementById('searchKhuVuc').value = '';
        document.getElementById('filterTinh').value = '';
        searchValue = '';
        filterTinh = '';
        applyFilters();
    };
    document.getElementById('prevPageBtn').onclick = function() {
        if (currentPage > 1) { currentPage--; renderTable(); renderPagination(); }
    };
    document.getElementById('nextPageBtn').onclick = function() {
        const totalPages = Math.ceil(filteredKhuVuc.length / itemsPerPage);
        if (currentPage < totalPages) { currentPage++; renderTable(); renderPagination(); }
    };
    // Modal logic
    const khuVucModal = document.getElementById('khuVucModal');
    const deleteModal = document.getElementById('deleteModal');
    let editingId = null;
    document.getElementById('btnAddKhuVuc').onclick = async function() {
        editingId = null;
        document.getElementById('khuVucModalTitle').innerHTML = '<i class="fas fa-map"></i> Thêm khu vực';
        document.getElementById('tenKhuVuc').value = '';
        document.getElementById('maTinh').value = '';
        document.getElementById('tenKhuVucError').textContent = '';
        document.getElementById('maTinhError').textContent = '';
        await loadTinhOptions();
        khuVucModal.classList.add('show');
    };
    document.getElementById('cancelModalBtn').onclick = function() { khuVucModal.classList.remove('show'); };
    document.getElementById('closeModalBtn').onclick = function() { khuVucModal.classList.remove('show'); };
    document.getElementById('saveKhuVucBtn').onclick = async function() {
        const tenKhuVuc = document.getElementById('tenKhuVuc').value.trim();
        const maTinh = document.getElementById('maTinh').value;
        if (!tenKhuVuc) {
            document.getElementById('tenKhuVucError').textContent = 'Tên khu vực không được để trống';
            return;
        }
        document.getElementById('tenKhuVucError').textContent = '';
        if (!maTinh) {
            document.getElementById('maTinhError').textContent = 'Vui lòng chọn tỉnh thành';
            return;
        }
        document.getElementById('maTinhError').textContent = '';
        showLoadingModal(true);
        let url = '/api/Location/add-khu-vuc';
        let method = 'POST';
        if (editingId) {
            url = `/api/Location/Edit-khu-vuc/${editingId}`;
            method = 'PUT';
        }
        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tenKhuVuc, maTinh: parseInt(maTinh) })
        });
        const data = await res.json();
        showLoadingModal(false);
        if (data.success) {
            khuVucModal.classList.remove('show');
            showAlert('success', data.message);
            await loadKhuVuc();
        } else {
            showAlert('danger', data.message);
        }
    };
    // Edit
    document.getElementById('khuVucTableBody').onclick = function(e) {
        const btn = e.target.closest('.btn-edit');
        if (btn) {
            editingId = btn.dataset.id;
            document.getElementById('khuVucModalTitle').innerHTML = '<i class="fas fa-map"></i> Sửa khu vực';
            document.getElementById('tenKhuVuc').value = btn.dataset.name;
            document.getElementById('maTinh').value = btn.dataset.matinh;
            document.getElementById('tenKhuVucError').textContent = '';
            document.getElementById('maTinhError').textContent = '';
            loadTinhOptions(btn.dataset.matinh).then(() => {
                khuVucModal.classList.add('show');
            });
        }
        const btnDel = e.target.closest('.btn-delete');
        if (btnDel) {
            document.getElementById('deleteKhuVucName').textContent = btnDel.dataset.name;
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
        const res = await fetch(`/api/Location/delete-khu-vuc/${id}`, { method: 'DELETE' });
        const data = await res.json();
        showLoadingModal(false);
        if (data.success) {
            deleteModal.classList.remove('show');
            showAlert('success', data.message);
            await loadKhuVuc();
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
    function showLoadingModal(show) { loadingOverlayModal.style.display = show ? 'flex' : 'none'; }
    loadTinhOptions();
    loadKhuVuc();
}); 