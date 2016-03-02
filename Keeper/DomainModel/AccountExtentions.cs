namespace Keeper.DomainModel
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
                destination.Deposit = (Keeper.DomainModel.Deposit.Deposit)source.Deposit.Clone();
                destination.Deposit.ParentAccount = destination;
            }

            foreach (var account in source.Children)
            {
                var child = new Account();
                CloneFrom(child, account);
                destination.Children.Add(child);
            }
        }
    }
}