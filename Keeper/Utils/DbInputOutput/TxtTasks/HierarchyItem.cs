namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	public class HierarchyItem<T>
	{
		public int Depth { get; set; }
		public T Item { get; set; }
		public HierarchyItem(int depth, T item)
		{
			Depth = depth;
			Item = item;
		}
	}
}