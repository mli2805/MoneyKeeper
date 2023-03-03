using System.Linq;
using MigraDoc.DocumentObjectModel;

namespace Keeper2018
{
    public static partial class PdfCarReportFactory
    {
        private static void DrawTagTables(this Section section, CarReportData carReportData, bool isByTags, bool isBynInReport)
        {
            section.DrawOneTagTable(carReportData.Tags[0], isBynInReport);
            section.DrawOneTagTable(carReportData.Tags[1], isBynInReport);
            section.AddPageBreak();
            if (isByTags)
            {
                section.DrawOneTagTable(carReportData.Tags[2], isBynInReport);
                section.DrawOneTagTable(carReportData.Tags[3], isBynInReport);
                section.AddPageBreak();
                section.DrawOneTagTable(carReportData.Tags[4], isBynInReport);
            }
            else
            {
                section.DrawOneTagTable(carReportData.Tags[4], isBynInReport);
            }
        }

        private static void DrawOneTagTable(this Section section, CarReportTable tag, bool isBynInReport)
        {
            var caption = section.AddParagraph(tag.Russian);
            caption.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            caption.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.25;

            var clnDate = table.AddColumn("2.8cm");
            clnDate.Format.Alignment = ParagraphAlignment.Center;

            var clnSum = table.AddColumn("3.5cm");
            clnSum.Format.Alignment = ParagraphAlignment.Right;
            clnSum.RightPadding = Unit.FromCentimeter(0.5);

            var clnMileage = table.AddColumn("2.5cm");
            clnMileage.Format.Alignment = ParagraphAlignment.Center;

            var clnComment = table.AddColumn("10cm");
            clnComment.Format.Alignment = ParagraphAlignment.Left;

            foreach (var tableRow in tag.Table)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{tableRow.Date:dd/MM/yyyy}");
                var amount = isBynInReport
                    ? tableRow.AmountInCurrency + (tableRow.AmountInCurrency.EndsWith("usd")
                        ? "" : $"\n {tableRow.AmountInUsd:#,0.##} usd")
                    : $" {tableRow.AmountInUsd:#,0.##} usd";
                row.Cells[1].AddParagraph($"{amount}");
                row.Cells[2].AddParagraph(tableRow.Mileage == 0 ? "" : $"{tableRow.Mileage} км");
                row.Cells[3].AddParagraph($"{tableRow.Comment}");
            }
            var totalRow = table.AddRow();
            totalRow.Cells[0].AddParagraph("Итого");
            var sum = tag.Table.Sum(r => r.AmountInUsd);
            totalRow.Cells[1].AddParagraph($"{sum:#,0.##} usd");

            var offset = section.AddParagraph();
            offset.Format.SpaceAfter = Unit.FromCentimeter(1);
        }
    }
}