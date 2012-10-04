using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class Category
  {
    public string Name { get; set; }
    public Category Parent { get; set; }
    public List<Category> Children { get; set; }

    public Category()
    {
      Name = "";
      Parent = null;
      Children = new List<Category>();
    }

    public Category(string name) : this()
    {
      Name = name;
    }

  }

}
