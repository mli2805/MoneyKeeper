using System;
using System.Windows.Media;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrOutcomeWithTransfer : TrExchange
    {
        public override Brush TransactionFontColor => Brushes.Red; 
    }
}