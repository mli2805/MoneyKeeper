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

        public PeriodChoiceControlModel Model { get; set; }

//        private const int MinCenterPartWidth = 30;
//        private bool _btnFromIsHolded;
//        private bool _btnToIsHolded;
//        private double _btnFromStartX;
//        private double _btnToStartX;
//        private bool _centralPartDoubleClick;
//        private bool _centralPartIsHolded;
//        private double _centralPartStartX;
//
//        private Thickness _btnFromMargin;
//        private Thickness _btnToMargin;
//        private double _leftPartWidth;
//        private double _centerPartWidth;
//        private Thickness _centerPartMargin;
//        private Thickness _rightPartMargin;
//        public Thickness BtnFromMargin
//        {
//            get { return _btnFromMargin; }
//            set
//            {
//                if (value.Equals(_btnFromMargin)) return;
//                _btnFromMargin = value;
//                OnPropertyChanged();
//            }
//        }
//        public Thickness BtnToMargin
//        {
//            get { return _btnToMargin; }
//            set
//            {
//                if (value.Equals(_btnToMargin)) return;
//                double delta = _btnToMargin.Left - value.Left;
//                _btnToMargin = value;
//                RightPartMargin = new Thickness(RightPartMargin.Left - delta, 0, 0, 0);
//                OnPropertyChanged();
//            }
//        }
//        public double LeftPartWidth
//        {
//            get { return _leftPartWidth; }
//            set
//            {
//                if (value.Equals(_leftPartWidth)) return;
//                _leftPartWidth = value;
//                OnPropertyChanged();
//            }
//        }
//        public double CenterPartWidth
//        {
//            get { return _centerPartWidth; }
//            set
//            {
//                if (value.Equals(_centerPartWidth)) return;
//                _centerPartWidth = value;
//                OnPropertyChanged();
//            }
//        }
//        public Thickness CenterPartMargin
//        {
//            get { return _centerPartMargin; }
//            set
//            {
//                if (value.Equals(_centerPartMargin)) return;
//                _centerPartMargin = value;
//                OnPropertyChanged();
//            }
//        }
//        public Thickness RightPartMargin
//        {
//            get { return _rightPartMargin; }
//            set
//            {
//                if (value.Equals(_rightPartMargin)) return;
//                _rightPartMargin = value;
//                OnPropertyChanged();
//            }
//        }
        #endregion
        public PeriodChoiceControl()
        {
           Model = new PeriodChoiceControlModel();
            InitializeComponent();

            ControlGrid.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var width = ControlGrid.ActualWidth;

            Console.WriteLine("{0}  {1}", FromPoint, ToPoint);
            SetPositions(width*FromPoint - 4, width*(ToPoint - FromPoint));
            Model._btnFromIsHolded = false;
            Model._btnToIsHolded = false;
            Model._centralPartIsHolded = false;
            Model._centralPartDoubleClick = false;
        }

        private void SetPositions(double btnFromMarginLeft, double centerPartWidth)
        {
            Model.BtnFromMargin = new Thickness(btnFromMarginLeft, 0, 0, 0);
            Model.LeftPartWidth = btnFromMarginLeft + 4;
            Model.CenterPartMargin = new Thickness(Model.LeftPartWidth, 0, 0, 0);
            Model.CenterPartWidth = centerPartWidth;
            Model.BtnToMargin = new Thickness(btnFromMarginLeft + centerPartWidth - 1, 0, -4, 0);
            Model.RightPartMargin = new Thickness(Model.LeftPartWidth + Model.CenterPartWidth - 1, 0, 0, 0);
        }

        private void RefreshDependencyProperties()
        {
            double k = 100 / ControlGrid.ActualWidth;
            FromPoint = (Model.BtnFromMargin.Left + 4) * k;
            ToPoint = (Model.BtnFromMargin.Left + 4 + Model.CenterPartWidth) * k;
        }

        #region btnFrom reactions
        private void BtnFromPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model._btnFromIsHolded = true;
            Model._btnFromStartX = e.GetPosition(this).X;
        }

        private void BtnFrom_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Model._btnFromIsHolded)
            {
                var delta = e.GetPosition(this).X - Model._btnFromStartX;
                MoveBtnFrom(delta);
                Model._btnFromStartX = e.GetPosition(this).X;
            }
        }

        private void MoveBtnFrom(double delta)
        {
            if (delta > Model.CenterPartWidth - PeriodChoiceControlModel.MinCenterPartWidth) return;
            if (Model.BtnFromMargin.Left + delta < -4) delta = -Model.BtnFromMargin.Left - 4;

            Model.BtnFromMargin = new Thickness(Model.BtnFromMargin.Left + delta, 0, 0, 0);
            Model.LeftPartWidth = Model.BtnFromMargin.Left + 4;
            Model.CenterPartMargin = new Thickness(Model.LeftPartWidth, 0, 0, 0);
            Model.CenterPartWidth -= delta;

            RefreshDependencyProperties();
        }

        private void BtnFromPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model._btnFromIsHolded = false;
        }
        #endregion

        #region btnTo reactions
        private void BtnToPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model._btnToIsHolded = true;
            Model._btnToStartX = e.GetPosition(this).X;
        }
        private void BtnToPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Model._btnToIsHolded)
            {
                var delta = e.GetPosition(this).X - Model._btnToStartX;
                MoveBtnTo(delta);
                Model._btnToStartX = e.GetPosition(this).X;
            }
        }

        private void MoveBtnTo(double delta)
        {
            if (Model.CenterPartWidth - PeriodChoiceControlModel.MinCenterPartWidth + delta < 0) return;
            if (RightPart.ActualWidth - delta <= 0) return;

            Model.BtnToMargin = new Thickness(Model.BtnToMargin.Left + delta, 0, -4, 0);
            Model.CenterPartWidth += delta;

            RefreshDependencyProperties();
        }

        private void BtnToPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model._btnToIsHolded = false;
        }
        #endregion

        #region CentralPart reactions
        private void CentralPartPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Model._centralPartIsHolded)
            {
                var delta = e.GetPosition(this).X - Model._centralPartStartX;
                MoveCentralPart(delta);
                Model._centralPartStartX = e.GetPosition(this).X;
            }
        }

        private void MoveCentralPart(double delta)
        {
            Console.WriteLine("central part is moving");

            Model.BtnFromMargin = new Thickness(Model.BtnFromMargin.Left + delta, 0, 0, 0);
            Model.LeftPartWidth = Model.BtnFromMargin.Left + 4;
            Model.CenterPartMargin = new Thickness(Model.LeftPartWidth, 0, 0, 0);
            Model.BtnToMargin = new Thickness(Model.BtnToMargin.Left + delta, 0, -4, 0);

            RefreshDependencyProperties();
        }
        private void CenterPartPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Model._centralPartDoubleClick) ShrinkAroundX(e.GetPosition(this).X);
                else ExpandOnAllRange();
                Model._centralPartDoubleClick = !Model._centralPartDoubleClick;

                Console.WriteLine("actual width is {0} central part starts from {1} and has width {2}", this.ActualWidth, Model.CenterPartMargin.Left, Model.CenterPartWidth);
                RefreshDependencyProperties();
            }
            else
            {
                Model._centralPartIsHolded = true;
                Model._centralPartStartX = e.GetPosition(this).X;
            }
            
        }
        private void CentralPartPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model._centralPartIsHolded = false;
        }

        private void ExpandOnAllRange()
        {
            SetPositions(-4, ControlGrid.ActualWidth);
        }

        private void ShrinkAroundX(double x)
        {
            double left = x - PeriodChoiceControlModel.MinCenterPartWidth / 2;
            if (left < -4) left = -4;
            if (left > ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth - 4) left = ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth - 4;
            SetPositions(left, PeriodChoiceControlModel.MinCenterPartWidth);
        }
        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.IsLoaded || !e.WidthChanged) return;
            var koeff = e.NewSize.Width/e.PreviousSize.Width;
            SetPositions(Model.BtnFromMargin.Left * koeff, Model.CenterPartWidth * koeff);
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
