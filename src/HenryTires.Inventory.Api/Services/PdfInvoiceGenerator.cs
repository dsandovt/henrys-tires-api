using HenryTires.Inventory.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HenryTires.Inventory.Api.Services;

public class PdfInvoiceGenerator
{
    public byte[] GenerateInvoice(InvoiceDto invoice)
    {
        // Configure QuestPDF license (Community license for development)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);

                page.Header().Element(content => ComposeHeader(content, invoice));
                page.Content().Element(content => ComposeContent(content, invoice));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(invoice.GeneratedAtUtc.ToString("MM/dd/yyyy HH:mm:ss UTC"));
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
                col.Item().Text(invoice.CompanyInfo.LegalName).FontSize(16).SemiBold();

                if (!string.IsNullOrEmpty(invoice.CompanyInfo.TradeName))
                    col.Item().Text(invoice.CompanyInfo.TradeName).FontSize(10).FontColor(Colors.Grey.Darken1);

                col.Item().Text(invoice.CompanyInfo.AddressLine1).FontSize(9);
                col.Item().Text(invoice.CompanyInfo.CityStateZip).FontSize(9);
                col.Item().Text(invoice.CompanyInfo.Phone).FontSize(9);
            });

            column.Item().PaddingTop(15).PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, InvoiceDto invoice)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Spacing(12);

            // Invoice metadata section
            column.Item().Row(row =>
            {
                // Left column - Invoice details
                row.RelativeItem().Column(col =>
                {
                    col.Spacing(3);
                    col.Item().Text("INVOICE").FontSize(14).SemiBold();
                    col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").FontSize(10);
                    col.Item().Text($"Date: {invoice.InvoiceDateUtc:MM/dd/yyyy}").FontSize(10);
                    col.Item().Text($"Branch: {invoice.BranchCode} - {invoice.BranchName}").FontSize(10);
                    col.Item().Text($"Payment Method: {invoice.PaymentMethod}").FontSize(10);
                });

                // Right column - Customer information
                row.RelativeItem().Column(col =>
                {
                    col.Spacing(3);
                    col.Item().Text("BILL TO").FontSize(10).SemiBold();

                    if (!string.IsNullOrEmpty(invoice.CustomerName))
                        col.Item().Text(invoice.CustomerName).FontSize(10);

                    if (!string.IsNullOrEmpty(invoice.CustomerNumber))
                        col.Item().Text($"Customer #: {invoice.CustomerNumber}").FontSize(9).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrEmpty(invoice.CustomerPhone))
                        col.Item().Text($"Phone: {invoice.CustomerPhone}").FontSize(9).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrEmpty(invoice.PONumber))
                        col.Item().Text($"PO #: {invoice.PONumber}").FontSize(9).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrEmpty(invoice.ServiceRep))
                        col.Item().Text($"Service Rep: {invoice.ServiceRep}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            // Line items table - minimal design with tax/fee indicators
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Item Code
                    columns.RelativeColumn(5); // Description
                    columns.RelativeColumn(2); // Condition
                    columns.RelativeColumn(1.5f); // Qty
                    columns.RelativeColumn(2); // Unit Price
                    columns.RelativeColumn(2); // Total
                    columns.RelativeColumn(1); // T/F markers
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Item Code");
                    header.Cell().Element(HeaderStyle).Text("Description");
                    header.Cell().Element(HeaderStyle).Text("Condition");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Qty");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Price");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Total");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("Taxable");

                    static IContainer HeaderStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Black).Padding(5);
                    }
                });

                // Items
                foreach (var line in invoice.Lines)
                {
                    table.Cell().Element(CellStyle).Text(line.ItemCode).FontSize(9);
                    table.Cell().Element(CellStyle).Text(line.Description).FontSize(9);
                    table.Cell().Element(CellStyle).Text(line.Condition ?? "").FontSize(9);
                    table.Cell().Element(CellStyle).AlignRight().Text(line.Quantity.ToString()).FontSize(9);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{line.Currency} {line.UnitPrice:N2}").FontSize(9);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{line.Currency} {line.LineTotal:N2}").FontSize(9);

                    // Taxable indicator
                    var taxableText = line.IsTaxable ? "Yes" : "No";
                    table.Cell().Element(CellStyle).AlignCenter().Text(taxableText).FontSize(8).FontColor(Colors.Grey.Darken1);

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(3);
                    }
                }
            });

            column.Item().PaddingTop(5);

            // Notes section
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().Column(col =>
                {
                    col.Item().Text("Notes:").FontSize(9).SemiBold();
                    col.Item().Text(invoice.Notes).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
                column.Item().PaddingTop(5);
            }

            // Totals section - complete with all fields
            column.Item().AlignRight().Column(col =>
            {
                col.Spacing(2);

                var currency = invoice.Lines.Any() ? invoice.Lines.First().Currency : "USD";

                col.Item().Row(row =>
                {
                    row.AutoItem().Width(150).Text("Subtotal:");
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.Subtotal:N2}");
                });

                // Show tax calculation details
                if (invoice.Totals.TaxableBase > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(150).Text($"Taxable Base:");
                        row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.TaxableBase:N2}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    col.Item().Row(row =>
                    {
                        var taxPercent = Math.Round(invoice.Totals.SalesTaxRate * 100, 0);
                        row.AutoItem().Width(150).Text($"Sales Tax ({taxPercent}%):");
                        row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.SalesTaxAmount:N2}");
                    });
                }

                // Discount if applicable
                if (invoice.Totals.Discount > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(150).Text("Discount:");
                        row.AutoItem().Width(100).AlignRight().Text($"-{currency} {invoice.Totals.Discount:N2}").FontColor(Colors.Red.Medium);
                    });
                }

                col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Black);

                col.Item().PaddingTop(3).Row(row =>
                {
                    row.AutoItem().Width(150).Text("Grand Total:").SemiBold().FontSize(12);
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.GrandTotal:N2}").SemiBold().FontSize(12);
                });

                // Amount paid and due
                if (invoice.Totals.AmountPaid > 0)
                {
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.AutoItem().Width(150).Text("Amount Paid:");
                        row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.AmountPaid:N2}");
                    });
                }

                col.Item().Row(row =>
                {
                    row.AutoItem().Width(150).Text("Amount Due:").SemiBold();
                    row.AutoItem().Width(100).AlignRight().Text($"{currency} {invoice.Totals.AmountDue:N2}").SemiBold();
                });
            });

            column.Item().PaddingTop(15);

            // DISCLAIMER SECTION - MOST IMPORTANT
            column.Item().Column(col =>
            {
                col.Spacing(5);

                col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                col.Item().Text("DISCLAIMER").FontSize(9).SemiBold();

                col.Item().Text("ALL NEW TIRES ARE PURCHASED \"AS-IS\" WITHOUT WARRANTY UNLESS PROVIDED BY MANUFACTURE. ALL SALES ARE FINAL. NEW TIRES AND INNER TUBERS ARE SOLD \"AS-IS\" WITH ALL FAULTS AND WITH NO WARRANTY BY HENRY'S TIRES. HENRY'S TIRES HEREBY DISCLAIMS ANY WARRANTIES EXPRESSED OR IMPLIED OF MERCHANTABILITY OR FIRNESS FOR ANY PARTICULAR PURPOSE AND WITHOUT WARRANTY OR ANY KIND OR NATURE AS TO THE DESIGN, MANUFACTURE, STRUCTURAL INTEGRITY OR EXPECTED LIFE OF THE TIRE, INNER TUBE AND/OR CHAINS. BUYER ACKNOWLEDGES THAT HE/SHE HAS EXAMINED THE TIRE AND/OR INNER TUBE AND ACCEPTS THE SAME \"AS-IS\" WITH NO WARRANTIES OR GARENTEES. BUY AT YOUR OWN RISK. HENRYS TIRES DOES NOT EXTEND WARRANTIES, EITHER EXPRESS OR IMPLIED HENRYS TIRES DOES NOT ASSUME ANY WARRANTY OR LEGAL OBLIGATION OF ANY MANUFACTURER, DISTRIBUTOR, OR IMPORTER OF ANY PRODUCT OFFERED FOR SALE BY HENRYS TIRES. NO HENRYS TIRES EMPLOYEE OR DEALER HAS THE AUTHORITY TO MAKE ANY WARRANTY, REPRESENTATION, PROMISE OR AGREEMENT ON BEHALF OF HENRY'S TIRES EXCEPT AND REPRESENTATIONS MADE IN WRITING BY THE COMPANY'S PRESIDENT. TO THE EXTENT PERMITTED BY LAW, HENRYS TIRES DISCLAIMS LIABILITY FOR ALL CONSEQUENTIAL AND INCIDENTAL DAMAGES. BY SIGNING BELOW, I AGREE THAT I HAVE READ AND UNDERSTAND THAT I AM BUYING USED TIRES AT MY OWN RISK AND THAT HENRYS TIRES MAKES NO REPRESENTATIONS ABOUT THE CONDITION THAT THERE IS NO EXPRESSED OR IMPLIED WARRANTY.")
                    .FontSize(9)
                    .LineHeight(1.3f);

                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PRINT NAME").FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    row.ConstantItem(20); // Spacing

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("SIGNATURE").FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    row.ConstantItem(20); // Spacing

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DATE").FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Black);
                    });
                });
            });
        });
    }
}
