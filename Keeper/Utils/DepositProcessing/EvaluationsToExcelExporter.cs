using System.Composition;
using System.Drawing;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Extentions;
using Microsoft.Office.Interop.Excel;

namespace Keeper.Utils.DepositProcessing
{
    [Export]
    public class EvaluationsToExcelExporter
    {
        public void ExportEvaluations(Deposit deposit)
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
            ws.Range["A1"].EntireRow.WrapText = true; // ������ ������ �� ������ ���� ���� ���������� ����
            ws.Range["D1", "E1"].Merge();
            ws.Range["A1"].EntireRow.VerticalAlignment = XlVAlign.xlVAlignCenter;
            ws.Activate();
            ws.Application.ActiveWindow.SplitRow = 2;
            ws.Application.ActiveWindow.FreezePanes = true;
        }

        private void ExportEvaluationHeader(Worksheet ws)
        {
            ws.Cells[1, 2] = "����";
            ws.Cells[1, 3] = "������� �� ����� ���";
            ws.Cells[1, 4] = "������";
            ws.Cells[1, 6] = "�������� �� ��������� ����";
            ws.Cells[1, 7] = "�� ����������� ��������";
            ws.Cells[1, 8] = "�������� ����������� ������";
            ws.Cells[1, 9] = "����";
            ws.Cells[1, 10] = "����������� �� ����";
            ws.Cells[1, 11] = "����������� ����������� ������";
        }

        private static void SetFormatForData(Worksheet ws)
        {
            ws.Range["B1"].EntireColumn.ColumnWidth = 12;
            ws.Range["B1"].EntireColumn.NumberFormat = "dd MMM yyyy";
            ws.Range["B1"].EntireColumn.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            ws.Range["C1"].EntireColumn.ColumnWidth = 15;
            ws.Range["C1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["D1"].EntireColumn.ColumnWidth = 5;
            ws.Range["E1"].EntireColumn.ColumnWidth = 2;
            ws.Range["F1"].EntireColumn.ColumnWidth = 15;
            ws.Range["F1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["G1"].EntireColumn.ColumnWidth = 15;
            ws.Range["G1"].EntireColumn.NumberFormat = "[Green]#,0";
            ws.Range["H1"].EntireColumn.ColumnWidth = 15;
            ws.Range["H1"].EntireColumn.NumberFormat = "[Green]#,0";
            ws.Range["I1"].EntireColumn.ColumnWidth = 9;
            ws.Range["I1"].EntireColumn.Font.Color = ColorTranslator.ToOle(Color.Silver);
            ws.Range["I1"].EntireColumn.NumberFormat = "#,0";
            ws.Range["J1"].EntireColumn.ColumnWidth = 12;
            ws.Range["J1"].EntireColumn.NumberFormat = "[Red]#,0";
            ws.Range["K1"].EntireColumn.ColumnWidth = 12;
            ws.Range["K1"].EntireColumn.NumberFormat = "[Red]#,0";
        }

        private void ExportEvaluationData(Worksheet ws, Deposit deposit)
        {
            var i = 4;
            decimal totalProcents = 0;
            decimal totalDevaluation = 0;
            foreach (var line in deposit.CalculationData.DailyTable)
            {
                totalProcents += line.DayProcents;
                totalDevaluation += line.DayDevaluation;

                ExportLineData(ws, i, line, totalProcents, totalDevaluation);
                if (deposit.IsItDayToPayProcents(line.Date)) HighLightPaymentLine(ws, i);

                i++;
            }
        }

        private static void HighLightPaymentLine(Worksheet ws, int i)
        {
            ws.Range["A"+i].EntireRow.Interior.Color = ColorTranslator.ToOle(Color.Yellow);
        }

        private static void ExportLineData(Worksheet ws, int i, DepositDailyLine line, decimal totalProcents,
            decimal totalDevaluation)
        {
            ws.Cells[i, 2] = line.Date;
            ws.Cells[i, 3] = line.Balance;
            ws.Cells[i, 4] = line.DepoRate;
            ws.Cells[i, 5] = "%";
            ws.Cells[i, 6] = line.DayProcents;
            ws.Cells[i, 7] = line.NotPaidProcents;
            ws.Cells[i, 8] = totalProcents;
            ws.Cells[i, 9] = line.CurrencyRate;
            ws.Cells[i, 10] = line.DayDevaluation;
            ws.Cells[i, 11] = totalDevaluation;
        }
    }
}