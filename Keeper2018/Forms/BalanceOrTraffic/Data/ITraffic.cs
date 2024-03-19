using System;
using System.Collections.Generic;

namespace Keeper2018
{
    interface ITraffic
    {
        void EvaluateAccount();
        IEnumerable<KeyValuePair<DateTime, ListLine>>ColoredReport(BalanceOrTraffic mode);
        string Total { get; }

    }
}