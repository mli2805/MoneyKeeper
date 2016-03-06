using System;
using System.Collections.ObjectModel;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.DomainModel.DbTypes
{
    /// <summary>
    /// <see cref="DbExporter"/> is factory for the KeeperDb
    /// </summary>
    [Serializable]
    public class KeeperDb
    {
        public ObservableCollection<Account> Accounts { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<CurrencyRate> CurrencyRates { get; set; }
        public ObservableCollection<NbRate> OfficialRates { get; set; }
        public ObservableCollection<ArticleAssociation> ArticlesAssociations { get; set; }
        public ObservableCollection<BankDepositOffer> BankDepositOffers { get; set; }

        public ObservableCollection<TranWithTags> TransWithTags { get; set; }
    }
}
