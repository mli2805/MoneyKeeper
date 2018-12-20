using System.Collections.Generic;
using Caliburn.Micro;
using OxyPlot.Axes;

namespace Keeper2018
{
    public class LongTermChartViewModel : Screen
    {
        private string _caption;

        public LongTermChartModel Model { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initalize(string caption, List<CurrencyRatesModel> rates)
        {
            _caption = caption;
            Model = new LongTermChartModel();
            Model.Build(rates);
        }


        public void ToggleLogarithm()
        {
            var currentIsLogarithmic = Model.LongTermModel.Axes[1].GetType() == typeof(LogarithmicAxis);
            if (currentIsLogarithmic)
                Model.LongTermModel.Axes[1] = new LinearAxis { Position = AxisPosition.Left };
            else
                Model.LongTermModel.Axes[1] = new LogarithmicAxis { Position = AxisPosition.Left };
            Model.LongTermModel.InvalidatePlot(true);
        }

    }
}
