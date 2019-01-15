using System;
using System.IO;
using Autofac;

namespace Keeper2018.Tests
{
    public class SystemUnderTest
    {
        public readonly ILifetimeScope GlobalScope;
        public KeeperDb Db { get; set; }


        public SystemUnderTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacKeeper>();
            Db = new KeeperDb();
            builder.RegisterInstance(Db);
            var container = builder.Build();
            GlobalScope = container.BeginLifetimeScope();
        }

        public void DeserializeDb()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(basePath, @"..\..\sut\KeeperDb.bin");
            Db.Bin = DbSerializer.Deserialize(fullPath);
        }
    }
}
