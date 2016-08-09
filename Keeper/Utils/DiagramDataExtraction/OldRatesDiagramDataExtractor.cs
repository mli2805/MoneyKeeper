using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating.Ilya;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class OldRatesDiagramDataExtractor
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public OldRatesDiagramDataExtractor(KeeperDb db)
        {
            _db = db;
        }


        public Dictionary<DateTime, MoneyBag> DepositBalancesForPeriodInCurrencies(Period period)
        {
            var result = new Dictionary<DateTime, MoneyBag>();
            var currentMoneyBag = new MoneyBag();
            var currentDate = period.Start;

            foreach (var tran in _db.TransWithTags)
            {
                if (currentDate != tran.Timestamp.Date)
                {
                    result.Add(currentDate, currentMoneyBag);
                    currentDate = tran.Timestamp.Date;
                }

                currentMoneyBag = currentMoneyBag + ProcessOneTran(tran);
            }
            return result;
        }

        private MoneyBag ProcessOneTran(TranWithTags tran)
        {
            var result = new MoneyBag();
            if (tran.MyAccount.IsDeposit()) result = tran.MoneyBagForAccount(tran.MyAccount);
            if (tran.MySecondAccount != null && tran.MySecondAccount.IsDeposit()) result = result + tran.MoneyBagForAccount(tran.MySecondAccount);
            return result;
        }


    }
}
