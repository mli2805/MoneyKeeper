﻿using System;

using Keeper.Utils;

using NUnit.Framework;

using FluentAssertions;

namespace Keeper.UnitTests.DomainModel
{
	[TestFixture]
	public class TestPeriod
	{
		[Test]
		public void ParameterlessCtor_Should_Set_Start_To_0()
		{
			new Period().Start.Should().Be(new DateTime(0));
		}


	}
}