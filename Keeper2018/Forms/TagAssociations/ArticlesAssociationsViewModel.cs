﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018
{
    public class LineModel
    {
        public string ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public string Tag { get; set; }
        public AssociationType Destination { get; set; }

        public int CompareTo(object obj)
        {
            return string.Compare(ExternalAccount, ((string)obj), StringComparison.Ordinal);
        }
        public Brush FontColor => OperationType.FontColor();

        public override string ToString()
        {
            return $"{OperationType.ToString()} {ExternalAccount} {Tag} {Destination.ToString()}";
        }
    }

    public class ArticlesAssociationsViewModel : Screen
    {
        private static KeeperDb _db;

        public ObservableCollection<LineModel> Rows { get; set; }

        private LineModel _selectedRow;
        public LineModel SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedAssociationText));
            }
        }

        public string SelectedAssociationText => SelectedRow?.ToString();
        public static List<string> ExternalAccounts { get; private set; }

        public static List<string> AssociatedArticles { get; private set; }

        public static List<OperationType> OperationTypes { get; private set; }
        public static List<AssociationType> AssociationTypes { get; private set; }

        public ArticlesAssociationsViewModel(KeeperDb db)
        {
            _db = db;
        



//            var view = CollectionViewSource.GetDefaultView(Rows);
//            view.SortDescriptions.Add(new SortDescription("OperationType", ListSortDirection.Ascending));
//            view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
        }

        public void Init()
        {
            InitializeListsForCombobox();
            InitilizeGrid();
        }

        private void InitilizeGrid()
        {
//            Rows = new ObservableCollection<TagAssociationModel>();
//            foreach (var tagAssociationModel in _db.AssociationModels)
//            {
//                var extAcc = ExternalAccounts.FirstOrDefault(e => Equals(e, tagAssociationModel.ExternalAccount));
//                var tag = AssociatedArticles.FirstOrDefault(a => Equals(a, tagAssociationModel.Tag));
//                if (extAcc == null || tag == null)
//                    continue;
//                Rows.Add(tagAssociationModel);
//            }

            Rows = _db.AssociationModels;
            SelectedRow = Rows.First();
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = _db.GetLeavesOf("Внешние").Select(x=>x.Name).ToList();
            AssociatedArticles = _db.GetLeavesOf("Все доходы").Select(x=>x.Name).ToList();
            AssociatedArticles.AddRange(_db.GetLeavesOf("Все расходы").Select(x=>x.Name).ToList());
            OperationTypes = Enum.GetValues(typeof (OperationType)).Cast<OperationType>().ToList();
            AssociationTypes = Enum.GetValues(typeof (AssociationType)).Cast<AssociationType>().ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ассоциации категорий";

        }

        public void CloseView()
        {
            TryClose();
        }

    }
}