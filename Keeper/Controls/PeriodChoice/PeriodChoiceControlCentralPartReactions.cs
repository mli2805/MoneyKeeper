using System.Windows;

namespace Keeper.Controls.PeriodChoice
{
    public static class PeriodChoiceControlCentralPartReactions
    {
        public static void ReactCentralPartPreviewMouseMove(this PeriodChoiceControlModel model, double x, double rightPartWidth)
        {
            var delta = x - model.CentralPartStartX;
            model.CentralPartStartX = x;

            if (model.BtnFromMargin.Left + delta < -4) delta = -model.BtnFromMargin.Left - 4;
            if (rightPartWidth - delta <= 0) return;

            model.BtnFromMargin = new Thickness(model.BtnFromMargin.Left + delta, 0, 0, 0);
            model.LeftPartWidth = model.BtnFromMargin.Left + 4;
            model.CenterPartMargin = new Thickness(model.LeftPartWidth, 0, 0, 0);
            model.BtnToMargin = new Thickness(model.BtnToMargin.Left + delta, 0, -4, 0);
        }

        public static void ReactCentralPartPreviewMouseDown(this PeriodChoiceControlModel model, double x)
        {
            model.CentralPartIsHolded = true;
            model.CentralPartStartX = x;
        }

        public static void ReactCentralPartDoubleClick(this PeriodChoiceControlModel model, double x, double controlActualWidth)
        {
            if (model.CentralPartDoubleClick) 
                model.ShrinkAroundX(x, controlActualWidth);
            else
                model.SetPositions(-4, controlActualWidth); // expand central part all over control
            model.CentralPartDoubleClick = !model.CentralPartDoubleClick;
        }
        private static void ShrinkAroundX(this PeriodChoiceControlModel model, double x, double controlActualWidth)
        {
            double left = x - PeriodChoiceControlModel.MinCenterPartWidth / 2;
            if (left < -4) left = -4;
            if (left > controlActualWidth - PeriodChoiceControlModel.MinCenterPartWidth - 4) left = controlActualWidth - PeriodChoiceControlModel.MinCenterPartWidth - 4;
            model.SetPositions(left, PeriodChoiceControlModel.MinCenterPartWidth);
        }




    }
}
