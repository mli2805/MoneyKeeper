using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Keeper2018.Properties;

namespace Keeper2018
{
    public sealed partial class PeriodChoiceControl : INotifyPropertyChanged
    {
        #region Control's outer properties for choice result
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(Tuple<DateTime, DateTime>),
                typeof(PeriodChoiceControl));

        public Tuple<DateTime, DateTime> Interval
        {
            get => (Tuple<DateTime, DateTime>)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        public static readonly DependencyProperty IntervalModeProperty =
            DependencyProperty.Register("IntervalMode", typeof(DiagramIntervalMode),
                typeof(PeriodChoiceControl));

        public DiagramIntervalMode IntervalMode
        {
            get => (DiagramIntervalMode)GetValue(IntervalModeProperty);
            set => SetValue(IntervalModeProperty, value);
        }
        #endregion

        public PeriodChoiceControlModel Model { get; set; }
        private PointsPeriodConvertor _pointsPeriodConvertor;

        private double _fromPoint;

        private double _toPoint;

        public PeriodChoiceControl()
        {
            Model = new PeriodChoiceControlModel();
            InitializeComponent();

            ControlGrid.DataContext = this;
            _pointsPeriodConvertor = new PointsPeriodConvertor(
                new Tuple<DateTime, DateTime>(new DateTime(2002, 1, 1), DateTime.Today), DiagramIntervalMode.Months);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var width = ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth;
            var points = _pointsPeriodConvertor.PeriodToPoints(Interval);
            Model.SetPositions(width * points.Item1/100 - 4, width * ((points.Item2 - points.Item1)/100) + PeriodChoiceControlModel.MinCenterPartWidth);
            PeriodTitle = GetPeriodTitle(Interval, IntervalMode);
            Model.ResetFlags();
        }

        private void RefreshDependencyProperties()
        {
            double k = 100 / (ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth);
            _fromPoint = (Model.BtnFromMargin.Left + 3) * k;
            _toPoint = (Model.BtnToMargin.Left - PeriodChoiceControlModel.MinCenterPartWidth + 4) * k;

            Interval = _pointsPeriodConvertor.PointsToPeriod(_fromPoint, _toPoint);
            PeriodTitle = GetPeriodTitle(Interval, IntervalMode);
        }

        #region btnFrom reactions
        private void BtnFromPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.ReactBtnFromPreviewMouseDown(e.GetPosition(this).X);
        }
        private void BtnFromPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!Model.BtnFromIsHolded) return;
            Model.ReactBtnFromPreviewMouseMove(e.GetPosition(this).X);
            RefreshDependencyProperties();
        }
        private void BtnFromPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Model.BtnFromIsHolded = false;
        }
        #endregion 

        #region btnTo reactions
        private void BtnToPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.ReactBtnToPreviewMouseDown(e.GetPosition(this).X);
        }
        private void BtnToPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!Model.BtnToIsHolded) return;

            Model.ReactBtnToPreviewMouseMove(e.GetPosition(this).X, RightPart.ActualWidth);
            RefreshDependencyProperties();
        }
        private void BtnToPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Model.BtnToIsHolded = false;
        }
        #endregion

        #region CentralPart reactions
        private void CentralPartPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!Model.CentralPartIsHolded) return;
            Model.ReactCentralPartPreviewMouseMove(e.GetPosition(this).X, RightPart.ActualWidth);
            RefreshDependencyProperties();
        }
        private void CenterPartPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Model.ReactCentralPartDoubleClick(e.GetPosition(this).X, ControlGrid.ActualWidth);
                RefreshDependencyProperties();
            }
            else
                Model.ReactCentralPartPreviewMouseDown(e.GetPosition(this).X);
        }
        private void CentralPartPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Model.CentralPartIsHolded = false;
        }
        #endregion

        private void UserControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded || !e.WidthChanged) return;
            var koeff = e.NewSize.Width / e.PreviousSize.Width;
            Model.SetPositions(Model.BtnFromMargin.Left * koeff, Model.CenterPartWidth * koeff);
        }

        private string _periodTitle;
        public string PeriodTitle
        {
            get { return _periodTitle; }
            set
            {
                if (value == _periodTitle) return;
                _periodTitle = value;
                OnPropertyChanged();
            }
        }
        
        private string GetPeriodTitle(Tuple<DateTime, DateTime> period, DiagramIntervalMode mode)
        {
            switch (mode)
            {
                case DiagramIntervalMode.Years:
                    return period.Item1.Year == period.Item2.Year
                        ? $"{period.Item1.Year}"
                        : $"{period.Item1.Year} - {period.Item2.Year}";
                case DiagramIntervalMode.Months:
                    return period.Item1.IsMonthTheSame(period.Item2) 
                        ? $"{period.Item1:MMMM yyyy}" 
                        : $"{period.Item1:MMMM yyyy} - {period.Item2:MMMM yyyy}";
                 case DiagramIntervalMode.Days:
                    return period.Item1.Date.Equals(period.Item2.Date) 
                        ? $"{period.Item1:d}" 
                        : $"{period.Item1:d} - {period.Item2:d}";
            }
            return "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
