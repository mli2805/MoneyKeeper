using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Trans;
using Keeper.Utils.Common;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [Export]
    public class DbClassesInstanceDumper : IDbClassesInstanceDumper
    {
        public string Dump(HierarchyItem<Account> account)
        {
            var shiftedName = new string(' ', account.Level * 2) + account.Item.Name;
            var parentForDump = account.Item.Parent?.Id ?? 0;
            return account.Item.Id + " ; " + shiftedName + " ; " + parentForDump + " ; " +
              account.Item.IsFolder + " ; " + account.Item.IsClosed + " ; " + account.Item.IsExpanded;
        }
        public string Dump(ArticleAssociation association)
        {
            return association.ExternalAccount.Id + " ; " +
                   association.AssociatedArticle.Id + " ; " +
                   association.OperationType + " ; " +
                   association.IsTwoWay;
        }
        public string Dump(CurrencyRate rate)
        {
            return rate.BankDay.ToString("dd/MM/yyyy") + " ; " +
                   rate.Currency + " ; " + rate.Rate.ToString(new CultureInfo("en-US"));
        }
        public string Dump(NbRate rate)
        {
            return rate.Date.ToString("dd/MM/yyyy") + " ; " +
                   rate.UsdRate.ToString(new CultureInfo("en-US")) + " ; " + rate.EurRate.ToString(new CultureInfo("en-US")) + " ; " + rate.RurRate.ToString(new CultureInfo("en-US"));
        }
        public string Dump(TranWithTags tranWithTags)
        {
            return tranWithTags.Timestamp.ToString("dd/MM/yyyy") + " ; " + tranWithTags.Operation + " ; " + tranWithTags.ReceiptId + " ; " +
                   tranWithTags.MyAccount.Id + " ; " + tranWithTags.DumpOfSecondAccount() + " ; " +
                   tranWithTags.Amount.ToString(new CultureInfo("en-US")) + " ; " + tranWithTags.Currency + " ; " +
                   tranWithTags.AmountInReturn.ToString(new CultureInfo("en-US")) + " ; " + tranWithTags.CurrencyInReturn + " ; " +
                   Dump(tranWithTags.Tags, tranWithTags.Timestamp) + " ; " + tranWithTags.Comment;
        }

        private string Dump(List<Account> tags, DateTime timestamp)
        {
            if (tags == null || tags.Count == 0) return " ";
            string result = "";
            for (int i = 0; i < tags.Count; i++)
            {
//                result = result + TransformTagName(tags[i].Name, timestamp) + " | ";

                var tagId = tags[i].Id == 286 ? 241 : tags[i].Id; // АСБ Кредит ЖСК
                result = result + tagId + " | ";
            }
            result = result.Substring(0, result.Length - 3);
            return result;
        }

//        private string TransformTagName(string oldName, DateTime timestamp)
//        {
//            if (oldName == "Подаркополучатели") return "Родственники";
//
//            if (oldName == "На карманные расходы" || oldName == "На мероприятия") return "Чилдрены";
//
//            if (oldName == "Авто топливо")
//                return timestamp < new DateTime(2014, 04, 19) ? "Scenic2 - авто топливо" : "Scenic3 - авто топливо";
//            if (oldName == "Ремонт авто")
//                return timestamp < new DateTime(2014, 04, 19) ? "Scenic2 - авто ремонт" : "Scenic3 - авто ремонт";
//            if (oldName == "Обслуживание авто")
//                return timestamp < new DateTime(2014, 04, 19) ? "Scenic2 - авто обслуживание" : "Scenic3 - авто обслуживание";
//            if (oldName == "Авторасходы до 05/2010")
//            {
//                if (timestamp < new DateTime(2006, 10, 7))
//                    return "Golf - авто несортированные";
//                if (timestamp < new DateTime(2009, 4, 16))
//                    return "Passat - авто несортированные";
//                return "Scenic2 - авто несортированные";
//            }
//            return oldName;
//        }

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
            var startDate = $"{deposit.StartDate.Date:dd/MM/yyyy}";
            var finishDate = $"{deposit.FinishDate.Date:dd/MM/yyyy}";
            var comment = deposit.Comment?.Replace("\r\n", "|") ?? "";

            return deposit.ParentAccount.Id + " ; " + deposit.DepositOffer.Id + " ; " + deposit.AgreementNumber +
                " ; " + startDate + " ; " + finishDate + " ; " + deposit.ShortName + " ; " + comment;
        }

        public string Dump(BankDepositRateLine bankDepositRateLine, int accountId)
        {
            var dateFrom = $"{bankDepositRateLine.DateFrom:dd/MM/yyyy}";
            return accountId + " ; " + dateFrom + " ; " + bankDepositRateLine.AmountFrom.ToString(new CultureInfo("en-US")) + " ; "
                 + bankDepositRateLine.AmountTo.ToString(new CultureInfo("en-US")) + " ; " + bankDepositRateLine.Rate.ToString(new CultureInfo("en-US"));
        }

        public string Dump(BankDepositOffer offer)
        {
            return offer.Id + " ; " + offer.BankAccount.Id + " ; " + offer.DepositTitle + " ; " + offer.Currency + " ; " +
                   Dump(offer.CalculatingRules) + " ; " + offer.Comment;
        }

    }
}