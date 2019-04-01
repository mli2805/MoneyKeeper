using System;
using System.Windows;

namespace Keeper2018
{
    public static class PeriodChoiceControlBtnToReactions
    {
        public static void ReactBtnToPreviewMouseDown(this PeriodChoiceControlModel model, double x)
        {
            model.BtnToIsHolded = true;
            model.BtnToStartX = x;
        }
        public static void ReactBtnToPreviewMouseMove(this PeriodChoiceControlModel model, double x, double rightPartWidth)
        {
            var delta = x - model.BtnToStartX;
            model.BtnToStartX = x;
            if (model.CenterPartWidth - PeriodChoiceControlModel.MinCenterPartWidth + delta < 0) return;
            if (rightPartWidth - delta <= 0) return;

            model.BtnToMargin = new Thickness(model.BtnToMargin.Left + delta, 0, -4, 0);
            model.CenterPartWidth += delta;

            Console.WriteLine($@"CenterPartWidth = {model.CenterPartWidth}  CentralPartMargin = {model.CenterPartMargin.Left}");
        }

    }
}
