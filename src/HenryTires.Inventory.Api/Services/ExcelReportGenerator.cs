using ClosedXML.Excel;
using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Api.Services;

public class ExcelReportGenerator
{
    private static DateTime ToLocal(DateTime utc, int offsetMinutes)
        => utc.AddMinutes(offsetMinutes);

    private static string FormatTimestamp(DateTime utc, int offsetMinutes)
        => ToLocal(utc, offsetMinutes).ToString("MM/dd/yyyy hh:mm:ss tt");

    private static string FormatDate(DateTime utc, int offsetMinutes)
        => ToLocal(utc, offsetMinutes).ToString("MM/dd/yyyy");

    private static string FormatDateTime(DateTime utc, int offsetMinutes)
        => ToLocal(utc, offsetMinutes).ToString("MM/dd/yyyy hh:mm tt");

    // ── Corporate color palette ─────────────────────────────────────

    private static readonly XLColor Primary = XLColor.FromHtml("#1F2937");         // dark slate — titles
    private static readonly XLColor Secondary = XLColor.FromHtml("#4B5563");       // muted gray — labels
    private static readonly XLColor Accent = XLColor.FromHtml("#2563EB");          // corporate blue — totals
    private static readonly XLColor HeaderBg = XLColor.FromHtml("#F3F4F6");        // very light gray — table headers
    private static readonly XLColor Border = XLColor.FromHtml("#E5E7EB");          // subtle border
    private static readonly XLColor TextColor = XLColor.FromHtml("#111827");       // body text

    // ── Shared styling helpers ──────────────────────────────────────

    private static void StyleTitle(IXLCell cell, string text)
    {
        cell.Value = text;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 13;
        cell.Style.Font.FontColor = Primary;
    }

