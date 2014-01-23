using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
  [Export]
  class ArticlesAssociationsViewModel : Screen
  {
    private readonly KeeperDb _db;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    public ObservableCollection<ArticleAssociation> Rows { get; set; }
    public static List<Account> ExternalAccounts { get; private set; }
    public static List<Account> AssociatedArticles { get; private set; }

    [ImportingConstructor]
    public ArticlesAssociationsViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _accountTreeStraightener = accountTreeStraightener;

      InitializeListsForCombobox();

      Rows = _db.ArticlesAssociations;

      var view = CollectionViewSource.GetDefaultView(Rows);
      view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
    }

    private void InitializeListsForCombobox()
    {
      ExternalAccounts =
        (_accountTreeStraightener.Flatten(_db.Accounts).Where(
          account => account.Is("Внешние") && account.Children.Count == 0)).ToList();
      AssociatedArticles = (_accountTreeStraightener.Flatten(_db.Accounts).Where(account =>
                                                                                 (account.GetRootName() == "Все доходы" ||
                                                                                  account.GetRootName() == "Все расходы") &&
                                                                                 account.Children.Count == 0)).ToList();
    }

    protected override void OnViewLoaded(object view)
    {
      InitializeListsForCombobox();
      DisplayName = "Ассоциации категорий";
    }

    public void CloseView()
    {
      TryClose();
    }

  }
}
