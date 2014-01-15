using System.Composition;

namespace Keeper.Models
{
  [Export]
  [Shared]
  public class ShellModel
  {
    public ShellModel()
    {
      MyForestModel = new AccountForestModel();
    }

    public AccountForestModel MyForestModel { get; set; }
  }
}
