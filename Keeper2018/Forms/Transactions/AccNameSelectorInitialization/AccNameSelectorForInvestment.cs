﻿using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public partial class AccNameSelector
    {
        public AccNameSelectorVm InitializeForInvestments(InvestTranModel tran)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                case InvestOperationType.PayBaseCommission:
                case InvestOperationType.PayBuySellFee:
                case InvestOperationType.PayWithdrawalTax:
                    return Build("Откуда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 3).ToButtonsDictionary(),
                        _comboTreesProvider.AccNamesForInvestmentExpense, tran.AccountModel?.Id ?? 695);

                case InvestOperationType.EnrollCouponOrDividends:
                    return Build("Откуда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 3).ToButtonsDictionary(),
                        _comboTreesProvider.AccNamesForInvestmentIncome, tran.AccountModel?.Id ?? 696);
                default:
                case InvestOperationType.WithdrawFromTrustAccount:
                    return Build("Куда", 
                        _dataModel.ButtonCollections.First(c => c.Id == 3).ToButtonsDictionary(),
                        _comboTreesProvider.AccNamesForInvestmentExpense, tran.AccountModel?.Id ?? 829);
            }
        }
    }
}