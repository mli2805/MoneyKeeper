using System;

namespace Keeper.DomainModel.DbTypes
{
    [Serializable]
    public class NbRate
    {
        public DateTime Date { get; set; }
        public double UsdRate { get; set; }
        public double EurRate { get; set; }
        public double RurRate { get; set; }

        public double Busket { get { return Math.Pow(UsdRate, 0.3)*Math.Pow(EurRate, 0.3)*Math.Pow(RurRate, 0.4); } }
    }
}
