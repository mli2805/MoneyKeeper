using Keeper.DomainModel.Enumes;

namespace Keeper.Utils
{
  public class OperationTypesFilter
  {
    public bool IsOn { get; set; }
    public OperationType Operation { get; set; }

    /// <summary>
    /// таким конструктором создается ВЫключенный фильтр
    /// ему не нужен тип операции, он пропускает все типы
    /// </summary>
    public OperationTypesFilter() { IsOn = false; }

    /// <summary>
    /// а такой фильтр пропускает только "свою" операцию
    /// </summary>
    /// <param name="operation"></param>
    public OperationTypesFilter(OperationType operation)
    {
      IsOn = true;
      Operation = operation;
    }

    public override string ToString()
    {
      return IsOn ? Operation.ToString() : "<no filter>";
    }
  }
}
