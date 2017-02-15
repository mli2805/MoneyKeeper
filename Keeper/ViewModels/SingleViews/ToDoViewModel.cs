using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Caliburn.Micro;
using Keeper.Properties;

namespace Keeper.ViewModels.SingleViews
{
  class ToDoViewModel : Screen
  {
    public static Encoding Encoding1251 = Encoding.GetEncoding(1251);
    public static string FullFileName = Path.Combine(Settings.Default.KeeperFolder, Settings.Default.ToDoFile);

    private ObservableCollection<string> _toDoList;
    private string _newJob;
    private bool _isEscapePressed;

    public ObservableCollection<string> ToDoList
    {
      get { return _toDoList; }
      set
      {
        if (Equals(value, _toDoList)) return;
        _toDoList = value;
        NotifyOfPropertyChange(() => ToDoList);
      }
    }

    public string NewJob
    {
      get { return _newJob; }
      set
      {
        if (value == _newJob) return;
        _newJob = value;
        NotifyOfPropertyChange(() => NewJob);
      }
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "TODO List";

      ToDoList = new ObservableCollection<string>(File.ReadAllLines(FullFileName,Encoding1251));
      _isEscapePressed = false;
    }

    public override void CanClose(Action<bool> callback)
    {
      if (!_isEscapePressed)
         File.WriteAllLines(FullFileName, ToDoList, Encoding1251);
      callback(true);
    }

    public void Add()
    {
      ToDoList.Add(NewJob);
      NewJob = "";
    }

    public void SaveClose()
    {
      TryClose();
    }

    public void CloseWithoutSaving()
    {
      _isEscapePressed = true;
      TryClose();
    }


  }


}
