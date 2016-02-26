using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.Common;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [Export]
    public class DbClassesInstanceDumper : IDbClassesInstanceDumper
    {
        public string Dump(HierarchyItem<Account> account)
        {
            var shiftedName = new string(' ', account.Level * 2) + account.Item.Name;
            var parentForDump = account.Item.Parent == null ? 0 : account.Item.Parent.Id;
            return account.Item.Id + " ; " + shiftedName + " ; " + parentForDump + " ; " +
              account.Item.IsFolder + " ; " + account.Item.IsClosed + " ; " + account.Item.IsExpanded;
        }
        public string Dump(ArticleAssociation association)
        {
            return association.ExternalAccount + " ; " +
                   association.OperationType + " ; " +
                   association.AssociatedArticle;
        }
        public string Dump(CurrencyRate rate)
        {
            return Convert.ToString(rate.BankDay) + " ; " +
                   rate.Currency + " ; " + rate.Rate;
        }
        public string Dump(NbRate rate)
        {
            return Convert.ToString(rate.Date) + " ; " +
                   rate.UsdRate + " ; " + rate.EurRate + " ; " + rate.RurRate;
        }
        public string Dump(Transaction transaction)
        {
            return Convert.ToString(transaction.Timestamp) + " ; " + transaction.Operation + " ; " +
                   transaction.Debet + " ; " + transaction.Credit + " ; " + transaction.Amount + " ; " + transaction.Currency + " ; " +
                   transaction.Article + " ; " + transaction.Comment + " ; " + transaction.Guid;
        }

        public string Dump(TranWithTags tranWithTags)
        {
            return Convert.ToString(tranWithTags.Timestamp) + " ; " + tranWithTags.Operation + " ; " +
                   tranWithTags.MyAccount + " ; " + tranWithTags.MySecondAccount + " ; " + 
                   tranWithTags.Amount + " ; " + tranWithTags.Currency + " ; " +
                   tranWithTags.AmountInReturn + " ; " + tranWithTags.CurrencyInReturn + " ; " + 
                   Dump(tranWithTags.Tags) + " ; " + tranWithTags.Comment;
        }

        private string Dump(List<Account> tags)
        {
            if (tags == null || tags.Count == 0) return " ";
            var result = tags.First().Name;
            for (int i = 1; i < tags.Count; i++)
            {
                result = result + " | " + tags[i].Name;
            }
            return result;
        }

        public string Dump(BankDepositCalculatingRules rules)
        {
            var result = "";
            if (rules == null) return "0000000 ; 0.0";

            result += rules.IsFactDays ? "1" : "0";
            result += rules.EveryStartDay ? "1" : "0";
            result += rules.EveryFirstDayOfMonth ? "1" : "0";
            result += rules.EveryLastDayOfMonth ? "1" : "0";
            result += rules.IsCapitalized ? "1" : "0";
            result += rules.IsRateFixed ? "1" : "0";
            result += rules.HasAdditionalProcent ? "1" : "0";

            result += " ; ";
            result += rules.AdditionalProcent;

            return result;
        }

        public string Dump(Deposit deposit)
        {
            var startDate = string.Format("{0:dd/MM/yyyy}", deposit.StartDate.Date);
            var finishDate = string.Format("{0:dd/MM/yyyy}", deposit.FinishDate.Date);
            var comment = deposit.Comment == null ? "" : deposit.Comment.Replace("\r\n", "|");

            return deposit.ParentAccount.Id + " ; " + deposit.DepositOffer.Id + " ; " + deposit.AgreementNumber +
                " ; " + startDate + " ; " + finishDate + " ; " + deposit.ShortName + " ; " + comment;
        }

        public string Dump(BankDepositRateLine bankDepositRateLine, int accountId)
        {
            var dateFrom = string.Format("{0:dd/MM/yyyy}", bankDepositRateLine.DateFrom);
            return accountId + " ; " + dateFrom + " ; " + bankDepositRateLine.AmountFrom + " ; " + bankDepositRateLine.AmountTo + " ; " + bankDepositRateLine.Rate;
        }

        public string Dump(BankDepositOffer offer)
        {
            return offer.Id + " ; " + offer.BankAccount.Name + " ; " + offer.DepositTitle + " ; " + offer.Currency + " ; " +
                   Dump(offer.CalculatingRules) + " ; " + offer.Comment;
        }

    }
}