using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class ArticlesAssociationsViewModel : Screen
    {
        private static KeeperDataModel _dataModel;

        public ObservableCollection<TagAssociationModel> Rows { get; set; }

        private TagAssociationModel _selectedRow;
        public TagAssociationModel SelectedRow
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

        public ArticlesAssociationsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Init()
        {
            InitializeListsForCombobox();
            InitilizeGrid();
        }

        private void InitilizeGrid()
        {
            Rows = new ObservableCollection<TagAssociationModel>();
            foreach (var lineModel in _dataModel.TagAssociations.Select(a => Map(a)))
            {
                Rows.Add(lineModel);
            }
            SelectedRow = Rows.First();
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = _dataModel.GetLeavesOf("Внешние").Select(x => x.Name).ToList();
            AssociatedArticles = _dataModel.GetLeavesOf("Все доходы").Select(x => x.Name).ToList();
            AssociatedArticles.AddRange(_dataModel.GetLeavesOf("Все расходы").Select(x => x.Name).ToList());
            OperationTypes = Enum.GetValues(typeof(OperationType)).Cast<OperationType>().ToList();
            AssociationTypes = Enum.GetValues(typeof(AssociationType)).Cast<AssociationType>().ToList();
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

            _dataModel.TagAssociations = tagAssociations;
        }

        private TagAssociation Map(TagAssociationModel tagAssociationModel)
        {
            return new TagAssociation
            {
                Id = tagAssociationModel.Id,
                OperationType = tagAssociationModel.OperationType,
                ExternalAccount = _dataModel.GetLeavesOf("Внешние").First(x => x.Name == tagAssociationModel.ExternalAccount).Id,
                Tag = tagAssociationModel.OperationType == OperationType.Доход
                    ? _dataModel.GetLeavesOf("Все доходы").First(x => x.Name == tagAssociationModel.Tag).Id
                    : _dataModel.GetLeavesOf("Все расходы").First(x => x.Name == tagAssociationModel.Tag).Id,
                Destination = tagAssociationModel.Destination
            };
        }

        private TagAssociationModel Map(TagAssociation tagAssociation)
        {
            return new TagAssociationModel()
            {
                Id = tagAssociation.Id,
                OperationType = tagAssociation.OperationType,
                ExternalAccount = _dataModel.AcMoDict[tagAssociation.ExternalAccount].Name,
                Tag = _dataModel.AcMoDict[tagAssociation.Tag].Name,
                Destination = tagAssociation.Destination,
            };
        }
    }
}
