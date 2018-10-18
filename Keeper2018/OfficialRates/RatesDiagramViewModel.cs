using Caliburn.Micro;
using OxyPlot;

namespace Keeper2018
{
    public class RatesDiagramViewModel : Screen
    {
        private PlotModel _myPlotModel;
        public PlotModel MyPlotModel
        {
            get { return _myPlotModel; }
            set
            {
                if (Equals(value, _myPlotModel)) return;
                _myPlotModel = value;
                NotifyOfPropertyChange(() => MyPlotModel);
            }
        }
    }
}
