-- =============================================
-- SAMPLE DATA INSERTION
-- =============================================
USE PRN212_P_QLBT;
GO

-- 1. Insert 1 Teacher (Giáo viên)
INSERT INTO giao_vien (msgv, ho_ten, email, mat_khau)
VALUES 
    ('GV001', N'Trần Tuấn Anh', 'anhtt@university.edu.vn', '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6');
GO

-- 2. Insert 2 Classes (Lớp)
-- Note: id is IDENTITY(1,1), so they will automatically be assigned id 1 and 2.
INSERT INTO lop (msgv, ten_lop, nien_khoa, chuyen_nganh)
VALUES 
    ('GV001', N'SE1701', 'Su26', N'Kỹ thuật Phần mềm'),
    ('GV001', N'IS1702', 'Su26', N'Hệ thống Thông tin');
GO

-- 3. Insert 20 Students (Sinh viên)
INSERT INTO sinh_vien (mssv, ho_ten, email, mat_khau)
VALUES 
    ('SV001', N'Nguyễn Văn Một',     'motnv@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV002', N'Trần Thị Hai',       'haitt@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV003', N'Lê Văn Ba',          'balv@student.edu.vn',   '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV004', N'Phạm Thị Bốn',       'bonpt@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV005', N'Hoàng Văn Năm',      'namhv@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV006', N'Đinh Thị Sáu',       'saudt@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV007', N'Vũ Văn Bảy',         'bayvv@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV008', N'Đặng Thị Tám',       'tamdt@student.edu.vn',  '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV009', N'Bùi Văn Chín',       'chinbv@student.edu.vn', '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV010', N'Đỗ Thị Mười',        'muoidt@student.edu.vn', '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV011', N'Hồ Văn Mười Một',    'mot11hv@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV012', N'Ngô Thị Mười Hai',   'hai12nt@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV013', N'Dương Văn Mười Ba',  'ba13dv@student.edu.vn', '$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV014', N'Lý Thị Mười Bốn',    'bon14lt@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV015', N'Phan Văn Mười Lăm',  'lam15pv@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV016', N'Trịnh Thị Mười Sáu', 'sau16tt@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV017', N'Mai Văn Mười Bảy',   'bay17mv@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV018', N'Đào Thị Mười Tám',   'tam18dt@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV019', N'Đoàn Văn Mười Chín', 'chin19dv@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6'),
    ('SV020', N'Lâm Thị Hai Mươi',   'muoi20lt@student.edu.vn','$2a$11$.ijxR6jO0gP8mwLHrxOv6ObLJQlj8CcEFnyEwSXCKDRicn1xogfu6');
GO

-- 4. Assign Students to Classes equally (10 students per class)
-- Assuming the Lớp table generated ID 1 for SE1701 and ID 2 for IS1702
INSERT INTO sinh_vien_lop (lop_id, mssv)
VALUES 
    -- Class 1: First 10 students
    (1, 'SV001'), (1, 'SV002'), (1, 'SV003'), (1, 'SV004'), (1, 'SV005'),
    (1, 'SV006'), (1, 'SV007'), (1, 'SV008'), (1, 'SV009'), (1, 'SV010'),
    
    -- Class 2: Next 10 students
    (2, 'SV011'), (2, 'SV012'), (2, 'SV013'), (2, 'SV014'), (2, 'SV015'),
    (2, 'SV016'), (2, 'SV017'), (2, 'SV018'), (2, 'SV019'), (2, 'SV020');
GO

-- 5. Insert Assignments (Bài tập) - no problem files
-- Class 1 (SE1701): 3 assignments
-- Class 2 (IS1702): 3 assignments

INSERT INTO bai_tap (lop_id, tieu_de, mo_ta, ten_file_de, duong_dan_file_de, han_nop, ngay_tao)
VALUES
    -- SE1701
    (1, N'Lab 01 - TCP Socket Programming',
        N'Implement a simple TCP echo server and client in C#. The server must handle multiple concurrent clients using ThreadPool. Submit a .zip containing source code and a brief report.',
        NULL, NULL,
        '2026-07-10 23:59:00', '2026-06-20 08:00:00'),

    (1, N'Lab 02 - Multithreading & Synchronization',
        N'Build a producer-consumer pipeline using BlockingCollection<T>. Demonstrate correct use of CancellationToken for graceful shutdown. Submit source code only.',
        NULL, NULL,
        '2026-07-24 23:59:00', '2026-06-27 08:00:00'),

    (1, N'Lab 03 - Entity Framework Core',
        N'Design and implement a code-first EF Core data layer for a library management domain. Include at least 3 entities with proper relationships, migrations, and seed data.',
        NULL, NULL,
        '2026-08-07 23:59:00', '2026-07-04 08:00:00'),

    -- IS1702
    (2, N'Assignment 01 - Database Normalization',
        N'Given the attached denormalized dataset, normalize it to 3NF. Provide the ERD, SQL DDL script, and a written justification for each normalization step.',
        NULL, NULL,
        '2026-07-12 23:59:00', '2026-06-21 08:00:00'),

    (2, N'Assignment 02 - REST API Design',
        N'Design a RESTful API for an inventory management system. Deliver an OpenAPI 3.0 specification (YAML) and a Postman collection demonstrating all endpoints.',
        NULL, NULL,
        '2026-07-26 23:59:00', '2026-06-28 08:00:00'),

    (2, N'Assignment 03 - System Analysis Report',
        N'Conduct a requirements analysis for a cafeteria ordering system. Deliverables: use-case diagram, swimlane process diagram, and a 5-page SRS document.',
        NULL, NULL,
        '2026-08-09 23:59:00', '2026-07-05 08:00:00');
GO