using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountCombo
    {
        public string Name { get; set; }
        public override string ToString() => Name;
    }
    public class TagAssociationsViewModel : Screen
    {
        public KeeperDb KeeperDb { get; }

        public List<AccountCombo> ExampleList2 { get; set; }
        public AccountCombo SelectedExample2 { get; set; }

        public List<AccountModel> ExternalAccounts { get; set; }
        public AccountModel SelectedAccount { get; set; }
        public List<AccountModel> AssociatedTags { get; set; }
        public List<OperationType> OperationTypes { get; set; }
        public List<AssociationType> Destinations { get; set; }
        public AssociationType SelectedDestination { get; set; }

        public ObservableCollection<TagAssociation> Rows { get; set; }

        private TagAssociation _selectedRow;
        public TagAssociation SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public TagAssociationsViewModel(KeeperDb keeperDb)
        {
            KeeperDb = keeperDb;
            Rows = new ObservableCollection<TagAssociation>();
            foreach (var association in keeperDb.TagAssociations)
            {
                Rows.Add(association);
            }
            SelectedRow = Rows.First();

            InitializeListsForCombobox();
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = KeeperDb.GetLeavesOf("Внешние");
            SelectedAccount = ExternalAccounts.First();

            ExampleList2 = KeeperDb.GetLeavesOf("Внешние").Select(x => new AccountCombo() { Name = x.Name }).ToList();
            SelectedExample2 = ExampleList2.First();



            AssociatedTags = KeeperDb.GetLeavesOf("Все доходы");
            AssociatedTags.AddRange(KeeperDb.GetLeavesOf("Все расходы"));

            OperationTypes = Enum.GetValues(typeof(OperationType)).Cast<OperationType>().ToList();

            Destinations = Enum.GetValues(typeof(AssociationType)).Cast<AssociationType>().ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Tag associations";
        }

        public void Close() { TryClose(); }
    }
}
