using System.Collections.Generic;

namespace Keeper2018
{
    public class ButtonCollectionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<AccountModel> AccountModels { get; set; } = new List<AccountModel>();

        public override string ToString()
        {
            return Name;
        }
    }
}