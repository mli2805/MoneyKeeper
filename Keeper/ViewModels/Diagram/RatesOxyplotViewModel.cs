using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using Keeper.Utils.DiagramDomainModel;
using Keeper.Utils.OxyPlots;
using OxyPlot;

namespace Keeper.ViewModels.Diagram
{
    class RatesOxyplotViewModel : Screen
    {
        private readonly DiagramData _diagramData;

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
        public RatesDiagramContentModel ContentModel { get; set; }

        public List<string> HintsSource { get; set; }
        public RatesOxyplotViewModel(DiagramData diagramData)
        {
            _diagramData = diagramData;
            ContentModel = new RatesDiagramContentModel()
            {
                IsCheckedUsdNbRb = true,
                IsCheckedMyUsd = false,
                IsCheckedEurNbRb = true,
                IsCheckedEurUsdNbRb = false,
                IsCheckedRurNbRb = true,
                IsCheckedRurUsd = false,
                IsCheckedBusketNbRb = true,
                IsCheckedLogarithm = false,
                IsCheckedUnify = false
            };
            ContentModel.PropertyChanged += ContentModel_PropertyChanged;

            InitializeDiagram();
        }

        void ContentModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InitializeDiagram();
        }

        private void InitializeDiagram()
        {
            HintsSource = new List<string>(){"Mouse wheel - Zoom", "Right button - Move", "Ctrl + Right button - Select rect"};
            var ratesOxyplotBuilder = new RatesOxyplotBuilder(_diagramData);

            var temp = ratesOxyplotBuilder.Do(ContentModel);
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _diagramData.Caption;
        }
    }
}
