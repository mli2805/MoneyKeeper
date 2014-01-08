using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel();
		}
	}
	public class MainWindowViewModel
	{
		public MainWindowViewModel()
		{
			var temp = new PlotModel("Square wave");
			var ls = new LineSeries("sin(x)+sin(3x)/3+sin(5x)/5+...");
			int n = 30;
			for (double x = -10; x < 10; x += 0.0001)
			{
				double y = 0;
				for (int i = 0; i < n; i++)
				{
					int j = i * 2 + 1;
					y += Math.Sin(j * x) / j;
				}
				ls.Points.Add(new DataPoint(x, y));
			}
			temp.Series.Add(ls);
			temp.Axes.Add(new LinearAxis(AxisPosition.Left, -4, 4));
			temp.Axes.Add(new LinearAxis(AxisPosition.Bottom));
			MyPlotModel = temp;         // this is raising the INotifyPropertyChanged event			
		}

		public PlotModel MyPlotModel { get; set; }
	}
}
