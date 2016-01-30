using System.Windows.Media;

namespace Keeper.DomainModel.Transactions
{
    public class TrOutcomeWithTransfer : TrExchange
    {
        public override Brush TransactionFontColor => Brushes.Red; 
    }
}