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

        public ObservableCollection<TrBase> Convert()
        {
            var result = new ObservableCollection<TrBase>();
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
            return result;
        }

        private TrBase ConvertExchangeTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            if (firstPartOfExchange.Comment.Contains("cycle")) return ConvertToForexTransaction(firstPartOfExchange, transaction);
            return firstPartOfExchange.Debet != transaction.Credit
                ? ConvertToExchangeWithTransferTransaction(firstPartOfExchange, transaction)
                : ConvertToExchangeTransaction(firstPartOfExchange, transaction);
        }

        private TrExchange ConvertToExchangeTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TrExchange()
            {
                Timestamp = firstPartOfExchange.Timestamp,
                MyAccount = firstPartOfExchange.Debet,
                Amount = (double)firstPartOfExchange.Amount,
                Currency = firstPartOfExchange.Currency,
                AmountInReturn = (double)transaction.Amount,
                CurrencyInReturn = transaction.Currency,
                Tags = new List<Account>(),
                Comment = firstPartOfExchange.Comment
            };
            result.Tags.Add(firstPartOfExchange.Credit);
            return result;  
        }

        private TrExchangeWithTransfer ConvertToExchangeWithTransferTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TrExchangeWithTransfer
            {
                Timestamp = firstPartOfExchange.Timestamp,
                MyAccount = firstPartOfExchange.Debet,
                Amount = (double)firstPartOfExchange.Amount,
                Currency = firstPartOfExchange.Currency,

                MySecondAccount = transaction.Credit,

                AmountInReturn = (double)transaction.Amount,
                CurrencyInReturn = transaction.Currency,
                Tags = new List<Account>(),
                Comment = firstPartOfExchange.Comment
            };
            result.Tags.Add(firstPartOfExchange.Credit);
            return result;
        }

        private TrForex ConvertToForexTransaction(Transaction firstPartOfExchange, Transaction transaction)
        {
            var result = new TrForex()
            {
                Timestamp = firstPartOfExchange.Timestamp,
                MyAccount = firstPartOfExchange.Debet,
                Amount = (double)firstPartOfExchange.Amount,
                Currency = firstPartOfExchange.Currency,

                MySecondAccount = transaction.Credit,

                AmountInReturn = (double)transaction.Amount,
                CurrencyInReturn = transaction.Currency,
                Tags = new List<Account>(),
                Comment = firstPartOfExchange.Comment
            };
            result.Tags.Add(firstPartOfExchange.Credit);
            return result;
        }

        private TrBase ConvertOneTransaction(Transaction transaction)
        {
            switch (transaction.Operation)
            {
                case OperationType.Доход: return ConvertToIncomeTransaction(transaction);
                case OperationType.Расход: return ConvertToOutcomeTransaction(transaction);
                case OperationType.Перенос: return ConvertToTransferTransaction(transaction);
                default: return null;
            }
        }

        private TrTransfer ConvertToTransferTransaction(Transaction transaction)
        {
            var result = new TrTransfer()
            {
                Timestamp = transaction.Timestamp,
                MyAccount = transaction.Debet,
                MySecondAccount = transaction.Credit,
                Amount = (double)transaction.Amount,
                Currency = transaction.Currency,
                Tags = new List<Account>(),
                Comment = transaction.Comment
            };
            return result;
        }

        private TrOutcome ConvertToOutcomeTransaction(Transaction transaction)
        {
            var result = new TrOutcome()
            {
                Timestamp = transaction.Timestamp,
                MyAccount = transaction.Debet,
                Amount = (double)transaction.Amount,
                Currency = transaction.Currency,
                Tags = new List<Account>(),
                Comment = transaction.Comment
            };
            result.Tags.Add(transaction.Credit);
            result.Tags.Add(transaction.Article);
            return result;
        }

        private TrIncome ConvertToIncomeTransaction(Transaction transaction)
        {
            var result = new TrIncome()
            {
                Timestamp = transaction.Timestamp,
                MyAccount = transaction.Credit,
                Amount = (double)transaction.Amount,
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
