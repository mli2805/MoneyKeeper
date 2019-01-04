using System.Collections.Generic;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Keeper2018
{
    public class PdfProvider
    {
        private readonly KeeperDb _db;

        public PdfProvider(KeeperDb db)
        {
            _db = db;
        }

        public PdfDocument Create(AccountModel accountModel)
        {
            Document doc = new Document();
            Section section = doc.AddSection();
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText($"{accountModel.Name}");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);
            var table = section.AddTable();
            table.Style = "Table";
            //            table.Borders.Color = Color.FromRgbColor(0x80, new Color());
            table.Borders.Width = 0.25;
            //            table.Borders.Left.Width = 0.5;
            //            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0; 
            //  table.TopPadding = Unit.FromCentimeter(1);
            //  table.SetEdge(0,0,3,0, Edge.DiagonalDown, BorderStyle.Single, 0.75, Color.Empty);
            
            var column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;
            column.RightPadding = Unit.FromCentimeter(0.5);
            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            foreach (var pdfTrafficLine in GetTagTraffic(accountModel))
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{pdfTrafficLine.Date:dd/MM/yyyy}");
                row.Cells[1].AddParagraph($"{pdfTrafficLine.Balance}");
                row.Cells[2].AddParagraph($"{pdfTrafficLine.Comment}");
            }

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfDocumentRenderer.Document = doc;
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private List<PdfTrafficLine> GetTagTraffic(AccountModel tag)
        {
            var result = new List<PdfTrafficLine>();
            foreach (var transaction in _db.Bin.Transactions.Values)
            {
                var balanceForTag = transaction.BalanceForTag(_db, tag.Id);
                if (balanceForTag != null)
                    result.Add(new PdfTrafficLine
                    {
                        Date = transaction.Timestamp,
                        Balance = balanceForTag,
                        Comment = transaction.Comment
                    });
            }
            return result;
        }
    }
}