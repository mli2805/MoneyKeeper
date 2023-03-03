using System.Linq;
using MigraDoc.DocumentObjectModel;

namespace Keeper2018
{
    public static partial class PdfCarReportFactory
    {
        private static void DrawTagTables(this Section section, CarReportData carReportData, bool isByTags)
        {
            section.DrawOneTagTable(carReportData.Tags[0]);
            section.DrawOneTagTable(carReportData.Tags[1]);
            section.AddPageBreak();
            if (isByTags)
            {
                section.DrawOneTagTable(carReportData.Tags[2]);
                section.DrawOneTagTable(carReportData.Tags[3]);
                section.AddPageBreak();
                section.DrawOneTagTable(carReportData.Tags[4]);
            }
            else
            {
                section.DrawOneTagTable(carReportData.Tags[4]);
            }
        }

        private static void DrawOneTagTable(this Section section, CarReportTable tag)
        {
            var caption = section.AddParagraph(tag.Russian);
            caption.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            caption.Format.SpaceAfter = Unit.FromCentimeter(0.3);

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

            foreach (var tableRow in tag.Table)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{tableRow.Date:dd/MM/yyyy}");
                var amount = tableRow.AmountInCurrency + (tableRow.AmountInCurrency.EndsWith("usd") ? "" : $"\n {tableRow.AmountInUsd:#,0.##} usd");
                row.Cells[1].AddParagraph($"{amount}");
                row.Cells[2].AddParagraph($"{tableRow.Comment}");
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