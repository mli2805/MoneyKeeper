using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Models;
using Keeper.Properties;
using Keeper.Utils.Balances;
using Keeper.Utils.Common;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.ViewModels.Shell
{
  [Export(typeof(IShell))] // это для загрузчика, который ищет главное окно проги
  [Export(typeof(ShellViewModel))]
  [Shared] // это для класса Account чтобы засунуть в свойство SelectedAccountInShell 
  public class ShellViewModel : Screen, IShell
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    public MainMenuViewModel MainMenuViewModel { get; set; }
    public AccountForestViewModel AccountForestViewModel { get; set; }
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; set; }
    public BalanceListViewModel BalanceListViewModel { get; set; }

    private readonly ShellModel _shellModel;
    private KeeperDb _db;
    readonly DbLoadResult _loadResult;
    private readonly List<Screen> _launchedForms = new List<Screen>();
    private bool _isDbLoadingSuccessed;

    private readonly IDbToTxtSaver _txtSaver;
    private readonly DbBackuper _backuper;
    readonly IDbFromTxtLoader _dbFromTxtLoader;
    private readonly BalancesForShellCalculator _balanceCalculator;

    #region  поля/свойства в классе Модели к которым биндятся визуальные элементы из Вью
    private Account _selectedAccountInShell;
    public Account SelectedAccountInShell
    {
      get { return _selectedAccountInShell; }
      set
      {
        _selectedAccountInShell = value;
        _shellModel.MyTwoSelectorsModel.IsPeriodMode = !value.Is("Мои");
        Period period = value.Is("Мои") ? new Period(new DateTime(0), BalanceDate) : PaymentsPeriod;
//        AccountBalanceInUsd = String.Format("{0:#,#} usd", _balanceCalculator.CountBalances(SelectedAccountInShell, period, BalanceList));
        NotifyOfPropertyChange(() => SelectedAccountInShell);
      }
    }



//    #region BalancesList
//    public ObservableCollection<string> BalanceList { get; set; }
//    private string _accountBalanceInUsd;
//    public string AccountBalanceInUsd
//    {
//      get { return _accountBalanceInUsd; }
//      private set
//      {
//        if (value == _accountBalanceInUsd) return;
//        _accountBalanceInUsd = value;
//        NotifyOfPropertyChange(() => AccountBalanceInUsd);
//      }
//    }
//    #endregion

    #region DatesSelection

    private DateTime _balanceDate;
    private Period _paymentsPeriod;
