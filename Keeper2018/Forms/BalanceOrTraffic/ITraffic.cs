using System.Collections.Generic;

namespace Keeper2018
{
    interface ITraffic
    {
        void RegisterTran(TransactionModel tran);
        IEnumerable<string> Report();
    }
}