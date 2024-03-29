using System.Windows.Input;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for ExpenseByCategoriesView.xaml
    /// </summary>
    public partial class ExpenseByCategoriesView
    {
        private bool _isLeftCtrlPressed;
        private bool _isRightCtrlPressed;

        public ExpenseByCategoriesView()
        {
            InitializeComponent();
        }

        private void ExpenseByCategoriesView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                _isLeftCtrlPressed = true;
            if (e.Key == Key.RightCtrl)
                _isRightCtrlPressed = true;

            if (e.Key == Key.Left)
            {
                if (_isLeftCtrlPressed)
                    PeriodChoiceControl.MoveButtonFromLeft();
                else if (_isRightCtrlPressed)
                    PeriodChoiceControl.MoveButtonToLeft();
                else
                    PeriodChoiceControl.MoveCentralPartLeft();
            }
            else if (e.Key == Key.Right)
            {
                if (_isLeftCtrlPressed)
                    PeriodChoiceControl.MoveButtonFromRight();
                else if (_isRightCtrlPressed)
                    PeriodChoiceControl.MoveButtonToRight();
                else
                    PeriodChoiceControl.MoveCentralPartRight();
            }
        }

        private void ExpenseByCategoriesView_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                _isLeftCtrlPressed = false;
            if (e.Key == Key.RightCtrl)
                _isRightCtrlPressed = false;
        }
    }
}
