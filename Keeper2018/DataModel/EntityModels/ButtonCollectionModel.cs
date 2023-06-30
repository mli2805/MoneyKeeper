using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class ButtonCollectionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<AccountItemModel> AccountModels { get; set; } = new List<AccountItemModel>();

        public override string ToString()
        {
            return Name;
        }

        public Dictionary<string, int> ToButtonsDictionary()
        {
            return AccountModels.ToDictionary(account => account.ButtonName, account => account.Id);
        }
    }
}