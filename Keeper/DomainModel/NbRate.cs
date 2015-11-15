using System;

namespace Keeper.DomainModel
{
    [Serializable]
    public class NbRate
    {
        public DateTime Date { get; set; }
        public double UsdRate { get; set; }
        public double EurRate { get; set; }
        public double RurRate { get; set; }
    }
}
