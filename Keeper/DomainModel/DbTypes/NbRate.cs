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

        public double Busket1 => Math.Pow(UsdRate, 1.0/3)*Math.Pow(EurRate, 1.0/3)*Math.Pow(RurRate, 1.0/3);
        public double Busket2 => Math.Pow(UsdRate, 0.3)*Math.Pow(EurRate, 0.3)*Math.Pow(RurRate, 0.4);
        public double Busket3 => Math.Pow(UsdRate, 0.3)*Math.Pow(EurRate, 0.2)*Math.Pow(RurRate, 0.5);
    }
}
