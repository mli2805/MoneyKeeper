using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.ViewModels;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [Export(typeof(IDbFromTxtLoader))]
    [Export(typeof(ILoader))]
    public class DbFromTxtLoader : IDbFromTxtLoader, ILoader
    {
        public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

        private readonly DbClassesInstanceParser _dbClassesInstanceParser;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly Encoding _encoding1251 = Encoding.GetEncoding(1251);
        public DbLoadResult Result;

        public string FileExtension { get { return ".txt"; } }

        public DbLoadResult Load(string filename)
        {
            return LoadDbFromTxt(Path.GetDirectoryName(filename));
        }

        [ImportingConstructor]
        public DbFromTxtLoader(DbClassesInstanceParser dbClassesInstanceParser, AccountTreeStraightener accountTreeStraightener)
        {
            _dbClassesInstanceParser = dbClassesInstanceParser;
            _accountTreeStraightener = accountTreeStraightener;
        }

        public DbLoadResult LoadDbFromTxt(string path)
        {
            var db = new KeeperDb { Accounts = LoadAccounts(path) };
            var accountsPlaneList = _accountTreeStraightener.Flatten(db.Accounts).ToList();
            db.BankDepositOffers = LoadFrom(path, "BankDepositOffers.txt",
                           _dbClassesInstanceParser.BankDepositOfferFromString, accountsPlaneList);
            if (Result != null) return Result;
            LoadDepositOffersRates(path, db.BankDepositOffers.ToList());
            LoadDeposits(path, accountsPlaneList, db.BankDepositOffers.ToList());
            if (Result != null) return Result;
            db.Transactions = LoadFrom(path, "Transactions.txt",
                           _dbClassesInstanceParser.TransactionFromStringWithNames, accountsPlaneList);
            if (Result != null) return Result;
            db.ArticlesAssociations = LoadFrom(path, "ArticlesAssociations.txt",
                           _dbClassesInstanceParser.ArticleAssociationFromStringWithNames, accountsPlaneList);
            if (Result != null) return Result;
            db.CurrencyRates = LoadFrom(path, "CurrencyRates.txt",
                           _dbClassesInstanceParser.CurrencyRateFromString, accountsPlaneList);
            if (Result != null) return Result;
            db.OfficialRates = LoadFrom(path, "OfficialRates.txt",
                _dbClassesInstanceParser.OfficialRateFromString, accountsPlaneList);
            if (Result != null) return Result;
            return new DbLoadResult(db);
        }

        private ObservableCollection<T> LoadFrom<T>(string path, string filename, Func<string, IEnumerable<Account>, T> parseLine, List<Account> accountsPlaneList)
        {
            var fullFilename = Path.Combine(path, filename);
            if (!File.Exists(fullFilename))
            {
                Result = new DbLoadResult(325, "File <" + fullFilename + "> not found");
                return null;
            }

            var content = File.ReadAllLines(fullFilename, _encoding1251).Where(s => !String.IsNullOrWhiteSpace(s));
            var wrongContent = new List<string>();
            var result = new ObservableCollection<T>();

            foreach (var s in content)
            {
                try
                {
                    result.Add(parseLine(s, accountsPlaneList));
                }
                catch (Exception)
                {
                    wrongContent.Add(s);
                }
            }
            if (wrongContent.Count != 0)
            {
                File.WriteAllLines(Path.ChangeExtension(Path.Combine(path, filename), "err"), wrongContent, _encoding1251);
                Result = new DbLoadResult(326, "Ошибки загрузки смотри в файле " + Path.ChangeExtension(filename, "err"));
            }
            return result;
        }

        private void LoadDepositOffersRates(string path, List<BankDepositOffer> depositOffers)
        {
            var filename = Path.Combine(path, "BankDepositOffersRates.txt");
            if (!File.Exists(filename))
            {
                Result = new DbLoadResult(315, "File <BankDepositOffersRates.txt> not found");
                return;
            }

            var content = File.ReadAllLines(filename, _encoding1251).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            foreach (var s in content)
            {
                _dbClassesInstanceParser.DepositRateLineFromString(s, depositOffers);
            }
        }

        private void LoadDeposits(string path, List<Account> plainList, List<BankDepositOffer> depositOffers)
        {
            var filename = Path.Combine(path, "Deposits.txt");
            if (!File.Exists(filename))
            {
                Result = new DbLoadResult(315, "File <Deposits.txt> not found");
                return;
            }

            var content = File.ReadAllLines(filename, _encoding1251).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            foreach (var s in content)
            {
                _dbClassesInstanceParser.DepositFromString(s, plainList, depositOffers);
            }
        }

        #region // Accounts
        private ObservableCollection<Account> LoadAccounts(string path)
        {
            var filename = Path.Combine(path, "Accounts.txt");
            if (!File.Exists(filename))
            {
                Result = new DbLoadResult(315, "File <Accounts.txt> not found");
                return null;
            }

            var content = File.ReadAllLines(filename, _encoding1251).Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            var accounts = new ObservableCollection<Account>();
            foreach (var s in content)
            {
                int parentId;
                var account = _dbClassesInstanceParser.AccountFromString(s, out parentId);
                if (parentId == 0)
                {
                    BuildBranchFromRoot(account, content);
                    accounts.Add(account);
                }
            }
            return accounts;
        }

        private void BuildBranchFromRoot(Account root, List<string> content)
        {
            foreach (var s in content)
            {
                if (s == "") continue;
                int parentId;
                var account = _dbClassesInstanceParser.AccountFromString(s, out parentId);
                if (parentId == root.Id)
                {
                    account.Parent = root;
                    root.Children.Add(account);
                    BuildBranchFromRoot(account, content);
                }
            }
        }
        #endregion
    }
}
