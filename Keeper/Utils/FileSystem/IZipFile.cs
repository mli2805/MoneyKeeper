using System;
using System.Collections.Generic;

namespace Keeper.Utils.FileSystem
{
	public interface IZipFile : IEnumerable<IZipEntry>, IDisposable {}
}