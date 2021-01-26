using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace KeeperDomain
{
    public static class BinSerializer
    {
        private static void MadeDbxBackup(string filename)
        {
            if (!File.Exists(filename)) return;

            var ss = filename.Substring(0, filename.Length - 3);
            var backupFilename = ss + "bac";
            File.Copy(filename, backupFilename, true);
        }

        public static async Task<LibResult> Serialize(KeeperBin bin)
        {
            var path = PathFactory.GetDbFullPath();
            MadeDbxBackup(path);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, bin);
                }
                await Task.Delay(1);
                return new LibResult();
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        public static async Task<LibResult> Deserialize(string path)
        {
            try
            {
                await Task.Delay(1);
                using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    var keeperBin = binaryFormatter.Deserialize(fStream);
                    return new LibResult(true, keeperBin);
                }
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }
    }
}