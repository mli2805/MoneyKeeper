using System.Collections.Generic;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Transactions
{
    public static class TranExtentions
    {
        public static TranWithTags Clone(this TranWithTags tran)
        {
            var result = new TranWithTags
            {
                Timestamp = tran.Timestamp,
                Operation = tran.Operation,
                MyAccount = tran.MyAccount,
                MySecondAccount = tran.MySecondAccount,
                Amount = tran.Amount,
                Currency = tran.Currency,
                AmountInReturn = tran.AmountInReturn,
                CurrencyInReturn = tran.CurrencyInReturn
            };
            if (tran.Tags != null)
            {
                result.Tags = new List<Account>();
                foreach (var tag in tran.Tags)
                {
                    result.Tags.Add(tag);
                }
            }
            result.Comment = tran.Comment;
            return result;
        }

        public static Brush TranFontColor(this TranWithTags tran)
        {
            if (tran.Operation == OperationType.Доход) return Brushes.Blue;
            if (tran.Operation == OperationType.Расход) return Brushes.Red;
            if (tran.Operation == OperationType.Перенос) return Brushes.Black;
            if (tran.Operation == OperationType.Обмен) return Brushes.Green;
            return Brushes.Gray;
        }
    }
}
