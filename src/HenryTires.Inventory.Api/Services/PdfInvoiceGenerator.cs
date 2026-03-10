using HenryTires.Inventory.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HenryTires.Inventory.Api.Services;

public class PdfInvoiceGenerator
{
    // Corporate color palette
    private static readonly string Primary = "#1F2937";        // dark slate — titles
    private static readonly string Secondary = "#4B5563";      // muted gray — labels
    private static readonly string Accent = "#2563EB";         // corporate blue — totals
    private static readonly string HeaderBg = "#F3F4F6";       // very light gray — table headers
    private static readonly string Border = "#E5E7EB";         // subtle border
    private static readonly string TextColor = "#111827";       // body text

    private static DateTime ToLocal(DateTime utc, int offsetMinutes) => utc.AddMinutes(offsetMinutes);

    public byte[] GenerateInvoice(InvoiceDto invoice, int offsetMinutes)
    {
        // Configure QuestPDF license (Community license for development)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(35);

                page.Header().Element(content => ComposeHeader(content, invoice));
                page.Content().Element(content => ComposeContent(content, invoice, offsetMinutes));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    var generatedLocal = ToLocal(invoice.GeneratedAtUtc, offsetMinutes);
                    text.Span(generatedLocal.ToString("MM/dd/yyyy hh:mm:ss tt"));
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, InvoiceDto invoice)
    {
        container.Column(column =>
        {
            // Company header - left-aligned, clean typography
            column.Item().Column(col =>
            {
                col.Spacing(2);
                col.Item().Text(invoice.CompanyInfo.LegalName).FontSize(16).SemiBold().FontColor(TextColor);

                if (!string.IsNullOrEmpty(invoice.CompanyInfo.TradeName))
                    col.Item().Text(invoice.CompanyInfo.TradeName).FontSize(10).FontColor(Secondary);

                col.Item().Text(invoice.CompanyInfo.AddressLine1).FontSize(9).FontColor(TextColor);
                col.Item().Text(invoice.CompanyInfo.CityStateZip).FontSize(9).FontColor(TextColor);
                col.Item().Text(invoice.CompanyInfo.Phone).FontSize(9).FontColor(TextColor);
            });

            column.Item().PaddingTop(15).PaddingBottom(10).LineHorizontal(1).LineColor(Border);
        });
    }

    private const int MaxLinesPerPage = 6;

    private void ComposeContent(IContainer container, InvoiceDto invoice, int offsetMinutes)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Spacing(12);

            // Invoice metadata section
            ComposeInvoiceMetadata(column, invoice, offsetMinutes);

