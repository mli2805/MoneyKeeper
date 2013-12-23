namespace Keeper.Utils.DbInputOutput
{
  internal interface IDbUnzipper
  {
	  DbLoadResult UnzipArchive(string zipFilename);
  }
}