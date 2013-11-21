using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;

namespace Keeper.DbInputOutput
{
  class BinaryCrypto
  {
    public static KeeperDb Db
    {
      get { return IoC.Get<KeeperDb>(); }
      set 
      { 
        Db.Accounts  = value.Accounts;
        Db.ArticlesAssociations = value.ArticlesAssociations;
        Db.CurrencyRates = value.CurrencyRates;

        // сеттер переделан ради сортировки транзакций
        Db.Transactions = new ObservableCollection<Transaction>();
        var transactions = from transaction in value.Transactions orderby transaction.Timestamp select transaction;
        foreach (var transaction in transactions)
        {
          Db.Transactions.Add(transaction);
        }

        Db.AccountsPlaneList = KeeperDb.FillInAccountsPlaneList(Db.Accounts);
      }
    }

    public static void DbCryptoSerialization()
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
          binaryFormatter.Serialize(cryptoStream, Db);
        }
      }
    }

    public static bool DbCryptoDeserialization(string filename)
    {
      byte[] key = { 0xc5, 0x51, 0xf6, 0x4e, 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0x62, 0x3f, 0x27, 0x00, 0xca };
      byte[] initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0x8c, 0xb4, 0x92, 0xd0, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0xd8 };

      using (Stream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        var rmCrypto = new RijndaelManaged();

        using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateDecryptor(key, initVector), CryptoStreamMode.Read))
        {
          var binaryFormatter = new BinaryFormatter();
          Db = (KeeperDb)binaryFormatter.Deserialize(cryptoStream);
        }
      }
      return true;
    }
  }
}

// кроме того пробовал

// SOAP serialization - не обрабатывает дженерики

// XML serialization
// дженерик проглотила нормально
// сломалась на дереве счетов - Account содержит Account
