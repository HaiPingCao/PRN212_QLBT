using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;

namespace QLBT.Server.Dal
{
    internal class LopDao
    {
        public static List<Lop> GetAll()
        {
            var db = new Prn212PQlbtContext();
            return db.Lops.ToList();
        }

        public static Lop? GetById(int id)
        {
            var db = new Prn212PQlbtContext();
            return db.Lops.Find(id);
        }

        public static void Add(Lop lop)
        {
            var db = new Prn212PQlbtContext();
            db.Lops.Add(lop);
            db.SaveChanges();
        }

        public static void Update(Lop lopNew)
        {
            var db = new Prn212PQlbtContext();
            var lop = db.Lops.Find(lopNew.Id);
            if (lop != null)
            {
                lop.Msgv = lopNew.Msgv;
                lop.TenLop = lopNew.TenLop;
                lop.NienKhoa = lopNew.NienKhoa;
                lop.ChuyenNganh = lopNew.ChuyenNganh;
                db.SaveChanges();
            }
        }

        public static void Delete(int id)
        {
            var db = new Prn212PQlbtContext();
            var lop = db.Lops.Find(id);
            if (lop != null)
            {
                db.Lops.Remove(lop);
                db.SaveChanges();
            }
        }
    }
}
