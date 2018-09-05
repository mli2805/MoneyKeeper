namespace Keeper2018
{
    public static class AccountFactory
    {
       
        public static Account CreateExample()
        {


            var result = new Account("Мои");

            var a1 = new Account("На руках");
            result.Items.Add(a1);

            var a2 = new Account("Мне должны");
            result.Items.Add(a2);

            var a3 = new Account("Депозиты");
            result.Items.Add(a3);

            var a11 = new Account("Наличные");
            a1.Items.Add(a11);

            var a12 = new Account("Карточки");
            a1.Items.Add(a12);

            var a13 = new Account("Детские");
            a1.Items.Add(a13);

            var a14 = new Account("Закрытые");
            a1.Items.Add(a14);

            var a111 = new Account("Мой кошелек");
            a11.Items.Add(a111);

            var a112 = new Account("Юлин кошелек");
            a11.Items.Add(a112);

            var a113 = new Account("Шкаф");
            a11.Items.Add(a113);

            return result;
        }
    }
}