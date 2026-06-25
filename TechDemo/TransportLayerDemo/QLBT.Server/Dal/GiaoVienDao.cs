using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;

namespace QLBT.Server.Dal
{
    internal class GiaoVienDao
    {
        public static List<GiaoVien> GetAll()
        {
            var db = new Prn212PQlbtContext();
            return db.GiaoViens.ToList();
        }

        public static GiaoVien? GetById(string msgv)
        {
            var db = new Prn212PQlbtContext();
            return db.GiaoViens.Find(msgv);
        }

        public static void Add(GiaoVien giaoVien)
        {
            var db = new Prn212PQlbtContext();
            db.GiaoViens.Add(giaoVien);
            db.SaveChanges();
        }

        public static void Update(GiaoVien gvNew)
        {
            var db = new Prn212PQlbtContext();
            var g = db.GiaoViens.Find(gvNew.Msgv);
            if (g != null)
            {
                g.HoTen = gvNew.HoTen;
                g.Email = gvNew.Email;
                g.MatKhau = gvNew.MatKhau;
                db.SaveChanges();
            }
        }

        public static void Delete(string msgv)
        {
            var db = new Prn212PQlbtContext();
            var giaoVien = db.GiaoViens.Find(msgv);
            if (giaoVien != null)
            {
                db.GiaoViens.Remove(giaoVien);
                db.SaveChanges();
            }
        }
    }
}
