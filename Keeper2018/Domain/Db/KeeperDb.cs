using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class KeeperDb
    {
        public List<Account> AccountPlaneList { get; set; } = new List<Account>();
        public List<OfficialRates> OfficialRates { get; set; } = new List<OfficialRates>();
        public List<Transaction> Transactions { get;set; } = new List<Transaction>();
    }
}