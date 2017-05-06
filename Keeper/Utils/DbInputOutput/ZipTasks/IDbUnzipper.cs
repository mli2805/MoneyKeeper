using Keeper.Utils.DbInputOutput.CompositeTasks;

namespace Keeper.Utils.DbInputOutput.ZipTasks
{
  internal interface IDbUnzipper
  {
	  DbLoadResult UnzipArchive(string zipFilename);
  }
}