using Keeper.DomainModel;

namespace Keeper.DbInputOutput
{
	interface IDbSerializer {
		void EncryptAndSerialize(KeeperDb db);
		KeeperDb DecryptAndDeserialize(string filename);
	}
}