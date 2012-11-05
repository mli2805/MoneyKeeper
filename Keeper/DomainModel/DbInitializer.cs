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
    private Account PrepareIncomesCategoriesTree()
    {
      var account = new Account("Все доходы");

      var account1 = new Account("Зарплата");
      account.Children.Add(account1);
      account1.Parent = account;

      account = new Account("Зарплата моя официальная");
      account1.Children.Add(account);
      account.Parent = account1;

      account = new Account("Зарплата моя 2я часть");
      account1.Children.Add(account);
      account.Parent = account1;

      account1 = new Account("Процентные доходы");
      account.Parent.Parent.Children.Add(account1);

      return account;
    }

    private Account PrepareExpensesCategoriesTree()
    {
      var account = new Account("Все расходы");
      var account1 = new Account("Продукты");
      account.Children.Add(account1);
      account1.Parent = account;
      account1 = new Account("Коммунальные платежи");
      account.Children.Add(account1);
      account1.Parent = account;
      account1 = new Account("Автомобиль");
      account.Children.Add(account1);
      account1.Parent = account;
      return account1;

    }
    #endregion

    protected override void Seed(KeeperDb context)
    {

      #region // счета
      var accountMine = new Account("Мои");
//      account.IsSelected = true;

      var accountHands = new Account("На руках");
      accountMine.Children.Add(accountHands);
      accountHands.Parent = accountMine;
      var accountDeposites = new Account("Депозиты");
      accountMine.Children.Add(accountDeposites);
      accountDeposites.Parent = accountMine;

      var accountCash = new Account("Кошельки");
      accountHands.Children.Add(accountCash);
      accountCash.Parent = accountHands;
      var accountMyWallet = new Account("Мой бумажник");
      accountCash.Children.Add(accountMyWallet);
      accountMyWallet.Parent = accountCash;

      context.Accounts.Add(accountMine);

      var accountExternals = new Account("Внешние");
      var accountOptixsoft = new Account("ОптикСофт");
      accountExternals.Children.Add(accountOptixsoft);
      accountOptixsoft.Parent = accountExternals;
      var accountShops = new Account("Магазины");
      accountExternals.Children.Add(accountShops);
      accountShops.Parent = accountExternals;

      context.Accounts.Add(accountExternals);
      #endregion

      #region // категории
      var incomeAccountSalarySecondPart = PrepareIncomesCategoriesTree();
      context.Accounts.Add(incomeAccountSalarySecondPart);

      var expenseAccountCar = PrepareExpensesCategoriesTree();
      context.Accounts.Add(expenseAccountCar);
      #endregion

      #region // транзакции

      var transaction = new Transaction
      {
        Timestamp = DateTime.Now, 
        Operation = OperationType.Доход, 
        Debet = accountOptixsoft, 
        Credit = accountMyWallet, 
        Amount = 450, 
        Currency = CurrencyCodes.USD, 
        Article = incomeAccountSalarySecondPart, 
        Comment = "за август"
      };

      var transaction2 = new Transaction 
      {
        Timestamp = DateTime.Now,
        Operation = OperationType.Расход,
        Debet = accountMyWallet, 
        Credit = accountShops,
        Amount = 10000,
        Currency = CurrencyCodes.BYR,
        Article = expenseAccountCar,
        Comment = "Лампочка левого стопа"
      };
      context.Transactions.Add(transaction2);
      context.Transactions.Add(transaction2.Clone());
      context.Transactions.Add(transaction);
      context.Transactions.Add(transaction2.Clone());
      context.Transactions.Add(transaction2.Clone());
      context.Transactions.Add(transaction.Clone());
      context.Transactions.Add(transaction.Clone());
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
