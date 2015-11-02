using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Keeper.Annotations;

namespace Keeper.Controls.PeriodChoice
{
    public partial class PeriodChoiceControl : INotifyPropertyChanged
    {
        #region Control's outer properties for choice result
        public static readonly DependencyProperty FromPointProperty =
            DependencyProperty.Register("FromPoint", typeof(double),
                typeof(PeriodChoiceControl));
        public double FromPoint
        {
            get{return (double)GetValue(FromPointProperty);}
            set{SetValue(FromPointProperty, value);}
        }
        public static readonly DependencyProperty ToPointProperty =
            DependencyProperty.Register("ToPoint", typeof(double),
                typeof(PeriodChoiceControl));
        public double ToPoint
        {
            get{return (double)GetValue(ToPointProperty);}
            set { SetValue(ToPointProperty, value); }
        }
        #endregion

        public PeriodChoiceControlModel Model { get; set; }
        public PeriodChoiceControl()
        {
            Model = new PeriodChoiceControlModel();
            InitializeComponent();

            ControlGrid.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var width = ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth;
            Model.SetPositions(width * FromPoint - 4, width * (ToPoint - FromPoint) + PeriodChoiceControlModel.MinCenterPartWidth);
            Model.ResetFlags();
        }
        private void RefreshDependencyProperties()
        {
            double k = 100 / (ControlGrid.ActualWidth - PeriodChoiceControlModel.MinCenterPartWidth);
            FromPoint = (Model.BtnFromMargin.Left + 3) * k;
            ToPoint = (Model.BtnToMargin.Left - PeriodChoiceControlModel.MinCenterPartWidth + 4) * k;
        }

        #region btnFrom reactions
        private void BtnFromPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model.ReactBtnFromPreviewMouseDown(e.GetPosition(this).X);
        }
        private void BtnFromPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Model.BtnFromIsHolded) return;
            Model.ReactBtnFromPreviewMouseMove(e.GetPosition(this).X);
            RefreshDependencyProperties();
        }
        private void BtnFromPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model.BtnFromIsHolded = false;
        }
        #endregion 

        #region btnTo reactions
        private void BtnToPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model.ReactBtnToPreviewMouseDown(e.GetPosition(this).X);
        }
        private void BtnToPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Model.BtnToIsHolded) return;

            Model.ReactBtnToPreviewMouseMove(e.GetPosition(this).X, RightPart.ActualWidth);
            RefreshDependencyProperties();
        }
        private void BtnToPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model.BtnToIsHolded = false;
        }
        #endregion

        #region CentralPart reactions
        private void CentralPartPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Model.CentralPartIsHolded) return;
            Model.ReactCentralPartPreviewMouseMove(e.GetPosition(this).X, RightPart.ActualWidth);
            RefreshDependencyProperties();
        }
        private void CenterPartPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Model.ReactCentralPartDoubleClick(e.GetPosition(this).X, ControlGrid.ActualWidth);
                RefreshDependencyProperties();
            }
            else
                Model.ReactCentralPartPreviewMouseDown(e.GetPosition(this).X);
        }
        private void CentralPartPreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Model.CentralPartIsHolded = false;
        }
        #endregion

        private void UserControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.IsLoaded || !e.WidthChanged) return;
            var koeff = e.NewSize.Width / e.PreviousSize.Width;
            Model.SetPositions(Model.BtnFromMargin.Left * koeff, Model.CenterPartWidth * koeff);
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
