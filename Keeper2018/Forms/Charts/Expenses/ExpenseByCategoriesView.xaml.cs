using System.Windows.Input;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for ExpenseByCategoriesView.xaml
    /// </summary>
    public partial class ExpenseByCategoriesView
    {
        public ExpenseByCategoriesView()
        {
            InitializeComponent();
        }

        private void ExpenseByCategoriesView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                PeriodChoiceControl.DoLeft();
            }
            else if (e.Key == Key.Right)
            {
                PeriodChoiceControl.DoRight();
            }
        }
    }
}
