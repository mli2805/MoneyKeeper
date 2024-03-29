﻿using System;
using System.Composition;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows;
using Keeper.DomainModel.DbTypes;
using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput
{
    [Export(typeof(IDbSerializer))]
    [Export(typeof(ILoader))]
    class DbSerializer : IDbSerializer, ILoader
    {
        private void MadeDbxBackup(string filename)
        {
            if (!File.Exists(filename)) return;

            var ss = filename.Split('.');
            var backupFilename = ss[0] + ".bac";
            File.Copy(filename, backupFilename, true);
        }

        public void EncryptAndSerialize(KeeperDb db, string filename)
        {
            try
            {
                MadeDbxBackup(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            byte[] key = { 0xc5, 0x51, 0xf6, 0x4e, 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0x62, 0x3f, 0x27, 0x00, 0xca };
            byte[] initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0x8c, 0xb4, 0x92, 0xd0, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0xd8 };

            try
            {
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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

        public string FileExtension => ".dbx";

        public DbLoadResult Load(string filename)
        {
            var db = DecryptAndDeserialize(filename);
            return db == null ? new DbLoadResult(0x11, "Problem with dbx file!") : new DbLoadResult(db);
        }
    }
}

// кроме того пробовал

// SOAP serialization - не обрабатывает дженерики

// XML serialization
// дженерик проглотила нормально
// сломалась на дереве счетов - Account содержит Account

// DataContractJsonSerializer тоже сломался на счетах (цикл)
// Newtonsoft.Json  тоже сломался на счетах (цикл)