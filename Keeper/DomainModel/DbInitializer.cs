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
      var account = new Account("Мои");
      account.IsAggregate = true;

      var account1 = new Account("Депозиты", true);
      account.Children.Add(account1);
      account1.Parent = account;

      var account2 = new Account("АСБ \"Ваш выбор\" 14.01.2012 -");
      account1.Children.Add(account2);
      account2.Parent = account1;

      account.Children.Add(new Account("Кошельки", true));

      account.IsSelected = true;

      return account2;
    }

    private Category PrepareIncomesCategoriesTree()
    {
      var category = new Category("Все доходы");

      var category1 = new Category("Зарплата");
      category.Children.Add(category1);
      category1.Parent = category;

      category = new Category("Зарплата моя официальная");
      category1.Children.Add(category);
      category.Parent = category1;

      category = new Category("Зарплата моя 2я часть");
      category1.Children.Add(category);
      category.Parent = category1;

      category1 = new Category("Процентные доходы");
      category.Parent.Parent.Children.Add(category1);

      return category;
    }

    private Category PrepareExpensesCategoriesTree()
    {
      var category = new Category("Все расходы");
      var category1 = new Category("Продукты");
      category.Children.Add(category1);
      category1.Parent = category;
      category1 = new Category("Коммунальные платежи");
      category.Children.Add(category1);
      category1.Parent = category;
      category1 = new Category("Автомобиль");
      category.Children.Add(category1);
      category1.Parent = category;
      return category1;

    }
    #endregion

    protected override void Seed(KeeperDb context)
    {
      var accountForExpense = PrepareAccountsTree();
      context.Accounts.Add(accountForExpense);
      var account = new Account("Внешние", true);
      var account2 = new Account("ОптикСофт");
      account2.Parent = account;
      context.Accounts.Add(account2);

      var incomeCategory = PrepareIncomesCategoriesTree();
      context.Categories.Add(incomeCategory);

      var expenseCategory = PrepareExpensesCategoriesTree();
      context.Categories.Add(expenseCategory);
//      var expenseCategory2 = new ExpenseCategory("Обслуживание");
//      expenseCategory2.Parent = expenseCategory;

      context.Transactions.Add(new Transaction
                                 {
                                   OperationType = OperationType.Income,
                                   AccountFrom = account2,
                                   AmountFrom = 450,
                                   Article = incomeCategory
                                 });
      var transaction2 = new Transaction 
      {
        OperationType = OperationType.Expense,
        AccountFrom = accountForExpense, 
        AmountFrom = 10000,
        Article = expenseCategory,
        Comment = "Лампочка левого стопа"
      };
      context.Transactions.Add(transaction2);
//      transaction = new Transaction{OperationType = OperationType.Transfer,AccountFrom = ,AccountTo = ,AmountFrom = 500,AmountTo = 500};
//      transaction = new Transaction{OperationType = OperationType.Excange,AccountFrom = ,AmountFrom = 100,AccountTo = ,AmountTo = 861000,Comment = "В БелПСБ по 8610"};

      var currencyRate = new CurrencyRate();
      currencyRate.Rate = 8540;
      context.CurrencyRates.Add(currencyRate);

      context.SaveChanges();

    }
  }
}
