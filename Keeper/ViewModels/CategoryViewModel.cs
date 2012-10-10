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
  class CategoryViewModel : Screen
  {
    public Category CategoryInWork { get; set; }
    public FormMode CurrentMode { get; set; }

    public void PrepareForCreate(Category parentCategory)
    {
      CategoryInWork = parentCategory is ExpenseCategory ? (Category)new ExpenseCategory() : new IncomeCategory();
      CategoryInWork.Parent = parentCategory;
    }

    public void PrepareForEdit(Category editingCategory)
    {
      CategoryInWork = editingCategory;
    }

    public CategoryViewModel(Category category, FormMode mode)
    {
      CurrentMode = mode;
      if (mode == FormMode.Create) PrepareForCreate(category);
      else if (mode == FormMode.Edit) PrepareForEdit(category);
      else MessageBox.Show("Form mode can't be " + mode, "Something is wrong!");
    }

    public void Accept()
    {
      if (CurrentMode == FormMode.Create) 
        CategoryInWork.Parent.Children.Add(CategoryInWork);
      TryClose(true);
    }


  }
}
