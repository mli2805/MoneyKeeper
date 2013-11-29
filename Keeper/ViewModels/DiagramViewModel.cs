using Caliburn.Micro;
using Keeper.Utils.Diagram;

namespace Keeper.ViewModels
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
