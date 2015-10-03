using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Keeper.Annotations;

namespace Keeper.Controls
{
    /// <summary>
    /// Interaction logic for PeriodChoiceControl.xaml
    /// </summary>
    public partial class PeriodChoiceControl : INotifyPropertyChanged
    {
        #region Control's outer properties for choice result

        public static readonly DependencyProperty FromPointProperty =
            DependencyProperty.Register("FromPoint", typeof (double),
                typeof (PeriodChoiceControl));

        public double FromPoint
        {
            get { return (double) GetValue(FromPointProperty); }
            set
            {
                SetValue(FromPointProperty, value);
                
            }
        }

        public static readonly DependencyProperty ToPointProperty =
            DependencyProperty.Register("ToPoint", typeof (double),
                typeof (PeriodChoiceControl));

        public double ToPoint
        {
            get { return (double)GetValue(ToPointProperty); }
            set { SetValue(ToPointProperty, value); }
        }
        #endregion

        #region Control's inner fields and properties for coordinates and sizes

        private int _minCenterPartWidth;

        private bool _btnFromIsHolded;
        private bool _btnToIsHolded;
        private double _btnFromStartX;
        private double _btnToStartX;
        private bool _centralPartDoubleClick;

        private Thickness _btnFromMargin;
        private Thickness _btnToMargin;
        private double _leftPartWidth;
        private double _centerPartWidth;
        private Thickness _centerPartMargin;
        private Thickness _rightPartMargin;
        public Thickness BtnFromMargin
        {
            get { return _btnFromMargin; }
            set
            {
                if (value.Equals(_btnFromMargin)) return;
                _btnFromMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness BtnToMargin
        {
            get { return _btnToMargin; }
            set
            {
                if (value.Equals(_btnToMargin)) return;
                double delta = _btnToMargin.Left - value.Left;
                _btnToMargin = value;
                RightPartMargin = new Thickness(RightPartMargin.Left - delta, 0, 0, 0);
                OnPropertyChanged();
            }
        }
        public double LeftPartWidth
        {
            get { return _leftPartWidth; }
            set
            {
                if (value.Equals(_leftPartWidth)) return;
                _leftPartWidth = value;
                OnPropertyChanged();
            }
        }
        public double CenterPartWidth
        {
            get { return _centerPartWidth; }
            set
            {
                if (value.Equals(_centerPartWidth)) return;
                _centerPartWidth = value;
                OnPropertyChanged();
            }
        }
        public Thickness CenterPartMargin
        {
            get { return _centerPartMargin; }
            set
            {
                if (value.Equals(_centerPartMargin)) return;
                _centerPartMargin = value;
                OnPropertyChanged();
            }
        }
        public Thickness RightPartMargin
        {
            get { return _rightPartMargin; }
            set
            {
                if (value.Equals(_rightPartMargin)) return;
                _rightPartMargin = value;
                OnPropertyChanged();
            }
        }
        #endregion
        public PeriodChoiceControl()
        {
            InitializeComponent();

            ControlGrid.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var width = ControlGrid.ActualWidth;
            _minCenterPartWidth = (int)(width*0.1);

            SetPositions(width*FromPoint, width*(ToPoint - FromPoint));
            _btnFromIsHolded = false;
            _btnToIsHolded = false;
            _centralPartDoubleClick = false;
        }

        private void SetPositions(double btnFromMarginLeft, double centerPartWidth)
        {
            BtnFromMargin = new Thickness(btnFromMarginLeft, 0, 0, 0);
            LeftPartWidth = btnFromMarginLeft + 4;
            CenterPartMargin = new Thickness(LeftPartWidth, 0, 0, 0);
            CenterPartWidth = centerPartWidth;
            BtnToMargin = new Thickness(btnFromMarginLeft + centerPartWidth - 1, 0, -4, 0);
            RightPartMargin = new Thickness(LeftPartWidth + CenterPartWidth - 1, 0, 0, 0);
        }

        private void RefreshDependencyProperties()
        {
            double k = 100 / ControlGrid.ActualWidth;
            FromPoint = (_btnFromMargin.Left + 4) * k;
            ToPoint = (_btnFromMargin.Left + 4 + _centerPartWidth) * k;
        }

        #region btnFrom reactions
        private void BtnFromPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _btnFromIsHolded = true;
            _btnFromStartX = e.GetPosition(this).X;
        }

        private void BtnFrom_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_btnFromIsHolded)
            {
                var delta = e.GetPosition(this).X - _btnFromStartX;
                MoveBtnFrom(delta);
                _btnFromStartX = e.GetPosition(this).X;
            }
        }

        private void MoveBtnFrom(double delta)
        {
            if (delta > CenterPartWidth - _minCenterPartWidth) return;
            if (BtnFromMargin.Left + delta < -4) delta = -BtnFromMargin.Left - 4;

            BtnFromMargin = new Thickness(BtnFromMargin.Left + delta, 0, 0, 0);
            LeftPartWidth = BtnFromMargin.Left + 4;
            CenterPartMargin = new Thickness(LeftPartWidth, 0, 0, 0);
            CenterPartWidth -= delta;

            RefreshDependencyProperties();
        }

        private void BtnFromPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _btnFromIsHolded = false;
        }
        #endregion

        #region btnTo reactions
        private void BtnToPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _btnToIsHolded = true;
            _btnToStartX = e.GetPosition(this).X;
        }
        private void BtnToPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_btnToIsHolded)
            {
                var delta = e.GetPosition(this).X - _btnToStartX;
                MoveBtnTo(delta);
                _btnToStartX = e.GetPosition(this).X;
            }
        }

        private void MoveBtnTo(double delta)
        {
            if (CenterPartWidth - _minCenterPartWidth + delta < 0) return;
            if (RightPart.ActualWidth - delta <= 0) return;

            BtnToMargin = new Thickness(BtnToMargin.Left + delta, 0, -4, 0);
            CenterPartWidth += delta;

            RefreshDependencyProperties();
        }

        private void BtnToPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _btnToIsHolded = false;
        }
        #endregion

        #region CentralPart reactions
        private void CenterPartPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (_centralPartDoubleClick) ShrinkAroundX(e.GetPosition(this).X);
                else ExpandOnAllRange();
                _centralPartDoubleClick = !_centralPartDoubleClick;

                Console.WriteLine("actual width is {0} central part starts from {1} and has width {2}", this.ActualWidth, CenterPartMargin.Left, CenterPartWidth);
                RefreshDependencyProperties();
            }
        }

        private void ExpandOnAllRange()
        {
            SetPositions(-4, ControlGrid.ActualWidth);
        }

        private void ShrinkAroundX(double x)
        {
            double left = x - _minCenterPartWidth / 2;
            if (left < -4) left = -4;
            if (left > ControlGrid.ActualWidth - _minCenterPartWidth - 4) left = ControlGrid.ActualWidth - _minCenterPartWidth - 4;
            SetPositions(left, _minCenterPartWidth);
        }
        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            if (e.WidthChanged)
            {
                var koeff = e.NewSize.Width/e.PreviousSize.Width;
                _minCenterPartWidth = (int)(e.NewSize.Width * 0.1);
                SetPositions(BtnFromMargin.Left * koeff, CenterPartWidth * koeff);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
