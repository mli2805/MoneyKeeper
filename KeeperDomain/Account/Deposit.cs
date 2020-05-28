using System;

namespace KeeperDomain
{
    [Serializable]
    public class Deposit
    {
        public int Id { get; set; }
        public int MyAccountId { get; set; }
        public int DepositOfferId { get; set; }
        public string Serial { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string ShortName { get; set; }
        public string Comment { get; set; }

        public PayCard Card;

        public string Dump()
        {
//            if (MyAccountId == 0)
//                MessageBox.Show($"MyAccountId == 0    {StartDate}  {FinishDate}  {ShortName}  {Comment}");
            return MyAccountId + " ; " + DepositOfferId + " ; " + Serial +" ; " + 
                   $"{StartDate.Date:dd/MM/yyyy}" + " ; " + $"{FinishDate.Date:dd/MM/yyyy}" + " ; " + 
                   ShortName + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }


}