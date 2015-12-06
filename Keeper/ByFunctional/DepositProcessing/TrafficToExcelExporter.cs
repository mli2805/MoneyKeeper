using System.Composition;
using Keeper.DomainModel.Deposit;
using Microsoft.Office.Interop.Excel;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class TrafficToExcelExporter
    {
        public void ExportTraffic(Deposit deposit)
        {
            var ws = CreateWorksheet();

            SetFormatForData(ws);
            ExportEvaluationData(ws, deposit);

            ExportEvaluationHeader(ws);
            SetFormatForHeader(ws);
        }

        private static Worksheet CreateWorksheet()
        {
            var xlApp = new Application { Visible = true };
            var wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            return (Worksheet)wb.Worksheets[1];
        }

        private void SetFormatForHeader(Worksheet ws)
        {
            ws.Range["A1"].EntireRow.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            ws.Range["A1"].EntireRow.WrapText = true; // высота строки не должна была быть выставлена явно
            ws.Range["A1"].EntireRow.VerticalAlignment = XlVAlign.xlVAlignCenter;
            ws.Activate();
            ws.Application.ActiveWindow.SplitRow = 2;
            ws.Application.ActiveWindow.FreezePanes = true;
        }

        private void ExportEvaluationHeader(Worksheet ws)
        {
            ws.Cells[1, 2] = "Дата";
            ws.Cells[1, 3] = "Остаток до операции";
            ws.Cells[1, 4] = "Приход";
            ws.Cells[1, 5] = "Расход";
            ws.Cells[1, 6] = "Остаток после операции";
            ws.Cells[1, 7] = "Контрагент";
            ws.Cells[1, 8] = "Примечание";
        }

        private static void SetFormatForData(Worksheet ws)
        {
            ws.Range["B1"].EntireColumn.ColumnWidth = 12;
            ws.Range["B1"].EntireColumn.NumberFormat = "dd MMM yyyy";
            ws.Range["B1"].EntireColumn.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            ws.Range["C1"].EntireColumn.ColumnWidth = 15;
            ws.Range["C1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["D1"].EntireColumn.ColumnWidth = 15;
            ws.Range["D1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["E1"].EntireColumn.ColumnWidth = 15;
            ws.Range["E1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["F1"].EntireColumn.ColumnWidth = 15;
            ws.Range["F1"].EntireColumn.NumberFormat = "#,0";

            ws.Range["G1"].EntireColumn.ColumnWidth = 35;
            ws.Range["H1"].EntireColumn.ColumnWidth = 35;
        }

        private void ExportEvaluationData(Worksheet ws, Deposit deposit)
        {
            int i = 4;
            decimal total = 0;
            foreach (var depositTransaction in deposit.CalculationData.Traffic)
            {
                ExportLineData(ws, i, depositTransaction, ref total);
                i++;
            }
        }
        private static void ExportLineData(Worksheet ws, int i, DepositTransaction line, ref decimal total)
        {
            ws.Cells[i, 2] = line.Timestamp;
            ws.Cells[i, 3] = total;

            if (line.IsIncome())
                ws.Cells[i, 4] = line.Amount;
            else
                ws.Cells[i, 5] = line.Amount;
            total = total + line.Amount * line.Destination();
            ws.Cells[i, 6] = total;

            ws.Cells[i, 7] = line.Counteragent;
            ws.Cells[i, 8] = line.Comment;
        }
    }
}