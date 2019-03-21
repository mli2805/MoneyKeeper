using System;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public class CarReportProvider
    {
        private int _accountId; // Scenic3 = 716
        private static readonly string[] TagRussians =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт регуляр", "авто топливо", "авто прочее" };
        private static readonly string[] TagEnglish =
            { "buy-sell", "state", "car repair", "expendables", "car fuel", "other stuff" };

        private readonly KeeperDb _db;

        public CarReportProvider(KeeperDb db)
        {
            _db = db;
        }

        public PdfDocument CreateCarReport(int accountId)
        {
            _accountId = accountId;
            var carReportData = ExtractCarData();

            Document doc = new Document();
            Section chartSection = doc.AddSection();
            var paragraph = chartSection.AddParagraph();
            paragraph.AddFormattedText(
                $"Renault Grand Scenic III 1.5dci 2010 г.в. {carReportData.StartDate:dd/MM/yyyy} - {carReportData.FinishDate:dd/MM/yyyy}");
          //  paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            FillChart(chartSection, carReportData);
            FillAggregateTable(chartSection, carReportData);
            FillTotals(chartSection, carReportData);

            Section tablesSection = doc.AddSection();
            tablesSection.PageSetup.TopMargin = 20;
            tablesSection.PageSetup.BottomMargin = 10;
            FillTagTables(tablesSection, carReportData);

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfDocumentRenderer.Document = doc;
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private CarReportData ExtractCarData()
        {
            var result = new CarReportData();
            var carAccount = _db.AcMoDict[_accountId];
            for (int i = 0; i < TagRussians.Length; i++)
            {
                var tag = carAccount.Children.First(c => c.Name.Contains(TagRussians[i]));
                var rows = _db.GetTableForTag(tag);
                result.Tags.Add(new PdfReportTable(TagRussians[i], TagEnglish[i], rows));
            }

            result.StartDate = result.Tags[0].Table[0].Date;

            //            var sold = new PdfReportTableRow()
            //            {
            //                Date = new DateTime(2019, 4, 15),
            //                AmountInUsd = 9200,
            //                AmountInCurrency = "9200 usd",
            //                Comment = "продажа",
            //            };
            //            result.Tags[0].Table.Add(sold);
            //            result.FinishDate = sold.Date;

            result.FinishDate = DateTime.Today;
            return result;
        }

        private void FillChart(Section section, CarReportData carReportData)
        {
            var chart3 = section.AddChart(ChartType.PieExploded2D);
            Series pieSeries = chart3.SeriesCollection.AddSeries();
            pieSeries.DataLabel.Type = DataLabelType.Value;
            pieSeries.DataLabel.Position = DataLabelPosition.OutsideEnd;
            pieSeries.HasDataLabel = true;
            chart3.Width = Unit.FromCentimeter(16);
            chart3.Height = Unit.FromCentimeter(16);
            XSeries pieXSeries = chart3.XValues.AddXSeries();

            foreach (var tag in carReportData.Tags)
            {
                pieSeries.Add(Math.Round((double)-tag.Table.Sum(r => r.AmountInUsd)));
                pieXSeries.Add(tag.English);
            }

            chart3.BottomArea.AddLegend();
            chart3.XAxis.MajorTickMark = TickMarkType.Outside;
            chart3.XAxis.Title.Caption = "X-Axis";
            chart3.YAxis.MajorTickMark = TickMarkType.Outside;
            chart3.YAxis.TickLabels.Format = "#0%";
            chart3.YAxis.TickLabels.Font.Size = 0.01f;
            chart3.YAxis.TickLabels.Font.Color = Colors.Black;
            chart3.PivotChart = true;
            chart3.HasDataLabel = true;
        }

        private void FillAggregateTable(Section section, CarReportData carReportData)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Расходы по категориям");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var column = table.AddColumn("8cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            var total = carReportData.Tags.Sum(t => t.Table.Sum(r => r.AmountInUsd));
            foreach (var tag in carReportData.Tags)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{tag.Russian}");
                var amount = tag.Table.Sum(r => r.AmountInUsd);
                row.Cells[1].AddParagraph($"$ {-amount:#,0}");
                row.Cells[2].AddParagraph($"{amount / total * 100:N} %");
            }

            var rowTotal = table.AddRow();
            rowTotal.Cells[1].AddParagraph($"$ {-total:#,0}");
            rowTotal.Format.Font.Bold = true;

         }

        private void FillTotals(Section section, CarReportData carReportData)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.7);

            var total = carReportData.Tags.Sum(t => t.Table.Sum(r => r.AmountInUsd));
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var column = table.AddColumn("5cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("5.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("5.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            var row = table.AddRow();
            row.Borders.Visible = false;
            row.Cells[2].AddParagraph("при условии продажи за 8000 usd");
            var total2 = total + 8000;

            row = table.AddRow();
            row.Cells[1].AddParagraph($"$ {-total:#,0}");
            row.Cells[2].AddParagraph($"$ {-total2:#,0}");
            row.Format.Font.Bold = true;

            row = table.AddRow();
            var inAday = -total / (carReportData.FinishDate - carReportData.StartDate).Days;
            var inAday2 = -total2 / (carReportData.FinishDate - carReportData.StartDate).Days;
            row.Cells[0].AddParagraph("в день");
            row.Cells[1].AddParagraph($"$ {inAday:N}");
            row.Cells[2].AddParagraph($"$ {inAday2:N}");

            row = table.AddRow();
            var inAyear = (double)-total / (carReportData.FinishDate - carReportData.StartDate).Days * 365.0;
            var inAyear2 = (double)-total2 / (carReportData.FinishDate - carReportData.StartDate).Days * 365.0;
            row.Cells[0].AddParagraph("в год");
            row.Cells[1].AddParagraph($"$ {inAyear:N}");
            row.Cells[2].AddParagraph($"$ {inAyear2:N}");

        }

        private void FillTagTables(Section section, CarReportData carReportData)
        {
            section.DrawTableFromTag(carReportData.Tags[0]);
            section.DrawTableFromTag(carReportData.Tags[1]);
            section.AddPageBreak();
            section.DrawTableFromTag(carReportData.Tags[2]);
            section.AddPageBreak();
            section.DrawTableFromTag(carReportData.Tags[3]);
        }
    }
}
