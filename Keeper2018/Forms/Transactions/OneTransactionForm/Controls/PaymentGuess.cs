using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class PaymentGuess
    {
        private static readonly Dictionary<PaymentWay, List<int>> Dict = new Dictionary<PaymentWay, List<int>>
        {
            {
                PaymentWay.ОплатаПоЕрип,
                new List<int>
                {
                    192, 363, 752, 285,  // коммунальные квартира и дача и кварт1, кредит
                    270, 736, 183, // велком, школьная столовая, государство
                }
            },
            {
                PaymentWay.КартаТерминал,
                new List<int>
                {
                    179, 180, 303, 763,     // магазины, рынки, страховщики, бассейн
                    353, 255, 254, 253,     // медицина, аптеки(прочее),прочие харчевни, столовая водоканала, 
                }
            },
            {
                PaymentWay.КартаДругое,
                new List<int>
                {
                    264,   //  минтранс
                }
            },
            {
                PaymentWay.ПриложениеПродавца,
                new List<int>()
                {
                    831 // eCommerce
                }
            },
            {
                PaymentWay.БанкСписал,
                new List<int>()
                {
                    220  // банки
                }
            }
        };

        public static PaymentWay GuessPaymentWay(TransactionModel tran)
        {
            if (tran.Operation != OperationType.Расход)
                return PaymentWay.НеЗадано;

            if (tran.MyAccount.Is(160)) // наличные
                return PaymentWay.Наличные;

            // если карточка то предполагаем по Контрагенту и Категории
            if (tran.MyAccount.Is(161) || tran.MyAccount.Is(830)) // карты и закрытые карты
            {
                if (tran.Counterparty == null) return PaymentWay.НеЗадано;

                foreach (var paymantWay in Dict.Keys)
                {
                    if (Dict[paymantWay].Any(grou => tran.Counterparty.Is(grou)))
                        if (paymantWay == PaymentWay.КартаТерминал)
                            return tran.MyAccount.BankAccount.PayCard.IsVirtual
                                ? PaymentWay.ТелефонТерминал
                                : PaymentWay.КартаТерминал;
                        else
                            return paymantWay;
                }


                if (tran.Category == null) return PaymentWay.НеЗадано;

                foreach (var paymantWay in Dict.Keys)
                {
                    if (Dict[paymantWay].Any(grou => tran.Category.Is(grou)))
                        if (paymantWay == PaymentWay.КартаТерминал)
                            return tran.MyAccount.BankAccount.PayCard.IsVirtual
                                ? PaymentWay.ТелефонТерминал
                                : PaymentWay.КартаТерминал;
                        else
                            return paymantWay;
                }

                return PaymentWay.НеЗадано;
            }
            return PaymentWay.НеЗадано;
        }

    }
}