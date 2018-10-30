using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Keeper2018.Properties;

namespace Keeper2018
{
    public static class DbUtils
    {
        public class Db2018
        {
            public ObservableCollection<Account> Accounts { get; set; }
            public ObservableCollection<NbRbRate> OfficialRates { get; set; }
        }

        public static Account GetById(int id, ICollection<Account> roots)
        {
            foreach (var account in roots)
            {
                if (account.Id == id) return account;
                var acc = GetById(id, account.Children);
                if (acc != null) return acc;
            }
            return null;
        }

        public static string GetTxtFullPath(string filename)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = (string)Settings.Default["DataFolder"];
            var dataPath = Path.Combine(path, dataFolder);
            return Path.Combine(dataPath, filename);
        }

    }
}