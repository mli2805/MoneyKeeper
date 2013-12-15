using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput
{
	interface IDbSerializer {
		void EncryptAndSerialize(KeeperDb db);
		KeeperDb DecryptAndDeserialize(string filename);
	}
}