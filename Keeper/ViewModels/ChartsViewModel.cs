using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.ViewModels
{
  public class PieItem
  {
    public string Subject { get; set; }
    public decimal Amount { get; set; }

    public PieItem(string subject, decimal amount)
    {
      Subject = subject;
      Amount = amount;
    }
  }
  class ChartsViewModel
  {
//    public List<KeyValuePair<string, int>> PieData { get; set; }

    public List<PieItem> PieData { get; set; }

    public ChartsViewModel()
    {
      PieData = new List<PieItem>
                        {
                          new PieItem("Developer", 60),
                          new PieItem("Misc", 20),
                          new PieItem("Tester", 50),
                          new PieItem("QA", 30),
                          new PieItem("Project Manager", 40)
                        };
    }
  }
}
