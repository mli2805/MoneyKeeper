using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    interface IOneTranView
    {
        TranWithTags GetTran();
        void SetTran(TranWithTags tran);
    }
}
