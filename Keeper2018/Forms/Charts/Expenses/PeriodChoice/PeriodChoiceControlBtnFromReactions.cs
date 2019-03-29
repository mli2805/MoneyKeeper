using System.Windows;

namespace Keeper2018
{
    public static class PeriodChoiceControlBtnFromReactions
    {
        public static void ReactBtnFromPreviewMouseDown(this PeriodChoiceControlModel model, double x)
        {
            model.BtnFromIsHolded = true;
            model.BtnFromStartX = x;
        }
        public static void ReactBtnFromPreviewMouseMove(this PeriodChoiceControlModel model, double x)
        {
            var delta = x - model.BtnFromStartX;
            model.BtnFromStartX = x;
            if (delta > model.CenterPartWidth - PeriodChoiceControlModel.MinCenterPartWidth) return;
            if (model.BtnFromMargin.Left + delta < -4) delta = -model.BtnFromMargin.Left - 4;

            model.BtnFromMargin = new Thickness(model.BtnFromMargin.Left + delta, 0, 0, 0);
            model.LeftPartWidth = model.BtnFromMargin.Left + 4;
            model.CenterPartMargin = new Thickness(model.LeftPartWidth, 0, 0, 0);
            model.CenterPartWidth -= delta;
        }

    }
}
