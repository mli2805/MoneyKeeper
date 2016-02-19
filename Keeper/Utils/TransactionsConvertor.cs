using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;

namespace Keeper.Utils
{
    [Export]
    public class TransactionsConvertor
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public TransactionsConvertor(KeeperDb db)
        {
            _db = db;
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
                        result.Add(ConvertExchangeTransaction(firstPartOfExchange, transaction));
                }
                else result.Add(ConvertOneTransaction(transaction));
            }
            _db.TransWithTags = result;
        }

        private TranWithTags ConvertExchangeTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            if (firstPartOfExchange.Comment.Contains("cycle")) return ConvertToForexTransaction(firstPartOfExchange, transaction);
            return firstPartOfExchange.Debet != transaction.Credit
                ? ConvertToExchangeWithTransferTransaction(firstPartOfExchange, transaction)
                : ConvertToExchangeTransaction(firstPartOfExchange, transaction);
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
                AmountInReturn = transaction.Amount,
                CurrencyInReturn = transaction.Currency,
                Tags = new List<Account>(),
                Comment = firstPartOfExchange.Comment
            };
            result.Tags.Add(firstPartOfExchange.Credit);
            return result;  
        }

        private TranWithTags ConvertToExchangeWithTransferTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TranWithTags
            {
                Timestamp = firstPartOfExchange.Timestamp,
                Operation = OperationType.ОбменПеренос,
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
            return result;
        }

        private TranWithTags ConvertToForexTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TranWithTags()
            {
                Timestamp = firstPartOfExchange.Timestamp,
                Operation = OperationType.Форекс,
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
