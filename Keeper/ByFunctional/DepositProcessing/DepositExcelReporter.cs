using System.Composition;
using Keeper.DomainModel.Deposit;
using Microsoft.Office.Interop.Excel;

namespace Keeper.ByFunctional.DepositProcessing
{
  [Export]
  public class DepositExcelReporter
  {
    public void Run(Deposit deposit)
    {
      var ws = CreateWorksheet();

      SetFormatForData(ws);
      ImportEvaluationData(ws, deposit);

      ImportEvaluationHeader(ws);
      SetFormatForHeader(ws);
    }

    private static Worksheet CreateWorksheet()
    {
      var xlApp = new Microsoft.Office.Interop.Excel.Application { Visible = true };
      var wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
      return (Worksheet)wb.Worksheets[1];
    }

    private void SetFormatForHeader(Worksheet ws)
    {
      ws.Range["A1"].EntireRow.HorizontalAlignment = XlHAlign.xlHAlignCenter;
      ws.Range["A1"].EntireRow.WrapText = true; // высота строки не должна была быть выставлена явно
      ws.Range["D1", "E1"].Merge();
      ws.Range["A1"].EntireRow.VerticalAlignment = XlVAlign.xlVAlignCenter;
    }

    private void ImportEvaluationHeader(Worksheet ws)
    {
      ws.Cells[1, 2] = "Дата";
      ws.Cells[1, 3] = "Остаток на конец дня";
      ws.Cells[1, 4] = "Ставка";
      ws.Cells[1, 6] = "Проценты за эту ночь";
      ws.Cells[1, 7] = "Не выплаченные проценты";
      ws.Cells[1, 8] = "Проценты нарастающим итогом";
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
      ws.Range["H1"].EntireColumn.NumberFormat = "[Blue]#,0";
    }

    private void ImportEvaluationData(Worksheet ws, Deposit deposit)
    {
      int i = 3;
      decimal total = 0;
      foreach (var line in deposit.CalculationData.DailyTable)
      {
        total += line.DayProfit;

        ws.Cells[i, 2] = line.Date;
        ws.Cells[i, 3] = line.Balance;
        ws.Cells[i, 4] = line.DepoRate;
        ws.Cells[i, 5] = "%";
        ws.Cells[i, 6] = line.DayProfit;
        ws.Cells[i, 7] = line.NotPaidProfit;
        ws.Cells[i, 8] = total;

        i++;
      }
    }
  }
}