using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentTransactionsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<InvestmentTransaction> Transactions { get; set; }
        public InvestmentTransaction SelectedTransaction { get; set; }
        public List<InvestmentAsset> Assets { get; set; }

        public InvestmentTransactionsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Assets = _dataModel.InvestmentAssets;

            Transactions = new ObservableCollection<InvestmentTransaction>(_dataModel.InvestmentTransactions);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Проводки";
        }

        public void DeleteSelected()
        {
            if (SelectedTransaction != null)
                Transactions.Remove(SelectedTransaction);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.InvestmentTransactions = Transactions.ToList();
            base.CanClose(callback);
        }
    }
}
