using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    public static class ListsForComboTrees
    {
        public static readonly KeeperDb _db = IoC.Get<KeeperDb>();
        public static List<AccName> MyAccNamesForIncome { get; set; }
        public static List<AccName> AccNamesForIncomeTags { get; set; }

        public static List<AccName> MyAccNamesForExpense { get; set; }
        public static List<AccName> AccNamesForExpenseTags { get; set; }

        public static List<AccName> MyAccNamesForTransfer { get; set; }
        public static List<AccName>  AccNamesForTransferTags { get; set; }

        public static List<AccName> MyAccNamesForExchange { get; set; }
        public static List<AccName> AccNamesForExchangeTags { get; set; }

        public static List<AccName> AccNamesForFilterTags { get; set; }

        public static void InitializeLists()
        {
            InitializeListsForIncome();
            InitializeListsForExpense();
            InitializeListsForTransfer();
            InitializeListsForExchange();
            InitializeListForFilterTags();
        }

        public static void InitializeListForFilterTags()
        {
            // All Tags
            AccNamesForFilterTags = new List<AccName>();
            var list = new List<string>() { "�������", "��� ������", "��� �������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForFilterTags.Add(root);
            }
        }
        private static void InitializeListsForIncome()
        {
            // Income
            MyAccNamesForIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("���"),
                    new List<string> {"��������", "�������� ��������", "��� ������"})
            };

            // Income Tags
            AccNamesForIncomeTags = new List<AccName>();
            var list = new List<string>() { "������������", "�����", "�����������", "��� ������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForIncomeTags.Add(root);
            }
        }

        private static void InitializeListsForExpense()
        {
            // Expense
            MyAccNamesForExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("���"),
                    new List<string> {"��������", "�������� ��������", "��� ������", "��������"})
            };

            // Expense Tags
            AccNamesForExpenseTags = new List<AccName>();
            var list = new List<string>() { "����������������", "�����", "�����������", "��� �������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForExpenseTags.Add(root);
            }
        }

        private static void InitializeListsForTransfer()
        {
            // Transfer
            MyAccNamesForTransfer = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("���"),
                                                       new List<string> {"��������", "�������� ��������"})
            };

            // Transfer Tags
            AccNamesForTransferTags = new List<AccName>();
            var list = new List<string>() { "������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForTransferTags.Add(root);
            }
        }

        private static void InitializeListsForExchange()
        {
            // Exchange
            MyAccNamesForExchange = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("���"),
                                                       new List<string> {"��������", "�������� ��������"})
            };

            // Exchange Tags
            AccNamesForExchangeTags = new List<AccName>();
            var list = new List<string>() { "�����", "������" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
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