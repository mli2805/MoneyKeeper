using System;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class TransferTranViewModel : Screen, IOneTranView
    {
        [ImportingConstructor]
        public TransferTranViewModel()
        {
        }

        public TranWithTags GetTran()
        {
            throw new NotImplementedException();
        }

        public void SetTran(TranWithTags tran)
        {
            
        }
    }
}
