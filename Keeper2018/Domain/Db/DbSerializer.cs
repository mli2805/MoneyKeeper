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

            var ss = filename.Split('.');
            var backupFilename = ss[0] + ".bac";
            File.Copy(filename, backupFilename, true);
        }

     
        public static async Task<bool> Serialize(KeeperDb db)
        {
            var serializableKeeperDb = new SerializableKeeperDb(db);

            var path = DbUtils.GetTxtFullPath("KeeperDb.bin");
            MadeDbxBackup(path);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, serializableKeeperDb);
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

        public static async Task<KeeperDb> Deserialize()
        {
            var path = DbUtils.GetTxtFullPath("KeeperDb.bin");
            await Task.Delay(1);
            try
            {
                using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    var serializableKeeperDb = (SerializableKeeperDb)binaryFormatter.Deserialize(fStream);
                    return serializableKeeperDb.GetKeeperDb();
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