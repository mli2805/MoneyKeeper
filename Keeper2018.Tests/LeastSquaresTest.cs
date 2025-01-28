using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Keeper2018.Tests
{
    [TestClass]
    public sealed class LeastSquaresTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            // arrange
            var data = new List<(int, double)>() { (1, 5.3), (2, 6.3), (3, 4.8), (4, 3.8), (5, 3.3) };

            // act
            var result = LeastSquares.GetLinear(data);

            // assert
            var delta = 0.00001;
            Assert.AreEqual(-0.65, result.Item1, delta);
            Assert.AreEqual(6.65, result.Item2, delta);
        }
        
        [TestMethod]
        public void TestMethod2()
        {
            // arrange
            var data = new List<(int, double)>() { (-1, 1), (1, 3), (4, 6), (5, 7) };

            // act
            var result = LeastSquares.GetLinear(data);

            // assert
            var delta = 0.00001;
            Assert.AreEqual(1.0, result.Item1, delta);
            Assert.AreEqual(2.0, result.Item2, delta);
        }
    }
}
