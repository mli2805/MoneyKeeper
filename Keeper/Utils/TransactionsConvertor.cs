using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.Utils.AccountEditing;

namespace Keeper.Utils
{
    [Export]
    public class TransactionsConvertor
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;


        [ImportingConstructor]
        public TransactionsConvertor(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
        }

        public void Convert()
        {
            var result = new ObservableCollection<TranWithTags>();
            Transaction firstPartOfExchange = null;
            foreach (var transaction in _db.Transactions)
            {
                if (transaction.IsExchange())
                {
                    if (transaction.Operation == OperationType.Расход)
                        firstPartOfExchange = transaction;
                    else
                        result.Add(ConvertToExchangeTransaction(firstPartOfExchange, transaction));
                }
                else result.Add(ConvertOneTransaction(transaction));
            }
            _db.TransWithTags = result;
        }

        private TranWithTags ConvertToExchangeTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TranWithTags()
            {
                Timestamp = firstPartOfExchange.Timestamp,
                Operation = OperationType.Обмен,
                MyAccount = firstPartOfExchange.Debet,
                Amount = firstPartOfExchange.Amount,
                Currency = firstPartOfExchange.Currency,
                MySecondAccount = transaction.Credit,
                AmountInReturn = transaction.Amount,
                CurrencyInReturn = transaction.Currency,
                Tags = new List<Account>(),
                Comment = firstPartOfExchange.Comment
            };
            result.Tags.Add(firstPartOfExchange.Credit);
            if (firstPartOfExchange.Comment != null && firstPartOfExchange.Comment.Contains("cycle"))
                result.Tags.Add(_accountTreeStraightener.Seek("Форекс",_db.Accounts));
            return result;  
        }


        private TranWithTags ConvertOneTransaction(Transaction transaction)
        {
            switch (transaction.Operation)
            {
                case OperationType.Доход: return ConvertToIncomeTransaction(transaction);
                case OperationType.Расход: return ConvertToOutcomeTransaction(transaction);
                case OperationType.Перенос: return ConvertToTransferTransaction(transaction);
                default: return null;
            }
        }

        private TranWithTags ConvertToTransferTransaction(Transaction transaction)
        {
            var result = new TranWithTags()
            {
                Timestamp = transaction.Timestamp,
                Operation = OperationType.Перенос,
                MyAccount = transaction.Debet,
                MySecondAccount = transaction.Credit,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Tags = new List<Account>(),
                Comment = transaction.Comment
            };
            if (transaction.Comment.Contains("cycle"))
                result.Tags.Add(_accountTreeStraightener.Seek("Форекс", _db.Accounts));
            return result;
        }

        private TranWithTags ConvertToOutcomeTransaction(Transaction transaction)
        {
            var result = new TranWithTags()
            {
                Timestamp = transaction.Timestamp,
                Operation = OperationType.Расход,
                MyAccount = transaction.Debet,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Tags = new List<Account>(),
                Comment = transaction.Comment
            };
            result.Tags.Add(transaction.Credit);
            result.Tags.Add(transaction.Article);
            return result;
        }

        private TranWithTags ConvertToIncomeTransaction(Transaction transaction)
        {
            var result = new TranWithTags()
            {
                Timestamp = transaction.Timestamp,
                Operation = OperationType.Доход,
                MyAccount = transaction.Credit,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Tags = new List<Account>(),
                Comment = transaction.Comment
            };
            result.Tags.Add(transaction.Debet);
            result.Tags.Add(transaction.Article);
            return result;
        }
    }
}
