﻿using System.Collections.Generic;

namespace Keeper2018
{
    public class ComboTreesProvider
    {
        private readonly KeeperDataModel _dataModel;

        public List<AccName> MyAccNamesForIncome { get; set; }
        public List<AccName> AccNamesForIncomeTags { get; set; }

        public List<AccName> MyAccNamesForExpense { get; set; }
        public List<AccName> AccNamesForExpenseTags { get; set; }

        public List<AccName> MyAccNamesForTransfer { get; set; }
        public List<AccName> AccNamesForTransferTags { get; set; }

        public List<AccName> MyAccNamesForExchange { get; set; }
        public List<AccName> AccNamesForExchangeTags { get; set; }

        public List<AccName> AccNamesForFilterTags { get; set; }

        public List<AccName> AccNamesForInvestmentExpense { get; set; }
        public List<AccName> AccNamesForInvestmentIncome { get; set; }

        public ComboTreesProvider(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public List<AccName> GetAllIncomeTags()
        {
            return new List<AccName> { new AccName().PopulateFromAccount(_dataModel.AcMoDict[185], null) };
        }

        public List<AccName> GetFullBranch(int branchId)
        {
            return new List<AccName> { new AccName().PopulateFromAccount(_dataModel.AcMoDict[branchId], null) };
        }

        public void Initialize()
        {
            InitializeListsForIncome();
            InitializeListsForExpense();
            InitializeListsForTransfer();
            InitializeListsForExchange();
            InitializeListForFilterTags();
            InitializeForInvestments();
        }

        private void InitializeListForFilterTags()
        {
            // All Tags
            AccNamesForFilterTags = new List<AccName>();
            var list = new List<int>() { 157, 185, 189 };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_dataModel.AcMoDict[element], null);
                AccNamesForFilterTags.Add(root);
            }
        }
        private void InitializeListsForIncome()
        {
            // Income
            MyAccNamesForIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[158], new List<int> {393, 235})
            };

            // Income Tags
            AccNamesForIncomeTags = new List<AccName>();
            var list = new List<int>() { 724, 723, 220, 183, 185 };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_dataModel.AcMoDict[element], null);
                AccNamesForIncomeTags.Add(root);
            }
        }

        private void InitializeForInvestments()
        {
            AccNamesForInvestmentExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[161], null)
            };
            AccNamesForInvestmentIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[220], null)
            };
        }

        private void InitializeListsForExpense()
        {
            // Expense
            MyAccNamesForExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[158], new List<int> {166})
            };

            // Expense Tags
            AccNamesForExpenseTags = new List<AccName>();
            var list = new List<int>() { 724, 723, 220, 183, 189 };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_dataModel.AcMoDict[element], null);
                AccNamesForExpenseTags.Add(root);
            }
        }

        private void InitializeListsForTransfer()
        {
            // Transfer
            MyAccNamesForTransfer = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[158], new List<int> {393, 235,})
            };

            // Transfer Tags
            AccNamesForTransferTags = new List<AccName>();
            var list = new List<int>() { 579 };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_dataModel.AcMoDict[element], null);
                AccNamesForTransferTags.Add(root);
            }
        }

        private void InitializeListsForExchange()
        {
            // Exchange
            MyAccNamesForExchange = new List<AccName>
            {
                new AccName().PopulateFromAccount(_dataModel.AcMoDict[158], new List<int> {393, 235,})
            };

            // Exchange Tags
            AccNamesForExchangeTags = new List<AccName>();
            var list = new List<int>() { 220, 579, };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_dataModel.AcMoDict[element], null);
                AccNamesForExchangeTags.Add(root);
            }
        }

    }


}