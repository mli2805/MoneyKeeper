﻿using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Deposits;

namespace Keeper.ViewModels
{
    [Export]
    public class DepositViewModel : Screen
    {
        private readonly DepositCalculator _depositCalculator;
        private readonly DepositReporter _depositReporter;
        private readonly DepositExcelReporter _depositExcelReporter;
        private bool _canRenew;

        public IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

        public Deposit Deposit { get; set; }
        public ObservableCollection<string> ReportHeader { get; set; }
        public ObservableCollection<DepositReportBodyLine> ReportBody { get; set; }
        public ObservableCollection<string> ReportFooter { get; set; }
        public Account NewAccountForDeposit { get; set; }
        public bool CanRenew
        {
            get { return _canRenew; }
            set
            {
                if (value.Equals(_canRenew)) return;
                _canRenew = value;
                NotifyOfPropertyChange(() => CanRenew);
            }
        }

        [ImportingConstructor]
        public DepositViewModel(DepositCalculator depositCalculator, 
            DepositReporter depositReporter, DepositExcelReporter depositExcelReporter)
        {
            _depositCalculator = depositCalculator;
            _depositReporter = depositReporter;
            _depositExcelReporter = depositExcelReporter;
            NewAccountForDeposit = null;
        }

        public void SetAccount(Account account)
        {
            Deposit = account.Deposit;
            ReportHeader = _depositReporter.BuildReportHeader(Deposit);
            ReportBody = _depositReporter.BuildReportBody(Deposit);
            ReportFooter = _depositReporter.BuildReportFooter(Deposit);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Deposit.ParentAccount.Name;
            CanRenew = Deposit.CalculationData.State != DepositStates.Закрыт;
        }

        public void ExtractEvaluationsToExcel()
        {
            _depositExcelReporter.Run(Deposit);
        }

        public void Renew()
        {
            var renewDepositViewModel = IoC.Get<RenewDepositViewModel>();
            renewDepositViewModel.SetOldDeposit(Deposit);
            WindowManager.ShowDialog(renewDepositViewModel);
            if (renewDepositViewModel.NewDeposit != null)
            {
                NewAccountForDeposit = renewDepositViewModel.NewDeposit;
                CanRenew = Deposit.CalculationData.State != DepositStates.Закрыт;
                OnRenewPressed(new RenewPressedEventArgs(NewAccountForDeposit));
            }
        }

        public void Exit()
        {
            TryClose();
        }

        public event RenewPressedEventHandler RenewPressed;
        protected virtual void OnRenewPressed(RenewPressedEventArgs e)
        {
            if (RenewPressed != null) RenewPressed(this, e);
        }
    }

    public delegate void RenewPressedEventHandler(object sender, RenewPressedEventArgs e);
    public class RenewPressedEventArgs : EventArgs
    {
        public readonly Account NewAccount;

        public RenewPressedEventArgs(Account newAccount)
        {
            NewAccount = newAccount;
        }
    }

}