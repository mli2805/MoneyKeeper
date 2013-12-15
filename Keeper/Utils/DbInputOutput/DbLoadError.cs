namespace Keeper.DbInputOutput
{
	public class DbLoadError
	{
		public int Code { get; set; }
		public string Explanation { get; set; }

		public void Add(int code, string explanation)
		{
			Code = code;
			Explanation += explanation + "\n";
		}

	}
}