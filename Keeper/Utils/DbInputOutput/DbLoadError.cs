namespace Keeper.Utils.DbInputOutput
{
	public class DbLoadError
	{
		public int Code { get; set; }
		public string Explanation { get; set; }

		public void Set(int code, string explanation)
		{
			Code = code;
			Explanation += explanation + "\n";
		}

	}
}