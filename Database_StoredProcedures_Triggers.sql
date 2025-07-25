create database DangThuyTien;
use DangThuyTien;

CREATE TABLE KHACHHANG
(
  MaKH_ CHAR(4) NOT NULL,
  HoTen_ nvarchar(50) NOT NULL,
  NgaySinh_ DATE NOT NULL,
  GioiTinh_ nvarchar(3) NOT NULL,
  PRIMARY KEY (MaKH_)
);

CREATE TABLE SANPHAM_
(
  MaSP_ CHAR(4) NOT NULL,
  TenSP_ nvarchar(50) NOT NULL,
  DonGia_ INT NOT NULL,
  PRIMARY KEY (MaSP_)
);

CREATE TABLE HOADON_
(
  MaHD_ CHAR(4) NOT NULL,
  NgayLap_ DATE NOT NULL,
  MaKH_ CHAR(4) NOT NULL,
  TongTien_ INT, -- them cot TongTien
  PRIMARY KEY (MaHD_),
  FOREIGN KEY (MaKH_) REFERENCES KHACHHANG(MaKH_)
);

CREATE TABLE CTHD
(
  SoLuong_ INT NOT NULL,
  MaSP_ CHAR(4) NOT NULL,
  MaHD_ CHAR(4) NOT NULL,
  PRIMARY KEY (MaSP_, MaHD_),
  FOREIGN KEY (MaSP_) REFERENCES SANPHAM_(MaSP_),
  FOREIGN KEY (MaHD_) REFERENCES HOADON_(MaHD_)
);

-- them du lieu mau
INSERT INTO KHACHHANG VALUES
('KH01', N'Nguyễn Văn A', '1990-05-10', N'Nam'),
('KH02', N'Lê Thị B', '1995-07-20', N'Nữ'),
('KH03', N'Trần Văn C', '1988-02-14', N'Nam'),
('KH04', N'Phạm Thị D', '1992-11-30', N'Nữ');

INSERT INTO SANPHAM_ VALUES
('SP01', N'Chuột Logitech', 350000),
('SP02', N'Bàn phím Cơ', 800000),
('SP03', N'Màn hình LG 24"', 3000000),
('SP04', N'Tai nghe Sony', 1500000);

INSERT INTO HOADON_ VALUES
('HD01', '2025-07-01', 'KH01'),
('HD02', '2025-07-02', 'KH02'),
('HD03', '2025-07-03', 'KH03'),
('HD04', '2025-07-04', 'KH04');

INSERT INTO CTHD VALUES
('HD01', 'SP01', 2),
('HD02', 'SP02', 1),
('HD03', 'SP03', 1),
('HD04', 'SP04', 3);

-- 2.1 stored procedure cap nhat tong tien
CREATE PROCEDURE sp_CapNhatTongTien
    @MaHD CHAR(4) = NULL
AS
BEGIN
    IF @MaHD IS NOT NULL
    BEGIN
        UPDATE HOADON_
        SET TongTien_ = (
            SELECT SUM(c.SoLuong_ * s.DonGia_)
            FROM CTHD c, SANPHAM_ s
            WHERE c.MaSP_ = s.MaSP_ AND c.MaHD_ = @MaHD
        )
        WHERE MaHD_ = @MaHD;
    END
    ELSE
    BEGIN
        UPDATE HOADON_
        SET TongTien_ = (
            SELECT SUM(c.SoLuong_ * s.DonGia_)
            FROM CTHD c, SANPHAM_ s
            WHERE c.MaSP_ = s.MaSP_ AND c.MaHD_ = HOADON_.MaHD_
        );
    END
END;
GO

-- test thu tuc
EXEC sp_CapNhatTongTien;

-- xem ket qua
SELECT * FROM HOADON_;
SELECT * FROM CTHD;

