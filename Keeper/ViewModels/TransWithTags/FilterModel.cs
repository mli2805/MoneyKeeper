﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls.SubControls.TagPickingControl;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    public class FilterModel : PropertyChangedBase
    {
        public List<OperationTypesFilter> OperationTypes { get; set; } = InitOperationTypesFilter();
        private OperationTypesFilter _myOperationType;
        public OperationTypesFilter MyOperationType
        {
            get { return _myOperationType; }
            set
            {
                if (value == _myOperationType) return;
                _myOperationType = value;
                NotifyOfPropertyChange();
            }
        }
        private static List<OperationTypesFilter> InitOperationTypesFilter()
        {
            var result = new List<OperationTypesFilter>();
            // <no filter>
            result.Add(new OperationTypesFilter());
            // filters for every operation type
            var operationTypes = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();
            result.AddRange(from operationType in operationTypes
                select new OperationTypesFilter(operationType));
            return result;
        }

        private List<AccName> _availableAccNames;
        public List<AccName> AvailableAccNames
        {
            get { return _availableAccNames; }
            set
            {
                if (Equals(value, _availableAccNames)) return;
                _availableAccNames = value;
                NotifyOfPropertyChange();
            }
        }

        private AccName _myAccName;
        public AccName MyAccName
        {
            get { return _myAccName; }
            set
            {
                if (Equals(value, _myAccName)) return;
                _myAccName = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isAccNamePosition1;
        public bool IsAccNamePosition1
        {
            get { return _isAccNamePosition1; }
            set
            {
                if (value == _isAccNamePosition1) return;
                _isAccNamePosition1 = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isAccNamePosition2;
        public bool IsAccNamePosition2
        {
            get { return _isAccNamePosition2; }
            set
            {
                if (value == _isAccNamePosition2) return;
                _isAccNamePosition2 = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isAccNamePosition12;
        public bool IsAccNamePosition12
        {
            get { return _isAccNamePosition12; }
            set
            {
                if (value == _isAccNamePosition12) return;
                _isAccNamePosition12 = value;
                NotifyOfPropertyChange();
            }
        }

        private string _amount;
        public string Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _amountEqualTo;
        public bool AmountEqualTo
        {
            get { return _amountEqualTo; }
            set
            {
                if (value == _amountEqualTo) return;
                _amountEqualTo = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _amountLessThan;
        public bool AmountLessThan
        {
            get { return _amountLessThan; }
            set
            {
                if (value == _amountLessThan) return;
                _amountLessThan = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _amountGreaterThan;
        public bool AmountGreaterThan
        {
            get { return _amountGreaterThan; }
            set
            {
                if (value == _amountGreaterThan) return;
                _amountGreaterThan = value;
                NotifyOfPropertyChange();
            }
        }
        public List<CurrencyCodesFilter> Currencies { get; set; } = InitCurrencyCodesFilter();

        private CurrencyCodesFilter _myCurrency;
        public CurrencyCodesFilter MyCurrency
        {
            get { return _myCurrency; }
            set
            {
                if (Equals(value, _myCurrency)) return;
                _myCurrency = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isCurrencyPosition1;
        public bool IsCurrencyPosition1
        {
            get { return _isCurrencyPosition1; }
            set
            {
                if (value == _isCurrencyPosition1) return;
                _isCurrencyPosition1 = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isCurrencyPosition2;
        public bool IsCurrencyPosition2
        {
            get { return _isCurrencyPosition2; }
            set
            {
                if (value == _isCurrencyPosition2) return;
                _isCurrencyPosition2 = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isCurrencyPosition12;
        public bool IsCurrencyPosition12
        {
            get { return _isCurrencyPosition12; }
            set
            {
                if (value == _isCurrencyPosition12) return;
                _isCurrencyPosition12 = value;
                NotifyOfPropertyChange();
            }
        }
        private static List<CurrencyCodesFilter> InitCurrencyCodesFilter()
        {
            var result = new List<CurrencyCodesFilter>();
            // <no filter>
            result.Add(new CurrencyCodesFilter());
            // filters for every currency
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            result.AddRange(from currency in currencies
                select new CurrencyCodesFilter(currency));
            return result;
        }

        private bool _isTagsJoinedByAnd;
        public bool IsTagsJoinedByAnd
        {
            get { return _isTagsJoinedByAnd; }
            set
            {
                if (value == _isTagsJoinedByAnd) return;
                _isTagsJoinedByAnd = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<AccName> MyTags { get; set; } = new ObservableCollection<AccName>();

        public TagPickerVm MyTagPickerVm { get; set; } 
        

        private bool _isTagsJoinedByOr;
        public bool IsTagsJoinedByOr
        {
            get { return _isTagsJoinedByOr; }
            set
            {
                if (value == _isTagsJoinedByOr) return;
                _isTagsJoinedByOr = value;
                NotifyOfPropertyChange();
            }
        }

        private string _myComment;

        public string MyComment
        {
            get { return _myComment; }
            set
            {
                if (value == _myComment) return;
                _myComment = value;
                NotifyOfPropertyChange();
            }
        }

        [ImportingConstructor]
        public FilterModel(KeeperDb db, AccNameSelectionControlInitializer accNameSelectionControlInitializer)
        {
            AvailableAccNames = new List<AccName>
            {
                new AccName().PopulateFromAccount(db.SeekAccount("Мои"), new List<string>())
            };
            IsAccNamePosition12 = true;
            AmountEqualTo = true;
            IsCurrencyPosition12 = true;
            MyTagPickerVm = new TagPickerVm { TagSelectorVm = accNameSelectionControlInitializer.ForFilter(), Tags = MyTags};
            IsTagsJoinedByAnd = true;
            CleanAll();
            MyTags.CollectionChanged += MyTags_CollectionChanged;
        }

        private void MyTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(MyTags));
        }

        public void CleanAll()
        {
            MyOperationType = OperationTypes.FirstOrDefault();
            MyAccName = AvailableAccNames.FirstOrDefault();
            Amount = null;
            MyCurrency = Currencies.FirstOrDefault();
            MyTagPickerVm.Tags.Clear();
            MyComment = "";
        }
        public void CleanProperty(int propertyNumber)
        {
            switch (propertyNumber)
            {
                case 1: MyOperationType = OperationTypes.FirstOrDefault(); return;
                case 2: MyAccName = AvailableAccNames.FirstOrDefault(); return;
                case 3: Amount = null; return;
                case 4: MyCurrency = Currencies.FirstOrDefault(); return;
                case 5: MyTagPickerVm.Tags.Clear(); return;
                case 6: MyComment = ""; return;
                case 99: CleanAll(); return;
            }
        }
    }
}