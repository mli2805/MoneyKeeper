using System;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;

namespace Keeper2018
{
    public static partial class PdfCarReportFactory
    {
        private static void DrawChart(this Section section, CarReportData carReportData)
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

        private static void DrawAggregateTable(this Section section, CarReportData carReportData)
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
    }
}
