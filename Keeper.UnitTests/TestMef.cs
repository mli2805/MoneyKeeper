using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using FluentAssertions;

using Keeper.DomainModel;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
	[TestFixture]
	public sealed class TestMef
	{
		[Export]
		[PartCreationPolicy(CreationPolicy.Shared)]
		internal class Importer
		{
			[Import]
			public KeeperDb Db { get; set; }
		}
		[Export]
		[PartCreationPolicy(CreationPolicy.Shared)]
		internal class Exporter
		{
			[Export]
			public KeeperDb Db { get; set; }
		}
		CompositionContainer mCompositionContainer;

		[SetUp]
		public void Setup()
		{
			mCompositionContainer = new CompositionContainer(new AssemblyCatalog(typeof(Exporter).Assembly));
		}

		[Test]
		public void Export_When_Property_Is_Null_Should_Work_And_Import_Null()
		{
			mCompositionContainer.GetExport<Importer>().Value.Db.Should().BeNull();
		}

		[Test]
		public void Export_When_Property_Is_Not_Null_Should_Work_And_Import_Not_Null()
		{
			mCompositionContainer.GetExport<Exporter>().Value.Db = new KeeperDb();
			mCompositionContainer.GetExport<Importer>().Value.Db.Should().NotBeNull();
		}
	}
}