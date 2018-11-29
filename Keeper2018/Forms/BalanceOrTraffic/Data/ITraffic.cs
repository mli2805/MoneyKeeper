using System.Collections.Generic;

namespace Keeper2018
{
    interface ITraffic
    {
        void Evaluate();
        IEnumerable<string> Report(BalanceOrTraffic mode);
        string Total { get; }

    }
}