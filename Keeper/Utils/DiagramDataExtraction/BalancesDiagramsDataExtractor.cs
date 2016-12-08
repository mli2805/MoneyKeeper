using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.DiagramDomainModel;
using Keeper.Utils.Rates;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class BalancesDiagramsDataExtractor
    {
        private readonly KeeperDb _db;
        
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public BalancesDiagramsDataExtractor(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            
            _rateExtractor = rateExtractor;
        }
        /// <summary>
        /// довольно медленно
        /// может _ratesAsDictionary позволит ускорить?
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public List<DiagramPoint> GetBalances(Every frequency)
        {
            var root = _db.SeekAccount("Мои");
            var result = new List<DiagramPoint>();
            var currentDate = new DateTime(2001, 12, 31); // считать надо всегда с самого начала, иначе остаток неправильный будет
            var currentMoneyBag = new MoneyBag();

            foreach (var tran in _db.TransWithTags)
            {
                while (currentDate < tran.Timestamp.Date)
                {
                    if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency))
                    {
                        var sum = _rateExtractor.GetUsdEquivalent(currentMoneyBag, tran.Timestamp.Date);
                        if (sum != 0) // если вернулся 0 - это гэпы без курсов в начале времен
                            result.Add(new DiagramPoint(currentDate, (double)sum));
                        else
                        {
                            var lastSum = result.Last().CoorYdouble;
                            result.Add(new DiagramPoint(currentDate, lastSum));
                        }
                    }
                    currentDate = currentDate.AddDays(1);

                }
                currentMoneyBag = currentMoneyBag + tran.MoneyBagForAccount(root);
            }
            result.Add(new DiagramPoint(currentDate, (double)_rateExtractor.GetUsdEquivalent(currentMoneyBag, currentDate)));
            return result;
        }

        public List<DiagramPoint> MonthlyResults()
        {
            var result = new List<DiagramPoint>();
            var balances = GetBalances(Every.Month);

            for (var i = 1; i < balances.Count; i++)
            {
                result.Add(new DiagramPoint(balances[i].CoorXdate, balances[i].CoorYdouble - balances[i - 1].CoorYdouble));
            }
            return result;
        }
    }
}