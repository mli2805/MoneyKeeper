using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper.DomainModel
{
  public interface IKeeperDb {
    ObservableCollection<Account> Accounts { get; set; }
    ObservableCollection<Transaction> Transactions { get; set; }
    ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
    ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }
    List<Account> AccountsPlaneList { get; set; }
  }
}