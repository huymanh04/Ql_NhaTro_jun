document.addEventListener('DOMContentLoaded', function () {
    loadBanners(); loadTinhThanh(); loadTheloaiPhongTro();
    function updateFullwidthClass() {
        const el = document.querySelector('.fullwidth-wrapper, .fullwidth-wrappee');
        if (!el) return;
        if (window.innerWidth <= 768) {
            el.classList.remove('fullwidth-wrapper');
            el.classList.add('fullwidth-wrappee');
        } else {
            el.classList.remove('fullwidth-wrappee');
            el.classList.add('fullwidth-wrapper');
        }
    }
    document.addEventListener('DOMContentLoaded', updateFullwidthClass);
    window.addEventListener('resize', updateFullwidthClass);
    const rangeInput = document.querySelectorAll(".range-input input");
    const sliderTrack = document.querySelector(".slider-track");
    const minValueText = document.getElementById("min-value-text");
    const maxValueText = document.getElementById("max-value-text");
    const priceRangeSelected = document.getElementById("price-range-selected");
    const minPriceTag = document.getElementById("min-price-tag");
    const maxPriceTag = document.getElementById("max-price-tag");
    const minPriceInput = document.getElementById("minPrice");
    const maxPriceInput = document.getElementById("maxPrice");
    const priceGap = 500; // Khoảng cách tối thiểu 500 nghìn
    function formatPrice(price) {
        return price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    }
    function updatePriceTag(input, priceTag) {
        const inputPercent = (input.value / input.max) * 100;
        const tagPosition = inputPercent + "%";
        priceTag.style.left = tagPosition;
        priceTag.textContent = formatPrice(input.value) + ".000đ";
    }
    function updateSlider() {
        let minVal = parseInt(rangeInput[0].value);
        let maxVal = parseInt(rangeInput[1].value);
        if (maxVal - minVal < priceGap) {
            if (this === rangeInput[0]) {
                rangeInput[0].value = maxVal - priceGap;
                minVal = maxVal - priceGap;
            } else {
                rangeInput[1].value = minVal + priceGap;
                maxVal = minVal + priceGap;
            }
        }
        minValueText.textContent = formatPrice(minVal) + ".000đ";
        maxValueText.textContent = formatPrice(maxVal) + ".000đ";
        priceRangeSelected.textContent = formatPrice(minVal) + ".000đ - " + formatPrice(maxVal) + ".000đ";
        minPriceInput.value = minVal * 1000;
        maxPriceInput.value = maxVal * 1000;
        const minPercent = (minVal / rangeInput[0].max) * 100;
        const maxPercent = (maxVal / rangeInput[1].max) * 100;
        sliderTrack.style.left = minPercent + "%";
        sliderTrack.style.width = (maxPercent - minPercent) + "%";
        updatePriceTag(rangeInput[0], minPriceTag);
        updatePriceTag(rangeInput[1], maxPriceTag);
    }
    rangeInput.forEach(input => {
        input.addEventListener("input", updateSlider);
        input.addEventListener("mousedown", function () {
            let priceTag = this === rangeInput[0] ? minPriceTag : maxPriceTag;
            priceTag.classList.add("visible");
        });
        document.addEventListener("mouseup", function () {
            minPriceTag.classList.remove("visible");
            maxPriceTag.classList.remove("visible");
        });
    });
    updateSlider();
});
async function loadTinhThanh() {
    try {
        const response = await fetch('/api/Location/Tinh-thanh');
        if (response.ok) {
            const result = await response.json();
            const tinhList = result.data;
            const select = document.getElementById('locationSelect');
            tinhList.forEach(tinh => {
                const option = document.createElement('option');
                option.value = tinh.maTinh;
                option.textContent = tinh.tenTinh;
                select.appendChild(option);
            });
        } else {
            alert('Không thể load danh sách tỉnh thành');
        }
    } catch (e) {
        console.error('Lỗi gọi API:', e);
    }
}
async function loadTheloaiPhongTro() {
    const slider = document.getElementById('typeroom');
    try {
        const response = await fetch('/api/Roomtype/get-type-room');
        if (!response.ok) throw new Error('Không thể load danh sách loại phòng');
        const dataa = await response.json();
        const dataaaaaa = dataa.data;
        dataaaaaa.forEach(item => {
            const div = document.createElement('div');
            div.className = 'col-md-3';
            div.innerHTML = `
                <div class="category-card">
                    <img src="data:image/png;base64,${item.imageBase64}" class="img-fluid" alt="${item.tenTheLoai}">
                    <div class="category-content">
                        <h3>${item.tenTheLoai}</h3>
                        <p>${item.moTa || ''}</p>
                      <a href="/Phongtro/Index?location=${encodeURIComponent(item.maTheLoai)}" class="btn btn-outline-primary">Xem ngay</a>
                    </div>
                </div>
            `;
            slider.appendChild(div);
        });
    } catch (error) {
        slider.innerHTML = `<p class="text-danger">${error.message}</p>`;
    }
}
async function loadBanners() {
    try {
        const res = await fetch('/api/Banner/get-banner', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include'
        });
        const bannersa = await res.json();
        const banners = bannersa.data;
        if (!banners || banners.length === 0) return;
        const container = document.getElementById('promo-banner-fullwidth');
        const slider = document.getElementById('promo-banner-slider');
        const controlsContainer = document.getElementById('promo-banner-controls');
        container.style.display = 'block';
        let activeBanners = banners.filter(b => b.isActive);
        const slideCount = activeBanners.length;
        if (slideCount === 0) return;
        activeBanners.forEach(item => {
            const div = document.createElement('div');
            div.className = 'promo-banner-slide';
            div.style.backgroundImage = `url('data:image/png;base64,${item.imageBase64}')`;
            div.innerHTML = `
                <div class="promo-banner-slide-overlay"></div>
                <div class="promo-banner-slide-content">
                    <h2>${item.title}</h2>
                    <p>${item.content}</p>
                    <a href="${item.redirectUrl}" class="promo-banner-cta-button">${item.text}</a>
                </div>
            `;
            slider.appendChild(div);
        });
        slider.style.width = (slideCount * 100) + '%';
        const slides = slider.querySelectorAll('.promo-banner-slide');
        slides.forEach(slide => {
            slide.style.width = (100 / slideCount) + '%';
        });
        let currentIndex = 0;
        let autoSlideInterval;
        for (let i = 0; i < slideCount; i++) {
            const btn = document.createElement('button');
            btn.classList.add('promo-banner-control-btn');
            if (i === 0) btn.classList.add('active');
            btn.setAttribute('data-index', i);
            controlsContainer.appendChild(btn);
            btn.addEventListener('click', () => {
                goToSlide(i);
                stopAutoSlide();
                startAutoSlide();
            });
        }
        const controlBtns = controlsContainer.querySelectorAll('.promo-banner-control-btn');
        function goToSlide(index) {
            if (index < 0) index = slideCount - 1;
            if (index >= slideCount) index = 0;
            const translatePercent = index * (100 / slideCount);
            slider.style.transform = `translateX(-${translatePercent}%)`;
            controlBtns.forEach(btn => btn.classList.remove('active'));
            controlBtns[index].classList.add('active');
            currentIndex = index;
        }
        function startAutoSlide() {
            autoSlideInterval = setInterval(() => {
                goToSlide(currentIndex + 1);
            }, 5000);
        }
        function stopAutoSlide() {
            clearInterval(autoSlideInterval);
        }
        document.getElementById('promo-banner-prev').addEventListener('click', () => {
            goToSlide(currentIndex - 1);
            stopAutoSlide();
            startAutoSlide();
        });
        document.getElementById('promo-banner-next').addEventListener('click', () => {
            goToSlide(currentIndex + 1);
            stopAutoSlide();
            startAutoSlide();
        });
        startAutoSlide();
    } catch (err) {
        console.error('Lỗi tải banner:', err);
    }
}
let featuredRooms = [];
let featuredImages = [];
document.addEventListener('DOMContentLoaded', function () {
    loadFeaturedRooms();
    document.querySelectorAll('.filter-buttons button').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.filter-buttons button').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            filterFeaturedRooms(this.getAttribute('data-filter'));
        });
    });
});
async function loadFeaturedRooms() {
    const listDiv = document.getElementById('featured-rooms-list');
    listDiv.innerHTML = '<div class="text-center py-5 w-100"><div class="spinner-border text-primary"></div></div>';
    try {
        const res = await fetch('/api/Room/get-all-room?page=1&pageSize=8');
        const json = await res.json();
        if (!json.success || !json.data) throw new Error('Không có dữ liệu phòng');
        let rooms = (json.data.Phong || json.data.phong || []).filter(r => r.ConTrong === true || r.conTrong === true);
        rooms = rooms.sort((a, b) => (b.MaPhong || b.maPhong) - (a.MaPhong || a.maPhong)).slice(0, 8);
        featuredRooms = rooms;
        featuredImages = json.data.HinhAnh || json.data.hinhAnh || [];
        renderFeaturedRooms(rooms);
    } catch (err) {
        listDiv.innerHTML = `<div class='text-danger text-center py-5 w-100'>${err.message}</div>`;
    }
}
function filterFeaturedRooms(type) {
    let rooms = [...featuredRooms];
    if (type === 'low') {
        rooms = rooms.sort((a, b) => (a.Gia || a.gia) - (b.Gia || b.gia));
    } else if (type === 'new') {
        rooms = rooms.sort((a, b) => (b.MaPhong || b.maPhong) - (a.MaPhong || a.maPhong));
    } else if (type === 'vip') {
        rooms = rooms.sort((a, b) => (b.Gia || b.gia) - (a.Gia || a.gia));
    }
    renderFeaturedRooms(rooms.slice(0, 8));
}
function renderFeaturedRooms(rooms) {
    const listDiv = document.getElementById('featured-rooms-list');
    const imgMap = {};
    featuredImages.forEach(img => {
        if ((img.IsMain || img.isMain) && !imgMap[img.MaPhong || img.maPhong]) {
            imgMap[img.MaPhong || img.maPhong] = img.DuongDanHinhBase64 || img.duongDanHinhBase64;
        }
    });
    listDiv.innerHTML = rooms.map(room => {
        const id = room.MaPhong || room.maPhong;
        const tenPhong = room.TenPhong || room.tenPhong;
        const gia = room.Gia || room.gia;
        const dienTich = room.DienTich || room.dienTich;
        const imgSrc = imgMap[id] ? `data:image/png;base64,${imgMap[id]}` : '/images/no-image.png';
        return `
            <div class="col-md-3 col-sm-6 mb-4">
                <div class="card h-100 shadow-sm room-card-seo">
                    <a href="/Phongtro/Detail/${id}" title="Xem chi tiết phòng ${tenPhong}">
                        <img src="${imgSrc}" class="card-img-top" alt="Phòng ${tenPhong}" loading="lazy" style="height:200px;object-fit:cover;">
                    </a>
                    <div class="card-body">
                        <h3 class="card-title fs-5 mb-2">
                            <a href="/Phongtro/Detail/${id}" class="text-dark text-decoration-none" title="${tenPhong}">
                                ${tenPhong}
                            </a>
                        </h3>
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <span class="badge bg-primary">${dienTich} m²</span>
                            <span class="fw-bold text-danger">${Number(gia).toLocaleString('vi-VN')}đ/tháng</span>
                        </div>
                        <a href="/Phongtro/Detail/${id}" class="btn btn-outline-primary w-100 mt-3" title="Xem chi tiết phòng">Xem chi tiết</a>
                    </div>
                </div>
            </div>
        `;
    }).join('');
} 