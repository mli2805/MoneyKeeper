using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace Keeper2018
{
    public static class DbSerializer
    {
        private static void MadeDbxBackup(string filename)
        {
            if (!File.Exists(filename)) return;

            var ss = filename.Substring(0, filename.Length - 3);
            var backupFilename = ss + "bac";
            File.Copy(filename, backupFilename, true);
        }

        public static async Task<bool> Serialize(KeeperBin bin)
        {
            var path = DbUtils.GetDbFullPath();
            MadeDbxBackup(path);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, bin);
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

        public static async Task<KeeperBin> Deserialize()
        {
            var path = DbUtils.GetDbFullPath();
            if (!File.Exists(path)) return null;
            await Task.Delay(1);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return (KeeperBin)binaryFormatter.Deserialize(fStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}