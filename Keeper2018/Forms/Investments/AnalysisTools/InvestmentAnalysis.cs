using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAnalysis
    {
        private readonly KeeperDataModel _dataModel;

        public InvestmentAnalysis(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Analyze(InvestmentAsset asset, Period period)
        {
            var trs = _dataModel.InvestTranModels
                .Where(t => t.Asset.Ticker == asset.Ticker)
                .ToList();

            var amountBefore = 0;
            decimal sumBefore = 0;
            foreach (var investTranModel in trs)
            {
                if (investTranModel.Timestamp < period.StartDate)
                {
                    switch (investTranModel.InvestOperationType)
                    {
                        case InvestOperationType.BuyBonds:
                        case InvestOperationType.BuyStocks:
                            amountBefore += investTranModel.AssetAmount;
                            sumBefore += investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                            break;
                        case InvestOperationType.SellBonds:
                        case InvestOperationType.SellStocks:
                            amountBefore -= investTranModel.AssetAmount;
                            sumBefore -= investTranModel.CurrencyAmount + investTranModel.CouponAmount;
                            break;
                    }
                }
            }

            // цена и колво (и курсы если не долларовый) на начало периода
            // а также средняя цена единицы актива
            // ( с учетом покупок и продаж до начала периода, а также комиссии за операции)
            // и справочно транзакции до начала периода


            // транзакции в течении периода

            // цена и колво (и курсы если не долларовый) на конец периода

            // накопленный купонный доход
            // выплаченный купонный доход или дивиденды

            // изменение стоимости актива (изменение с учетом курса) за период 
            // изменение стоимости актива (изменение с учетом курса) с момента покупки
            // 
        }
    }
}