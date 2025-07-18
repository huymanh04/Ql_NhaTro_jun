// wwwroot/js/quanlyuser.js
// JS cho trang Quản lý tài khoản người dùng

// ========== CONFIG ========== //
const API_BASE = '/api/Admin';
const USERS_API = `${API_BASE}/Users`;
const CREATE_API = `${API_BASE}/CreateUser`;
const UPDATE_API = `${API_BASE}/UpdateUser`;
const DELETE_API = `${API_BASE}/DeleteUser`;
const ROLE_API = `${API_BASE}/ChangeRole`;

// ========== STATE ========== //
let users = [];
let currentPage = 1;
let itemsPerPage = 10;
let totalUsers = 0;
let searchQuery = '';
let roleFilter = '';
let editingUserId = null;
let changingRoleUserId = null;
let deletingUserId = null;

// ========== DOM ========== //
const $ = (s) => document.querySelector(s);
const $$ = (s) => document.querySelectorAll(s);

// ========== UTILS ========== //
function showLoading(show) {
    $('#loadingOverlay').style.display = show ? 'flex' : 'none';
}
function showAlert(msg, type = 'success') {
    const alert = document.createElement('div');
    alert.className = `alert alert-${type}`;
    alert.innerHTML = msg;
    $('#alertContainer').appendChild(alert);
    setTimeout(() => alert.remove(), 3000);
}
function resetForm() {
    $('#userForm').reset();
    $('#userId').value = '';
    $$('#userForm .error-message').forEach(e => e.textContent = '');
    $('#matKhau').value = '';
    $('#passwordHint').style.display = 'block';
    $('#passwordRequired').style.display = 'inline';
}
function closeModal(id) {
    $(id).classList.remove('show');
    $(id).style.display = 'none';
}
function openModal(id) {
    $$('.modal').forEach(m => { m.classList.remove('show'); m.style.display = 'none'; });
    $(id).classList.add('show');
    $(id).style.display = 'flex';
}

// ========== API CALLS ========== //
async function fetchUsers() {
    showLoading(true);
    try {
        const params = new URLSearchParams({
            page: currentPage,
            pageSize: itemsPerPage,
            search: searchQuery,
            role: roleFilter
        });
        const res = await fetch(`${USERS_API}?${params}`);
        if (!res.ok) throw new Error('Lỗi khi tải danh sách tài khoản');
        const data = await res.json();
        users = data.data || [];
        totalUsers = data.total || 0;
        renderUsers();
        renderPagination();
    } catch (e) {
        showAlert(e.message, 'danger');
    } finally {
        showLoading(false);
    }
}

async function createUser(user) {
    showLoading(true);
    try {
        const res = await fetch(CREATE_API, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(user)
        });
        if (!res.ok) throw new Error('Tạo tài khoản thất bại');
        showAlert('Tạo tài khoản thành công!');
        fetchUsers();
        closeModal('#userModal');
    } catch (e) {
        showAlert(e.message, 'danger');
    } finally {
        showLoading(false);
    }
}

async function updateUser(user) {
    showLoading(true);
    try {
        const res = await fetch(UPDATE_API, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(user)
        });
        if (!res.ok) throw new Error('Cập nhật tài khoản thất bại');
        showAlert('Cập nhật tài khoản thành công!');
        fetchUsers();
        closeModal('#userModal');
    } catch (e) {
        showAlert(e.message, 'danger');
    } finally {
        showLoading(false);
    }
}

