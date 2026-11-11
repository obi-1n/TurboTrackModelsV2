using ClosedXML.Excel;

namespace TurboTrackModels_Dotnet_Core.Controllers;

internal static class ReportExcel
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Create(
        string sheetName,
        IReadOnlyList<string> headers,
        IEnumerable<object?[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);
        for (var index = 0; index < headers.Count; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
            sheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var column = 0; column < row.Length; column++)
            {
                SetCellValue(sheet.Cell(rowIndex, column + 1), row[column]);
            }

            rowIndex++;
        }

        if (rowIndex > 2)
        {
            sheet.Range(1, 1, rowIndex - 1, headers.Count).SetAutoFilter();
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Clear();
                break;
            case string text:
                cell.Value = text;
                break;
            case int integer:
                cell.Value = integer;
                break;
            case long longInteger:
                cell.Value = longInteger;
                break;
            case decimal number:
                cell.Value = number;
                break;
            case double floatingPoint:
                cell.Value = floatingPoint;
                break;
            case DateTime date:
                cell.Value = date;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            default:
                cell.Value = value.ToString() ?? string.Empty;
                break;
        }
    }
}
