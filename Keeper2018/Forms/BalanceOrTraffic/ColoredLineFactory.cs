using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class ColoredLineFactory
    {
        public static ListLine ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $@"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign)}";
            return new ListLine($"  {shortLine}   {GetPp(tran)} {tran.Comment}", GetColor(tran, sign))
            {
                TooltipLines = dataModel.BuildTooltip(tran)
            };
        }

        private static string GetPp(TransactionModel tran)
        {
            if (tran.Tags.Any(t => t.Id == 208)) return "%%";
            if (tran.Tags.Any(t => t.Id == 701)) return "манибэк";
            return "";
        }

        public static ListLine ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign, out decimal inUsd)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign, out inUsd)}";
            return new ListLine($"  {shortLine}   {tran.Comment}", GetColor(tran, sign))
            {
                TooltipLines = dataModel.BuildTooltip(tran)
            };
        }

        public static ListLine ColoredLineOneAccountExchange(this KeeperDataModel dataModel, TransactionModel tran)
        {
            var minus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount * -1)}";
            var plus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)}";
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {minus} -> {plus}";
            return new ListLine($"  {shortLine}   {tran.Comment}", Brushes.Black)
            {
                TooltipLines = dataModel.BuildTooltip(tran)
            };
        }

        private static Brush GetColor(TransactionModel tran, int sign)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return Brushes.Blue;
                case OperationType.Расход:
                    return Brushes.Red;
                case OperationType.Обмен:
                    return Brushes.DarkGreen;
                default:
                    return sign == 1 ? Brushes.DarkBlue : Brushes.DarkRed;
            }
        }

        private static List<TransactionTooltipLine> BuildTooltip(this KeeperDataModel dataModel, TransactionModel tran)
        {
            var result = new List<TransactionTooltipLine> {
                new TransactionTooltipLine("Когда: ", tran.Timestamp.ToString("dd-MM-yyyy HH:mm"))
            };

            if (tran.Operation == OperationType.Перенос || tran.Operation == OperationType.Обмен)
            {
                result.Add(new TransactionTooltipLine($"{tran.Operation} с:", tran.MyAccount.Name));
                result.Add(new TransactionTooltipLine(" на:", tran.MySecondAccount.Name));
            }
            else
            {
                result.Add(new TransactionTooltipLine(
                    tran.Operation == OperationType.Доход ? "На:" : "С: ", tran.MyAccount.Name));
                result.Add(new TransactionTooltipLine(
                    tran.Operation == OperationType.Доход ? "Кто:" : "Кому:", tran.Counterparty.Name));
                result.Add(new TransactionTooltipLine("За что: ", tran.Category.Name));
            }

            if (tran.Operation == OperationType.Обмен)
                result.Add(new TransactionTooltipLine("", GetRealExchangeRate(tran)));

            result.Add(new TransactionTooltipLine(tran.Operation == OperationType.Обмен ? "Сдал" : "Сколько: ",
                dataModel.AmountWithUsdAndRate(tran.Timestamp, tran.Currency, tran.Amount)));

            if (tran.Operation == OperationType.Обмен)
                result.Add(new TransactionTooltipLine("Получил: ",
                    dataModel.AmountWithUsdAndRate(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)));

            if (tran.Tags.Any())
                result.Add(new TransactionTooltipLine("Тэги:",
                    string.Join($"{Environment.NewLine}", tran.Tags.Select(t => t.Name))));

            result.Add(new TransactionTooltipLine("Коментарий: ", tran.Comment));
            return result;
        }

        private static string GetRealExchangeRate(TransactionModel tran)
        {
            decimal exchangeRate;
            string currencies;
            if (tran.Amount > tran.AmountInReturn)
            {
                exchangeRate = tran.Amount / tran.AmountInReturn;
                currencies = $"{tran.Currency.ToString().ToLower()}/{tran.CurrencyInReturn.ToString().ToLower()}";
            }
            else
            {
                exchangeRate = tran.AmountInReturn / tran.Amount;
                currencies = $"{tran.CurrencyInReturn.ToString().ToLower()}/{tran.Currency.ToString().ToLower()}";
            }
            return $" (Курс обмена {exchangeRate:F5} {currencies})";
        }
    }
}