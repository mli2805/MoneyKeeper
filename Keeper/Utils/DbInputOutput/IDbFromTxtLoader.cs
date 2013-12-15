using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Keeper.DomainModel;

namespace Keeper.DbInputOutput
{
	interface IDbFromTxtLoader {
		DbLoadError LoadFromLastZip(ref KeeperDb db);
		DbLoadError LoadDbFromTxt(ref KeeperDb db);
		void UnzipAllTables();
		ObservableCollection<T> LoadFrom<T>(string filename, Func<string, List<Account>, T> parseLine, List<Account> accountsPlaneList);
		bool LoadAccounts(ref KeeperDb db);
	}
}