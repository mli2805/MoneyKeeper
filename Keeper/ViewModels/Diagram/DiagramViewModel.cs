using Caliburn.Micro;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.ViewModels.Diagram
{
    internal class DiagramViewModel : Screen
    {
        public DiagramData ModelDataProperty { get; set; }

        public DiagramViewModel(DiagramData diagramData)
        {
            ModelDataProperty = diagramData;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = ModelDataProperty.Caption;
        }
    }

}