async function deleteUser(id) {
    showLoading(true);
    try {
        const res = await fetch(`${DELETE_API}/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error('Xóa tài khoản thất bại');
        showAlert('Đã xóa tài khoản!');
        fetchUsers();
        closeModal('#deleteModal');
    } catch (e) {
        showAlert(e.message, 'danger');
    } finally {
        showLoading(false);
    }
}

async function changeRole(id, newRole) {
    showLoading(true);
    try {
        const res = await fetch(ROLE_API, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id, newRole })
        });
        if (!res.ok) throw new Error('Thay đổi vai trò thất bại');
        showAlert('Đã thay đổi vai trò!');
        fetchUsers();
        closeModal('#roleModal');
    } catch (e) {
        showAlert(e.message, 'danger');
    } finally {
        showLoading(false);
    }
}

// ========== RENDER ========== //
function renderUsers() {
    const tbody = $('#usersTableBody');
    tbody.innerHTML = '';
    if (!users.length) {
        tbody.innerHTML = '<tr><td colspan="7">Không có tài khoản nào</td></tr>';
        return;
    }
    users.forEach(u => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${u.id}</td>
            <td>${u.hoTen}</td>
            <td>${u.email}</td>
            <td>${u.soDienThoai}</td>
            <td>${roleName(u.vaiTro)}</td>
            <td>${u.trangThai ? 'Hoạt động' : 'Khóa'}</td>
            <td>
                <button class="btn btn-sm btn-edit" data-id="${u.id}"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-role" data-id="${u.id}" data-name="${u.hoTen}" data-role="${u.vaiTro}"><i class="fas fa-user-shield"></i></button>
                <button class="btn btn-sm btn-delete" data-id="${u.id}" data-name="${u.hoTen}"><i class="fas fa-trash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
    // TODO: render mobile cards if needed
}
function roleName(role) {
    switch (role) {
        case 2: return 'Admin';
        case 1: return 'Quản lý';
        default: return 'Khách hàng';
    }
}
function renderPagination() {
    const info = $('#paginationInfo');
    const start = (currentPage - 1) * itemsPerPage + 1;
    const end = Math.min(currentPage * itemsPerPage, totalUsers);
    info.textContent = `Hiển thị ${totalUsers ? start : 0} - ${end} của ${totalUsers} tài khoản`;
    // ... render page numbers, enable/disable prev/next ...
    $('#prevPageBtn').disabled = currentPage === 1;
    $('#nextPageBtn').disabled = currentPage * itemsPerPage >= totalUsers;
}

// ========== EVENT HANDLERS ========== //
document.addEventListener('DOMContentLoaded', () => {
    fetchUsers();
    // Add User
    $('#addUserBtn').onclick = () => {
        resetForm();
        $('#modalTitleText').textContent = 'Tạo tài khoản mới';
        $('#passwordHint').style.display = 'none';
        $('#passwordRequired').style.display = 'inline';
        openModal('#userModal');
        editingUserId = null;
    };
    // Save User
    $('#saveUserBtn').onclick = async () => {
        const user = {
            id: $('#userId').value,
            hoTen: $('#hoTen').value.trim(),
            email: $('#email').value.trim(),
            soDienThoai: $('#soDienThoai').value.trim(),
            matKhau: $('#matKhau').value,
            vaiTro: +$('#vaiTro').value
        };
        // Validate
        let valid = true;
        if (!user.hoTen) { $('#hoTenError').textContent = 'Vui lòng nhập họ tên'; valid = false; } else { $('#hoTenError').textContent = ''; }
        if (!user.email) { $('#emailError').textContent = 'Vui lòng nhập email'; valid = false; } else { $('#emailError').textContent = ''; }
        if (!user.soDienThoai) { $('#soDienThoaiError').textContent = 'Vui lòng nhập số điện thoại'; valid = false; } else { $('#soDienThoaiError').textContent = ''; }
        if (!user.vaiTro && user.vaiTro !== 0) { $('#vaiTroError').textContent = 'Vui lòng chọn vai trò'; valid = false; } else { $('#vaiTroError').textContent = ''; }
        if (!editingUserId && !user.matKhau) { $('#matKhauError').textContent = 'Vui lòng nhập mật khẩu'; valid = false; } else { $('#matKhauError').textContent = ''; }
        if (!valid) return;
        if (editingUserId) {
            user.id = editingUserId;
            await updateUser(user);
        } else {
            await createUser(user);
        }
    };
    // Cancel User Modal
    $('#cancelUserModalBtn').onclick = () => closeModal('#userModal');
    $('#closeUserModalBtn').onclick = () => closeModal('#userModal');
    // Edit User
    $('#usersTableBody').onclick = (e) => {
        const btn = e.target.closest('.btn-edit');
        if (btn) {
            const id = btn.dataset.id;
            const user = users.find(u => u.id == id);
            if (user) {
                resetForm();
                $('#modalTitleText').textContent = 'Chỉnh sửa tài khoản';
                $('#userId').value = user.id;
                $('#hoTen').value = user.hoTen;
                $('#email').value = user.email;
                $('#soDienThoai').value = user.soDienThoai;
                $('#vaiTro').value = user.vaiTro;
                $('#matKhau').value = '';
                $('#passwordHint').style.display = 'block';
                $('#passwordRequired').style.display = 'none';
                openModal('#userModal');
                editingUserId = user.id;
            }
        }
        // Change Role
        const btnRole = e.target.closest('.btn-role');
        if (btnRole) {
            const id = btnRole.dataset.id;
            const name = btnRole.dataset.name;
            const role = btnRole.dataset.role;
            $('#roleUserName').textContent = name;
            $('#newRole').value = role;
            openModal('#roleModal');
            changingRoleUserId = id;
        }
        // Delete
        const btnDelete = e.target.closest('.btn-delete');
        if (btnDelete) {
            const id = btnDelete.dataset.id;
            const name = btnDelete.dataset.name;
            $('#deleteUserName').textContent = name;
            openModal('#deleteModal');
            deletingUserId = id;
        }
    };
    // Role Modal
    $('#confirmRoleChangeBtn').onclick = async () => {
        const newRole = +$('#newRole').value;
        if (changingRoleUserId && !isNaN(newRole)) {
            await changeRole(changingRoleUserId, newRole);
            changingRoleUserId = null;
        }
    };
    $('#cancelRoleModalBtn').onclick = () => { closeModal('#roleModal'); changingRoleUserId = null; };
    $('#closeRoleModalBtn').onclick = () => { closeModal('#roleModal'); changingRoleUserId = null; };
    // Delete Modal
    $('#confirmDeleteBtn').onclick = async () => {
        if (deletingUserId) {
            await deleteUser(deletingUserId);
            deletingUserId = null;
        }
    };
    $('#cancelDeleteBtn').onclick = () => { closeModal('#deleteModal'); deletingUserId = null; };
    $('#closeDeleteModalBtn').onclick = () => { closeModal('#deleteModal'); deletingUserId = null; };
    // Search, Filter, Pagination
    $('#searchInput').oninput = (e) => { searchQuery = e.target.value; currentPage = 1; fetchUsers(); };
    $('#roleFilter').onchange = (e) => { roleFilter = e.target.value; currentPage = 1; fetchUsers(); };
    $('#clearFiltersBtn').onclick = () => {
        $('#searchInput').value = '';
        $('#roleFilter').value = '';
        searchQuery = '';
        roleFilter = '';
        currentPage = 1;
        fetchUsers();
    };
    $('#itemsPerPage').onchange = (e) => { itemsPerPage = +e.target.value; currentPage = 1; fetchUsers(); };
    $('#prevPageBtn').onclick = () => { if (currentPage > 1) { currentPage--; fetchUsers(); } };
    $('#nextPageBtn').onclick = () => { if (currentPage * itemsPerPage < totalUsers) { currentPage++; fetchUsers(); } };
    // TODO: render page numbers if needed
}); 