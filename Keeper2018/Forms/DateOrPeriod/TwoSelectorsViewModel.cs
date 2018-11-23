using Caliburn.Micro;

namespace Keeper2018
{
    public class TwoSelectorsViewModel : Screen
    {
        public TwoSelectorsModel MyTwoSelectorsModel { get; set; }

        public TwoSelectorsViewModel()
        {
            MyTwoSelectorsModel = new TwoSelectorsModel();
            MyTwoSelectorsModel.IsPeriodMode = false;
        }
    }
}
