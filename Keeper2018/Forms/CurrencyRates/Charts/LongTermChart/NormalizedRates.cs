namespace Keeper2018
{
    public class NormalizedRates
    {
        public double UsdNb;
        public double EurNb;
        public double RubNb;
        public double EurUsd => EurNb / UsdNb;

        public double UsdMy;
        public double RubUsd;
    }
}