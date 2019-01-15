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
        }

        [Given(@"Открываю форму транзакций")]
        public void GivenОткрываюФормуТранзакций()
        {
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
