using Microsoft.EntityFrameworkCore;
using QLBT.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QLBT.Server.Services
{
    public class TeacherSubmissionService : ITeacherSubmissionService
    {
        private readonly Prn212PQlbtContext _db;

        public TeacherSubmissionService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<TeacherSubmissionItem> GetAll(int? lopId, int? baiTapId)
        {
            var query = _db.BaiNops
                .Include(b => b.BaiTap).ThenInclude(bt => bt.Lop)
                .AsQueryable();

            if (lopId.HasValue)
                query = query.Where(b => b.LopId == lopId.Value);

            if (baiTapId.HasValue)
                query = query.Where(b => b.BaiTapId == baiTapId.Value);

            var rows = query
                .OrderByDescending(b => b.NgayNop)
                .Select(b => new
                {
                    b.Id,
                    b.BaiTapId,
                    b.LopId,
                    b.Mssv,
                    b.TenFile,
                    b.SoLanNop,
                    b.NgayNop,
                    TieuDe = b.BaiTap.TieuDe,
                    TenLop = b.BaiTap.Lop.TenLop
                })
                .ToList();

            var mssvs = rows.Select(r => r.Mssv).Distinct().ToList();
            var hoTenByMssv = _db.SinhViens
                .Where(s => mssvs.Contains(s.Mssv))
                .ToDictionary(s => s.Mssv, s => s.HoTen);

            return rows.Select(r => new TeacherSubmissionItem
            {
                Id = r.Id,
                Mssv = r.Mssv,
                HoTenSinhVien = hoTenByMssv.TryGetValue(r.Mssv, out var hoTen) ? hoTen : r.Mssv,
                LopId = r.LopId,
                TenLop = r.TenLop,
                BaiTapId = r.BaiTapId,
                TieuDeBaiTap = r.TieuDe,
                TenFile = r.TenFile,
                SoLanNop = r.SoLanNop,
                NgayNop = r.NgayNop
            }).ToList();
        }

        public List<BaiTapOption> GetAllBaiTaps()
        {
            return _db.BaiTaps
                .OrderBy(bt => bt.TieuDe)
                .Select(bt => new BaiTapOption { Id = bt.Id, LopId = bt.LopId, TieuDe = bt.TieuDe })
                .ToList();
        }

        public bool Delete(int id)
        {
            var bn = _db.BaiNops.FirstOrDefault(b => b.Id == id);
            if (bn == null) return false;

            if (!string.IsNullOrEmpty(bn.DuongDanFile) && File.Exists(bn.DuongDanFile))
                File.Delete(bn.DuongDanFile);

            _db.BaiNops.Remove(bn);
            _db.SaveChanges();
            return true;
        }
    }
}
