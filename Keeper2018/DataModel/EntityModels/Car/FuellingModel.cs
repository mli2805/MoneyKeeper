using System;
using KeeperDomain;

namespace Keeper2018
{
    public class FuellingModel
    {
        public int Id { get; set; } //PK
        public TransactionModel Transaction { get; set; }

        public double Volume { get; set; }
        public FuelType FuelType { get; set; }

        // дублирование данных из TransactionModel, не хранятся, а заполняются на этапе загрузки бд, нужны только для отображения на 
        public DateTime Timestamp { get; set; }
        public int CarAccountId { get; set; } // хранится в TransactionModel как один из тэгов
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public string Comment { get; set; }


        // Вычислимые поля
        public decimal OneLitrePrice { get; set; }
        public decimal OneLitreInUsd { get; set; }


        public FuellingModel Clone()
        {
            return (FuellingModel)MemberwiseClone();
        }
      
    }
}