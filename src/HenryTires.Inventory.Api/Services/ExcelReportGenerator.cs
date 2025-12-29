using ClosedXML.Excel;
using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Api.Services;

public class ExcelReportGenerator
{
    public byte[] GenerateStockReport(StockReportDto report)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Report");

        // Title
        worksheet.Cell(1, 1).Value = "Henry's Tires - Stock by Location Report";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Report info
        int currentRow = 2;
        worksheet.Cell(currentRow, 1).Value = "Generated:";
        worksheet.Cell(currentRow, 2).Value = report.GeneratedAtUtc.ToString("MM/dd/yyyy HH:mm:ss UTC");
        currentRow++;

        if (!string.IsNullOrEmpty(report.BranchCode))
        {
            worksheet.Cell(currentRow, 1).Value = "Branch:";
            worksheet.Cell(currentRow, 2).Value = $"{report.BranchCode} - {report.BranchName}";
            currentRow++;
        }

        currentRow++; // Empty row

        // Headers
        var headerRow = currentRow;
        worksheet.Cell(headerRow, 1).Value = "Item Code";
        worksheet.Cell(headerRow, 2).Value = "Description";
        worksheet.Cell(headerRow, 3).Value = "Condition";
        worksheet.Cell(headerRow, 4).Value = "On Hand";
        worksheet.Cell(headerRow, 5).Value = "Reserved";
        worksheet.Cell(headerRow, 6).Value = "Available";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        currentRow++;

        // Data rows
        foreach (var row in report.Rows)
        {
            worksheet.Cell(currentRow, 1).Value = row.ItemCode;
            worksheet.Cell(currentRow, 2).Value = row.Description;
            worksheet.Cell(currentRow, 3).Value = row.Condition;
            worksheet.Cell(currentRow, 4).Value = row.OnHand;
            worksheet.Cell(currentRow, 5).Value = row.Reserved;
            worksheet.Cell(currentRow, 6).Value = row.Available;
            currentRow++;
        }

        currentRow++; // Empty row

        // Totals section
        worksheet.Cell(currentRow, 1).Value = "TOTALS";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        currentRow++;

        worksheet.Cell(currentRow, 2).Value = "New Tires:";
        worksheet.Cell(currentRow, 3).Value = "On Hand";
        worksheet.Cell(currentRow, 4).Value = report.Totals.NewOnHand;
        worksheet.Cell(currentRow, 5).Value = "Reserved";
        worksheet.Cell(currentRow, 6).Value = report.Totals.NewReserved;
        worksheet.Cell(currentRow, 7).Value = "Available";
        worksheet.Cell(currentRow, 8).Value = report.Totals.NewAvailable;
        currentRow++;

        worksheet.Cell(currentRow, 2).Value = "Used Tires:";
        worksheet.Cell(currentRow, 3).Value = "On Hand";
        worksheet.Cell(currentRow, 4).Value = report.Totals.UsedOnHand;
        worksheet.Cell(currentRow, 5).Value = "Reserved";
        worksheet.Cell(currentRow, 6).Value = report.Totals.UsedReserved;
        worksheet.Cell(currentRow, 7).Value = "Available";
        worksheet.Cell(currentRow, 8).Value = report.Totals.UsedAvailable;
        currentRow++;

