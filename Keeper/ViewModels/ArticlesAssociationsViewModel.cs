using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class ArticlesAssociationsViewModel : Screen
  {
    private KeeperDb _db;

    public ObservableCollection<ArticleAssociation> Rows { get; set; }

    public ArticlesAssociationsViewModel(KeeperDb db)
    {
      _db = db;
      Rows = _db.ArticlesAssociations;

      var view = CollectionViewSource.GetDefaultView(Rows);
      view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Ассоциации категорий";
    }

    public void CloseView()
    {
      TryClose();
    }

  }
}
