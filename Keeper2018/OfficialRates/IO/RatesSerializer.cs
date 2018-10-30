using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace Keeper2018
{
    public static class RatesSerializer
    {
        public static async Task<bool> SerializeRates(List<OfficialRates> rates)
        {
            var path = DbUtils.GetTxtFullPath("OfficialRates.bin");
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, rates);
                }
                await Task.Delay(1);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static async Task<List<OfficialRates>> DeserializeRates()
        {
            var path = DbUtils.GetTxtFullPath("OfficialRates.bin");
            await Task.Delay(1);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return (List<OfficialRates>)binaryFormatter.Deserialize(fStream);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}