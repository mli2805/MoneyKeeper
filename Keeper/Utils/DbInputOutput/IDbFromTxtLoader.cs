using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	interface IDbFromTxtLoader {
		DbLoadError LoadFromLastZip(ref KeeperDb db);
    DbLoadError LoadDbFromTxt(ref KeeperDb db, string path);
		void UnzipAllTables();
	}
}