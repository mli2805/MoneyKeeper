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
        private CarVm _car;
        private static readonly string[] TagRussians =
            { "покупка-продажа", "государство", "авто ремонт", "ремонт ДТП", "регулярн обслуживание", "авто топливо", "авто прочее" };
        private static readonly string[] TagEnglish =
            { "buy-sell", "state", "car repair", "accident repair", "expendables", "car fuel", "other stuff" };

        private readonly KeeperDataModel _dataModel;

        public CarReportProvider(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public PdfDocument CreateCarReport(int carId)
        {
            _car = _dataModel.Cars.First(c => c.Id == carId);
            var isCurrentCar = _dataModel.Cars.Last().Id == carId;

            Document doc = new Document();

            Section firstPageSection = doc.AddSection();
            firstPageSection.PageSetup.TopMargin = 40;
            firstPageSection.PageSetup.BottomMargin = 10;
            var paragraph = firstPageSection.AddParagraph();
            paragraph.AddFormattedText($"{_car.Title} {_car.IssueYear} г.в. {_car.PurchaseDate:dd/MM/yyyy} - {_car.SaleDate:dd/MM/yyyy}");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            var carReportData = ExtractCarData();
            if (isCurrentCar)
            {
                carReportData.Tags[0].Table.Add(new PdfReportTableRow()
                {
                    Date = DateTime.Today,
                    AmountInUsd = _car.SupposedSalePrice,
                    AmountInCurrency = $"{_car.SupposedSalePrice} usd",
                    Comment = "предполагаемая продажа",
                });
            }
            carReportData.FinishDate = carReportData.Tags[0].Table.Last().Date;

            FillChart(firstPageSection, carReportData);
            FillAggregateTable(firstPageSection, carReportData);
            FillTotals(firstPageSection, carReportData);

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
            var carAccount = _dataModel.AcMoDict[_car.CarAccountId];
            for (int i = 0; i < TagRussians.Length; i++)
            {
                var tag = carAccount.Children.First(c => c.Name.Contains(TagRussians[i]));
                var rows = _dataModel.GetTableForTag(tag);
                result.Tags.Add(new PdfReportTable(TagRussians[i], TagEnglish[i], rows));
            }

            result.StartDate = result.Tags[0].Table[0].Date;
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
            var totalFuelling = carReportData.Tags.FirstOrDefault(t => t.English == "car fuel")?.Table
                .Sum(r => r.AmountInUsd);
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var column = table.AddColumn("7cm");
            column.Format.Alignment = ParagraphAlignment.Left;
            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);

            var row = table.AddRow();
            row.Borders.Visible = false;
            row.Cells[0].MergeRight = 1;
            var paragraph = row.Cells[0].AddParagraph($"{_car.SaleDate.ToShortDateString()} - {_car.PurchaseDate.ToShortDateString()}");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            row = table.AddRow();
            row.Cells[0].AddParagraph($"{(_car.SaleDate - _car.PurchaseDate).Days} дн.");
            var inAday = -total / (carReportData.FinishDate - carReportData.StartDate).Days;
            row.Cells[1].AddParagraph("в день");
            row.Cells[2].AddParagraph($"$ {inAday:N}");

            row = table.AddRow();
            row.Cells[0].AddParagraph($"{ (carReportData.FinishDate - carReportData.StartDate).Days / 365.0:#.00} лет.");
            var inAyear = (double)-total / (carReportData.FinishDate - carReportData.StartDate).Days * 365.0;
            row.Cells[1].AddParagraph("в год");
            row.Cells[2].AddParagraph($"$ {inAyear:N}");

            row = table.AddRow();
            row.Borders.Visible = false;
            row.Cells[0].MergeRight = 1;
            paragraph = row.Cells[0].AddParagraph($"{_car.SaleMileage} - {_car.PurchaseMileage} = {_car.SaleMileage - _car.PurchaseMileage} км");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);


            row = table.AddRow();
            row.Cells[0].MergeRight = 1;
            row.Cells[0].AddParagraph("все расходы на 1 км");
            var forKm = (double)-total / (_car.SaleMileage - _car.PurchaseMileage);
            row.Cells[2].AddParagraph($"$ {forKm:N}");

            row = table.AddRow();
            row.Cells[0].MergeRight = 1;
            row.Cells[0].AddParagraph("топливо на 1 км");
            forKm = (double)-totalFuelling / (_car.SaleMileage - _car.PurchaseMileage);
            row.Cells[2].AddParagraph($"$ {forKm:N}");

            if (_car.Id >= 3)
            {
                var totalLitres = _dataModel.FuellingVms
                    .Where(f => f.CarAccountId == _car.CarAccountId)
                    .Sum(f => f.Volume);

                row = table.AddRow();
                row.Borders.Visible = false;

                row.Cells[0].MergeRight = 1;
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.1);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

                row = table.AddRow();
                row.Cells[0].AddParagraph($"приобретено топлива");
                row.Cells[1].AddParagraph($"{totalLitres} л");
                var literInUsd = (double)-totalFuelling / totalLitres;
                row.Cells[2].AddParagraph($"$ {literInUsd:N} за литр");

                row = table.AddRow();
                row.Cells[0].AddParagraph("расход топлива на 100 км");
                var litresFor100 = totalLitres / (_car.SaleMileage - _car.PurchaseMileage) * 100;
                row.Cells[1].AddParagraph($"{litresFor100:N} л");
                var usdFor100 = (double)-totalFuelling / (_car.SaleMileage - _car.PurchaseMileage) * 100;
                row.Cells[2].AddParagraph($"$ {usdFor100:N}");

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
