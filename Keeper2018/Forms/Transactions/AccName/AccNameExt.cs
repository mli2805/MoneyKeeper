using System.Collections.Generic;

namespace Keeper2018
{
    public static class AccNameExt
    {
        public static AccName FindThroughTheForest(this List<AccName> roots, string name)
        {
            foreach (var root in roots)
            {
                var result = root.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

        private static AccName FindThroughTree(this AccName accName, string name)
        {
            if (name == accName.Name) return accName;
            foreach (var child in accName.Children)
            {
                var result = child.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

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