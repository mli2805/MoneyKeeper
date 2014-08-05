using System;
using System.Composition;
using System.Globalization;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
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
            return rate.BankDay.ToString(new CultureInfo("ru-Ru")) + " ; " +
                   rate.Currency + " ; " +
                   Math.Round(rate.Rate, 4);
        }
        public string Dump(Transaction transaction)
        {
            var s = Convert.ToString(transaction.Timestamp, new CultureInfo("ru-Ru")) + " ; " + transaction.Operation + " ; " +
                    transaction.Debet + " ; " + transaction.Credit + " ; " + transaction.Amount + " ; " + transaction.Currency + " ; " +
                    transaction.Amount2 + " ; ";

            if (transaction.Currency2 == null || transaction.Currency2 == 0) s = s + "null";
            else s = s + transaction.Currency2;

            s = s + " ; " + transaction.Article + " ; " + transaction.Comment;
            return s;
        }

        public string Dump(BankDepositCalculatingRules rules)
        {
            var result = "";
            if (rules == null) return "000000";

            result += rules.IsFactDays ? "1" : "0";
            result += rules.OnlyAtTheEnd ? "1" : "0";
            result += rules.EveryStartDay ? "1" : "0";
            result += rules.EveryFirstDayOfMonth ? "1" : "0";
            result += rules.EveryLastDayOfMonth ? "1" : "0";
            result += rules.IsCapitalized ? "1" : "0";

            return result;
        }

        public string Dump(Deposit deposit)
        {
            var startDate = string.Format("{0:dd/MM/yyyy}", deposit.StartDate.Date);
            var finishDate = string.Format("{0:dd/MM/yyyy}", deposit.FinishDate.Date);
            var comment = deposit.Comment == null ? "" : deposit.Comment.Replace("\r\n", "|");

            return deposit.ParentAccount.Id + " ; " + deposit.DepositOffer.Id + " ; " + deposit.AgreementNumber +
                " ; " + startDate + " ; " + finishDate + " ; " + comment;
        }

        public string Dump(DepositRateLine depositRateLine, int accountId)
        {
            var dateFrom = string.Format("{0:dd/MM/yyyy}", depositRateLine.DateFrom);
            return accountId + " ; " + dateFrom + " ; " + depositRateLine.AmountFrom + " ; " + depositRateLine.AmountTo + " ; " + depositRateLine.Rate;
        }

        public string Dump(BankDepositOffer offer)
        {
            return offer.Id + " ; " + offer.BankAccount.Name + " ; " + offer.DepositTitle + " ; " + offer.Currency + " ; " +
                   Dump(offer.CalculatingRules) + " ; " + offer.Comment;
        }

    }
}