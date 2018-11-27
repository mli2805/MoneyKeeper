using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DepositReportViewModel : Screen
    {
        public DepositReportModel Model { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Model.DepositName;
        }

        public void Close() { TryClose(); }
    }

    public class DepositReportFactory
    {
        private readonly KeeperDb _db;
        private AccountModel _accountModel;

        private Money _before = new Money();

        public DepositReportFactory(KeeperDb db)
        {
            _db = db;
        }

        public DepositReportModel Create(AccountModel accountModel)
        {
            _accountModel = accountModel;
            var model = new DepositReportModel();
            model.DepositName = accountModel.Name;

            var balanceOfAccount = new BalanceOfAccount(_db, accountModel, new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            balanceOfAccount.Evaluate();
            model.DepositState = balanceOfAccount.AmountInUsd == 0 ? "Депозит закрыт" : "Действующий депозит";

            foreach (var tran in _db.TransactionModels)
            {
                var line = RegisterTran(tran);
                if (line != null)
                    model.Traffic.Add(line);
            }

            return model;
        }

        private DepositReportTrafficLine RegisterTran(TransactionModel tran)
        {
            var comment = "";
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    if (tran.MyAccount.Id == _accountModel.Id)
                    {
                        comment = "проценты";
                    }
                    break;
                case OperationType.Перенос:
                    if (tran.MyAccount.Id == _accountModel.Id)
                    {
                        comment = "снятие";
                    }
                    if (tran.MySecondAccount.Id == _accountModel.Id)
                    {
                        comment = "взнос";
                    }
                    break;
                case OperationType.Обмен:
                    if (tran.MyAccount.Id == _accountModel.Id && tran.MySecondAccount.Id == _accountModel.Id)
                    {
                        comment = "обмен на вкладе";

                    }
                    else if (tran.MyAccount.Id == _accountModel.Id)
                    {
                        comment = "снятие";
                    }
                    else if (tran.MySecondAccount.Id == _accountModel.Id)
                    {
                        comment = "взнос";
                    }
                    break;
                default: return null;
            }
            if (comment == "") return null;

            var result = new DepositReportTrafficLine();
            result.Date = tran.Timestamp;
            result.Comment = string.IsNullOrEmpty(tran.Comment) ? comment : tran.Comment;
            return result;
        }
    }
}
