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
    }
}