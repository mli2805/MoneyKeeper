using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interactivity;

namespace Keeper.XamlExtensions
{
	public class CollapseAction : TriggerAction<Button>
	{
		public Dock Direction { get; set; }
		protected override void Invoke(object parameter)
		{
			// First find the nearest splitter
			var splitter = FindVisual<GridSplitter>(AssociatedObject);

			if (splitter != null)
			{
				var grid = FindVisual<Grid>(splitter); // Find nearest Grid
				if (grid != null)
				{
					ApplyDock(grid);
				}
			}
		}

		private void ApplyDock(Grid grid)
		{
			var rDef1 = grid.RowDefinitions.FirstOrDefault();
			var rDef2 = grid.RowDefinitions.LastOrDefault();
			switch (Direction)
			{
        case Dock.Left:
          rDef1.Height = new GridLength(1, GridUnitType.Star);
          rDef2.Height = new GridLength(2, GridUnitType.Star);
          break;
        case Dock.Top:
					rDef1.Height = new GridLength(0);
					rDef2.Height = new GridLength(1, GridUnitType.Star);
					break;
				case Dock.Bottom:
					rDef2.Height = new GridLength(0);
					rDef1.Height = new GridLength(1, GridUnitType.Star);
					break;
			}
		}

		private T FindVisual<T>(FrameworkElement relElt) where T : FrameworkElement
		{
			var parent = VisualTreeHelper.GetParent(relElt);

			while (parent != null && !(parent is T))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			return parent as T;
		}
	}
}