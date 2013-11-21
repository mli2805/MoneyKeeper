using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;
using Keeper.Utils.Diagram;

namespace Keeper.ViewModels
{
  internal class BarDiagramViewModel : Screen
  {
    public DiagramData ModelDataProperty { get; set; }

    public BarDiagramViewModel(DiagramData diagramData)
    {
      ModelDataProperty = diagramData;
    }

  }

}
