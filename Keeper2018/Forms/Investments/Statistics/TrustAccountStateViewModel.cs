using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountStateViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private TrustAccount _trustAccount;

        public List<TrustAccountLine> Rows { get; private set; }

        public TrustAccountStateViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _trustAccount.ToCombo;
        }

        public void Evaluate(TrustAccount trustAccount)
        {
            _trustAccount = trustAccount;

            Rows = new List<TrustAccountLine>();
            decimal balanceBefore = 0;

            foreach (var tr in _dataModel.InvestTranModels.Where(t => t.TrustAccount.Id == trustAccount.Id))
            {
                var line = tr.Create(balanceBefore);
                Rows.Add(line);
                balanceBefore = line.BalanceAfter;
            }
        }
    }
}
