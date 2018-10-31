﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    [Serializable]
    public class SerializableKeeperDb
    {
        public List<SerializableAccount> SerializableAccounts { get; set; }
        public ObservableCollection<OfficialRates> OfficialRates { get; set; }

        public SerializableKeeperDb(KeeperDb keeperDb)
        {
            SerializableAccounts = AccountMapper.Map(keeperDb.Accounts);
            OfficialRates = keeperDb.OfficialRates;
        }

        public KeeperDb GetKeeperDb()
        {
            var result = new KeeperDb()
            {
                Accounts = AccountMapper.Map(SerializableAccounts),
                OfficialRates = OfficialRates,
            };
            return result;
        }
    }
}