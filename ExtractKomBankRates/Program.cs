// See https://aka.ms/new-console-template for more information

using ExtractKomBankRates;
using KeeperDomain.Exchange;

Console.WriteLine("Hello, World!");

var lines = File.ReadAllLines(@"c:\temp\KomBankRates.csv");

var bnb = new List<ExchangeRates>();
foreach (var line in lines.Skip(1))
{
    var er = line.Parse();
    if (er != null)
        bnb.Add(er);
}

Console.WriteLine(bnb.Count);

var id = 0;
var prev = bnb[0];
var date = new DateTime(2021, 2, 16, 12, 0, 0);

var bnbD = new List<ExchangeRates>();
foreach (var line in bnb)
{
    while (line.Date > date)
    {
        if (prev.Date.Day != date.Day)
            prev.Date = date.Date;
        var item = prev.Clone();
        item.Id = ++id;
        bnbD.Add(item);
        date = date.AddDays(1);
    }

    prev = line;

}

File.WriteAllLines(@"c:\temp\ExchangeRates.txt", bnbD.Select(l => l.Dump()));




