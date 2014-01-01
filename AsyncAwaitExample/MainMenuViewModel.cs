using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace AsyncAwaitExample
{
  class MainMenuViewModel : Screen
  {
    public MainMenuViewModel()
    {
    }

    public string Str { get; set; }

    public void SaveDatabase()
    {
      MessageBox.Show("SaveDatabase");
    }
  }
}
