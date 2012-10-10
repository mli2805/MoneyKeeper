using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class DbInitializer:CreateDatabaseIfNotExists<KeeperDb>
  {
    #region // подготовка списков счетов и категорий
    private Account PrepareAccountsTree()
    {
      var account = new Account("Все");
      account.IsAggregate = true;

      var account1 = new Account("Депозиты", true);
      account.Children.Add(account1);

      account.Children.Add(new Account("Кошельки", true));

      var account2 = new Account("АСБ \"Ваш выбор\" 14.01.2012 -");
      account1.Children.Add(account2);

      account.IsSelected = true;

      return account;
    }

    private IncomeCategory PrepareIncomesCategoriesTree()
    {
      var category = new IncomeCategory("Все доходы");

//      var category1 = new IncomeCategory("Зарплата");
//      category.Children.Add(category1);
//      category1.Parent = category;

//      category = new IncomeCategory("Зарплата моя официальная");
//      category1.Children.Add(category);
//      category.Parent = category1;

//      category = new IncomeCategory("Зарплата моя 2я часть");
//      category1.Children.Add(category);
//      category.Parent = category1;

//      category1 = new IncomeCategory("Процентные доходы");
//      category.Parent.Parent.Children.Add(category1);

      return category;
    }

    private ExpenseCategory PrepareExpensesCategoriesTree()
    {
      var category = new ExpenseCategory("Все расходы");
//      var category1 = new ExpenseCategory("Продукты");
//      category.Children.Add(category1);
//      category1.Parent = category;
//      category1 = new ExpenseCategory("Автомобиль");
//      category.Children.Add(category1);
//      category1.Parent = category;
//      category1 = new ExpenseCategory("Коммунальные платежи");
//      category.Children.Add(category1);
//      category1.Parent = category;
      return category;

    }
    #endregion
    protected override void Seed(KeeperDb context)
    {
      context.Accounts.Add(PrepareAccountsTree());
      context.Incomes.Add(PrepareIncomesCategoriesTree());
      context.Expenses.Add(PrepareExpensesCategoriesTree());

      context.SaveChanges();

    }
  }
}
