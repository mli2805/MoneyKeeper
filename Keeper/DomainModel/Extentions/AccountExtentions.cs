using System.Collections.Generic;
using Keeper.DomainModel.DbTypes;

namespace Keeper.DomainModel.Extentions
{
    public static class AccountExtentions
    {
        public static void CloneFrom(this Account destination, Account source)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.IsFolder = source.IsFolder;
            destination.IsClosed = source.IsClosed;
            destination.Parent = source.Parent;

            if (source.Deposit != null)
            {
                destination.Deposit = (Deposit.Deposit)source.Deposit.Clone();
                destination.Deposit.ParentAccount = destination;
            }

            foreach (var account in source.Children)
            {
                var child = new Account();
                CloneFrom(child, account);
                destination.Children.Add(child);
            }
        }


        public static Account CloneAccount(Account source)
        {
            var result = new Account();

            result.Id = source.Id;
            result.Name = source.Name + "33";
            result.IsClosed = source.IsClosed;
            result.IsFolder = source.IsFolder;

            foreach (var sourceChild in source.Children)
            {
                var resultChild = CloneAccount(sourceChild);
                resultChild.Parent = result;
                result.Children.Add(resultChild);
            }

            return result;
        }
        public static List<Account> CloneForest(List<Account> source)
        {
            var resultForest = new List<Account>();

            foreach (var root in source)
            {
                var resultRoot = CloneAccount(root);
                resultForest.Add(resultRoot);
            }

            return resultForest;
        }
    }
}