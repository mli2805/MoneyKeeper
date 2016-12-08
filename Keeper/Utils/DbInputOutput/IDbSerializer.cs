using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.DbInputOutput
{
    interface IDbSerializer
    {
        void EncryptAndSerialize(KeeperDb db, string filename);
        KeeperDb DecryptAndDeserialize(string filename);
    }
}