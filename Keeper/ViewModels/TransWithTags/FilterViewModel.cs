using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class FilterViewModel : Screen
    {
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                NotifyOfPropertyChange();
            }
        }

        private int _top;
        public int Top
        {
            get { return _top; }
            set
            {
                if (value == _top) return;
                _top = value;
                NotifyOfPropertyChange();
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                if (value == _width) return;
                _width = value;
                NotifyOfPropertyChange();
            }
        }

        #region Properties
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
        public bool IsAccNamePosition1 { get; set; }
        public bool IsAccNamePosition2 { get; set; }
        public bool IsAccNamePosition12 { get; set; }

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

        public List<CurrencyCodesFilter> Currencies { get; set; } = InitCurrencyCodesFilter();

        private CurrencyCodesFilter _myCurrency;
        private string _myComment;
        private string _amount;
        private int _width;

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
        #endregion


        [ImportingConstructor]
        public FilterViewModel(AccountTreeStraightener accountTreeStraightener, KeeperDb db)
        {
            AvailableAccNames = new List<AccName>
            {
                new AccName().PopulateFromAccount(accountTreeStraightener.Seek("Мои", db.Accounts), new List<string>())
            };
            CleanAll();
            IsAccNamePosition12 = true;
        }

        public void PlaceIt(int left, int top, int width)
        {
            Left = left - width - 1;
            Top = top + 100;
            Width = width;
            
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Фильтр";
        }

        public void CleanAll()
        {
            MyOperationType = OperationTypes.FirstOrDefault();
            MyAccName = AvailableAccNames.FirstOrDefault();
            Amount = null;
            MyCurrency = Currencies.FirstOrDefault();
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
                case 5: MyComment = ""; return;
                case 99: CleanAll(); return;
            }
        }
    }
}
