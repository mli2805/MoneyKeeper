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
        }

        public void CurrentActionOnChanged(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.DoNothing:
                    MyStatusBarModel.Item0 = "Готово";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                    break;

                // 100
                case MainMenuAction.SaveDatabase:
                    MyStatusBarModel.Item0 = "Сохранение данных на диск...";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                    break;
                case MainMenuAction.LoadDatabase:
                    MyStatusBarModel.Item0 = "Загрузка данных с диска...";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                    break;
                case MainMenuAction.ClearDatabase:
                    MyStatusBarModel.Item0 = "Очистка БД...";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                    break;
                case MainMenuAction.RemoveExtraBackups:
                    MyStatusBarModel.Item0 = "Удаление идентичных резервных копий...";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                    break;

                // 200
                case MainMenuAction.ShowTransactionsForm:
                    MyStatusBarModel.Item0 = "Ввод транзакций";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                    break;
                case MainMenuAction.ShowCurrencyRatesForm:
                    MyStatusBarModel.Item0 = "Ввод курсов валют";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                    break;
                case MainMenuAction.ShowArticlesAssociationsForm:
                    MyStatusBarModel.Item0 = "Ввод ассоциаций";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                    break;
                case MainMenuAction.ShowMonthAnalysisForm:
                    MyStatusBarModel.Item0 = "Анализ месяца";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                    break;

                // 500
                case MainMenuAction.QuitApplication:
                    MyStatusBarModel.Item0 = "Идет завершение программы...";
                    MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                    break;
                default: break;
            }
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
