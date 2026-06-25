using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;

namespace QLBT.Server.Dal
{
    internal class BaiNopDao
    {
        public static List<BaiNop> GetAll()
        {
            var db = new Prn212PQlbtContext();
            return db.BaiNops.ToList();
        }

        public static BaiNop? GetById(int id)
        {
            var db = new Prn212PQlbtContext();
            return db.BaiNops.Find(id);
        }

        public static void Add(BaiNop baiNop)
        {
            var db = new Prn212PQlbtContext();
            db.BaiNops.Add(baiNop);
            db.SaveChanges();
        }

        public static void Update(BaiNop baiNopNew)
        {
            var db = new Prn212PQlbtContext();
            var baiNop = db.BaiNops.Find(baiNopNew.Id);
            if (baiNop != null)
            {
                baiNop.BaiTapId = baiNopNew.BaiTapId;
                baiNop.LopId = baiNopNew.LopId;
                baiNop.Mssv = baiNopNew.Mssv;
                baiNop.TenFile = baiNopNew.TenFile;
                baiNop.DuongDanFile = baiNopNew.DuongDanFile;
                baiNop.SoLanNop = baiNopNew.SoLanNop;
                baiNop.NgayNop = baiNopNew.NgayNop;
                db.SaveChanges();
            }
        }

        public static void Delete(int id)
        {
            var db = new Prn212PQlbtContext();
            var baiNop = db.BaiNops.Find(id);
            if (baiNop != null)
            {
                db.BaiNops.Remove(baiNop);
                db.SaveChanges();
            }
        }
    }
}
