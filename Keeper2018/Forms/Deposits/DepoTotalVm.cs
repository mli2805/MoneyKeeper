using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public class DepoTotalVm
    {
        public string Pieces { get; set; }
        public CurrencyCode Currency { get; set; }
        public DepoPartTotalVm Depo { get; set; } = new DepoPartTotalVm();
        public DepoPartTotalVm DepoAndMatras { get; set; } = new DepoPartTotalVm();
        public DepoPartTotalVm AllMine { get; set; } = new DepoPartTotalVm();
        public bool IsAggregateLine { get; set; }

        public Brush Fs => 
            IsAggregateLine 
            ? Brushes.Aquamarine 
            : Brushes.LightCyan;
    }

    public class DepoPartTotalVm
    {
        public int Pieces { get; set; }
        public string PiecesStr => Pieces == 0 ? "" : $"{Pieces} депо";

        public CurrencyCode Currency { get; set; }
        public decimal Sum { get; set; }
        public string SumStr => Sum == 0 ? "" : $"{Sum:N2} {Currency.ToString().ToLower()}";

        public decimal SumUsd { get; set; }
        public string SumUsdStr => SumUsd == 0 ? "" :  $"{SumUsd:N2} usd";

        public decimal Percent { get; set; }
        public string PercentStr => Sum == 0 ? "" :  $"{(Percent * 100):F1} %";


        public DepoPartTotalVm Clone()
        {
            return new DepoPartTotalVm()
            {
                Pieces = Pieces,
                Currency = Currency,
                Sum = Sum,
                SumUsd = SumUsd,
                Percent = Percent,
            };
        }
    }
}
