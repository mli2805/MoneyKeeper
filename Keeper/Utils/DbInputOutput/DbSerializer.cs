using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.DbInputOutput
{
	[Export(typeof(IDbSerializer))]
  class DbSerializer : IDbSerializer
	{

    public void EncryptAndSerialize(KeeperDb db)
    {
      byte[] key = { 0xc5, 0x51, 0xf6, 0x4e, 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0x62, 0x3f, 0x27, 0x00, 0xca };
      byte[] initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0x8c, 0xb4, 0x92, 0xd0, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0xd8 };

      if (!Directory.Exists(Settings.Default.SavePath)) Directory.CreateDirectory(Settings.Default.SavePath);
      var filename = Path.Combine(Settings.Default.SavePath, "Keeper.dbx");
      using (Stream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
      {
        var rmCrypto = new RijndaelManaged();

        using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateEncryptor(key, initVector), CryptoStreamMode.Write))
        {
          var binaryFormatter = new BinaryFormatter();
          binaryFormatter.Serialize(cryptoStream, db);
        }
      }
    }

    public KeeperDb DecryptAndDeserialize(string filename)
    {
      byte[] key = { 0xc5, 0x51, 0xf6, 0x4e, 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0x62, 0x3f, 0x27, 0x00, 0xca };
      byte[] initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0x8c, 0xb4, 0x92, 0xd0, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0xd8 };

      KeeperDb db;

      using (Stream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        var rmCrypto = new RijndaelManaged();

        using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateDecryptor(key, initVector), CryptoStreamMode.Read))
        {
          var binaryFormatter = new BinaryFormatter();
          db = (KeeperDb)binaryFormatter.Deserialize(cryptoStream);
        }
      }
      return db;
    }
  }
}

// кроме того пробовал

// SOAP serialization - не обрабатывает дженерики

// XML serialization
// дженерик проглотила нормально
// сломалась на дереве счетов - Account содержит Account
