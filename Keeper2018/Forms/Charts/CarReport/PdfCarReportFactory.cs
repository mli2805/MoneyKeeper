using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public static partial class PdfCarReportFactory
    {
        public static PdfDocument CreateCarReport(this KeeperDataModel dataModel, int carId, bool isByTags)
        {
            var car = dataModel.Cars.First(c => c.Id == carId);
            var carReportData = dataModel.ExtractCarReportData(car, isByTags);

            Document doc = new Document();

            Section firstPageSection = doc.AddSection();
            firstPageSection.PageSetup.TopMargin = 40;
            firstPageSection.PageSetup.BottomMargin = 10;
            var paragraph = firstPageSection.AddParagraph();
            paragraph.AddFormattedText($"{car.Title} {car.IssueYear} г.в. {car.PurchaseDate:dd/MM/yyyy} - {car.SaleDate:dd/MM/yyyy}");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);
           
            firstPageSection.DrawChart(carReportData);
            firstPageSection.DrawAggregateTable(carReportData);
            firstPageSection.DrawTotals(carReportData, car, dataModel);

            Section tablesSection = doc.AddSection();
            tablesSection.PageSetup.TopMargin = 20;
            tablesSection.PageSetup.BottomMargin = 10;
            tablesSection.DrawTagTables(carReportData, isByTags);

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfDocumentRenderer.Document = doc;
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }
    }
}
