using System;
using System.ComponentModel.DataAnnotations;

namespace KeeperDomain
{
    [Serializable]
    public class DepositOffer : IDumpable, IParsable<DepositOffer>
    {
        public int Id { get; set; } //PK
        [MaxLength(100)] public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public RateType RateType { get; set; }
        public bool IsAddLimited { get; set; }
        public int AddLimitInDays { get; set; }


        // при создании депозита или карточки эти поля возмутся из офера
        // а при создании банковского счета их надо будет выбрать руками
        public int BankId { get; set; }
        public CurrencyCode MainCurrency { get; set; }
        //

        public Duration DepositTerm { get; set; }

        // Для карточек 
        // иногда задан минимум при котором не берется плата за обслуживание или начисляется манибэк
        // иногда задат максимум после которого манибэк не начисляется
        public int MonthPaymentsMinimum { get; set; }
        public int MonthPaymentsMaximum { get; set; }

        [MaxLength(250)] public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + BankId + " ; " + Title + " ; " + IsNotRevocable + " ; " +
                   RateType + " ; " + IsAddLimited + " ; " + AddLimitInDays + " ; " +
                   MainCurrency + " ; " + (DepositTerm?.Dump() ?? new Duration().Dump()) + " ; " + 
                   MonthPaymentsMinimum + " ; " + MonthPaymentsMaximum + " ; " + Comment;
        }

        public DepositOffer FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            BankId = int.Parse(substrings[1]);
            Title = substrings[2].Trim();
            IsNotRevocable = bool.Parse(substrings[3].Trim());
            RateType = (RateType)Enum.Parse(typeof(RateType), substrings[4]);
            IsAddLimited = bool.Parse(substrings[5].Trim());
            AddLimitInDays = int.Parse(substrings[6]);
            MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[7]);
            DepositTerm = DurationFromStrings(substrings[8], substrings[9], substrings[10]);
            MonthPaymentsMinimum = int.Parse(substrings[11]);
            MonthPaymentsMaximum = int.Parse(substrings[12]);
            Comment = substrings[13].Trim();
            return this;
        }

        private Duration DurationFromStrings(string a, string b, string c)
        {
            return Convert.ToBoolean(a) ? new Duration() : new Duration(int.Parse(b), (Durations)Enum.Parse(typeof(Durations), c));
        }
    }
}