using System;
using System.Composition;
using System.Security.AccessControl;
using System.Windows;
using Keeper.ByFunctional.EvaluatingBalances;
using Keeper.DomainModel;
using Keeper.ViewModels.Shell;

namespace Keeper.Models.Shell
{
    [Export]
    [Shared]
    public class ShellModel
    {
        private readonly BalancesForShellCalculator _balancesForShellCalculator;

        public MainMenuModel MyMainMenuModel { get; set; }
        public AccountForestModel MyForestModel { get; set; }
        public BalanceListModel MyBalanceListModel { get; set; }
        public TwoSelectorsModel MyTwoSelectorsModel { get; set; }
        public StatusBarModel MyStatusBarModel { get; set; }

        [ImportingConstructor]
        public ShellModel(BalancesForShellCalculator balancesForShellCalculator)
        {
            _balancesForShellCalculator = balancesForShellCalculator;

            MyMainMenuModel = new MainMenuModel();
            MyMainMenuModel.PropertyChanged += MyMainMenuModelPropertyChanged;
            MyForestModel = new AccountForestModel();
            MyForestModel.PropertyChanged += MyForestModelPropertyChanged;
            MyBalanceListModel = new BalanceListModel();
            MyTwoSelectorsModel = new TwoSelectorsModel();
            MyTwoSelectorsModel.PropertyChanged += MyTwoSelectorsModelPropertyChanged;
            MyStatusBarModel = new StatusBarModel();
        }

        void MyMainMenuModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Action")
            {
                switch (MyMainMenuModel.Action)
                {
                    case Actions.Idle:
                        MyStatusBarModel.Item0 = "Готово";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;

                    case Actions.InputTransactions:
                        MyStatusBarModel.Item0 = "Ввод транзакций";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;
                    case Actions.InputRates:
                        MyStatusBarModel.Item0 = "Ввод курсов валют";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;
                    case Actions.InputAssociates:
                        MyStatusBarModel.Item0 = "Ввод ассоциаций";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;
                    case Actions.ShowAnalisys:
                        MyStatusBarModel.Item0 = "Анализ месяца";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;

                    case Actions.SaveDatabase:
                        MyStatusBarModel.Item0 = "Сохранение данных на диск...";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                        break;
                    case Actions.LoadDatabase:
                        MyStatusBarModel.Item0 = "Загрузка данных с диска...";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                        break;
                    case Actions.CleanDatabase:
                        MyStatusBarModel.Item0 = "Очистка БД...";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                        break;
                    case Actions.RemoveIdenticalBackups:
                        MyStatusBarModel.Item0 = "Удаление идентичных резервных копий...";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                        break;
                    case Actions.PrepareExit:
                        MyStatusBarModel.Item0 = "Идет завершение программы...";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Visible;
                        break;

                    case Actions.RefreshBalanceList:
                        RefreshBalanceListAccordinglyDatesInSelector();
                        MyStatusBarModel.Item0 = "Готово";
                        MyStatusBarModel.ProgressBarVisibility = Visibility.Collapsed;
                        break;

                    default: return;
                }
            }
        }

        private void RefreshBalanceListAccordinglyDatesInSelector()
        {
            if (MyTwoSelectorsModel.IsPeriodMode)
                MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.FillListForShellView(
                  MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList));
            else
                MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.FillListForShellView(
                  MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate),
                  MyBalanceListModel.BalanceList));
        }

        void MyTwoSelectorsModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChangeControlTypeTranslatedEvent") 
                MyTwoSelectorsModel.IsPeriodMode = !MyTwoSelectorsModel.IsPeriodMode;
            else 
                RefreshBalanceListAccordinglyDatesInSelector();
        }

        void MyForestModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedAccount")
            {
                MyBalanceListModel.Caption = MyForestModel.SelectedAccount.Name;
                if (MyTwoSelectorsModel.IsPeriodMode)
                {
                    MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.FillListForShellView(
                      MyForestModel.SelectedAccount, MyTwoSelectorsModel.TranslatedPeriod, MyBalanceListModel.BalanceList));
                }
                else
                {
                    MyBalanceListModel.AccountBalanceInUsd = String.Format("{0:#,#} usd", _balancesForShellCalculator.FillListForShellView(
                      MyForestModel.SelectedAccount, new Period(new DateTime(0), MyTwoSelectorsModel.TranslatedDate), MyBalanceListModel.BalanceList));
                }
            }

            if (e.PropertyName == "OpenedAccountPage")
                MyTwoSelectorsModel.IsPeriodMode = MyForestModel.OpenedAccountPage != 0;
        }

    }
}