//    private Visibility _balanceDateSelectControl;
//    private Visibility _paymentsPeriodSelectControl;
    #endregion

    #region StatusBar
    private string _message;
    public string Message
    {
      get { return _message; }
      private set
      {
        if (value == _message) return;
        _message = value;
        NotifyOfPropertyChange(() => Message);
      }
    }

    private string _statusBarItem0;
    public string StatusBarItem0
    {
      get { return _statusBarItem0; }
      private set
      {
        if (value.Equals(_statusBarItem0)) return;
        _statusBarItem0 = value;
        NotifyOfPropertyChange(() => StatusBarItem0);
      }
    }

    private Visibility _isProgressBarVisible;
    public Visibility IsProgressBarVisible
    {
      get { return _isProgressBarVisible; }
      private set
      {
        if (Equals(value, _isProgressBarVisible)) return;
        _isProgressBarVisible = value;
        NotifyOfPropertyChange(() => IsProgressBarVisible);
      }
    }
    #endregion

    #endregion


    [ImportingConstructor]
    public ShellViewModel(ShellModel shellModel, KeeperDb db, DbLoadResult loadResult, BalancesForShellCalculator balancesForShellCalculator,
       IDbToTxtSaver txtSaver, DbBackuper backuper, IDbFromTxtLoader dbFromTxtLoader)
    {
      _shellModel = shellModel;
      _db = db;
      _loadResult = loadResult;

      _isDbLoadingSuccessed = _db != null;
      if (!_isDbLoadingSuccessed)
      {
        MessageBox.Show(_loadResult.Explanation + "\nApplication will be closed!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      MainMenuViewModel = IoC.Get<MainMenuViewModel>();
      StatusBarItem0 = "Idle";
      IsProgressBarVisible = Visibility.Collapsed;

      InitBalanceControls();

      _balanceCalculator = balancesForShellCalculator;
      _txtSaver = txtSaver;
      _backuper = backuper;
      _dbFromTxtLoader = dbFromTxtLoader;

      AccountForestViewModel = IoC.Get<AccountForestViewModel>();
      TwoSelectorsViewModel = IoC.Get<TwoSelectorsViewModel>();
      _shellModel.MyTwoSelectorsModel.IsPeriodMode = false;
      _shellModel.MyTwoSelectorsModel.PropertyChanged += TwoSelectorsViewModel_PropertyChanged;

      BalanceListViewModel = IoC.Get<BalanceListViewModel>();
    }

    void TwoSelectorsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "TranslatedDate")
      {
        var BalanceList = new ObservableCollection<string>();
        var AccountBalanceInUsd = String.Format("{0:#,#} usd",
                  _balanceCalculator.CountBalances(SelectedAccountInShell, new Period(new DateTime(0), _shellModel.MyTwoSelectorsModel.TranslatedDate), BalanceList));
      }
      if (e.PropertyName == "TranslatedDate")
      {
        var BalanceList = new ObservableCollection<string>();
        var AccountBalanceInUsd = String.Format("{0:#,#} usd",
                  _balanceCalculator.CountBalances(SelectedAccountInShell, new Period(new DateTime(0), _shellModel.MyTwoSelectorsModel.TranslatedDate), BalanceList));
      }
    }

    private void InitBalanceControls()
    {
      _balanceDate = DateTime.Today.AddDays(1).AddSeconds(-1);
      _paymentsPeriod = new Period(new DayProcessor(DateTime.Today).BeforeThisDay(), new DayProcessor(DateTime.Today).AfterThisDay());
//      BalanceList = new ObservableCollection<string>();
      _isDbLoadingSuccessed = true;
    }

    protected override void OnViewLoaded(object view)
    {
      if (!_isDbLoadingSuccessed)
      {
        TryClose();
        return;
      }
      DisplayName = "Keeper (c) 2012-13";
      Message = DateTime.Today.ToString("dddd , dd MMMM yyyy");

      if (!ShowLogonForm()) TryClose();
    }

    public override async void CanClose(Action<bool> callback)
    {
      if (_isDbLoadingSuccessed)
      {
        foreach (var launchedForm in _launchedForms.Where(launchedForm => launchedForm.IsActive))
          launchedForm.TryClose();
        await Task.Run(() => SerializeWithProgressBar());
        //        await Task.Run(() => MakeBackupWithProgressBar());
        StatusBarItem0 = "Idle";
        IsProgressBarVisible = Visibility.Collapsed;
      }
      callback(true);
    }


    private void SerializeWithProgressBar()
    {
      StatusBarItem0 = "Сохранение данных на диск";
      IsProgressBarVisible = Visibility.Visible;
      new DbSerializer().EncryptAndSerialize(_db, Path.Combine(Settings.Default.DbPath, Settings.Default.DbxFile));
    }

//    private void RefreshBalanceList()
//    {
//       по возвращении на главную форму пересчитать остаток/оборот по выделенному счету/категории
//      var period = SelectedAccountInShell.Is("Мои")
//                     ? new Period(new DateTime(0), new DayProcessor(BalanceDate).AfterThisDay())
//                     : PaymentsPeriod;
//      _balanceCalculator.CountBalances(SelectedAccountInShell, period, BalanceList);
//      if (SelectedAccountInShell.Is("Мои")) BalanceDate = BalanceDate;
//      else PaymentsPeriod = PaymentsPeriod;
//    }

    // сохраняет резервную копию БД в текстовом виде , в шифрованный zip
    private void MakeBackupWithProgressBar()
    {
      StatusBarItem0 = "Создание резервной копии БД";
      IsProgressBarVisible = Visibility.Visible;
      _backuper.MakeDbBackupCopy();
    }

    public bool ShowLogonForm()
    {
      var logonViewModel = new LogonViewModel("1");
      WindowManager.ShowDialog(logonViewModel);
      return logonViewModel.Result;
    }

    public void ProgramExit()
    {
      TryClose();
    }

    #region date\period selection properties


    public DateTime BalanceDate
    {
      get { return _balanceDate; }
      set
      {
        _balanceDate = new DayProcessor(value.Date).AfterThisDay();
//        AccountBalanceInUsd = String.Format("{0:#,#} usd",
//          _balanceCalculator.CountBalances(SelectedAccountInShell, new Period(new DateTime(0), _balanceDate), BalanceList));
      }
    }

    public Period PaymentsPeriod
    {
      get { return _paymentsPeriod; }
      set
      {
        _paymentsPeriod = value;
//        AccountBalanceInUsd = string.Format("{0:#,#} usd",
//                          _balanceCalculator.CountBalances(SelectedAccountInShell, _paymentsPeriod, BalanceList));
      }
    }

    #endregion
  }
}
