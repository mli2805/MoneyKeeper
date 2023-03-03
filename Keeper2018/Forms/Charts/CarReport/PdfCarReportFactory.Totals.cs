using MigraDoc.DocumentObjectModel;
using System.Linq;

namespace Keeper2018
{
    public static partial class PdfCarReportFactory
    {
        private static void DrawTotals(this Section section, CarReportData carReportData, CarModel car, KeeperDataModel dataModel)
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
            var paragraph = row.Cells[0].AddParagraph($"{car.SaleDate.ToShortDateString()} - {car.PurchaseDate.ToShortDateString()}");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            row = table.AddRow();
            row.Cells[0].AddParagraph($"{(car.SaleDate - car.PurchaseDate).Days} дн.");
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
            paragraph = row.Cells[0].AddParagraph($"{car.SaleMileage} - {car.PurchaseMileage} = {car.SaleMileage - car.PurchaseMileage} км");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);


            row = table.AddRow();
            row.Cells[0].MergeRight = 1;
            row.Cells[0].AddParagraph("все расходы на 1 км");
            var forKm = (double)-total / (car.SaleMileage - car.PurchaseMileage);
            row.Cells[2].AddParagraph($"$ {forKm:N}");

            row = table.AddRow();
            row.Cells[0].MergeRight = 1;
            row.Cells[0].AddParagraph("топливо на 1 км");
            forKm = (double)-totalFuelling / (car.SaleMileage - car.PurchaseMileage);
            row.Cells[2].AddParagraph($"$ {forKm:N}");

            if (car.Id >= 3)
            {
                var totalLitres = dataModel.FuellingVms
                    .Where(f => f.CarAccountId == car.CarAccountId)
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
                var litresFor100 = totalLitres / (car.SaleMileage - car.PurchaseMileage) * 100;
                row.Cells[1].AddParagraph($"{litresFor100:N} л");
                var usdFor100 = (double)-totalFuelling / (car.SaleMileage - car.PurchaseMileage) * 100;
                row.Cells[2].AddParagraph($"$ {usdFor100:N}");

            }
        }
 }
}
