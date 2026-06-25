using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;

namespace QLBT.Server.Dal
{
    internal class BaiTapDao
    {
        public static List<BaiTap> GetAll()
        {
            var db = new Prn212PQlbtContext();
            return db.BaiTaps.ToList();
        }

        public static BaiTap? GetById(int id)
        {
            var db = new Prn212PQlbtContext();
            return db.BaiTaps.Find(id);
        }

        public static void Add(BaiTap baiTap)
        {
            var db = new Prn212PQlbtContext();
            db.BaiTaps.Add(baiTap);
            db.SaveChanges();
        }

        public static void Update(BaiTap baiTapNew)
        {
            var db = new Prn212PQlbtContext();
            var baiTap = db.BaiTaps.Find(baiTapNew.Id);
            if (baiTap != null)
            {
                baiTap.LopId = baiTapNew.LopId;
                baiTap.TieuDe = baiTapNew.TieuDe;
                baiTap.MoTa = baiTapNew.MoTa;
                baiTap.TenFileDe = baiTapNew.TenFileDe;
                baiTap.DuongDanFileDe = baiTapNew.DuongDanFileDe;
                baiTap.HanNop = baiTapNew.HanNop;
                baiTap.NgayTao = baiTapNew.NgayTao;
                db.SaveChanges();
            }
        }

        public static void Delete(int id)
        {
            var db = new Prn212PQlbtContext();
            var baiTap = db.BaiTaps.Find(id);
            if (baiTap != null)
            {
                db.BaiTaps.Remove(baiTap);
                db.SaveChanges();
            }
        }
    }
}
