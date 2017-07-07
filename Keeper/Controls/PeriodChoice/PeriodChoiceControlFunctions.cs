using System.Windows;

namespace Keeper.Controls.PeriodChoice
{
    public static class PeriodChoiceControlFunctions
    {
        public static void SetPositions(this PeriodChoiceControlModel model, double btnFromMarginLeft, double centerPartWidth)
        {
            model.BtnFromMargin = new Thickness(btnFromMarginLeft, 0, 0, 0);
            model.LeftPartWidth = btnFromMarginLeft + 4;
            model.CenterPartMargin = new Thickness(model.LeftPartWidth, 0, 0, 0);
            model.CenterPartWidth = centerPartWidth;
            model.BtnToMargin = new Thickness(btnFromMarginLeft + centerPartWidth - 1, 0, -4, 0);
            model.RightPartMargin = new Thickness(model.LeftPartWidth + model.CenterPartWidth - 1, 0, 0, 0);
        }

        public static void ResetFlags(this PeriodChoiceControlModel model)
        {
            model.BtnFromIsHolded = false;
            model.BtnToIsHolded = false;
            model.CentralPartIsHolded = false;
            model.CentralPartDoubleClick = false;
        }

    }
}
