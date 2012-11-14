﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using Caliburn.Micro;
using Keeper.DomainModel;
using System.Linq;

namespace Keeper.ViewModels
{
  [Export(typeof(IShell))]
  [Export(typeof(ShellViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
  public class TransactionsViewModel : Screen, IShell
  {
    public List<OperationType> OperationTypes { get; set; }
    public List<CurrencyCodes> CurrencyList { get; set; }

    public List<Account> PossibleDebet { get; set; }
    public List<Account> PossibleCredit { get; set; }
    public List<Account> PossibleArticle { get; set; }

    public List<Account> MyAccounts { get; set; }
    public List<Account> ExternalDebetAccounts { get; set; }
    public List<Account> ExternalCreditAccounts { get; set; }
    public List<Account> IncomeArticles { get; set; }
    public List<Account> ExpenseArticles { get; set; }

    public void ComboBoxesValues()
    {
      OperationTypes = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

      MyAccounts = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои")).ToList();
      ExternalDebetAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоДатели"))).ToList();
      ExternalCreditAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоПолучатели"))).ToList();
      IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все доходы")).ToList();
      ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все расходы")).ToList();
    }
    
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<Transaction> Rows { get; set; }
    public Transaction SelectedTransaction { get; set; }
    
    public TransactionsViewModel()
    {
      Db.Transactions.Load();
      Rows = Db.Transactions.Local;
      SelectedTransaction = Rows.First();

      //      SelectedTransaction = new Transaction();
      //      Rows.First().IsSelected = true;
      ComboBoxesValues();
    }

    protected override void OnViewLoaded(object view)
    {
    }

  }


}
