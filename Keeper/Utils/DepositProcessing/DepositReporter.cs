﻿using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.Rates;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public class DepositReporter
    {
        private readonly RateExtractor _rateExtractor;
        [ImportingConstructor]
        public DepositReporter(RateExtractor rateExtractor)
        {
            _rateExtractor = rateExtractor;
        }

        public ObservableCollection<string> BuildReportHeader(Deposit deposit)
        {
            var reportHeader = new ObservableCollection<string>();

            if (deposit.CalculationData.CurrentBalance == 0) reportHeader.Add("Депозит закрыт. Остаток 0.\n");
            else
            {
                reportHeader.Add(deposit.FinishDate < DateTime.Today ? "!!! Срок депозита истек !!!" : "Действующий депозит.");
                var templater = new Templater(deposit.DepositOffer.Currency);
                var balanceString = deposit.DepositOffer.Currency != CurrencyCodes.USD
                                      ? String.Format(templater.ForAmount()+" {2}  ($ {1:#,0.00} )",
                                                      deposit.CalculationData.CurrentBalance,
                                                      deposit.CalculationData.CurrentBalance /
                                                      (decimal)_rateExtractor.GetLastRate(deposit.DepositOffer.Currency),
                                                      deposit.DepositOffer.Currency.ToString().ToLower())
                                      : String.Format("{0:#,0.00} usd", deposit.CalculationData.CurrentBalance);
                reportHeader.Add(String.Format("Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
            }

            reportHeader.Add("    Дата             До операции               Приход                Расход                 После     Примечание");
            return reportHeader;
        }

        public ObservableCollection<DepositReportBodyLine> BuildReportBody(Deposit deposit)
        {
            var reportBody = new ObservableCollection<DepositReportBodyLine>();

            var isFirst = true;
            decimal beforeOperation = 0;
            foreach (var operation in deposit.CalculationData.Traffic)
            {
                var templater = new Templater(operation.Currency);
                var line = new DepositReportBodyLine { Day = operation.Timestamp.Date, BeforeOperation = String.Format(templater.ForAmount(),beforeOperation) };
                if (operation.TransactionType == DepositTransactionTypes.Расход)
                {
                    line.ExpenseColumn = String.Format(templater.ForAmount(), operation.Amount);
                    line.Comment = (deposit.CalculationData.CurrentBalance == 0 && operation == deposit.CalculationData.Traffic.Last())
                                     ? "закрытие депозита"
                                     : "частичное снятие";
                }
                else if (operation.TransactionType == DepositTransactionTypes.ОбменРасход)
                {
                    line.ExpenseColumn = String.Format(templater.ForAmount(), operation.Amount);
                    line.Comment = "обмен (сдал)";
                }
                else if (operation.TransactionType == DepositTransactionTypes.ОбменДоход)
                {
                    line.IncomeColumn = String.Format(templater.ForAmount(), operation.Amount);
                    line.Comment = "обмен (получил)";
                }
                else
                {
                    line.IncomeColumn = String.Format(templater.ForAmount(), operation.Amount);
                    if (operation.TransactionType == DepositTransactionTypes.Явнес) line.Comment = isFirst ? "открытие депозита" : "доп взнос";
                    if (operation.TransactionType == DepositTransactionTypes.Проценты) line.Comment = "начисление процентов";
                }
                beforeOperation += (operation.TransactionType == DepositTransactionTypes.Расход ||
                                 operation.TransactionType == DepositTransactionTypes.ОбменРасход) ? -operation.Amount : operation.Amount;
                line.AfterOperation = String.Format(templater.ForAmount(), beforeOperation); 
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
                reportFooter.Add(String.Format("Внесено  {0:#,0.00}$  причислено проц  {1:#,0.00}$     девальвация тела  {2:#,0.00}$     профит с учетом девальв. {3:#,0.00}$",
                    deposit.CalculationData.TotalMyInsInUsd, deposit.CalculationData.TotalPercentInUsd, 
                    deposit.CalculationData.CurrentDevaluationInUsd, deposit.CalculationData.CurrentProfitInUsd));
            else
                reportFooter.Add(String.Format("Внесено  {0:#,0.00} usd    причислено процентов {1:#,0.00} usd",
                    deposit.CalculationData.TotalMyInsInUsd, deposit.CalculationData.TotalPercentInUsd));

//            if (deposit.CalculationData.CurrentBalance != 0) ReportEstimations(deposit, reportFooter);
            if (deposit.CalculationData.State != DepositStates.Закрыт 
                && deposit.CalculationData.State != DepositStates.Просрочен
                && deposit.CalculationData.CurrentBalance != 0) ReportEstimations(deposit, reportFooter);
            return reportFooter;
        }

        private void ReportEstimations(Deposit deposit, ObservableCollection<string> reportFooter)
        {
            reportFooter.Add(String.Format("\nВ этом месяце ожидаются проценты {0}", ProcentPredictionRepresentation(
                deposit.CalculationData.Estimations.ProcentsInThisMonth, 
                deposit.DepositOffer.Currency, 
                deposit.CalculationData.DailyTable.First
                 (l => l.Date.Date == deposit.CalculationData.Estimations.PeriodForThisMonthPayment.Finish.Date).CurrencyRate,
                deposit.CalculationData.Estimations.PeriodForThisMonthPayment)));
            reportFooter.Add(String.Format("Всего ожидается процентов {0}", ProcentPredictionRepresentation(
                deposit.CalculationData.Estimations.ProcentsUpToFinish,
                deposit.DepositOffer.Currency,
                deposit.CalculationData.DailyTable.Last().CurrencyRate,
                deposit.CalculationData.Estimations.PeriodForUpToEndPayment)));

            if (deposit.DepositOffer.Currency == CurrencyCodes.USD)
                reportFooter.Add(String.Format("\nИтого прогноз по депозиту {0:#,0.00} usd",
                    deposit.CalculationData.Estimations.ProfitInUsd));
            else
            {
                reportFooter.Add(String.Format("\nИтоговый прогноз: "));
                reportFooter.Add(String.Format("    Всего процентов {0:#,0.00}$     девальвация тела  {1:#,0.00}$     профит с учетом девальвации {2:#,0.00}$", 
                    deposit.CalculationData.TotalPercentInUsd + deposit.CalculationData.Estimations.ProcentsUpToFinish / deposit.CalculationData.DailyTable.Last().CurrencyRate,
                    deposit.CalculationData.CurrentDevaluationInUsd + deposit.CalculationData.Estimations.DevaluationInUsd,
                    deposit.CalculationData.Estimations.ProfitInUsd
                    ));
            }
        }

        private static string ProcentPredictionRepresentation(decimal amount, CurrencyCodes currency, decimal rate, Period period)
        {
            if (amount == 0)
                return currency == CurrencyCodes.USD ? "0 usd" : String.Format("0 {0}    ($0)", currency.ToString().ToLower());

            if (currency == CurrencyCodes.USD) return String.Format("(за период {0})  {1:#,0.00} usd", period.ToStringOnlyDates(), amount);
            return String.Format("(за период {4})  {0:#,0} {1}   (по курсу {2} = ${3:#,0})", 
                 amount, currency.ToString().ToLower(), (int)rate, amount / rate, period.ToStringOnlyDates());
        }

    }
}