using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class ArticlesAssociationsViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<ArticleAssociation> Rows { get; set; }



    public ArticlesAssociationsViewModel()
    {
      Db.Accounts.Load();
      Db.ArticlesAssociations.Load();
      Rows = Db.ArticlesAssociations.Local;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Ассоциации категории";
    }


  }
}
