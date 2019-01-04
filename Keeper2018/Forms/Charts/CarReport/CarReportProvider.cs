using System;
using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public class CarReportProvider
    {
        private const int Scenic3Id = 716; // Scenic3
        private readonly KeeperDb _db;
        private CarReport _carReport = new CarReport();

        public CarReportProvider(KeeperDb db)
        {
            _db = db;
        }

        public PdfDocument CreateCarReport()
        {
            Document doc = new Document();
            Section section = doc.AddSection();
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Renault Grand Scenic III ");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            var carAccount = _db.AcMoDict[Scenic3Id];
            FillSection(section, carAccount);

            Section chartSection = doc.AddSection();
            FillChart(chartSection);

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfDocumentRenderer.Document = doc;
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void FillChart(Section section)
        {
            var chart3 = section.AddChart(ChartType.PieExploded2D);
            Series pieSeries = chart3.SeriesCollection.AddSeries();
            pieSeries.DataLabel.Type = DataLabelType.Value;
            pieSeries.DataLabel.Position = DataLabelPosition.OutsideEnd;
            pieSeries.HasDataLabel = true;
            chart3.Width = Unit.FromCentimeter(16);
            chart3.Height = Unit.FromCentimeter(16);
            XSeries pieXSeries = chart3.XValues.AddXSeries();

            foreach (var tuple in _carReport.Tags)
            {
                pieSeries.Add(Math.Round((double)-tuple.Item2));
                pieXSeries.Add(tuple.Item1);
            }

            chart3.BottomArea.AddLegend();
//            chart3.DataLabel.Font.Color = Color.Parse("black");
//            chart3.DataLabel.Type = DataLabelType.Percent;
//            chart3.DataLabel.Position = DataLabelPosition.OutsideEnd;
//            chart3.DataLabel.Format = "#0%";
            chart3.XAxis.MajorTickMark = TickMarkType.Outside;
            chart3.XAxis.Title.Caption = "X-Axis";
            chart3.YAxis.MajorTickMark = TickMarkType.Outside;
            chart3.YAxis.TickLabels.Format = "#0%";
            chart3.YAxis.TickLabels.Font.Size = 0.01f;
            chart3.YAxis.TickLabels.Font.Color = Colors.Black;
            chart3.PivotChart = true;
            chart3.HasDataLabel = true;
        }

        private void FillSection(Section section, AccountModel carAccount)
        {
            var tag = carAccount.Children.First(c => c.Name.Contains("покупка-продажа"));
            var sum = AddTableFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("покупка-продажа", sum));

            tag = carAccount.Children.First(c => c.Name.Contains("государство"));
            sum = AddTableFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("State payments", sum));

            tag = carAccount.Children.First(c => c.Name.Contains("авто ремонт"));
            sum = AddTableFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("авто ремонт", sum));

            tag = carAccount.Children.First(c => c.Name.Contains("ремонт регуляр"));
            sum = AddTableFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("ремонт регуляр", sum));

            tag = carAccount.Children.First(c => c.Name.Contains("авто топливо"));
            sum = AddParagraphFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("авто топлив", sum));

            tag = carAccount.Children.First(c => c.Name.Contains("авто прочее"));
            sum = AddTableFromTag(section, tag);
            _carReport.Tags.Add(new Tuple<string, decimal>("авто прочее", sum));

            var paragraph = section.AddParagraph($"Всего     {_carReport.Tags.Select(p => p.Item2).Sum():#,0.##}");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private decimal AddTableFromTag(Section section, AccountModel tag)
        {
            var caption = section.AddParagraph(tag.Name);
            caption.Format.SpaceBefore = Unit.FromCentimeter(1);
            caption.Format.SpaceAfter = Unit.FromCentimeter(1);

            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            var rows = _db.GetTable(tag);
            foreach (var tableRow in rows)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{tableRow.Date:dd/MM/yyyy}");
                var amount = tableRow.AmountInCurrency + (tableRow.AmountInCurrency.EndsWith("usd") ? "" : $"\n {tableRow.AmountInUsd:#,0.##} usd");
                row.Cells[1].AddParagraph($"{amount}");
                row.Cells[2].AddParagraph($"{tableRow.Comment}");
            }
            var totalRow = table.AddRow();
            totalRow.Cells[0].AddParagraph("Итого");
            var sum = rows.Sum(r => r.AmountInUsd);
            totalRow.Cells[1].AddParagraph($"{sum:#,0.##} usd");

            var offset = section.AddParagraph();
            offset.Format.SpaceAfter = Unit.FromCentimeter(1);
            return sum;
        }

        private decimal AddParagraphFromTag(Section section, AccountModel tag)
        {
            var caption = section.AddParagraph(tag.Name);
            caption.Format.SpaceBefore = Unit.FromCentimeter(1);
            caption.Format.SpaceAfter = Unit.FromCentimeter(1);

            var rows = _db.GetTable(tag);
            var sum = rows.Sum(r => r.AmountInUsd);
            var paragraph = section.AddParagraph($"Итого    {sum:#,0.##} usd");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);
            return sum;
        }

    }

    public class PdfReportTableRow
    {
        public DateTime Date;
        public string AmountInCurrency;
        public decimal AmountInUsd;
        public string Comment;
    }


    public class CarReport
    {
        public DateTime StartDate;
        public List<Tuple<string, decimal>> Tags = new List<Tuple<string, decimal>>();
    }

    public static class PdfReport

    {
        public static Chart GetChart()
        {
            Chart chart = new Chart(ChartType.Pie2D);
            return chart;
        }

        public static List<PdfReportTableRow> GetTable(this KeeperDb db, AccountModel tag)
        {
            var rows = new List<PdfReportTableRow>();
            foreach (var transaction in db.Bin.Transactions.Values)
            {
                var balanceForTag = transaction.BalanceForTag(db, tag.Id);
                if (balanceForTag == null) continue;
                var row = new PdfReportTableRow
                {
                    Date = transaction.Timestamp,
                    AmountInCurrency = balanceForTag.ToString(),
                    Comment = transaction.Comment
                };
                var moneyPair = balanceForTag.Currencies.First();
                row.AmountInUsd = moneyPair.Key == CurrencyCode.USD
                    ? moneyPair.Value
                    : db.AmountInUsd(transaction.Timestamp, moneyPair.Key, moneyPair.Value);

                rows.Add(row);
            }
            return rows;
        }

    }
}
