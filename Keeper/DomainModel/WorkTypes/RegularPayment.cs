using System;
using System.Runtime.Serialization;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.WorkTypes
{
  [DataContract]
  public class RegularPayment : ICloneable
  {
    [DataMember]public string Article { get; set; }
    [DataMember]public Decimal Amount { get; set; }
    [DataMember]public CurrencyCodes Currency { get; set; }
    [DataMember]public bool ShouldBeReminded { get; set; }
    [DataMember]public int DayOfMonth { get; set; }

    public object Clone()
    {
      return MemberwiseClone();
    }
  }
}
