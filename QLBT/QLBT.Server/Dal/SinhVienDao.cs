using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;

namespace QLBT.Server.Dal
{
    internal class SinhVienDao
    {
        public static List<SinhVien> GetAll()
        {
            var db = new Prn212PQlbtContext();
            return db.SinhViens.ToList();
        }

        public static SinhVien? GetById(string mssv)
        {
            var db = new Prn212PQlbtContext();
            return db.SinhViens.Find(mssv);
        }

        public static void Add(SinhVien sinhVien)
        {
            var db = new Prn212PQlbtContext();
            db.SinhViens.Add(sinhVien);
            db.SaveChanges();
        }

        public static void Update(SinhVien sinhVienNew)
        {
            var db = new Prn212PQlbtContext();
            var sinhVien = db.SinhViens.Find(sinhVienNew.Mssv);
            if (sinhVien != null)
            {
                sinhVien.HoTen = sinhVienNew.HoTen;
                sinhVien.Email = sinhVienNew.Email;
                sinhVien.MatKhau = sinhVienNew.MatKhau;
                db.SaveChanges();
            }
        }

        public static void Delete(string mssv)
        {
            var db = new Prn212PQlbtContext();
            var sinhVien = db.SinhViens.Find(mssv);
            if (sinhVien != null)
            {
                db.SinhViens.Remove(sinhVien);
                db.SaveChanges();
            }
        }
    }
}
