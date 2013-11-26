using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;
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

  }

}
