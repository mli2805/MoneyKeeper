using Autofac;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Keeper2018.Tests
{
    [Binding]
    public sealed class MoveTransactionsSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();

        [Given(@"Существует набор данных")]
        public void GivenСуществуетНаборДанных()
        {
            _sut.DeserializeDb();
            _sut.Db.Bin.Transactions.Count.Should().Be(63);
//            foreach (var account in _sut.Db.Bin.AccountPlaneList)
//            {
//                var accountModel = account.Map(_sut.Db.AcMoDict);
//            }
        }

        [Given(@"Открываю форму транзакций")]
        public void GivenОткрываюФормуТранзакций()
        {
            var unused = _sut.GlobalScope.Resolve<TransactionsViewModel>();
//            vm.Initialize();
        }

        [When(@"Я двигаю не первую транзакцию чека вверх")]
        public void WhenЯДвигаюНеПервуюТранзакциюЧекаВверх()
        {
        }

        [Then(@"Она меняется местами с вышестоявшей")]
        public void ThenОнаМеняетсяМестамиСВышестоявшей()
        {
        }

    }
}
