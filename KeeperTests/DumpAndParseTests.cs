using KeeperDomain;

namespace KeeperTests
{
    [TestClass]
    public class DumpAndParseTests
    {
        [TestMethod]
        public void TestAccount()
        {
            var account = new Account()
            {
                Id = 5,
                Name = "Test Account 5",
                ShortName = "TestAccount5",
                ButtonName = "ta5",
                IsExpanded = false,
                IsFolder = false,
                Comment = "Test Account 5 Comment",
            };

            var str = account.Dump(3);
            var account2 = new Account().FromString(str);
            Assert.IsNotNull(account2);
            Assert.AreEqual(5, account2.Id);
        }

        [TestMethod]
        public void FromTextLoaderTest()
        {
            // сильно долго (загрузка в программе меньше секунды, здесь более 30 секунд)
            var loadResult = TxtLoader.LoadAllFromNewTxt( @"..\..\..\Assets");
            Assert.AreEqual(loadResult.IsSuccess, true);

        }
    }
}