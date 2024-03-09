using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class SalaryChange : IDumpable, IParsable<SalaryChange>
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public DateTime FirstReceived { get; set; } = new DateTime(2008, 6, 1);
        public decimal Amount { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + EmployerId + " ; " +
                   FirstReceived.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " + Comment;
        }

        public SalaryChange FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            EmployerId = int.Parse(substrings[1]);
            FirstReceived = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            Amount = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            Comment = substrings[4].Trim();
            return this;
        }
    }
}