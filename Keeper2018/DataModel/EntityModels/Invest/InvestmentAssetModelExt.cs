using System;

namespace Keeper2018
{
    public static class InvestmentAssetModelExt
    {
        private static DateTime GetPreviousCouponDate(this InvestmentAssetModel asset)
        {
            DateTime temp = asset.BondExpirationDate;
            while (temp.AddDays(- asset.BondCouponPeriodDays) > DateTime.Today)
            {
                temp = temp.AddDays(-asset.BondCouponPeriodDays);
            }
            return temp;
        }

        public static decimal GetNkd(this InvestmentAssetModel asset)
        {
            var previousCouponDate = asset.GetPreviousCouponDate();
            var days = (DateTime.Today - previousCouponDate).Days;
            return asset.Nominal * (decimal)asset.CouponRate / 100 / 365 * days;
        }
    }
}