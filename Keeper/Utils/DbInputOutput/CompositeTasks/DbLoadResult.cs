using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.DbInputOutput.CompositeTasks
{
    public class DbLoadResult
    {

        public int Code { get; private set; }
        public string Explanation { get; private set; }
        public KeeperDb Db { get; private set; }

        public DbLoadResult(KeeperDb db)
        {
            Db = db;
            Code = 0;
            Explanation = "OK";
        }
        public DbLoadResult(int code, string explanation)
        {
            Code = code;
            Explanation = explanation;
        }

    }
}