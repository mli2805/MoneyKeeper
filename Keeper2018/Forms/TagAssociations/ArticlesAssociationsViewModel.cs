using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
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
            Rows = new ObservableCollection<LineModel>();
            foreach (var lineModel in _db.TagAssociationModels.Select(a => a.Map()))
            {
                Rows.Add(lineModel);
            }
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

        public override void CanClose(Action<bool> callback)
        {
            ReSaveAll();
            base.CanClose(callback);
        }

        public void CloseView()
        {

            TryClose();
        }

        private void ReSaveAll()
        {
            var tagAssociations = new List<TagAssociation>();
            foreach (var lineModel in Rows)
            {
                try
                {
                    tagAssociations.Add(Map(lineModel));
                }
                catch (Exception)
                {
                    MessageBox.Show($"Ошибка в линии {lineModel}");
                }
            }

            _db.Bin.TagAssociations = tagAssociations;
            _db.TagAssociationModels = new ObservableCollection<TagAssociationModel>
                (_db.Bin.TagAssociations.Select(a => a.Map(_db.AcMoDict)));

        }

        private TagAssociation Map(LineModel lineModel)
        {
            return new TagAssociation
            {
                OperationType = lineModel.OperationType,
                ExternalAccount = _db.GetLeavesOf("Внешние").First(x => x.Name == lineModel.ExternalAccount).Id,
                Tag = lineModel.OperationType == OperationType.Доход
                    ? _db.GetLeavesOf("Все доходы").First(x => x.Name == lineModel.Tag).Id
                    : _db.GetLeavesOf("Все расходы").First(x => x.Name == lineModel.Tag).Id,
                Destination = lineModel.Destination
            };
        }
    }
}
