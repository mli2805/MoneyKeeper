using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private Account _root;

        public Account Root
        {
            get => _root;
            set
            {
                if (Equals(value, _root)) return;
                _root = value;
                NotifyOfPropertyChange();
            }
        }

        public AccountTreeViewModel()
        {
            SeedRoot();
        }

        private void SeedRoot()
        {
            Root = new Account()
            {
                Title = "Мои",
                Children = new List<Account>(),
            };

            var a1 = new Account()
            {
                Title = "На руках",
                Parent = Root,
                Children = new List<Account>(),
            };
            Root.Children.Add(a1);

            var a2 = new Account()
            {
                Title = "Мне должны",
                Parent = Root,
                Children = new List<Account>(),
            };
            Root.Children.Add(a2);

            var a3 = new Account()
            {
                Title = "Депозиты",
                Parent = Root,
                Children = new List<Account>(),
            };
            Root.Children.Add(a3);

            var a11 = new Account()
            {
                Title = "Наличные",
                Parent = a1,
                Children = new List<Account>(),
            };
            a1.Children.Add(a11);

            var a12 = new Account()
            {
                Title = "Карточки",
                Parent = a1,
                Children = new List<Account>(),
            };
            a1.Children.Add(a12);

            var a13 = new Account()
            {
                Title = "Детские",
                Parent = a1,
                Children = new List<Account>(),
            };
            a1.Children.Add(a13);

            var a14 = new Account()
            {
                Title = "Закрытые",
                Parent = a1,
                Children = new List<Account>(),
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



        }
    }
}
