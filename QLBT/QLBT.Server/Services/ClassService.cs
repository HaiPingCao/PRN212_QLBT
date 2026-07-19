using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    public class ClassService : IClassService
    {
        private readonly Prn212PQlbtContext _db;

        public ClassService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<ClassItem> GetAll()
        {
            return _db.Lops
                .Include(x => x.SinhVienLops)
                .OrderBy(x => x.TenLop)
                .Select(x => new ClassItem
                {
                    Id = x.Id,
                    TenLop = x.TenLop,
                    KiHoc = x.KiHoc,
                    ChuyenNganh = x.ChuyenNganh,
                    SoSinhVien = x.SinhVienLops.Count(s => s.DangThamGia)
                })
                .ToList();
        }

        public ClassItem Add(CreateClassInput input)
        {
            var lop = new Lop
            {
                TenLop = input.TenLop,
                KiHoc = input.KiHoc,
                ChuyenNganh = input.ChuyenNganh
            };

            _db.Lops.Add(lop);
            _db.SaveChanges();

            return new ClassItem
            {
                Id = lop.Id,
                TenLop = lop.TenLop,
                KiHoc = lop.KiHoc,
                ChuyenNganh = lop.ChuyenNganh,
                SoSinhVien = 0
            };
        }

        public ClassItem? Update(UpdateClassInput input)
        {
            var lop = _db.Lops.FirstOrDefault(x => x.Id == input.Id);

            if (lop == null)
                return null;

            lop.TenLop = input.TenLop;
            lop.KiHoc = input.KiHoc;
            lop.ChuyenNganh = input.ChuyenNganh;

            _db.SaveChanges();

            return GetAll().First(x => x.Id == input.Id);
        }

        public bool Delete(int id)
        {
            var lop = _db.Lops
                .Include(x => x.SinhVienLops)
                .FirstOrDefault(x => x.Id == id);

            if (lop == null)
                return false;

            if (lop.SinhVienLops.Any())
                throw new Exception("Lớp vẫn còn sinh viên.");

            _db.Lops.Remove(lop);

            _db.SaveChanges();

            return true;
        }
    }
}