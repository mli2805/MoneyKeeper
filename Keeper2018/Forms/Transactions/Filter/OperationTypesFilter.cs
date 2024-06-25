using KeeperDomain;

namespace Keeper2018
{
    public class OperationTypesFilter
    {
        public bool IsOn { get; set; }
        public OperationType Operation { get; set; }

        /// <summary>
        /// таким конструктором создается ВЫключенный фильтр
        /// ему не нужен тип операции, он пропускает все типы
        /// </summary>
        public OperationTypesFilter() { IsOn = false; }

        /// <summary>
        /// а такой фильтр пропускает только "свою" операцию
        /// </summary>
        /// <param name="operation"></param>
        public OperationTypesFilter(OperationType operation)
        {
            IsOn = true;
            Operation = operation;
        }

        public override string ToString()
        {
            return IsOn ? Operation.ToString() : "<no filter>";
        }
    }
    
    public class PaymentWaysFilter
    {
        public bool IsOn { get; set; }
        public PaymentWay PaymentWay { get; set; }

       
        public PaymentWaysFilter() { IsOn = false; }

       
        public PaymentWaysFilter(PaymentWay paymentWay)
        {
            IsOn = true;
            PaymentWay = paymentWay;
        }

        public override string ToString()
        {
            return IsOn ? PaymentWay.ToString() : "<no filter>";
        }
    }
}