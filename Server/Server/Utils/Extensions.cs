using Server.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this AppDbContext db)  // SaveChangesEx라는 확장 메소드를 만들어서 사용
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool SaveChangesEx(this SharedDbContext db)  // SaveChangesEx라는 확장 메소드를 만들어서 사용
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
