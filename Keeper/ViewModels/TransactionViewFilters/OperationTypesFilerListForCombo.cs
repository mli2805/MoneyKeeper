using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// Filter's list for combobox on Transactions window
  /// </summary>
  public static class OperationTypesFilerListForCombo
  {
    public static List<OperationTypesFilter> FilterList { get; private set; }

    static OperationTypesFilerListForCombo()
    {
      FilterList = new List<OperationTypesFilter>();

      // <no filter>
      var filter = new OperationTypesFilter();
      FilterList.Add(filter);

      var operationTypesList = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();

      foreach (var operationType in operationTypesList)
      {
        filter = new OperationTypesFilter(operationType);
        FilterList.Add(filter);
      }

    }
  }
}
