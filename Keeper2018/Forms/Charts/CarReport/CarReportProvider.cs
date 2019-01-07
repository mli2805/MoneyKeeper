using System;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public class CarReportProvider
    {
        private const int Scenic3Id = 716; // Scenic3
        private static readonly string[] TagRussians =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт регуляр", "авто топливо", "авто прочее" };
        private static readonly string[] TagEnglish =
            { "buy-sell", "state", "car repair", "expendables", "car fuel", "other stuff" };

        private readonly KeeperDb _db;

        public CarReportProvider(KeeperDb db)
        {
            _db = db;
        }

        public PdfDocument CreateCarReport()
        {
            var carReportData = ExtractCarData();

            Document doc = new Document();
            Section chartSection = doc.AddSection();
            var paragraph = chartSection.AddParagraph();
            paragraph.AddFormattedText(
                $"Renault Grand Scenic III 1.5dci 2010 г.в. {carReportData.StartDate:dd/MM/yyyy} - {carReportData.FinishDate:dd/MM/yyyy}");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            FillChart(chartSection, carReportData);
            FillAggregateTable(chartSection, carReportData);

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
            var carAccount = _db.AcMoDict[Scenic3Id];
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

            var column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            var total = carReportData.Tags.Sum(t => t.Table.Sum(r => r.AmountInUsd));
            Row row;
            foreach (var tag in carReportData.Tags)
            {
                row = table.AddRow();
                row.Cells[0].AddParagraph($"{tag.Russian}");
                var amount = tag.Table.Sum(r => r.AmountInUsd);
                row.Cells[1].AddParagraph($"$ {-amount:#,0}");
                row.Cells[2].AddParagraph($"{amount / total * 100:N} %");
            }
            row = table.AddRow();
            row.Cells[1].AddParagraph($"$ {-total:#,0}");
            row = table.AddRow();
            var inAday = -total / (carReportData.FinishDate - carReportData.StartDate).Days;
            row.Cells[1].AddParagraph($"в день $ {inAday:N}");
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
