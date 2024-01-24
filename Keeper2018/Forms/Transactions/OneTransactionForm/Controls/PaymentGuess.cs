using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public static class PaymentGuess
    {
        private static readonly List<int> Erip = new List<int>
        {
            192, 363, 752, 285,  // коммунальные квартира и дача и кварт1, кредит
            270, 736, 183, // велком, школьная столовая, государство
        };

        private static readonly List<int> Terminal = new List<int> 
        { 
            179, 180, 303, 763,     // магазины, рынки, страховщики, бассейн
            353, 255, 254, 253,     // медицина, аптеки(прочее),прочие харчевни, столовая водоканала, 
        };

        private static readonly List<int> CardOther = new List<int>
        {
            264, 272,  //  минтранс, АЗС(новые)
            220, 831   // банки, eCommerce
        }; 
        

        public static PaymentWay GuessPaymentWay(TransactionModel tran)
        {
            if (tran.Operation != OperationType.Расход)
                return PaymentWay.НеЗадано;
            if (tran.MyAccount.Is(160) // наличные
                || tran.MyAccount.Is(386) // Яна
                || tran.MyAccount.Is(387) // Глеб
                || tran.MyAccount.Is(388)) // Борис
                return PaymentWay.Наличные;
            if (tran.MyAccount.Is(161) || tran.MyAccount.Is(830)) // карты и закрытые карты
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
                    foreach (var grou in Erip)
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