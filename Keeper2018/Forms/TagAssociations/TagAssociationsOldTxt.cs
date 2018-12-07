using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class TagAssociationsOldTxt
    {
        public static async Task<List<TagAssociation>> LoadFromOldTxtAsync(List<Account> accountsPlaneList)
        {
            await Task.Delay(1);
            return LoadFromOldTxt(accountsPlaneList).ToList();
        }

        private static IEnumerable<TagAssociation> LoadFromOldTxt(List<Account> accountsPlaneList)
        {
            var content = File.ReadAllLines(DbUtils.GetOldTxtFullPath("ArticlesAssociations.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

            foreach (var line in content)
            {
                var oneAssociation = TagAssociationFromString(line, accountsPlaneList);
                yield return (oneAssociation);
            }
        }

        private static TagAssociation TagAssociationFromString(string s, List<Account> accountsPlaneList)
        {
            var association = new TagAssociation();
            var substrings = s.Split(';');
            association.ExternalAccount = accountsPlaneList.First(account => account.Name == substrings[0].Trim()).Id;
            association.Tag = accountsPlaneList.First(account => account.Name == substrings[1].Trim()).Id;
            association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[2]);
            association.Destination = bool.Parse(substrings[3]) ? AssociationType.TwoWay : AssociationType.LeftToRight;
            return association;
        }
    }

   
}
