using System;
using System.Collections.ObjectModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Deposits;
using Microsoft.Office.Interop.Excel;

namespace Keeper.ViewModels
{
	[Export]
	public class DepositViewModel : Screen
	{
	  private readonly DepositReporter _depositReporter;
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
		public DepositViewModel(DepositReporter depositReporter)
		{
		  _depositReporter = depositReporter;
		  NewAccountForDeposit = null;
		}

    public void SetAccount(Deposit deposit)
		{
      Deposit = deposit;
      ReportHeader = _depositReporter.BuildReportHeader(deposit);
      ReportBody = _depositReporter.BuildReportBody(deposit);
      ReportFooter = _depositReporter.BuildReportFooter(deposit);
		}

		protected override void OnViewLoaded(object view)
		{
			DisplayName = Deposit.ParentAccount.Name;
			CanRenew = Deposit.Evaluations.State != DepositStates.Закрыт;
		}

    public void ExtractEvaluationsToExcel()
    {
      var xlApp = new Application();
      xlApp.Visible = true;
      var wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
      var ws = (Worksheet)wb.Worksheets[1];
      ws.Range["B1"].EntireColumn.ColumnWidth = 15;
      ws.Range["B1"].EntireColumn.NumberFormat = "dd MMM yyyy";
      ws.Range["C1"].EntireColumn.ColumnWidth = 15;
      ws.Range["C1"].EntireColumn.NumberFormat = "#,0";
      ws.Range["D1"].EntireColumn.ColumnWidth = 5;
      ws.Range["E1"].EntireColumn.ColumnWidth = 5;
      ws.Range["F1"].EntireColumn.ColumnWidth = 10;
      ws.Range["F1"].EntireColumn.NumberFormat = "#,0";
      ws.Range["G1"].EntireColumn.ColumnWidth = 12;
      ws.Range["G1"].EntireColumn.NumberFormat = "[Blue]#,0";

      int i = 0;
      decimal total = 0;
      foreach (var line in Deposit.Evaluations.ProcentEvaluation)
      {
        total += line.DayProfit;

        ws.Cells[i+2, 2] = line.Date;
        ws.Cells[i+2, 3] = line.Balance;
        ws.Cells[i+2, 4] = line.DepoRate;
        ws.Cells[i+2, 5] = "%";
        ws.Cells[i+2, 6] = line.DayProfit;
        ws.Cells[i+2, 7] = total;

        i++;
      }
    }

		public void Renew()
		{
			var renewDepositViewModel = IoC.Get<RenewDepositViewModel>();
			renewDepositViewModel.SetOldDeposit(Deposit);
			WindowManager.ShowDialog(renewDepositViewModel);
			if (renewDepositViewModel.NewDeposit != null)
			{
				NewAccountForDeposit = renewDepositViewModel.NewDeposit;
				CanRenew = Deposit.Evaluations.State != DepositStates.Закрыт;
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

  public class RenewPressedEventArgs : System.EventArgs
  {
    public readonly Account NewAccount;

    public RenewPressedEventArgs(Account newAccount)
    {
      NewAccount = newAccount;
    }
  }
  public delegate void RenewPressedEventHandler(object sender, RenewPressedEventArgs e);

}
