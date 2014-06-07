using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [System.Composition.Export] 
  public class DbClassesInstanceParser
  {
    public Transaction TransactionFromStringWithNames(string s, IEnumerable<Account> accountsPlaneList)
    {
      var transaction = new Transaction();
      var substrings = s.Split(';');
      transaction.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
      transaction.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      transaction.Debet = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
      transaction.Credit = accountsPlaneList.First(account => account.Name == substrings[3].Trim());
      transaction.Amount = Convert.ToDecimal(substrings[4]);
      transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[5]);
      transaction.Amount2 = Convert.ToDecimal(substrings[6]);
      if (substrings[7].Trim() == "null" || substrings[7].Trim() == "0") transaction.Currency2 = null;
      else
        transaction.Currency2 = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[7]);
      transaction.Article = substrings[8].Trim() != "" ? accountsPlaneList.First(account => account.Name == substrings[8].Trim()) : null;
      transaction.Comment = substrings[9].Trim();

      return transaction;
    }
    public CurrencyRate CurrencyRateFromString(string s, IEnumerable<Account> accountsPlaneList)
    {
      var rate = new CurrencyRate();
      int next = s.IndexOf(';');
      rate.BankDay = Convert.ToDateTime(s.Substring(0, next), new CultureInfo("ru-RU"));
      rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
      next += 6;
      rate.Rate = Convert.ToDouble(s.Substring(next + 2));
      return rate;
    }
    public ArticleAssociation ArticleAssociationFromStringWithNames(string s, IEnumerable<Account> accountsPlaneList)
    {
      var association = new ArticleAssociation();
      var substrings = s.Split(';');
      association.ExternalAccount = accountsPlaneList.First(account => account.Name == substrings[0].Trim());
      association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
      association.AssociatedArticle = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
      return association;
    }
    public Account AccountFromString(string s, out int parentId)
    {
      var account = new Account();
      var substrings = s.Split(';');
      account.Id = Convert.ToInt32(substrings[0]);
      account.Name = substrings[1].Trim();
      parentId = Convert.ToInt32(substrings[2]);
      account.IsFolder = Convert.ToBoolean(substrings[3]);
      account.IsClosed = Convert.ToBoolean(substrings[4]);
      account.IsExpanded = Convert.ToBoolean(substrings[5]);
      return account;
    }
    public DepositProcentsEvaluated DepositProcentEvaluationRulesFromString(string s)
    {
      var rules = new DepositProcentsEvaluated();
      rules.IsFactDays = s[0] == '1';
      rules.OnlyAtTheEnd = s[1] == '1';
      rules.EveryStartDay = s[2] == '1';
      rules.EveryFirstDayOfMonth = s[3] == '1';
      rules.EveryLastDayOfMonth = s[4] == '1';
      rules.IsCapitalized = s[5] == '1';
      return rules;
    }
    public void DepositFromString(string s, IEnumerable<Account> accountsPlaneList)
    {
      var deposit = new Deposit();
      var substrings = s.Split(';');
      deposit.ParentAccount = accountsPlaneList.First(account => account.Id == Convert.ToInt32(substrings[0]));
      deposit.Bank = accountsPlaneList.First(account => account.Id == Convert.ToInt32(substrings[1]));
      deposit.Title = substrings[2].Trim(); 
      deposit.AgreementNumber = substrings[3].Trim();
      deposit.StartDate = Convert.ToDateTime(substrings[4], new CultureInfo("ru-RU"));
      deposit.FinishDate = Convert.ToDateTime(substrings[5], new CultureInfo("ru-RU"));
      deposit.Currency = (CurrencyCodes) Enum.Parse(typeof (CurrencyCodes), substrings[6]);
      deposit.ProcentsEvaluated = DepositProcentEvaluationRulesFromString(substrings[7]);
      deposit.Comment = substrings[8].Replace("|", "\r\n");

      deposit.ParentAccount.Deposit = deposit;
    }

    public DepositRateLine DepositRateLineFromString(string s, IEnumerable<Account> accountsPlaneList)
    {
      var depositRateLine = new DepositRateLine();
      var substrings = s.Split(';');
      var depositAccount = accountsPlaneList.First(account => account.Id == Convert.ToInt32(substrings[0]));
      depositRateLine.DateFrom = Convert.ToDateTime(substrings[1], new CultureInfo("ru-RU"));
      depositRateLine.AmountFrom = Convert.ToDecimal(substrings[2]);
      depositRateLine.AmountTo = Convert.ToDecimal(substrings[3]);
      depositRateLine.Rate = Convert.ToDecimal(substrings[4]);

      depositAccount.Deposit.DepositRateLines.Add(depositRateLine);

      return depositRateLine;
    }


  }
}
