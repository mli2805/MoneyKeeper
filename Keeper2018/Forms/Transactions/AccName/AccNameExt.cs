using System.Collections.Generic;

namespace Keeper2018
{
    public static class AccNameExt
    {
        public static AccName FindThroughTheForestById(this List<AccName> roots, int accountId)
        {
            foreach (var root in roots)
            {
                var result = root.FindThroughTreeById(accountId);
                if (result != null) return result;
            }
            return null;
        }

        private static AccName FindThroughTreeById(this AccName accName, int accountId)
        {
            if (accountId == accName.Id) return accName;
            foreach (var child in accName.Children)
            {
                var result = child.FindThroughTreeById(accountId);
                if (result != null) return result;
            }
            return null;
        }

    }
}