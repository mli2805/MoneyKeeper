namespace Keeper2018
{
    public static class AccountFactory
    {
       
        public static Account CreateExample()
        {


            var result = new Account("Мои");

            var a1 = new Account("На руках");
            result.Items.Add(a1);

            var a2 = new Account()
            {
                Header = "Мне должны",
            };
            result.Items.Add(a2);

            var a3 = new Account()
            {
                Header = "Депозиты",
            };
            result.Items.Add(a3);

            var a11 = new Account()
            {
                Header = "Наличные",
            };
            a1.Items.Add(a11);

            var a12 = new Account()
            {
                Header = "Карточки",
            };
            a1.Items.Add(a12);

            var a13 = new Account()
            {
                Header = "Детские",
            };
            a1.Items.Add(a13);

            var a14 = new Account()
            {
                Header = "Закрытые",
            };
            a1.Items.Add(a14);

            var a111 = new Account()
            {
                Header = "Мой кошелек",
            };
            a11.Items.Add(a111);

            var a112 = new Account()
            {
                Header = "Юлин кошелек",
            };
            a11.Items.Add(a112);

            var a113 = new Account()
            {
                Header = "Шкаф",
            };
            a11.Items.Add(a113);

            return result;
        }
    }
}