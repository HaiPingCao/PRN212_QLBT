using QLBT.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QLBT.Server.Services
{
    public class TeacherAssignmentService : ITeacherAssignmentService
    {
        private readonly Prn212PQlbtContext _db;

        /// <summary>
        /// Thư mục gốc lưu dữ liệu bài tập/bài nộp. File đề được lưu tại
        /// {RootFolder}/ProblemFiles/{assignmentId}/{tenFileGoc}
        /// </summary>
        public string RootFolder { get; set; } = "Submissions";

        public TeacherAssignmentService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<TeacherAssignmentItem> GetAll()
        {
            return _db.BaiTaps
                .OrderByDescending(bt => bt.NgayTao)
                .Select(bt => new TeacherAssignmentItem
                {
                    Id = bt.Id,
                    LopId = bt.LopId,
                    TenLop = bt.Lop.TenLop,
                    TieuDe = bt.TieuDe,
                    MoTa = bt.MoTa,
                    TenFileDe = bt.TenFileDe,
                    HanNop = bt.HanNop,
                    NgayTao = bt.NgayTao,
                    SoBaiNop = bt.BaiNops.Count
                })
                .ToList();
        }

        public List<LopOption> GetAllLops()
        {
            return _db.Lops
                .OrderBy(l => l.TenLop)
                .Select(l => new LopOption
                {
                    Id = l.Id,
                    TenLop = l.TenLop,
                    NienKhoa = l.KiHoc,
                    ChuyenNganh = l.ChuyenNganh
                })
                .ToList();
        }

        public TeacherAssignmentItem Add(CreateAssignmentInput input)
        {
            var lop = _db.Lops.Find(input.LopId)
                ?? throw new InvalidOperationException("Lớp không tồn tại");

            var baiTap = new BaiTap
            {
                LopId = input.LopId,
                TieuDe = input.TieuDe,
                MoTa = input.MoTa,
                HanNop = input.HanNop,
                NgayTao = DateTime.Now
            };

            _db.BaiTaps.Add(baiTap);
            _db.SaveChanges(); // cần Id trước khi copy file vào thư mục theo assignmentId

            if (!string.IsNullOrWhiteSpace(input.SourceFilePath))
            {
                var (tenFile, duongDan) = CopyProblemFile(baiTap.Id, input.SourceFilePath);
                baiTap.TenFileDe = tenFile;
                baiTap.DuongDanFileDe = duongDan;
                _db.SaveChanges();
            }

            return new TeacherAssignmentItem
            {
                Id = baiTap.Id,
                LopId = baiTap.LopId,
                TenLop = lop.TenLop,
                TieuDe = baiTap.TieuDe,
                MoTa = baiTap.MoTa,
                TenFileDe = baiTap.TenFileDe,
                HanNop = baiTap.HanNop,
                NgayTao = baiTap.NgayTao,
                SoBaiNop = 0
            };
        }

        public TeacherAssignmentItem? Update(UpdateAssignmentInput input)
        {
            var baiTap = _db.BaiTaps.FirstOrDefault(b => b.Id == input.Id);
            if (baiTap == null) return null;

            baiTap.TieuDe = input.TieuDe;
            baiTap.MoTa = input.MoTa;
            baiTap.HanNop = input.HanNop;
            // LopId không được thay đổi khi sửa.

            if (!string.IsNullOrWhiteSpace(input.SourceFilePath))
            {
                // Xóa file đề cũ trước khi lưu file mới.
                if (!string.IsNullOrEmpty(baiTap.DuongDanFileDe) && File.Exists(baiTap.DuongDanFileDe))
                    File.Delete(baiTap.DuongDanFileDe);

                var (tenFile, duongDan) = CopyProblemFile(baiTap.Id, input.SourceFilePath);
                baiTap.TenFileDe = tenFile;
                baiTap.DuongDanFileDe = duongDan;
            }

            _db.SaveChanges();

            var lop = _db.Lops.Find(baiTap.LopId);

            return new TeacherAssignmentItem
            {
                Id = baiTap.Id,
                LopId = baiTap.LopId,
                TenLop = lop?.TenLop ?? "",
                TieuDe = baiTap.TieuDe,
                MoTa = baiTap.MoTa,
                TenFileDe = baiTap.TenFileDe,
                HanNop = baiTap.HanNop,
                NgayTao = baiTap.NgayTao,
                SoBaiNop = _db.BaiNops.Count(bn => bn.BaiTapId == baiTap.Id)
            };
        }

        public bool Delete(int id)
        {
            var baiTap = _db.BaiTaps.FirstOrDefault(b => b.Id == id);
            if (baiTap == null) return false;

            if (!string.IsNullOrEmpty(baiTap.DuongDanFileDe) && File.Exists(baiTap.DuongDanFileDe))
                File.Delete(baiTap.DuongDanFileDe);

            _db.BaiTaps.Remove(baiTap);
            _db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Copy file đề từ máy giáo viên vào {RootFolder}/ProblemFiles/{assignmentId}/, giữ nguyên tên gốc.
        /// Trả về (tên file, đường dẫn đầy đủ trên server).
        /// </summary>
        private (string TenFile, string DuongDan) CopyProblemFile(int assignmentId, string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Không tìm thấy file đề", sourceFilePath);

            var folder = Path.Combine(RootFolder, "ProblemFiles", assignmentId.ToString());
            Directory.CreateDirectory(folder);

            var fileName = Path.GetFileName(sourceFilePath);
            var destPath = Path.Combine(folder, fileName);

            File.Copy(sourceFilePath, destPath, overwrite: true);

            return (fileName, destPath);
        }
    }
}
