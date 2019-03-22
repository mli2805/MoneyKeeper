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
        private Car _car;
        private static readonly string[] TagRussians =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт ДТП", "ремонт регуляр", "авто топливо", "авто прочее" };
        private static readonly string[] TagEnglish =
            { "buy-sell", "state", "car repair", "accident repair", "expendables", "car fuel", "other stuff" };

        private readonly KeeperDb _db;

        public CarReportProvider(KeeperDb db)
        {
            _db = db;
        }

        public PdfDocument CreateCarReport(int accountId)
        {
            _accountId = accountId;
            _car = _db.Bin.Cars.First(c => c.AccountId == _accountId);

            Document doc = new Document();
            Section chartSection = doc.AddSection();
            chartSection.PageSetup.TopMargin = 40;
            var paragraph = chartSection.AddParagraph();
            paragraph.AddFormattedText($"{_car.Title} {_car.IssueYear} г.в. {_car.Start:dd/MM/yyyy} - {_car.Finish:dd/MM/yyyy}");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            var carReportData = ExtractCarData();
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
            decimal total2 = 0;
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var isLastCar = _db.Bin.Cars.Last().AccountId == _accountId;

            var column = table.AddColumn("5cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("5.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);

            if (isLastCar)
            {
                column = table.AddColumn("5.5cm");
                column.Format.Alignment = ParagraphAlignment.Right;

                var rowSupposedly = table.AddRow();
                rowSupposedly.Borders.Visible = false;
                rowSupposedly.Cells[2].AddParagraph($"при условии продажи за {_car.SupposedSale} usd");
                total2 = total + _car.SupposedSale;
            }

            var row = table.AddRow();
            row.Cells[1].AddParagraph($"$ {-total:#,0}");
            if (isLastCar)
                row.Cells[2].AddParagraph($"$ {-total2:#,0}");
            row.Format.Font.Bold = true;

            row = table.AddRow();
            var inAday = -total / (carReportData.FinishDate - carReportData.StartDate).Days;
            var inAday2 = -total2 / (carReportData.FinishDate - carReportData.StartDate).Days;
            row.Cells[0].AddParagraph("в день");
            row.Cells[1].AddParagraph($"$ {inAday:N}");
            if (isLastCar)
                row.Cells[2].AddParagraph($"$ {inAday2:N}");

            row = table.AddRow();
            var inAyear = (double)-total / (carReportData.FinishDate - carReportData.StartDate).Days * 365.0;
            row.Cells[0].AddParagraph("в год");
            row.Cells[1].AddParagraph($"$ {inAyear:N}");
            if (isLastCar)
            {
                var inAyear2 = (double)-total2 / (carReportData.FinishDate - carReportData.StartDate).Days * 365.0;
                row.Cells[2].AddParagraph($"$ {inAyear2:N}");
            }

            row = table.AddRow();
            row.Borders.Visible = false;
            row.Cells[0].MergeRight = 1;
            row.Cells[0].AddParagraph($"{_car.MileageFinish} - {_car.MileageStart} = {_car.MileageFinish - _car.MileageStart} км");
         
            row = table.AddRow();
            var forKm = (double)-total / (_car.MileageFinish - _car.MileageStart);
            row.Cells[0].AddParagraph("за 1 км");
            row.Cells[1].AddParagraph($"$ {forKm:N}");
            if (isLastCar)
            {
                var forKm2 = (double)-total2 / (_car.MileageFinish - _car.MileageStart);
                row.Cells[2].AddParagraph($"$ {forKm2:N}");
            }
        }

        private void FillTagTables(Section section, CarReportData carReportData)
        {
            section.DrawTableFromTag(carReportData.Tags[0]);
            section.DrawTableFromTag(carReportData.Tags[1]);
            section.AddPageBreak();
            section.DrawTableFromTag(carReportData.Tags[2]);
            section.DrawTableFromTag(carReportData.Tags[3]);
            section.AddPageBreak();
            section.DrawTableFromTag(carReportData.Tags[4]);
        }
    }
}
