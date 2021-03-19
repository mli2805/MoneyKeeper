using System;

namespace Keeper2018
{
    public class GoldCoinsModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double MinfinGold900Rate { get; set; }
        public double G5RubRate => 3.87 * MinfinGold900Rate;
        public double G10RubRate => 7.74 * MinfinGold900Rate;

        public double BynUsd { get; set; } = 1;
        public double MinfinUsd => MinfinGold900Rate / BynUsd;

        public double G5RubUsd => G5RubRate / BynUsd;
        public double G10RubUsd => G10RubRate / BynUsd;
    }
}