        worksheet.Cell(currentRow, 2).Value = "GRAND TOTAL:";
        worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 3).Value = "On Hand";
        worksheet.Cell(currentRow, 4).Value = report.Totals.TotalOnHand;
        worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 5).Value = "Reserved";
        worksheet.Cell(currentRow, 6).Value = report.Totals.TotalReserved;
        worksheet.Cell(currentRow, 6).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 7).Value = "Available";
        worksheet.Cell(currentRow, 8).Value = report.Totals.TotalAvailable;
        worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to memory stream and return bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateInventoryMovementsReport(InventoryMovementsReportDto report)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory Movements");

        // Title
        worksheet.Cell(1, 1).Value = "Henry's Tires - Inventory Movements Report";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Report info
        int currentRow = 2;
        worksheet.Cell(currentRow, 1).Value = "Generated:";
        worksheet.Cell(currentRow, 2).Value = report.GeneratedAtUtc.ToString("MM/dd/yyyy HH:mm:ss UTC");
        currentRow++;

        if (report.FromDateUtc.HasValue || report.ToDateUtc.HasValue)
        {
            worksheet.Cell(currentRow, 1).Value = "Period:";
            var period = "";
            if (report.FromDateUtc.HasValue) period += $"From {report.FromDateUtc.Value:MM/dd/yyyy}";
            if (report.ToDateUtc.HasValue) period += $" To {report.ToDateUtc.Value:MM/dd/yyyy}";
            worksheet.Cell(currentRow, 2).Value = period;
            currentRow++;
        }

        if (!string.IsNullOrEmpty(report.BranchCode))
        {
            worksheet.Cell(currentRow, 1).Value = "Branch:";
            worksheet.Cell(currentRow, 2).Value = $"{report.BranchCode} - {report.BranchName}";
            currentRow++;
        }

        if (!string.IsNullOrEmpty(report.TransactionType))
        {
            worksheet.Cell(currentRow, 1).Value = "Type:";
            worksheet.Cell(currentRow, 2).Value = report.TransactionType;
            currentRow++;
        }

        if (!string.IsNullOrEmpty(report.Status))
        {
            worksheet.Cell(currentRow, 1).Value = "Status:";
            worksheet.Cell(currentRow, 2).Value = report.Status;
            currentRow++;
        }

        worksheet.Cell(currentRow, 1).Value = "Total Transactions:";
        worksheet.Cell(currentRow, 2).Value = report.TotalCount;
        currentRow++;

        currentRow++; // Empty row

        // Headers
        var headerRow = currentRow;
        worksheet.Cell(headerRow, 1).Value = "Transaction #";
        worksheet.Cell(headerRow, 2).Value = "Branch";
        worksheet.Cell(headerRow, 3).Value = "Type";
        worksheet.Cell(headerRow, 4).Value = "Status";
        worksheet.Cell(headerRow, 5).Value = "Date";
        worksheet.Cell(headerRow, 6).Value = "Committed";
        worksheet.Cell(headerRow, 7).Value = "Item Code";
        worksheet.Cell(headerRow, 8).Value = "Condition";
        worksheet.Cell(headerRow, 9).Value = "Quantity";
        worksheet.Cell(headerRow, 10).Value = "Unit Price";
        worksheet.Cell(headerRow, 11).Value = "Currency";
        worksheet.Cell(headerRow, 12).Value = "Line Total";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 12);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        currentRow++;

        // Data rows
        foreach (var transaction in report.Transactions)
        {
            var firstLine = true;
            foreach (var line in transaction.Lines)
            {
                if (firstLine)
                {
                    worksheet.Cell(currentRow, 1).Value = transaction.TransactionNumber;
                    worksheet.Cell(currentRow, 2).Value = transaction.BranchCode;
                    worksheet.Cell(currentRow, 3).Value = transaction.Type;
                    worksheet.Cell(currentRow, 4).Value = transaction.Status;
                    worksheet.Cell(currentRow, 5).Value = transaction.TransactionDateUtc.ToString("MM/dd/yyyy");
                    worksheet.Cell(currentRow, 6).Value = transaction.CommittedAtUtc?.ToString("MM/dd/yyyy HH:mm") ?? "";
                    firstLine = false;
                }

                worksheet.Cell(currentRow, 7).Value = line.ItemCode;
                worksheet.Cell(currentRow, 8).Value = line.Condition;
                worksheet.Cell(currentRow, 9).Value = line.Quantity;
                worksheet.Cell(currentRow, 10).Value = line.UnitPrice;
                worksheet.Cell(currentRow, 11).Value = line.Currency;
                worksheet.Cell(currentRow, 12).Value = line.LineTotal;

                currentRow++;
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to memory stream and return bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
