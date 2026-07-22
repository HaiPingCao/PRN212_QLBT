-- =============================================
-- DATABASE: Quản lý và giao nộp bài tập
-- =============================================

-- 1. Switch to master to ensure we aren't using the database we want to drop
USE master;
GO

-- 2. Safely check for existence, kill active connections, and drop
IF DB_ID('PRN212_P_QLBT') IS NOT NULL
BEGIN
    ALTER DATABASE PRN212_P_QLBT SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PRN212_P_QLBT;
END
GO

-- 3. Create the fresh database
CREATE DATABASE PRN212_P_QLBT;
GO

-- 4. Switch context to the new database for table creation
USE PRN212_P_QLBT;
GO

-- =============================================
-- TABLES
-- =============================================

CREATE TABLE giao_vien (
    msgv        VARCHAR(20)   PRIMARY KEY,
    ho_ten      NVARCHAR(100) NOT NULL,
    email       VARCHAR(100)  NOT NULL UNIQUE,
    mat_khau    VARCHAR(255)  NOT NULL
);

CREATE TABLE hoc_ki (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_hoc_ki      NVARCHAR(20) NOT NULL UNIQUE,
    ngay_bat_dau    DATE NULL,
    ngay_ket_thuc   DATE NULL
);

CREATE TABLE lop (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    msgv             VARCHAR(20)   NOT NULL,
    ten_lop          NVARCHAR(50)  NOT NULL,
    hoc_ki_id        INT           NOT NULL,
    chuyen_nganh     NVARCHAR(100) NOT NULL,

    CONSTRAINT fk_lop_giao_vien
        FOREIGN KEY (msgv)
        REFERENCES giao_vien(msgv),

    CONSTRAINT fk_lop_hoc_ki
        FOREIGN KEY (hoc_ki_id)
        REFERENCES hoc_ki(id),

    CONSTRAINT uq_lop
        UNIQUE (
            ten_lop,
            hoc_ki_id,
            chuyen_nganh
        )
);

CREATE TABLE sinh_vien (
    mssv        VARCHAR(20)   PRIMARY KEY,
    ho_ten      NVARCHAR(100) NOT NULL,
    email       VARCHAR(100)  NOT NULL UNIQUE,
    mat_khau    VARCHAR(255)  NOT NULL
);

CREATE TABLE sinh_vien_lop (
    lop_id              INT         NOT NULL,
    mssv                VARCHAR(20) NOT NULL,
    dang_tham_gia       BIT         NOT NULL DEFAULT 1,
    ngay_tham_gia       DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ngay_roi_lop        DATETIME    NULL,

    PRIMARY KEY (lop_id, mssv),

    CONSTRAINT fk_svl_lop
        FOREIGN KEY (lop_id)
        REFERENCES lop(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_svl_sinh_vien
        FOREIGN KEY (mssv)
        REFERENCES sinh_vien(mssv)
        ON DELETE NO ACTION
);

CREATE TABLE bai_tap (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    lop_id              INT           NOT NULL,
    tieu_de             NVARCHAR(255) NOT NULL,
    mo_ta               NVARCHAR(MAX),
    ten_file_de         NVARCHAR(255),
    duong_dan_file_de   NVARCHAR(500),
    han_nop             DATETIME      NOT NULL,
    ngay_tao            DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_bai_tap_lop
        UNIQUE (id, lop_id),

    CONSTRAINT fk_bai_tap_lop
        FOREIGN KEY (lop_id)
        REFERENCES lop(id)
        ON DELETE CASCADE
);

CREATE TABLE bai_nop (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    bai_tap_id          INT           NOT NULL,
    lop_id              INT           NOT NULL,
    mssv                VARCHAR(20)   NOT NULL,
    ten_file            NVARCHAR(255) NOT NULL,
    duong_dan_file      NVARCHAR(500) NOT NULL,
    so_lan_nop          INT           NOT NULL DEFAULT 1,
    ngay_nop            DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    diem                DECIMAL(4,2)  NULL,

    CONSTRAINT uq_bai_nop
        UNIQUE (bai_tap_id, mssv),

    CONSTRAINT chk_so_lan_nop
        CHECK (so_lan_nop >= 1),

    CONSTRAINT chk_diem
        CHECK (diem IS NULL OR (diem >= 0 AND diem <= 10)),

    CONSTRAINT fk_bai_nop_bai_tap
        FOREIGN KEY (bai_tap_id, lop_id)
        REFERENCES bai_tap(id, lop_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_bai_nop_sinh_vien_lop
        FOREIGN KEY (lop_id, mssv)
        REFERENCES sinh_vien_lop(lop_id, mssv)
        ON DELETE NO ACTION
);
