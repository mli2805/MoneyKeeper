using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AddAndEditCategoryViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public String Title { get; set; }
    public Category CategoryInWork { get; set; }

    public List<Category> CategoriesForParentList { get; set; }
    public Category SelectedParent { get; set; }

    public AddAndEditCategoryViewModel(Category category, string title)
    {
      Title = title;
      CategoryInWork = category;
      PrepareParentComboBox();
    }

    public Category GetRoot(Category category)
    {
      if (category.Parent == null) return category;
      else return GetRoot(category.Parent);
    }

    public void GetBranchFromPoint(Category point, List<Category> list)
    {
      list.Add(point);
      foreach (var child in point.Children)
      {
        GetBranchFromPoint(child, list);
      }
    }

    private void PrepareParentComboBox()
    {
      var root = GetRoot(CategoryInWork);
      CategoriesForParentList = new List<Category>();
      GetBranchFromPoint(root,CategoriesForParentList);
      CategoriesForParentList.Remove(CategoryInWork); // не работает, т.к. разные инстансы , надо перегружать операцию сравнения (по Id)

//      CategoriesForParentList = new List<Category>(from category in Db.Categories.Local
//                                                   where category == root
//                                                   select category);
      SelectedParent = CategoryInWork.Parent;
    }

    public void Accept()
    {
      CategoryInWork.Parent = SelectedParent;
      TryClose(true);
    }


  }
}
