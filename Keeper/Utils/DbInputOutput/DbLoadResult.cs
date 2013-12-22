using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	public class DbLoadResult
	{

		public int Code { get; private set; }
		public string Explanation { get; private set; }
		public KeeperDb Db { get; private set; }

		public DbLoadResult(KeeperDb db)
		{
			Db = db;
			Explanation = "OK";
		}
		public DbLoadResult(int code, string explanation)
		{
			Code = code;
			Explanation = explanation;
		}

		public void Set(int code, string explanation)
		{
			Code = code;
			Explanation += explanation + "\n";
		}

	}
}