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

        public static Task<LibResult> Serialize(KeeperBin bin)
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
                return Task.FromResult(new LibResult());
            }
            catch (Exception e)
            {
                return Task.FromResult(new LibResult(e, "Bin.Serialize"));
            }
        }

        public static Task<LibResult> Deserialize(string path)
        {
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    var keeperBin = binaryFormatter.Deserialize(fStream);
                    return Task.FromResult(new LibResult(true, keeperBin));
                }
            }
            catch (Exception e)
            {
                return Task.FromResult(new LibResult(e));
            }
        }
    }
}