using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public class DepositTrafficExtractor
    {
        private readonly KeeperDb _db;
        private Deposit _deposit;

        [ImportingConstructor]
        public DepositTrafficExtractor(KeeperDb db)
        {
            _db = db;
        }

        public Deposit ExtractTraffic(Account account) // используется при месячном анализе и выгрузке в excel
        {
            _deposit = account.Deposit;
            _deposit.CalculationData = IoC.Get<DepositCalculationData>();
            var temp = Extract().ToList();
            temp.AddRange(ExtractSecondPart());
            _deposit.CalculationData.Traffic = temp.OrderBy(t => t.Timestamp).ToList();
            _deposit.CalculationData.CurrentCurrency = _deposit.CalculationData.Traffic.Any() 
                ? _deposit.CalculationData.Traffic.Last().Currency
                : _deposit.DepositOffer.Currency;
            return _deposit;
        }

//        private List<DepositTransaction> Extract()
//        {
//            return (from t in _db.TransWithTags
//                    where 
//                        t.MyAccount == _deposit.ParentAccount 
//                    orderby t.Timestamp
//                    join r in _db.CurrencyRates on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals new { r.BankDay.Date, r.Currency } into g
//                    from rate in g.DefaultIfEmpty()
//                    select new DepositTransaction
//                    {
//                        Amount = t.Amount,
//                        Timestamp = t.Timestamp,
//                        Currency = t.Currency.GetValueOrDefault(),
//                        Counteragent = GetDepositCounteragent(t),
//                        Comment = t.Comment != "" ? t.Comment : t.Operation == OperationType.Обмен ? "снятие с обменом валюты" : t.Operation == OperationType.Доход ? "проценты" : "частичное снятие",
//                        AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
//                        TransactionType = GetDepositOperationType(t, _deposit.ParentAccount)
//                    }).ToList();
//        }

        private IEnumerable<DepositTransaction> Extract()
        {
            var trans = _db.TransWithTags.Where(t => t.MyAccount == _deposit.ParentAccount).ToList();
            foreach (var tran in trans)
            {
                var rate = _db.CurrencyRates.FirstOrDefault(r =>
                    r.BankDay.Date == tran.Timestamp.Date && r.Currency == tran.Currency);
                var depoTran = new DepositTransaction()
                {
                    Amount = tran.Amount,
                    Timestamp = tran.Timestamp,
                    Currency = tran.Currency.GetValueOrDefault(),
                    Counteragent = GetDepositCounteragent(tran),
                    Comment = tran.Comment != "" 
                        ? tran.Comment 
                        : tran.Operation == OperationType.Обмен 
                            ? "снятие с обменом валюты" 
                            : tran.Operation == OperationType.Доход 
                                ? "проценты" 
                                : "частичное снятие",
                    AmountInUsd = rate != null ? tran.Amount / (decimal)rate.Rate : tran.Amount,
                    TransactionType = GetDepositOperationType(tran, _deposit.ParentAccount)
                };
                yield return depoTran;
            }
        }

//        private IEnumerable<DepositTransaction> ExtractSecondPart()
//        {
//            return from t in _db.TransWithTags
//                   where
//                       (t.MySecondAccount != null && t.MySecondAccount == _deposit.ParentAccount)
//                   orderby t.Timestamp
//                   join r in _db.CurrencyRates on new { t.Timestamp.Date, Currency = t.CurrencyInReturn.GetValueOrDefault() } equals
//                       new { r.BankDay.Date, r.Currency } into g
//                   from rate in g.DefaultIfEmpty()
//                   select new DepositTransaction
//                   {
//                       Amount = t.Operation == OperationType.Обмен ? t.AmountInReturn : t.Amount,
//                       Timestamp = t.Timestamp,
//                       Currency = t.CurrencyInReturn.GetValueOrDefault(),
//                       Counteragent = GetDepositCounteragent(t),
//                       Comment = t.Operation == OperationType.Обмен ? "обмен с пополнением депозита" : "пополнение депозита",
//                       AmountInUsd = rate != null ? t.AmountInReturn / (decimal)rate.Rate : t.AmountInReturn,
//                       TransactionType = DepositTransactionTypes.Явнес,
//                   };
//        }

        private IEnumerable<DepositTransaction> ExtractSecondPart()
        {
            var trans = _db.TransWithTags.Where(t => t.MySecondAccount == _deposit.ParentAccount).ToList();
            foreach (var tran in trans)
            {
                var currency = tran.Operation == OperationType.Перенос ? tran.Currency : tran.CurrencyInReturn;
                var rate = _db.CurrencyRates.FirstOrDefault(r =>
                    r.BankDay.Date == tran.Timestamp.Date && r.Currency == currency);
                var rateValue = rate == null ? 1 : rate.Rate;
                var depoTran = new DepositTransaction()
                {
                    Amount = tran.Operation == OperationType.Обмен ? tran.AmountInReturn : tran.Amount,
                    Timestamp = tran.Timestamp,
                    Currency = tran.CurrencyInReturn.GetValueOrDefault(),
                    Counteragent = GetDepositCounteragent(tran),
                    Comment = tran.Comment != ""
                        ? tran.Comment
                        : tran.Operation == OperationType.Обмен
                            ? "обмен с пополнением депозита" 
                            : "пополнение депозита",
                    AmountInUsd = tran.Operation == OperationType.Перенос ? tran.Amount / (decimal)rateValue : tran.AmountInReturn / (decimal)rateValue,
                    TransactionType = DepositTransactionTypes.Явнес,
                };
                yield return depoTran;
            }
        }

        private DepositTransactionTypes GetDepositOperationType(TranWithTags t, Account depositAccount)
        {
            return t.Operation == OperationType.Доход
                     ? DepositTransactionTypes.Проценты
                     : t.MyAccount == depositAccount
                         ? DepositTransactionTypes.Расход
                         : DepositTransactionTypes.Явнес;
        }

        private string GetDepositCounteragent(TranWithTags t)
        {
            return t.Operation == OperationType.Доход ?
                "банк" :
                t.MyAccount == _deposit.ParentAccount ?
                         t.MySecondAccount == null ? "" : t.MySecondAccount.Name
                         : t.MyAccount.Name;
        }
    }
}