            column.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Border);

            // Line items table — paginate in chunks of MaxLinesPerPage
            var linePages = invoice.Lines
                .Select((line, index) => new { line, index })
                .GroupBy(x => x.index / MaxLinesPerPage)
                .Select(g => g.Select(x => x.line).ToList())
                .ToList();

            for (int pageIdx = 0; pageIdx < linePages.Count; pageIdx++)
            {
                if (pageIdx > 0)
                {
                    column.Item().PageBreak();
                    // Repeat metadata on continuation pages
                    ComposeInvoiceMetadata(column, invoice, offsetMinutes);
                    column.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Border);
                    column.Item().Text($"(continued — page {pageIdx + 1} of {linePages.Count})").FontSize(8).FontColor(Secondary);
                }

                ComposeLineItemsTable(column, linePages[pageIdx]);
            }

            column.Item().PaddingTop(5);

            // Notes section
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().Column(col =>
                {
                    col.Item().Text("Notes:").FontSize(9).SemiBold().FontColor(Primary);
                    col.Item().Text(invoice.Notes).FontSize(8).FontColor(Secondary);
                });
                column.Item().PaddingTop(5);
            }

            // Totals section
            ComposeTotals(column, invoice);

            column.Item().PaddingTop(6);

            // Disclaimer section
            ComposeDisclaimer(column);
        });
    }

    private void ComposeInvoiceMetadata(ColumnDescriptor column, InvoiceDto invoice, int offsetMinutes)
    {
        column.Item().Row(row =>
        {
            // Left column - Invoice details
            row.RelativeItem().Column(col =>
            {
                col.Spacing(3);
                col.Item().Text(invoice.DocumentType ?? "INVOICE").FontSize(14).SemiBold().FontColor(Primary);
                col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").FontSize(10).FontColor(TextColor);
                var invoiceDateLocal = ToLocal(invoice.InvoiceDateUtc, offsetMinutes);
                col.Item().Text($"Date: {invoiceDateLocal:MM/dd/yyyy}").FontSize(10).FontColor(TextColor);
                col.Item().Text($"Branch: {invoice.BranchCode} - {invoice.BranchName}").FontSize(10).FontColor(TextColor);
                if (invoice.PaymentDetails != null && invoice.PaymentDetails.Any())
                {
                    col.Item().Text("Payment Methods:").FontSize(10).FontColor(TextColor);
                    foreach (var pd in invoice.PaymentDetails)
                    {
                        var checkInfo = !string.IsNullOrEmpty(pd.CheckNumber) ? $" (Check #{pd.CheckNumber})" : "";
                        col.Item().Text($"  {pd.Method} - ${pd.Amount:N2}{checkInfo}").FontSize(9).FontColor(Secondary);
                    }
                }
                else
                {
                    col.Item().Text($"Payment Method: {invoice.PaymentMethod}").FontSize(10).FontColor(TextColor);
                }
            });

            // Right column - Customer information
            row.RelativeItem().Column(col =>
            {
                col.Spacing(3);
                col.Item().Text("BILL TO").FontSize(10).SemiBold().FontColor(Primary);

                if (!string.IsNullOrEmpty(invoice.CustomerName))
                    col.Item().Text(invoice.CustomerName).FontSize(10).FontColor(TextColor);

                if (!string.IsNullOrEmpty(invoice.CustomerNumber))
                    col.Item().Text($"Customer #: {invoice.CustomerNumber}").FontSize(9).FontColor(Secondary);

                if (!string.IsNullOrEmpty(invoice.CustomerPhone))
                    col.Item().Text($"Phone: {invoice.CustomerPhone}").FontSize(9).FontColor(Secondary);

                if (!string.IsNullOrEmpty(invoice.PONumber))
                    col.Item().Text($"PO #: {invoice.PONumber}").FontSize(9).FontColor(Secondary);

                if (!string.IsNullOrEmpty(invoice.ServiceRep))
                    col.Item().Text($"Service Rep: {invoice.ServiceRep}").FontSize(9).FontColor(Secondary);
            });
        });
    }

    private void ComposeLineItemsTable(ColumnDescriptor column, List<InvoiceLineDto> lines)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Item Code
                columns.RelativeColumn(5); // Description
                columns.RelativeColumn(2); // Condition
                columns.RelativeColumn(1.5f); // Qty
                columns.RelativeColumn(2); // Unit Price
                columns.RelativeColumn(2); // Total
                columns.RelativeColumn(1); // Taxable
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("Item Code").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).Text("Description").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).Text("Condition").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).AlignRight().Text("Qty").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Price").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).AlignRight().Text("Total").FontSize(9).SemiBold().FontColor(Primary);
                header.Cell().Element(HeaderStyle).AlignCenter().Text("Taxable").FontSize(9).SemiBold().FontColor(Primary);

                static IContainer HeaderStyle(IContainer container)
                {
                    return container.Background(HeaderBg).BorderBottom(0.5f).BorderColor(Border).Padding(5);
                }
            });

            foreach (var line in lines)
            {
                table.Cell().Element(CellStyle).Text(line.ItemCode).FontSize(9);
                table.Cell().Element(CellStyle).Text(line.Description).FontSize(9);
                table.Cell().Element(CellStyle).Text(line.Condition ?? "").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text(line.Quantity.ToString()).FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{line.Currency} {line.UnitPrice:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{line.Currency} {line.LineTotal:N2}").FontSize(9);

                var taxableText = line.IsTaxable ? "Yes" : "No";
                table.Cell().Element(CellStyle).AlignCenter().Text(taxableText).FontSize(8).FontColor(Secondary);

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(0.5f).BorderColor(Border).PaddingVertical(4).PaddingHorizontal(3);
                }
            }
        });
    }

    private void ComposeTotals(ColumnDescriptor column, InvoiceDto invoice)
    {
        column.Item().AlignRight().Column(col =>
        {
            col.Spacing(2);

            var currency = invoice.Lines.Any() ? invoice.Lines.First().Currency : "USD";

            col.Item().Row(row =>
            {
                row.AutoItem().Width(150).Text("Subtotal:").FontColor(TextColor);
                row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.Subtotal:N2}").FontColor(TextColor);
            });

            if (invoice.Totals.TaxableBase > 0)
            {
                col.Item().Row(row =>
                {
                    row.AutoItem().Width(150).Text($"Taxable Base:").FontColor(Secondary);
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.TaxableBase:N2}").FontSize(9).FontColor(Secondary);
                });
                col.Item().Row(row =>
                {
                    var taxPercent = Math.Round(invoice.Totals.SalesTaxRate * 100, 0);
                    row.AutoItem().Width(150).Text($"Sales Tax ({taxPercent}%):").FontColor(TextColor);
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.SalesTaxAmount:N2}").FontColor(TextColor);
                });
            }

            if (invoice.Totals.Discount > 0)
            {
                col.Item().Row(row =>
                {
                    row.AutoItem().Width(150).Text("Discount:").FontColor(TextColor);
                    row.AutoItem().Width(100).AlignRight().Text($"-{currency} {invoice.Totals.Discount:N2}").FontColor(TextColor);
                });
            }

            col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Border);

            col.Item().PaddingTop(3).Row(row =>
            {
                row.AutoItem().Width(150).Text("Grand Total:").SemiBold().FontSize(12).FontColor(Accent);
                row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.GrandTotal:N2}").SemiBold().FontSize(12).FontColor(Accent);
            });

            if (invoice.Totals.AmountPaid > 0)
            {
                col.Item().PaddingTop(3).Row(row =>
                {
                    row.AutoItem().Width(150).Text("Amount Paid:").FontColor(TextColor);
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.AmountPaid:N2}").FontColor(TextColor);
                });
            }

            col.Item().Row(row =>
            {
                row.AutoItem().Width(150).Text("Amount Due:").SemiBold().FontColor(Accent);
                row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.AmountDue:N2}").SemiBold().FontColor(Accent);
            });
        });
    }

    private void ComposeDisclaimer(ColumnDescriptor column)
    {
        column.Item().Column(col =>
        {
            col.Spacing(3);

            col.Item().PaddingBottom(3).LineHorizontal(0.5f).LineColor(Border);

            col.Item().Text("DISCLAIMER").FontSize(5.5f).SemiBold().FontColor(Primary);

            col.Item().Text("ALL NEW TIRES ARE PURCHASED \"AS-IS\" WITHOUT WARRANTY UNLESS PROVIDED BY MANUFACTURE. ALL SALES ARE FINAL. NEW TIRES AND INNER TUBERS ARE SOLD \"AS-IS\" WITH ALL FAULTS AND WITH NO WARRANTY BY HENRY'S TIRES. HENRY'S TIRES HEREBY DISCLAIMS ANY WARRANTIES EXPRESSED OR IMPLIED OF MERCHANTABILITY OR FIRNESS FOR ANY PARTICULAR PURPOSE AND WITHOUT WARRANTY OR ANY KIND OR NATURE AS TO THE DESIGN, MANUFACTURE, STRUCTURAL INTEGRITY OR EXPECTED LIFE OF THE TIRE, INNER TUBE AND/OR CHAINS. BUYER ACKNOWLEDGES THAT HE/SHE HAS EXAMINED THE TIRE AND/OR INNER TUBE AND ACCEPTS THE SAME \"AS-IS\" WITH NO WARRANTIES OR GARENTEES. BUY AT YOUR OWN RISK. HENRYS TIRES DOES NOT EXTEND WARRANTIES, EITHER EXPRESS OR IMPLIED HENRYS TIRES DOES NOT ASSUME ANY WARRANTY OR LEGAL OBLIGATION OF ANY MANUFACTURER, DISTRIBUTOR, OR IMPORTER OF ANY PRODUCT OFFERED FOR SALE BY HENRYS TIRES. NO HENRYS TIRES EMPLOYEE OR DEALER HAS THE AUTHORITY TO MAKE ANY WARRANTY, REPRESENTATION, PROMISE OR AGREEMENT ON BEHALF OF HENRY'S TIRES EXCEPT AND REPRESENTATIONS MADE IN WRITING BY THE COMPANY'S PRESIDENT. TO THE EXTENT PERMITTED BY LAW, HENRYS TIRES DISCLAIMS LIABILITY FOR ALL CONSEQUENTIAL AND INCIDENTAL DAMAGES. BY SIGNING BELOW, I AGREE THAT I HAVE READ AND UNDERSTAND THAT I AM BUYING USED TIRES AT MY OWN RISK AND THAT HENRYS TIRES MAKES NO REPRESENTATIONS ABOUT THE CONDITION THAT THERE IS NO EXPRESSED OR IMPLIED WARRANTY.")
                .FontSize(5f)
                .LineHeight(1.0f);

            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("PRINT NAME").FontSize(7).FontColor(Secondary);
                    c.Item().PaddingTop(6).LineHorizontal(1).LineColor(TextColor);
                });

                row.ConstantItem(15);

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("SIGNATURE").FontSize(7).FontColor(Secondary);
                    c.Item().PaddingTop(6).LineHorizontal(1).LineColor(TextColor);
                });

                row.ConstantItem(15);

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("DATE").FontSize(7).FontColor(Secondary);
                    c.Item().PaddingTop(6).LineHorizontal(1).LineColor(TextColor);
                });
            });
        });
    }
}
