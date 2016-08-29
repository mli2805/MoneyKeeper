using System.Composition;
using System.Composition.Hosting;

using FluentAssertions;

using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.Utils.MEF;

using NUnit.Framework;

namespace Keeper.UnitTests
{
	[TestFixture]
	public sealed class TestMef
	{
		CompositionHost mContainer;
		[Export]
		[Shared]
		internal class Importer
		{
			[Import]
			public KeeperDb Db { get; set; }
		}
		[Export]
		[Shared]
		internal class Exporter
		{
			[Export]
			public KeeperDb Db { get; set; }
		}

		[SetUp]
		public void Setup()
		{
			mContainer = new ContainerBuilder()
				.WithAssembly(typeof(Exporter).Assembly).Build();
		}

		[Test]
		public void Export_When_Property_Is_Null_Should_Work_And_Import_Null()
		{
			mContainer.GetExport<Importer>().Db.Should().BeNull();
		}

		[Test]
		public void Export_When_Property_Is_Not_Null_Should_Work_And_Import_Not_Null()
		{
			mContainer.GetExport<Exporter>().Db = new KeeperDb();
			mContainer.GetExport<Importer>().Db.Should().NotBeNull();
		}
	}
}