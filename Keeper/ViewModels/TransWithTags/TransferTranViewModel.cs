using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
