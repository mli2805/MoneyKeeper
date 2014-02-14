﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
  public enum DepositOperations
  {
    Явнес,
    Проценты,
    Расход,
  }

  public class DepositTransaction
  {
    public DateTime Timestamp { get; set; }
    public DepositOperations TransactionType { get; set; }
    public Decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Decimal AmountInUsd { get; set; }
    public string Comment { get; set; }
  }

  [Serializable]
  public class DepositRateLine
  {
    public decimal AmountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal Rate { get; set; }
  }

  [Serializable]
	public class Deposit
	{
    public Account ParentAccount { get; set; }
    public Account Bank { get; set; }
    public string Title { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime FinishDate { get; set; }
		public CurrencyCodes Currency { get; set; }

    public decimal DepositRate { get; set; }
    public List<DepositRateLine> DepositRateLines { get; set; }

    [NonSerialized]
    private DepositEvaluations _evaluations;
    public DepositEvaluations Evaluations
    {
      get { return _evaluations; }
      set { _evaluations = value; }
    }
	}

  public class DepositEvaluations
  {
    public DepositStates State { get; set; }
    public List<DepositTransaction> Traffic { get; set; }
    public decimal TotalMyIns { get; set; }
    public decimal TotalPercent { get; set; }
    public decimal TotalMyOuts { get; set; }
    public decimal CurrentBalance { get { return TotalMyIns + TotalPercent - TotalMyOuts; } }
    public decimal CurrentProfit { get; set; }

    public decimal EstimatedProcents { get; set; }
    public decimal EstimatedProfitInUsd { get; set; }

    public Brush FontColor { get { return State == DepositStates.Закрыт ? Brushes.Gray : State == DepositStates.Просрочен ? Brushes.Red : Brushes.Blue; } }
  }
}
