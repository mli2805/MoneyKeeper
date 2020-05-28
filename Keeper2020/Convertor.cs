using System.Threading.Tasks;
using KeeperSqliteDb;

namespace Keeper2020
{
    public static class Convertor
    {
        public static async Task<int> Run()
        {

            using (KeeperContext db = new KeeperContext())
            {



                return await db.SaveChangesAsync();
            }
        }
    }
}
