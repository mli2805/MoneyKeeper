using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using Keeper.Annotations;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalancesFromTransWithTags;
using Keeper.ViewModels.Shell.MainMenuActions;

namespace Keeper.Models.Shell
{
    [Export]
    [Shared]
    public class ShellModel : INotifyPropertyChanged
    {
        private readonly BalancesForMainViewCalculator _balancesForMainViewCalculator;

        private MainMenuAction _currentAction;
        public MainMenuAction CurrentAction
        {
            get { return _currentAction; }
            set
            {
                _currentAction = value;
                CurrentActionOnChanged(_currentAction);
            }
        }

        public bool IsDbChanged
        {
            get { return _isDbChanged; }
            set
            {
                _isDbChanged = value; 
                if (_isDbChanged)
                    RefreshBalanceListAccordinglyDatesInSelector();
            }
        }

        public List<Screen> LaunchedForms = new List<Screen>();
        private bool _isExitPreparationDone;
        private bool _isDbChanged;

        public bool IsExitPreparationDone
        {
            get { return _isExitPreparationDone; }
            set
            {
                if (value == _isExitPreparationDone) return;
                _isExitPreparationDone = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthorizationFailed { get; set; }


        public AccountForestModel MyForestModel { get; set; }
        public BalanceListModel MyBalanceListModel { get; set; }
        public TwoSelectorsModel MyTwoSelectorsModel { get; set; }
        public StatusBarModel MyStatusBarModel { get; set; }

        public readonly MainMenu MainMenuDictionary;

        [ImportingConstructor]
        public ShellModel(BalancesForMainViewCalculator balancesForMainViewCalculator)
        {
            _balancesForMainViewCalculator = balancesForMainViewCalculator;

            MyForestModel = new AccountForestModel();
            MyForestModel.PropertyChanged += MyForestModelPropertyChanged;
            MyBalanceListModel = new BalanceListModel();
            MyTwoSelectorsModel = new TwoSelectorsModel();
            MyTwoSelectorsModel.PropertyChanged += MyTwoSelectorsModelPropertyChanged;
            MyStatusBarModel = new StatusBarModel();

            MainMenuDictionary = new MainMenu();
            MainMenuDictionary.Init();
        }

        public void CurrentActionOnChanged(MainMenuAction action)
        {
            MyStatusBarModel.Item0 = MainMenuDictionary.Actions[action].StatusBarMessage;
            MyStatusBarModel.ProgressBarVisibility = MainMenuDictionary.Actions[action].IsAsync
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void CloseAllLaunchedForms()
        {
            foreach (var launchedForm in LaunchedForms.Where(launchedForm => launchedForm.IsActive))
                launchedForm.TryClose();
        }
        private void RefreshBalanceListAccordinglyDatesInSelector()
        {
            if (MyTwoSelectorsModel.IsPeriodMode)
                MyBalanceListModel.AccountBalanceInUsd =
                    $"{_balancesForMainViewCalculator.FillListForShellView(MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList):#,0.##} usd";
            else
                MyBalanceListModel.AccountBalanceInUsd =
                    $"{_balancesForMainViewCalculator.FillListForShellView(MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList):#,0.##} usd";
        }

        void MyTwoSelectorsModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChangeControlTypeTranslatedEvent")
                MyTwoSelectorsModel.IsPeriodMode = !MyTwoSelectorsModel.IsPeriodMode;
            else
                RefreshBalanceListAccordinglyDatesInSelector();
        }

        void MyForestModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedAccount")
            {
                MyBalanceListModel.Caption = MyForestModel.SelectedAccount.Name;
                RefreshBalanceListAccordinglyDatesInSelector();
            }

            if (e.PropertyName == "OpenedAccountPage")
                MyTwoSelectorsModel.IsPeriodMode = MyForestModel.OpenedAccountPage != 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
