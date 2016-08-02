﻿using System;
using System.ComponentModel;
using System.Composition;
using System.Security.AccessControl;
using System.Windows;
using Keeper.DomainModel;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.BalancesFromTransWithTags;
using Keeper.ViewModels.Shell;

namespace Keeper.Models.Shell
{
    [Export]
    [Shared]
    public class ShellModel
    {
        private readonly BalancesForShellCalculator _balancesForShellCalculator;
        private readonly BalancesForMainViewCalculator _balancesForMainViewCalculator;

        public MainMenuModel MyMainMenuModel { get; set; }
        public AccountForestModel MyForestModel { get; set; }
        public BalanceListModel MyBalanceListModel { get; set; }
        public TwoSelectorsModel MyTwoSelectorsModel { get; set; }
        public StatusBarModel MyStatusBarModel { get; set; }

        [ImportingConstructor]
        public ShellModel(BalancesForShellCalculator balancesForShellCalculator, BalancesForMainViewCalculator balancesForMainViewCalculator)
        {
            _balancesForShellCalculator = balancesForShellCalculator;
            _balancesForMainViewCalculator = balancesForMainViewCalculator;

            MyMainMenuModel = new MainMenuModel();
            MyMainMenuModel.PropertyChanged += MyMainMenuModelPropertyChanged;
            MyForestModel = new AccountForestModel();
            MyForestModel.PropertyChanged += MyForestModelPropertyChanged;
            MyBalanceListModel = new BalanceListModel();
            MyTwoSelectorsModel = new TwoSelectorsModel();
            MyTwoSelectorsModel.PropertyChanged += MyTwoSelectorsModelPropertyChanged;
            MyStatusBarModel = new StatusBarModel();
        }

        void MyMainMenuModelPropertyChanged(object sender, PropertyChangedEventArgs e)
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

    }
}
