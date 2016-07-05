using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating;
using Microsoft.Vbe.Interop;

namespace Keeper.Utils
{
    [Export]
    class Denominator
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public Denominator(KeeperDb db)
        {
            _db = db;
        }

        public void Denominate()
        {
            List<Account> myAccounts = (new AccountTreeStraightener().Flatten(_db.Accounts).
                Where(a => (a.IsLeaf("Мои") || a.Name == "Для ввода стартовых остатков"))).ToList();
            var minutes = 0;
            foreach (var myAccount in myAccounts)
            {
                var amountByr = new AccountBalanceCalculator(_db).GetAccountBalanceOnlyForCurrency(myAccount,
                                    new Period(new DateTime(0), new DateTime(2016, 7, 1)), CurrencyCodes.BYR);
                if (amountByr > 0)
                {
                    minutes++;
                    DenominateOneAccount(myAccount, minutes, amountByr);
                }

            }
        }

        private void DenominateOneAccount(Account account, int minutes, decimal amount)
        {
            var denominationBank = new AccountTreeStraightener().Seek("Деноминация2016", _db.Accounts);
            var guid = Guid.NewGuid();
            var transaction1 = new Transaction()
            {
                Timestamp = new DateTime(2016, 7, 1, 0, minutes, 0),
                Guid = guid,
                Operation = OperationType.Расход,
                Debet = account,
                Credit = denominationBank,
                Amount = amount,
                Currency = CurrencyCodes.BYR,
            };
            var transaction2 = new Transaction()
            {
                Timestamp = new DateTime(2016, 7, 1, 0, minutes, 10),
                Guid = guid,
                Operation = OperationType.Доход,
                Debet = denominationBank,
                Credit = account,
                Amount = Math.Round(amount / 100) / 100,
                Currency = CurrencyCodes.BYN,
            };
            _db.Transactions.Add(transaction1);
            _db.Transactions.Add(transaction2);
        }

    }
}
