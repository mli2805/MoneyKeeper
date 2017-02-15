using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public class DepositReporter
    {
        public ObservableCollection<string> BuildReportHeader(Deposit deposit)
        {
            var reportHeader = new ObservableCollection<string>();

            if (deposit.CalculationData.CurrentBalance == 0) reportHeader.Add("Депозит закрыт. Остаток 0.\n");
            else
            {
                reportHeader.Add(deposit.FinishDate < DateTime.Today ? "!!! Срок депозита истек !!!" : "Действующий депозит.");
                var balanceString = deposit.CalculationData.CurrentBalanceToString();
                reportHeader.Add($"Остаток на {DateTime.Today:dd/MM/yyyy} составляет {balanceString} \n");
            }

            reportHeader.Add("    Дата             До операции               Приход                Расход                 После     Примечание");
            return reportHeader;
        }

        private string DecimalCurrencyToString(decimal amount, CurrencyCodes currency)
        {
            return currency == CurrencyCodes.BYR ? $"{amount:#,0.00}" : $"{amount:#,0}";
        }
        public ObservableCollection<DepositReportBodyLine> BuildReportBody(Deposit deposit)
        {
            var reportBody = new ObservableCollection<DepositReportBodyLine>();

            var isFirst = true;
            decimal beforeOperation = 0;
            foreach (var operation in deposit.CalculationData.Traffic)
            {
                var line = new DepositReportBodyLine { Day = operation.Timestamp.Date, BeforeOperation =  DecimalCurrencyToString(beforeOperation, deposit.DepositOffer.Currency) };
                if (operation.TransactionType == DepositTransactionTypes.Расход)
                {
                    line.ExpenseColumn = operation.AmountToString();
                    line.Comment = (deposit.CalculationData.CurrentBalance == 0 && operation == deposit.CalculationData.Traffic.Last())
                                     ? "закрытие депозита"
                                     : "частичное снятие";
                }
                else if (operation.TransactionType == DepositTransactionTypes.ОбменРасход)
                {
                    line.ExpenseColumn = operation.AmountToString();
                    line.Comment = "обмен (сдал)";
                }
                else if (operation.TransactionType == DepositTransactionTypes.ОбменДоход)
                {
                    line.IncomeColumn = operation.AmountToString();
                    line.Comment = "обмен (получил)";
                }
                else
                {
                    line.IncomeColumn = operation.AmountToString();
                    if (operation.TransactionType == DepositTransactionTypes.Явнес) line.Comment = isFirst ? "открытие депозита" : "доп взнос";
                    if (operation.TransactionType == DepositTransactionTypes.Проценты) line.Comment = "начисление процентов";
                }
                beforeOperation += (operation.TransactionType == DepositTransactionTypes.Расход ||
                                 operation.TransactionType == DepositTransactionTypes.ОбменРасход) ? -operation.Amount : operation.Amount;
                line.AfterOperation = DecimalCurrencyToString(beforeOperation, deposit.DepositOffer.Currency); 
                //        line.Comment += operation.Comment;
                if (operation.Comment != "") line.Comment = operation.Comment;
                reportBody.Add(line);
                isFirst = false;
            }

            return reportBody;
        }

        public ObservableCollection<string> BuildReportFooter(Deposit deposit)
        {
            var reportFooter = new ObservableCollection<string>();

            if (deposit.DepositOffer.Currency != CurrencyCodes.USD)
                reportFooter.Add(
                    $"Внесено  {deposit.CalculationData.TotalMyInsInUsd:#,0.00}$  причислено проц  {deposit.CalculationData.TotalPercentInUsd:#,0.00}$     девальвация тела  {deposit.CalculationData.CurrentDevaluationInUsd:#,0.00}$     профит с учетом девальв. {deposit.CalculationData.CurrentProfitInUsd:#,0.00}$");
            else
                reportFooter.Add(
                    $"Внесено  {deposit.CalculationData.TotalMyInsInUsd:#,0.00} usd    причислено процентов {deposit.CalculationData.TotalPercentInUsd:#,0.00} usd");

//            if (deposit.CalculationData.CurrentBalance != 0) ReportEstimations(deposit, reportFooter);
            if (deposit.CalculationData.State != DepositStates.Закрыт 
                && deposit.CalculationData.State != DepositStates.Просрочен
                && deposit.CalculationData.CurrentBalance != 0) ReportEstimations(deposit, reportFooter);
            return reportFooter;
        }

        private void ReportEstimations(Deposit deposit, ObservableCollection<string> reportFooter)
        {
            reportFooter.Add(
                $"\nВ этом месяце ожидаются проценты {ProcentPredictionRepresentation(deposit.CalculationData.Estimations.ProcentsInThisMonth, deposit.DepositOffer.Currency, deposit.CalculationData.DailyTable.First(l => l.Date.Date == deposit.CalculationData.Estimations.PeriodForThisMonthPayment.Finish.Date).CurrencyRate, deposit.CalculationData.Estimations.PeriodForThisMonthPayment)}");
            reportFooter.Add(
                $"Всего ожидается процентов {ProcentPredictionRepresentation(deposit.CalculationData.Estimations.ProcentsUpToFinish, deposit.DepositOffer.Currency, deposit.CalculationData.DailyTable.Last().CurrencyRate, deposit.CalculationData.Estimations.PeriodForUpToEndPayment)}");

            if (deposit.DepositOffer.Currency == CurrencyCodes.USD)
                reportFooter.Add(
                    $"\nИтого прогноз по депозиту {deposit.CalculationData.Estimations.ProfitInUsd:#,0.00} usd");
            else
            {
                var totalProcentInUsdString = deposit.CalculationData.DailyTable.Last().CurrencyRate == 0
                    ? "не задан курс"
                    : $"{deposit.CalculationData.TotalPercentInUsd + deposit.CalculationData.Estimations.ProcentsUpToFinish / deposit.CalculationData.DailyTable.Last().CurrencyRate:#,0.00}$";

                reportFooter.Add("\nИтоговый прогноз: ");
                reportFooter.Add(
                    $"    Всего процентов {totalProcentInUsdString}" +
                    $"     девальвация тела  {deposit.CalculationData.CurrentDevaluationInUsd + deposit.CalculationData.Estimations.DevaluationInUsd:#,0.00}$" +
                    $"     профит с учетом девальвации {deposit.CalculationData.Estimations.ProfitInUsd:#,0.00}$");
            }
        }

        private static string ProcentPredictionRepresentation(decimal amount, CurrencyCodes currency, decimal rate, Period period)
        {
            if (amount == 0)
                return currency == CurrencyCodes.USD ? "0 usd" : $"0 {currency.ToString().ToLower()}    ($0)";

            if (currency == CurrencyCodes.USD) return $"(за период {period.ToStringOnlyDates()})  {amount:#,0.00} usd";
            return rate == 0 ? "не задан курс" :
                String.Format("(за период {4})  {0:#,0} {1}   (по курсу {2} = ${3:#,0})", 
                 amount, currency.ToString().ToLower(), (int)rate, amount / rate, period.ToStringOnlyDates());
        }

    }
}
