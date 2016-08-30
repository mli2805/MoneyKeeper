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

        public List<CurrencyCodesFilter> Currencies { get; set; } = InitCurrencyCodesFilter();

        private CurrencyCodesFilter _myCurrency;
        private string _myComment;

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

        public void CleanOperationType()
        {
            MyOperationType = OperationTypes.FirstOrDefault();
        }

        public void CleanAccount()
        {
            MyAccName = AvailableAccNames.FirstOrDefault();
        }

        public void CleanCurrency()
        {
            MyCurrency = Currencies.FirstOrDefault();
        }

        public void CleanComment()
        {
            MyComment = "";
        }

        public void CleanAll()
        {
            CleanOperationType();
            CleanAccount();
            CleanCurrency();
            CleanComment();
        }
    }
}