    private static void StyleMeta(IXLWorksheet ws, int row, string label, string value)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 1).Style.Font.FontColor = Secondary;
        ws.Cell(row, 1).Style.Font.FontSize = 10;
        ws.Cell(row, 2).Value = value;
        ws.Cell(row, 2).Style.Font.FontSize = 10;
        ws.Cell(row, 2).Style.Font.FontColor = TextColor;
    }

    private static void StyleHeaderRange(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.FontSize = 10;
        range.Style.Font.FontColor = Primary;
        range.Style.Fill.BackgroundColor = HeaderBg;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        range.Style.Border.BottomBorderColor = Border;
    }

    private static void StyleTotalLabel(IXLCell cell, string text)
    {
        cell.Value = text;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontColor = Accent;
    }

    private static void StyleTotalValue(IXLCell cell, object value, string? format = null)
    {
        cell.Value = XLCellValue.FromObject(value);
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontColor = Accent;
        if (format != null) cell.Style.NumberFormat.Format = format;
    }

    private static void StyleTotalRow(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.TopBorderColor = Border;
    }

    // ── Stock Report ────────────────────────────────────────────────

    public byte[] GenerateStockReport(StockReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Stock Report");

        StyleTitle(ws.Cell(1, 1), "Stock by Location");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        row++;

        // Headers
        var headerRow = row;
        ws.Cell(headerRow, 1).Value = "Item Code";
        ws.Cell(headerRow, 2).Value = "Description";
        ws.Cell(headerRow, 3).Value = "Condition";
        ws.Cell(headerRow, 4).Value = "On Hand";
        ws.Cell(headerRow, 5).Value = "Reserved";
        ws.Cell(headerRow, 6).Value = "Available";
        StyleHeaderRange(ws.Range(headerRow, 1, headerRow, 6));
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.ItemCode;
            ws.Cell(row, 2).Value = r.Description;
            ws.Cell(row, 3).Value = r.Condition;
            ws.Cell(row, 4).Value = r.OnHand;
            ws.Cell(row, 5).Value = r.Reserved;
            ws.Cell(row, 6).Value = r.Available;
            row++;
        }

        row++;

        // Totals
        ws.Cell(row, 2).Value = "New";
        ws.Cell(row, 4).Value = report.Totals.NewOnHand;
        ws.Cell(row, 5).Value = report.Totals.NewReserved;
        ws.Cell(row, 6).Value = report.Totals.NewAvailable;
        row++;

        ws.Cell(row, 2).Value = "Used";
        ws.Cell(row, 4).Value = report.Totals.UsedOnHand;
        ws.Cell(row, 5).Value = report.Totals.UsedReserved;
        ws.Cell(row, 6).Value = report.Totals.UsedAvailable;
        row++;

        StyleTotalLabel(ws.Cell(row, 2), "Total");
        StyleTotalValue(ws.Cell(row, 4), report.Totals.TotalOnHand);
        StyleTotalValue(ws.Cell(row, 5), report.Totals.TotalReserved);
        StyleTotalValue(ws.Cell(row, 6), report.Totals.TotalAvailable);
        StyleTotalRow(ws.Range(row, 1, row, 6));

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Sales Report ────────────────────────────────────────────────

    public byte[] GenerateSalesReport(SalesReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales Report");

        StyleTitle(ws.Cell(1, 1), "Sales Report");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));

        if (report.FromDateUtc.HasValue && report.ToDateUtc.HasValue)
            StyleMeta(ws, row++, "Period", $"{FormatDate(report.FromDateUtc.Value, offsetMinutes)} – {FormatDate(report.ToDateUtc.Value, offsetMinutes)}");

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        StyleMeta(ws, row++, "Total Sales", report.TotalCount.ToString());
        row++;

        var headerRow = row;
        ws.Cell(headerRow, 1).Value = "Sale #";
        ws.Cell(headerRow, 2).Value = "Branch";
        ws.Cell(headerRow, 3).Value = "Date";
        ws.Cell(headerRow, 4).Value = "Customer";
        ws.Cell(headerRow, 5).Value = "Items";
        ws.Cell(headerRow, 6).Value = "Lines Summary";
        ws.Cell(headerRow, 7).Value = "Payment";
        ws.Cell(headerRow, 8).Value = "Total";
        ws.Cell(headerRow, 9).Value = "Currency";
        StyleHeaderRange(ws.Range(headerRow, 1, headerRow, 9));
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.SaleNumber;
            ws.Cell(row, 2).Value = $"{r.BranchCode} — {r.BranchName}";
            ws.Cell(row, 3).Value = FormatDate(r.SaleDateUtc, offsetMinutes);
            ws.Cell(row, 4).Value = r.CustomerName ?? "";
            ws.Cell(row, 5).Value = r.LineCount;
            ws.Cell(row, 6).Value = r.LinesSummary;
            ws.Cell(row, 7).Value = r.PaymentMethod;
            ws.Cell(row, 8).Value = r.Total;
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 9).Value = r.Currency;
            row++;
        }

        row++;
        StyleTotalLabel(ws.Cell(row, 7), "Total");
        StyleTotalValue(ws.Cell(row, 8), report.Totals.GrandTotal, "#,##0.00");
        StyleTotalRow(ws.Range(row, 1, row, 9));

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Inventory Movements Report ──────────────────────────────────

    public byte[] GenerateInventoryMovementsReport(InventoryMovementsReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Inventory Movements");

        StyleTitle(ws.Cell(1, 1), "Inventory Movements");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));

        if (report.FromDateUtc.HasValue || report.ToDateUtc.HasValue)
        {
            var period = "";
            if (report.FromDateUtc.HasValue) period += FormatDate(report.FromDateUtc.Value, offsetMinutes);
            if (report.ToDateUtc.HasValue) period += $" – {FormatDate(report.ToDateUtc.Value, offsetMinutes)}";
            StyleMeta(ws, row++, "Period", period);
        }

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        if (!string.IsNullOrEmpty(report.TransactionType))
            StyleMeta(ws, row++, "Type", report.TransactionType);

        if (!string.IsNullOrEmpty(report.Status))
            StyleMeta(ws, row++, "Status", report.Status);

        StyleMeta(ws, row++, "Transactions", report.TotalCount.ToString());
        row++;

        var headerRow = row;
        ws.Cell(headerRow, 1).Value = "Transaction #";
        ws.Cell(headerRow, 2).Value = "Branch";
        ws.Cell(headerRow, 3).Value = "Type";
        ws.Cell(headerRow, 4).Value = "Status";
        ws.Cell(headerRow, 5).Value = "Date";
        ws.Cell(headerRow, 6).Value = "Last Status";
        ws.Cell(headerRow, 7).Value = "Item Code";
        ws.Cell(headerRow, 8).Value = "Condition";
        ws.Cell(headerRow, 9).Value = "Quantity";
        StyleHeaderRange(ws.Range(headerRow, 1, headerRow, 9));
        row++;

        foreach (var transaction in report.Transactions)
        {
            var firstLine = true;
            foreach (var line in transaction.Lines)
            {
                if (firstLine)
                {
                    ws.Cell(row, 1).Value = transaction.Number;
                    ws.Cell(row, 2).Value = transaction.BranchCode;
                    ws.Cell(row, 3).Value = transaction.Type;
                    ws.Cell(row, 4).Value = transaction.Status;
                    ws.Cell(row, 5).Value = FormatDate(transaction.TransactionDateUtc, offsetMinutes);
                    var lastStatus = transaction.StatusHistory.LastOrDefault();
                    ws.Cell(row, 6).Value = lastStatus != null ? FormatDateTime(lastStatus.Date, offsetMinutes) : "";
                    firstLine = false;
                }

                ws.Cell(row, 7).Value = line.ItemCode;
                ws.Cell(row, 8).Value = line.Condition;
                ws.Cell(row, 9).Value = line.Quantity;
                row++;
            }
        }

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Daily Close Report ──────────────────────────────────────────

    public byte[] GenerateDailyCloseReport(DailyCloseReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Daily Close");

        StyleTitle(ws.Cell(1, 1), "Daily Close");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));
        StyleMeta(ws, row++, "Date", FormatDate(report.DateUtc, offsetMinutes));

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        row++;

        // Summary
        ws.Cell(row, 1).Value = "Total Sales";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = report.Summary.TotalSalesCount;
        row++;

        ws.Cell(row, 1).Value = "Total Amount";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = report.Summary.TotalAmount;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Value = report.Summary.Currency;
        row++;
        row++;

        // Payment breakdown
        ws.Cell(row, 1).Value = "Payment Method";
        ws.Cell(row, 2).Value = "Count";
        ws.Cell(row, 3).Value = "Amount";
        StyleHeaderRange(ws.Range(row, 1, row, 3));
        row++;

        foreach (var pb in report.Summary.PaymentBreakdown)
        {
            ws.Cell(row, 1).Value = pb.PaymentMethod;
            ws.Cell(row, 2).Value = pb.Count;
            ws.Cell(row, 3).Value = pb.Amount;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        row++;

        // Details
        if (report.GroupBy == "hour" && report.HourGroups != null)
        {
            foreach (var hourGroup in report.HourGroups)
            {
                ws.Cell(row, 1).Value = $"{hourGroup.HourLabel}  ({hourGroup.HourCount} sales — ${hourGroup.HourTotal:F2})";
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;

                WriteDetailHeaders(ws, row);
                row++;

                foreach (var sale in hourGroup.Sales)
                {
                    WriteDetailRow(ws, row, sale, offsetMinutes);
                    row++;
                }

                row++;
            }
        }
        else
        {
            WriteDetailHeaders(ws, row);
            row++;

            foreach (var sale in report.Details)
            {
                WriteDetailRow(ws, row, sale, offsetMinutes);
                row++;
            }
        }

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Kardex Report ───────────────────────────────────────────────

    public byte[] GenerateKardexReport(KardexReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Kardex");

        StyleTitle(ws.Cell(1, 1), "Kardex");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));
        StyleMeta(ws, row++, "Item", $"{report.ItemCode} — {report.ItemDescription}");

        if (!string.IsNullOrEmpty(report.Condition))
            StyleMeta(ws, row++, "Condition", report.Condition);

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        if (report.FromDateUtc.HasValue || report.ToDateUtc.HasValue)
        {
            var period = "";
            if (report.FromDateUtc.HasValue) period += FormatDate(report.FromDateUtc.Value, offsetMinutes);
            if (report.ToDateUtc.HasValue) period += $" – {FormatDate(report.ToDateUtc.Value, offsetMinutes)}";
            StyleMeta(ws, row++, "Period", period);
        }

        StyleMeta(ws, row, "Total In", report.TotalIn.ToString());
        ws.Cell(row, 3).Value = "Total Out";
        ws.Cell(row, 3).Style.Font.FontColor = Secondary;
        ws.Cell(row, 4).Value = report.TotalOut;
        row++;
        row++;

        var headerRow = row;
        ws.Cell(headerRow, 1).Value = "Date";
        ws.Cell(headerRow, 2).Value = "Reference";
        ws.Cell(headerRow, 3).Value = "Type";
        ws.Cell(headerRow, 4).Value = "Branch";
        ws.Cell(headerRow, 5).Value = "In";
        ws.Cell(headerRow, 6).Value = "Out";
        ws.Cell(headerRow, 7).Value = "Balance";
        ws.Cell(headerRow, 8).Value = "Notes";
        StyleHeaderRange(ws.Range(headerRow, 1, headerRow, 8));
        row++;

        foreach (var entry in report.Entries)
        {
            ws.Cell(row, 1).Value = FormatDate(entry.DateUtc, offsetMinutes);
            ws.Cell(row, 2).Value = entry.ReferenceNumber;
            ws.Cell(row, 3).Value = entry.Type;
            ws.Cell(row, 4).Value = entry.BranchCode;
            ws.Cell(row, 5).Value = entry.In;
            ws.Cell(row, 6).Value = entry.Out;
            ws.Cell(row, 7).Value = entry.Balance;
            ws.Cell(row, 8).Value = entry.Notes ?? "";
            row++;
        }

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Sales by Volume Report ──────────────────────────────────────

    public byte[] GenerateSalesByVolumeReport(SalesByVolumeReportDto report, int offsetMinutes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales by Volume");

        StyleTitle(ws.Cell(1, 1), "Sales by Volume");

        int row = 2;
        StyleMeta(ws, row++, "Generated", FormatTimestamp(report.GeneratedAtUtc, offsetMinutes));

        if (report.FromDateUtc.HasValue && report.ToDateUtc.HasValue)
            StyleMeta(ws, row++, "Period", $"{FormatDate(report.FromDateUtc.Value, offsetMinutes)} – {FormatDate(report.ToDateUtc.Value, offsetMinutes)}");

        if (!string.IsNullOrEmpty(report.BranchCode))
            StyleMeta(ws, row++, "Branch", $"{report.BranchCode} — {report.BranchName}");

        StyleMeta(ws, row, "Quantity", report.TotalQuantitySold.ToString());
        ws.Cell(row, 3).Value = "Revenue";
        ws.Cell(row, 3).Style.Font.FontColor = Secondary;
        ws.Cell(row, 4).Value = report.TotalRevenue;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        row++;
        row++;

        var headerRow = row;
        ws.Cell(headerRow, 1).Value = "Item Code";
        ws.Cell(headerRow, 2).Value = "Description";
        ws.Cell(headerRow, 3).Value = "Condition";
        ws.Cell(headerRow, 4).Value = "Qty Sold";
        ws.Cell(headerRow, 5).Value = "Revenue";
        ws.Cell(headerRow, 6).Value = "Currency";
        StyleHeaderRange(ws.Range(headerRow, 1, headerRow, 6));
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.ItemCode;
            ws.Cell(row, 2).Value = r.Description;
            ws.Cell(row, 3).Value = r.Condition;
            ws.Cell(row, 4).Value = r.QuantitySold;
            ws.Cell(row, 5).Value = r.Revenue;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = r.Currency;
            row++;
        }

        row++;
        StyleTotalLabel(ws.Cell(row, 3), "Total");
        StyleTotalValue(ws.Cell(row, 4), report.TotalQuantitySold);
        StyleTotalValue(ws.Cell(row, 5), report.TotalRevenue, "#,##0.00");
        StyleTotalRow(ws.Range(row, 1, row, 6));

        ws.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    // ── Daily close detail helpers ──────────────────────────────────

    private static void WriteDetailHeaders(IXLWorksheet ws, int row)
    {
        ws.Cell(row, 1).Value = "Sale #";
        ws.Cell(row, 2).Value = "Date";
        ws.Cell(row, 3).Value = "Customer";
        ws.Cell(row, 4).Value = "Items";
        ws.Cell(row, 5).Value = "Payment";
        ws.Cell(row, 6).Value = "Total";
        ws.Cell(row, 7).Value = "Currency";
        StyleHeaderRange(ws.Range(row, 1, row, 7));
    }

    private static void WriteDetailRow(IXLWorksheet ws, int row, DailyCloseDetailDto sale, int offsetMinutes)
    {
        ws.Cell(row, 1).Value = sale.SaleNumber;
        ws.Cell(row, 2).Value = FormatDateTime(sale.SaleDateUtc, offsetMinutes);
        ws.Cell(row, 3).Value = sale.CustomerName ?? "";
        ws.Cell(row, 4).Value = sale.LineCount;
        ws.Cell(row, 5).Value = sale.PaymentMethod;
        ws.Cell(row, 6).Value = sale.Total;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Value = sale.Currency;
    }

    // ── Shared ──────────────────────────────────────────────────────

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
