using System.Composition;
using System.IO;
using System.Runtime.Serialization.Json;
using Keeper.Utils.Common;

namespace Keeper.DomainModel
{
    [Export]
    [Shared]
    public class RegularPaymentsProvider
    {
        private readonly string _filename;

        [Export]
        public RegularPayments RegularPayments { get; set; }

        [ImportingConstructor]
        public RegularPaymentsProvider(IMySettings mySettings)
        {
            _filename = mySettings.GetCombinedSetting("RegularPaymentsFileFullPath");
            RegularPayments = Read() ?? new RegularPayments();
        }

        private RegularPayments Read()
        {
            if (!File.Exists(_filename)) return null;
            var stream = new FileStream(_filename, FileMode.Open);
            var jsonSerializer = new DataContractJsonSerializer(typeof(RegularPayments));
            return (RegularPayments)jsonSerializer.ReadObject(stream);
        }

        public void Write()
        {
            var stream = new FileStream(_filename, FileMode.Create);
            var jsonSerializer = new DataContractJsonSerializer(typeof(RegularPayments));
            jsonSerializer.WriteObject(stream, RegularPayments);

        }
    }
}