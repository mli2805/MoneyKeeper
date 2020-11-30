using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public static class PaymentGuess
    {
        private static List<int> ERIP = new List<int>
        {
            192, 363, 285,  // коммунальные квартира и дача, кредит
            270, 736, 183, // велком, школьная столовая, государство
        };

        private static List<int> Terminal = new List<int> { 179, 180, 303, 353, 254 }; // магазины, рынки, страховщики, медицина, прочие харчевни
        private static List<int> CardOther = new List<int> { 718, 264, 220 }; // заправка, минтранс, банки

        public static PaymentWay GuessPaymentWay(TransactionModel tran)
        {
            if (tran.Operation != OperationType.Расход)
                return PaymentWay.НеЗадано;
            if (tran.MyAccount.Is(160))
                return PaymentWay.Наличные;
            if (tran.MyAccount.Is(161))
            {
                foreach (var tag in tran.Tags)
                {
                    foreach (var grou in Terminal)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаТерминал;
                    }
                    foreach (var grou in CardOther)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаДругое;
                    }
                    foreach (var grou in ERIP)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаЕрип;
                    }
                }
                return PaymentWay.НеЗадано;
            }
            return PaymentWay.НеЗадано;
        } 
       
    }
}