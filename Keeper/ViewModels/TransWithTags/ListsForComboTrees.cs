using System.Collections.Generic;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    public static class ListsForComboTrees
    {
        public static readonly AccountTreeStraightener AccountTreeStraightener = new AccountTreeStraightener();
        public static List<AccName> MyAccNamesForIncome { get; set; }
        public static List<AccName> AccNamesForIncomeTags { get; set; }

        public static List<AccName> MyAccNamesForExpense { get; set; }
        public static List<AccName> AccNamesForExpenseTags { get; set; }

        public static List<AccName> MyAccNamesForTransfer { get; set; }
        public static List<AccName>  AccNamesForTransferTags { get; set; }

        public static List<AccName> MyAccNamesForExchange { get; set; }
        public static List<AccName> AccNamesForExchangeTags { get; set; }

        public static void InitializeLists(KeeperDb db)
        {
            InitializeListsForIncome(db);
            InitializeListsForExpense(db);
            InitializeListsForTransfer(db);
            InitializeListsForExchange(db);
        }
        private static void InitializeListsForIncome(KeeperDb db)
        {
            // Income
            MyAccNamesForIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("���", db.Accounts),
                    new List<string> {"��������", "�������� ��������", "��� ������"})
            };

            // Income Tags
            AccNamesForIncomeTags = new List<AccName>();
            var list = new List<string>() { "������������", "�����", "�����������", "��� ������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForIncomeTags.Add(root);
            }
        }

        private static void InitializeListsForExpense(KeeperDb db)
        {
            // Expense
            MyAccNamesForExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("���", db.Accounts),
                    new List<string> {"��������", "�������� ��������", "��� ������", "��������"})
            };

            // Expense Tags
            AccNamesForExpenseTags = new List<AccName>();
            var list = new List<string>() { "����������������", "�����", "�����������", "��� �������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForExpenseTags.Add(root);
            }
        }

        private static void InitializeListsForTransfer(KeeperDb db)
        {
            // Transfer
            MyAccNamesForTransfer = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("���", db.Accounts),
                                                       new List<string> {"��������", "�������� ��������"})
            };

            // Transfer Tags
            AccNamesForTransferTags = new List<AccName>();
            var list = new List<string>() { "������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForTransferTags.Add(root);
            }
        }

        private static void InitializeListsForExchange(KeeperDb db)
        {
            // Exchange
            MyAccNamesForExchange = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("���", db.Accounts),
                                                       new List<string> {"��������", "�������� ��������"})
            };

            // Exchange Tags
            AccNamesForExchangeTags = new List<AccName>();
            var list = new List<string>() { "�����", "������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForExchangeTags.Add(root);
            }
        }

        public static AccName FindThroughTheForest(this List<AccName> roots, string name)
        {
            foreach (var root in roots)
            {
                var result = root.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

    }
}