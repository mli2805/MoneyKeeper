using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class ComboTreesCaterer
    {
        private readonly KeeperDb _db;

        public List<AccName> MyAccNamesForIncome { get; set; }
        public List<AccName> AccNamesForIncomeTags { get; set; }

        public List<AccName> MyAccNamesForExpense { get; set; }
        public List<AccName> AccNamesForExpenseTags { get; set; }

        public List<AccName> MyAccNamesForTransfer { get; set; }
        public List<AccName> AccNamesForTransferTags { get; set; }

        public List<AccName> MyAccNamesForExchange { get; set; }
        public List<AccName> AccNamesForExchangeTags { get; set; }

        public List<AccName> AccNamesForFilterTags { get; set; }

        [ImportingConstructor]
        public ComboTreesCaterer(KeeperDb db)
        {
            _db = db;
            InitializeListsForIncome();
            InitializeListsForExpense();
            InitializeListsForTransfer();
            InitializeListsForExchange();
            InitializeListForFilterTags();
        }

        private void InitializeListForFilterTags()
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
        private void InitializeListsForIncome()
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

        private void InitializeListsForExpense()
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

        private void InitializeListsForTransfer()
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

        private void InitializeListsForExchange()
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

    }
}