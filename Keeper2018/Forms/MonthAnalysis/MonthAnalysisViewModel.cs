using System;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class MonthAnalysisViewModel : Screen
    {
        private readonly MonthAnalyser _monthAnalyser;
        private MonthAnalysisModel _model;
        public MonthAnalysisModel Model
        {
            get => _model;
            set
            {
                if (Equals(value, _model)) return;
                _model = value;
                NotifyOfPropertyChange();
            }
        }

        public MonthAnalysisViewModel(MonthAnalyser monthAnalyser)
        {
            _monthAnalyser = monthAnalyser;
        }

        public void Initialize()
        {
            _monthAnalyser.Initialize();
            Model = _monthAnalyser.Produce(DateTime.Today.GetStartOfMonth());
        }

        public void ShowPreviousMonth()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(-1));
        }

        public void ShowNextMonth()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(1));
        }

        public void ShowPreviousQuarter()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(-3));
        }

        public void ShowNextQuarter()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(3));
        }
        public void ShowNextYear()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(12));
        }

        public void ShowPreviousYear()
        {
            Model = _monthAnalyser.Produce(Model.StartDate.AddMonths(-12));
        }

    }
}
