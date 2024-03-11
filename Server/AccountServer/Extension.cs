using AccountServer.DB;

namespace AccountServer
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
    }
}
