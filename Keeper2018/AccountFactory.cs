using System.Collections.ObjectModel;

namespace Keeper2018
{
    public static class AccountFactory
    {
        public static Account CreateExample()
        {
            var result = new Account()
            {
                Title = "Мои",
                Children = new ObservableCollection<Account>(),
            };

            var a1 = new Account()
            {
                Title = "На руках",
                Parent = result,
                Children = new ObservableCollection<Account>(),
            };
            result.Children.Add(a1);

            var a2 = new Account()
            {
                Title = "Мне должны",
                Parent = result,
                Children = new ObservableCollection<Account>(),
            };
            result.Children.Add(a2);

            var a3 = new Account()
            {
                Title = "Депозиты",
                Parent = result,
                Children = new ObservableCollection<Account>(),
            };
            result.Children.Add(a3);

            var a11 = new Account()
            {
                Title = "Наличные",
                Parent = a1,
                Children = new ObservableCollection<Account>(),
            };
            a1.Children.Add(a11);

            var a12 = new Account()
            {
                Title = "Карточки",
                Parent = a1,
                Children = new ObservableCollection<Account>(),
            };
            a1.Children.Add(a12);

            var a13 = new Account()
            {
                Title = "Детские",
                Parent = a1,
                Children = new ObservableCollection<Account>(),
            };
            a1.Children.Add(a13);

            var a14 = new Account()
            {
                Title = "Закрытые",
                Parent = a1,
                Children = new ObservableCollection<Account>(),
            };
            a1.Children.Add(a14);

            var a111 = new Account()
            {
                Title = "Мой кошелек",
                Parent = a11,
            };
            a11.Children.Add(a111);

            var a112 = new Account()
            {
                Title = "Юлин кошелек",
                Parent = a11,
            };
            a11.Children.Add(a112);

            var a113 = new Account()
            {
                Title = "Шкаф",
                Parent = a11,
            };
            a11.Children.Add(a113);

            return result;
        }
    }
}