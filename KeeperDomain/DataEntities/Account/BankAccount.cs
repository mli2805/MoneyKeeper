using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class BankAccount : IDumpable, IParsable<BankAccount>
    {
        public int Id { get; set; } // совпадает с ID Account'a

        public int BankId { get; set; }
        public int DepositOfferId { get; set; } // не только по депозиту, но и по карте и даже расчетному счету могут начисляться %%
        public CurrencyCode MainCurrency { get; set; }
        
        public string AgreementNumber { get; set; } // номер договора, где он есть
        public string ReplenishDetails { get; set; } // реквизиты для пополнения
        public bool IsReplenishStopped { get; set; } // перенести сюда из Deposit


        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; } // max для расч счетов

        public bool IsMine { get; set; } 


        public string Dump()
        {
            return  Id + " ; " + BankId + " ; " + DepositOfferId + " ; " + 
                    MainCurrency + " ; " + AgreementNumber + " ; " + ReplenishDetails + " ; " + 
                    $"{StartDate.Date:dd/MM/yyyy}" + " ; " + $"{FinishDate.Date:dd/MM/yyyy}" + " ; " + 
                    IsMine;
        }

        public BankAccount FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);

            BankId = int.Parse(substrings[1]);
            DepositOfferId = int.Parse(substrings[2]);
            MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[3]);

            AgreementNumber = substrings[4].Trim();
            ReplenishDetails = substrings[5].Trim();

            StartDate = DateTime.ParseExact(substrings[6].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            FinishDate = DateTime.ParseExact(substrings[7].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            
            IsMine = Convert.ToBoolean(substrings[8]);

            return this;
        }
    }
}