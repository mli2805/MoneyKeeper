namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	public class HierarchyItem<T>
	{
		public int Level { get; set; }
		public T Item { get; set; }
		public HierarchyItem(int level, T item)
		{
			Level = level;
			Item = item;
		}
	}
}