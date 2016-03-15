using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.Controls.TagPickingControl;
using Keeper.DomainModel.Transactions;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Controls
{
    class IncomeControlVm
    {
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        public TranWithTags TranInWork { get; set; }
        public AccNameSelectorVm MyAccNameSelectorVm { get; set; }
        public AmountInputControlVm MyAmountInputControlVm { get; set; }
        public TagPickerVm MyTagPickerVm { get; set; }
        public DatePickerWithTrianglesVm MyDatePickerVm { get; set; }

        public string MyAccountBalance => _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        public string AmountInUsd => _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork);

        public IncomeControlVm(BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
        }
    }
}
