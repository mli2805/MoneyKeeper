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

        public void ToTheBeginning()
        {
            Model = _monthAnalyzer.AnalyzeFrom(new DateTime(2002,1,1), Model.IsYearAnalysisMode);
        }

        public void ToCurrentMoment()
        {
            var newStart = Model.IsYearAnalysisMode ? DateTime.Today.GetStartOfMonth() : DateTime.Today.GetStartOfYear();
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }
       
        public void LeftArrow()
        {
            if (Model.StartDate.Date == new DateTime(2002,1,1)) return;
            var newStart = Model.IsYearAnalysisMode ? Model.StartDate.AddYears(-1) : Model.StartDate.AddMonths(-1);
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }

        public void RightArrow()
        {
            var newStart = Model.IsYearAnalysisMode ? Model.StartDate.AddYears(1) : Model.StartDate.AddMonths(1);
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }

        public void ShiftLeftArrow()
        {
            if (Model.StartDate.Date == new DateTime(2002,1,1)) return;
            var newStart = Model.IsYearAnalysisMode ? Model.StartDate.AddYears(-1) : Model.StartDate.AddMonths(-3);
            if (newStart < new DateTime(2002, 1, 1))
                newStart = new DateTime(2002, 1, 1);
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }

        public void ShiftRightArrow()
        {
            var newStart = Model.IsYearAnalysisMode ? Model.StartDate.AddYears(1) : Model.StartDate.AddMonths(3);
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }

        // в режиме годы стрелки с любыми модификаторами двигают на 1 год
        // в режиме месяцы на 1 - 3 - 12 месяцев

        public void ControlRightArrow()
        {
            Model = _monthAnalyzer.AnalyzeFrom(Model.StartDate.AddYears(1), Model.IsYearAnalysisMode);
        }

        public void ControlLeftArrow()
        {
            if (Model.StartDate.Date == new DateTime(2002,1,1)) return;
            var newStart = (Model.StartDate.Year == 2002) ? new DateTime(2002, 1, 1) : Model.StartDate.AddYears(-1);
            Model = _monthAnalyzer.AnalyzeFrom(newStart, Model.IsYearAnalysisMode);
        }

        public void ToggleYearMonth()
        {
            var newMode = !Model.IsYearAnalysisMode;
            var newStart = newMode ? new DateTime(Model.StartDate.Year, 1, 1) : Model.StartDate;
            Model = _monthAnalyzer.AnalyzeFrom(newStart, newMode);
        }

    }
}
