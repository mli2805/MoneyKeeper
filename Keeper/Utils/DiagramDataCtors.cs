using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class DiagramDataCtors
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static int GetMonthlyResultData()
    {
      return Db.Accounts.Count;
    }
  }
}
