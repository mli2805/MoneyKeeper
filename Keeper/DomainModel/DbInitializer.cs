using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class DbInitializer : CreateDatabaseIfNotExists<KeeperDb>
  {
    #region // подготовка  категорий
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

      #region // счета
      var accountMine = new Account("Мои");
      accountMine.IsAggregate = true;
//      account.IsSelected = true;

      var accountHands = new Account("На руках", true);
      accountMine.Children.Add(accountHands);
      accountHands.Parent = accountMine;
      var accountDeposites = new Account("Депозиты", true);
      accountMine.Children.Add(accountDeposites);
      accountDeposites.Parent = accountMine;

      var accountCash = new Account("Кошельки", true);
      accountHands.Children.Add(accountCash);
      accountCash.Parent = accountHands;
      var accountMyWallet = new Account("Мой бумажник", false);
      accountCash.Children.Add(accountMyWallet);
      accountMyWallet.Parent = accountCash;

      context.Accounts.Add(accountMine);

      var accountExternals = new Account("Внешние", true);
      var accountOptixsoft = new Account("ОптикСофт",false);
      accountExternals.Children.Add(accountOptixsoft);
      accountOptixsoft.Parent = accountExternals;
      var accountShops = new Account("Магазины",false);
      accountExternals.Children.Add(accountShops);
      accountShops.Parent = accountExternals;

      context.Accounts.Add(accountExternals);
      #endregion

      #region // категории
      var incomeCategorySalarySecondPart = PrepareIncomesCategoriesTree();
      context.Categories.Add(incomeCategorySalarySecondPart);

      var expenseCategoryCar = PrepareExpensesCategoriesTree();
      context.Categories.Add(expenseCategoryCar);
      #endregion

      #region // транзакции
      context.Transactions.Add(new Transaction
                                 { Timestamp = DateTime.Now,
                                   Operation = OperationType.Income,
                                   Debet = accountOptixsoft,
                                   Credit = accountMyWallet,
                                   Amount = 450,
                                   Currency = CurrencyCodes.USD,
                                   Article = incomeCategorySalarySecondPart,
                                   Comment = "за август"
                                 });

      var transaction2 = new Transaction 
      {
        Timestamp = DateTime.Now,
        Operation = OperationType.Expense,
        Debet = accountMyWallet, 
        Credit = accountShops,
        Amount = 10000,
        Currency = CurrencyCodes.BYR,
        Article = expenseCategoryCar,
        Comment = "Лампочка левого стопа"
      };
      context.Transactions.Add(transaction2);
      #endregion

      #region // курсы валют
      var currencyRate = new CurrencyRate();
      currencyRate.Rate = 8540;
      context.CurrencyRates.Add(currencyRate);
      #endregion

      context.SaveChanges();

    }
  }
}
