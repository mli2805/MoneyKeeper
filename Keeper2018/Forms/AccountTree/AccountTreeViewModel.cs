using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using Keeper2018.ExpensesOnAccount;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneDepositViewModel _oneDepositViewModel;
        private readonly ExpensesOnAccountViewModel _expensesOnAccountViewModel;
        private readonly DepositReportViewModel _depositReportViewModel;
        private readonly BalanceVerificationViewModel _balanceVerificationViewModel;
        public IWindowManager WindowManager { get; }
        public ShellPartsBinder ShellPartsBinder { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDb KeeperDb { get; set; }

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
            AskDragAccountActionViewModel askDragAccountActionViewModel,
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel, 
            ExpensesOnAccountViewModel expensesOnAccountViewModel,
            DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            _expensesOnAccountViewModel = expensesOnAccountViewModel;
            _depositReportViewModel = depositReportViewModel;
            _balanceVerificationViewModel = balanceVerificationViewModel;
            WindowManager = windowManager;
            ShellPartsBinder = shellPartsBinder;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDb = keeperDb;
        }

        public void AddAccount()
        {
            var accountModel = new AccountModel("")
            {
                Id = KeeperDb.AcMoDict.Keys.Max() + 1,
                Owner = ShellPartsBinder.SelectedAccountModel
            };
            _oneAccountViewModel.Initialize(accountModel, true);
            WindowManager.ShowDialog(_oneAccountViewModel);
            if (!_oneAccountViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDb.Bin.AccountPlaneList.Add(accountModel.Map());
            KeeperDb.AcMoDict.Add(accountModel.Id, accountModel);
        }

        public void AddAccountDeposit()
        {
            var accountModel = new AccountModel("")
            {
                Id = KeeperDb.Bin.AccountPlaneList.Max(a => a.Id) + 1,
                Owner = ShellPartsBinder.SelectedAccountModel,
                Deposit = new Deposit()
            };
            _oneDepositViewModel.InitializeForm(accountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
            if (!_oneDepositViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDb.Bin.AccountPlaneList.Add(accountModel.Map());
            KeeperDb.AcMoDict.Add(accountModel.Id, accountModel);
        }

        public void ChangeAccount()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit)
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneDepositViewModel.InitializeForm(accountModel, false);
                WindowManager.ShowDialog(_oneDepositViewModel);

                if (_oneDepositViewModel.IsSavePressed)
                    KeeperDb.FlattenAccountTree();
            }
            else
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneAccountViewModel.Initialize(accountModel, false);
                WindowManager.ShowDialog(_oneAccountViewModel);

                if (_oneAccountViewModel.IsSavePressed)
                    KeeperDb.FlattenAccountTree();
            }
        }
        public void RemoveSelectedAccount()
        {
            KeeperDb.RemoveSelectedAccount();
        }

        public void ShowDepositReport()
        {
            _depositReportViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowDialog(_depositReportViewModel);
        }

        public void ShowVerificationForm()
        {
            _balanceVerificationViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowDialog(_balanceVerificationViewModel);
        }

        public void ShowFolderSummaryForm()
        {
            var folderSummaryViewModel = new FolderSummaryViewModel(KeeperDb);
            folderSummaryViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowWindow(folderSummaryViewModel);
        }

        public void ShowExpensesOnAccount()
        {
            _expensesOnAccountViewModel.Initialize(ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod);
            WindowManager.ShowDialog(_expensesOnAccountViewModel);
        }

        public void ShowTagInDetails()
        {
            var tag = ShellPartsBinder.SelectedAccountModel;
            var doc = new Document();
            var section = doc.AddSection();
            var tableData = new PdfReportTable(tag.Name, "", KeeperDb.GetTableForTag(tag));
            section.DrawTableFromTag(tableData);

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfDocumentRenderer.Document = doc;
            pdfDocumentRenderer.RenderDocument();
            string filename = $@"c:\temp\{tag.Name}.pdf";
            pdfDocumentRenderer.Save(filename);
            Process.Start(filename);
        }

        public void ConvertToDeposit()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit) return;
            ShellPartsBinder.SelectedAccountModel.Deposit =
                new Deposit
                {
                    MyAccountId = ShellPartsBinder.SelectedAccountModel.Id,

                };
            _oneDepositViewModel.InitializeForm(ShellPartsBinder.SelectedAccountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
        }
    }
}
