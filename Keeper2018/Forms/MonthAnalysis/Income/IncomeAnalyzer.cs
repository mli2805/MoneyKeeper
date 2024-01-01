using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper2018.BorderedList;
using KeeperDomain;

namespace Keeper2018
{
    public static class IncomeAnalyzer
    {
        public static void CollectIncome(this MonthAnalysisModel model, KeeperDataModel dataModel,
            DateTime startDate, DateTime finishMoment, bool isYearAnalysisMode)
        {
            var list = new ListOfLines(50);
            list.Add("Доходы", FontWeights.Bold, Brushes.Blue);

            var incomeTrans = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Доход
                            && t.Timestamp >= startDate && t.Timestamp <= finishMoment);

            IIncomeForPeriod data = isYearAnalysisMode
                ? dataModel.SortYearIncome(incomeTrans)
                : dataModel.SortMonthIncome(incomeTrans);

            data.Fill(list);
            model.Income = data.GetTotal();

            list.Add("");
            list.Add($"Итого {model.Income:#,0.00} usd", FontWeights.Bold, Brushes.Blue);

            model.IncomeViewModel = new BorderedListViewModel(list);
        }
    }
}