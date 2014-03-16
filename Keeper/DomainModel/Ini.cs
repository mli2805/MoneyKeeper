using System.Composition;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Keeper.Utils.Common;

namespace Keeper.DomainModel
{
  [DataContract]
  public class MonthlyTraffic
  {
    [DataMember] public decimal SalaryFull { get; set; }
    [DataMember] public decimal SalaryCard { get; set; }
    [DataMember] public decimal ApartmentsToLet { get; set; }
    [DataMember] public decimal GskPayment { get; set; }
  }

  [DataContract]
  public class Ini
  {
    [DataMember] public MonthlyTraffic MonthlyTraffic { get; set; }
    [DataMember] public int GskPaymentAlarmDay { get; set; }
    [DataMember] public int UtilityPaymentsAlarmDay { get; set; }
  }

  [Export]
  [Shared]
  public class IniProvider
  {
    [Export]
    public Ini MyIni { get; set; }

    private readonly IMySettings _mySettings;
    private readonly string _filename;

    [ImportingConstructor]
    public IniProvider(IMySettings mySettings)
    {
      _mySettings = mySettings;
      _filename = Path.Combine((string)_mySettings.GetSetting("DbPath"), (string)_mySettings.GetSetting("IniFile"));

      MyIni = ReadIni() ?? new Ini();
    }

    public void WriteIni()
    {
      var stream = new FileStream(_filename, FileMode.Create);
      var jsonSerializer = new DataContractJsonSerializer(typeof(Ini));
      jsonSerializer.WriteObject(stream, MyIni);
    }

    public Ini ReadIni()
    {
      if (!File.Exists(_filename)) return null;
      var stream = new FileStream(_filename, FileMode.Open);
      var jsonSerializer = new DataContractJsonSerializer(typeof(Ini));
      return (Ini)jsonSerializer.ReadObject(stream);
    }

  }
}
