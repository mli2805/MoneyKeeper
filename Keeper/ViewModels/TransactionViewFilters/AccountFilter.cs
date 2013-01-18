using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class AccountFilter
  {
    public bool IsOn { get; set; }
    public Account WantedAccount { get; set; }

    /// <summary>
    /// таким конструктором создается ВЫключенный фильтр
    /// ему не нужен счет, он пропускает все счета
    /// </summary>
    public AccountFilter() { IsOn = false; }

    /// <summary>
    /// а такой фильтр пропускает только искомый account
    /// </summary>
    /// <param name="account"></param>
    public AccountFilter(Account account)
    {
      IsOn = true;
      WantedAccount = account;
    }

    public override string ToString()
    {
      return IsOn ? WantedAccount.ToString() : "<no filter>";
    }
  }
}
