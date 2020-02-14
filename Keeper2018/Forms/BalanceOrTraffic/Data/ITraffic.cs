using System;
using System.Collections.Generic;

namespace Keeper2018
{
    interface ITraffic
    {
        void EvaluateAccount();
        IEnumerable<KeyValuePair<DateTime, string>> Report(BalanceOrTraffic mode);
        string Total { get; }

    }
}