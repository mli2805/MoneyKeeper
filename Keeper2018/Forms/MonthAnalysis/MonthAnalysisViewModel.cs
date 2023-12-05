using System;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class MonthAnalysisViewModel : Screen
    {
        private readonly MonthAnalyzer _monthAnalyzer;
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

        public MonthAnalysisViewModel(MonthAnalyzer monthAnalyzer)
        {
            _monthAnalyzer = monthAnalyzer;
        }

        public void Initialize()
        {
            Model = _monthAnalyzer.AnalyzeFrom(DateTime.Today.GetStartOfMonth());
        }

        public void ShowPreviousMonth()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(-1));
        }

        public void ShowNextMonth()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(1));
        }

        public void ShowPreviousQuarter()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(-3));
        }

        public void ShowNextQuarter()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(3));
        }
        public void ShowNextYear()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(12));
        }

        public void ShowPreviousYear()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddMonths(-12));
        }

    }
}
