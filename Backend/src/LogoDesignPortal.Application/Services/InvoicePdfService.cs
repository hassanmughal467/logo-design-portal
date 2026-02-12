using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LogoDesignPortal.Application.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly IApplicationDbContext _context;

    public InvoicePdfService(IApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceResponseDto invoice)
    {
        // Load business settings for company info on invoice
        var businessSettings = await _context.Settings
            .Where(s => s.Category == "Business" && !s.IsDeleted)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        var companyName = businessSettings.GetValueOrDefault("companyName", "Logo Design Agency");
        var companyEmail = businessSettings.GetValueOrDefault("email", "");
        var companyPhone = businessSettings.GetValueOrDefault("phone", "");
        var companyAddress = businessSettings.GetValueOrDefault("address", "");
        var companyCity = businessSettings.GetValueOrDefault("city", "");
        var companyState = businessSettings.GetValueOrDefault("state", "");
        var companyCountry = businessSettings.GetValueOrDefault("country", "");
        var companyPostalCode = businessSettings.GetValueOrDefault("postalCode", "");
        var companyWebsite = businessSettings.GetValueOrDefault("website", "");

        // Build full company address
        var companyFullAddress = BuildAddress(companyAddress, companyCity, companyState, companyCountry, companyPostalCode);

        // Load invoice settings for currency
        var invoiceSettings = await _context.Settings
            .Where(s => s.Category == "Invoice" && !s.IsDeleted)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        var currency = invoiceSettings.GetValueOrDefault("currency", "USD");
        var currencySymbol = GetCurrencySymbol(currency);

        // Status colors
        var statusColor = invoice.Status switch
        {
            "Paid" => "#22c55e",
            "Overdue" => "#ef4444",
            _ => "#f59e0b"
        };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // Header
                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        // Company Info (Left)
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(companyName)
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);

                            if (!string.IsNullOrWhiteSpace(companyFullAddress))
                                col.Item().Text(companyFullAddress).FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(companyPhone))
                                col.Item().Text($"Phone: {companyPhone}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(companyEmail))
                                col.Item().Text($"Email: {companyEmail}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(companyWebsite))
                                col.Item().Text($"Web: {companyWebsite}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        // Invoice Title & Status (Right)
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("INVOICE")
                                .FontSize(28).Bold().FontColor(Colors.Blue.Darken2);

                            col.Item().PaddingTop(5).Row(statusRow =>
                            {
                                statusRow.AutoItem()
                                    .Background(statusColor)
                                    .Padding(4)
                                    .Text(invoice.Status.ToUpper())
                                    .FontSize(10).Bold().FontColor(Colors.White);
                            });
                        });
                    });
                });

                // Content
                page.Content().PaddingVertical(20).Column(content =>
                {
                    // Invoice Details & Client Info
                    content.Item().Row(row =>
                    {
                        // Invoice Details (Left)
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Grey.Lighten4).Padding(12).Column(col =>
                        {
                            col.Item().Text("Invoice Details").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().PaddingTop(6);

                            col.Item().Row(r =>
                            {
                                r.AutoItem().Width(100).Text("Invoice #:").SemiBold();
                                r.RelativeItem().Text(invoice.InvoiceNumber);
                            });
                            col.Item().PaddingTop(3).Row(r =>
                            {
                                r.AutoItem().Width(100).Text("Issue Date:").SemiBold();
                                r.RelativeItem().Text(invoice.IssueDate.ToString("MMM dd, yyyy"));
                            });
                            col.Item().PaddingTop(3).Row(r =>
                            {
                                r.AutoItem().Width(100).Text("Due Date:").SemiBold();
                                r.RelativeItem().Text(invoice.DueDate?.ToString("MMM dd, yyyy") ?? "N/A");
                            });
                            col.Item().PaddingTop(3).Row(r =>
                            {
                                r.AutoItem().Width(100).Text("Billing Type:").SemiBold();
                                r.RelativeItem().Text(invoice.BillingTypeDisplay);
                            });
                            if (!string.IsNullOrWhiteSpace(invoice.PaymentMethod))
                            {
                                col.Item().PaddingTop(3).Row(r =>
                                {
                                    r.AutoItem().Width(100).Text("Payment:").SemiBold();
                                    r.RelativeItem().Text(invoice.PaymentMethod);
                                });
                            }
                            if (invoice.PaidDate.HasValue)
                            {
                                col.Item().PaddingTop(3).Row(r =>
                                {
                                    r.AutoItem().Width(100).Text("Paid Date:").SemiBold();
                                    r.RelativeItem().Text(invoice.PaidDate.Value.ToString("MMM dd, yyyy"))
                                        .FontColor("#22c55e");
                                });
                            }
                        });

                        row.ConstantItem(20); // Spacer

                        // Client Info (Right)
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Grey.Lighten4).Padding(12).Column(col =>
                        {
                            col.Item().Text("Bill To").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().PaddingTop(6);
                            col.Item().Text(invoice.ClientName).SemiBold().FontSize(11);

                            // Load client details for address
                            var clientProfile = _context.ClientProfiles
                                .Include(c => c.User)
                                .FirstOrDefault(c => c.UserId == invoice.ClientId && !c.IsDeleted);

                            if (clientProfile != null)
                            {
                                if (!string.IsNullOrWhiteSpace(clientProfile.CompanyName))
                                    col.Item().PaddingTop(2).Text(clientProfile.CompanyName).FontSize(9);

                                var clientAddress = BuildAddress(
                                    clientProfile.Address, clientProfile.City,
                                    clientProfile.State, clientProfile.Country,
                                    clientProfile.PostalCode);

                                if (!string.IsNullOrWhiteSpace(clientAddress))
                                    col.Item().PaddingTop(2).Text(clientAddress).FontSize(9);

                                if (!string.IsNullOrWhiteSpace(clientProfile.PhoneNumber))
                                    col.Item().PaddingTop(2).Text($"Phone: {clientProfile.PhoneNumber}").FontSize(9);

                                if (!string.IsNullOrWhiteSpace(clientProfile.User?.Email))
                                    col.Item().PaddingTop(2).Text($"Email: {clientProfile.User.Email}").FontSize(9);
                            }
                        });
                    });

                    content.Item().PaddingTop(20);

                    // Items Table
                    content.Item().Table(table =>
                    {
                        // Column definitions
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // #
                            columns.RelativeColumn(3);   // Description
                            columns.RelativeColumn(1.5f); // Order
                            columns.RelativeColumn(1);   // Amount
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                .Text("#").FontColor(Colors.White).SemiBold().FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                .Text("Description").FontColor(Colors.White).SemiBold().FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                .Text("Order").FontColor(Colors.White).SemiBold().FontSize(9);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).AlignRight()
                                .Text("Amount").FontColor(Colors.White).SemiBold().FontSize(9);
                        });

                        // Item rows
                        for (int i = 0; i < invoice.Items.Count; i++)
                        {
                            var item = invoice.Items[i];
                            var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            table.Cell().Background(bgColor).Padding(8)
                                .Text((i + 1).ToString()).FontSize(9);
                            table.Cell().Background(bgColor).Padding(8)
                                .Text(item.Description).FontSize(9);
                            table.Cell().Background(bgColor).Padding(8)
                                .Text(item.OrderTitle ?? (item.OrderId.HasValue ? item.OrderId.Value.ToString()[..8] : "—")).FontSize(9);
                            table.Cell().Background(bgColor).Padding(8).AlignRight()
                                .Text($"{currencySymbol}{item.Amount:N2}").FontSize(9);
                        }

                        // If no items, show a placeholder
                        if (!invoice.Items.Any())
                        {
                            table.Cell().ColumnSpan(4).Padding(12).AlignCenter()
                                .Text("No line items").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        }
                    });

                    content.Item().PaddingTop(10);

                    // Totals Section
                    content.Item().AlignRight().Width(250).Column(totals =>
                    {
                        // Subtotal
                        totals.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").SemiBold();
                            r.AutoItem().AlignRight().Text($"{currencySymbol}{invoice.Amount:N2}");
                        });

                        // Tax
                        if (invoice.TaxAmount > 0)
                        {
                            totals.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).Row(r =>
                            {
                                r.RelativeItem().Text("Tax:").SemiBold();
                                r.AutoItem().AlignRight().Text($"{currencySymbol}{invoice.TaxAmount:N2}");
                            });
                        }

                        // Total
                        totals.Item().Background(Colors.Blue.Darken2).Padding(8).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").Bold().FontColor(Colors.White).FontSize(12);
                            r.AutoItem().AlignRight().Text($"{currencySymbol}{invoice.TotalAmount:N2}")
                                .Bold().FontColor(Colors.White).FontSize(12);
                        });
                    });

                    // Notes
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        content.Item().PaddingTop(25).Column(notes =>
                        {
                            notes.Item().Text("Notes").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            notes.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.Grey.Lighten5).Padding(10)
                                .Text(invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });

                // Footer
                page.Footer().Column(footer =>
                {
                    footer.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Thank you for your business!").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span($"Generated on {DateTime.UtcNow:MMM dd, yyyy}  |  Page ")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateInvoiceReportPdfAsync(List<InvoiceResponseDto> invoices, string reportTitle = "Invoice Report")
    {
        // Load business settings
        var businessSettings = await _context.Settings
            .Where(s => s.Category == "Business" && !s.IsDeleted)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        var companyName = businessSettings.GetValueOrDefault("companyName", "Logo Design Agency");
        var companyEmail = businessSettings.GetValueOrDefault("email", "");
        var companyPhone = businessSettings.GetValueOrDefault("phone", "");

        // Load invoice settings for currency
        var invoiceSettings = await _context.Settings
            .Where(s => s.Category == "Invoice" && !s.IsDeleted)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        var currency = invoiceSettings.GetValueOrDefault("currency", "USD");
        var currencySymbol = GetCurrencySymbol(currency);

        // Calculate summary
        var totalAmount = invoices.Sum(i => i.TotalAmount);
        var paidAmount = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var pendingAmount = invoices.Where(i => i.Status != "Paid").Sum(i => i.TotalAmount);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // Header
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(companyName)
                                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrWhiteSpace(companyEmail))
                                col.Item().Text(companyEmail).FontSize(9).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(companyPhone))
                                col.Item().Text(companyPhone).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(reportTitle)
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Generated: {DateTime.UtcNow:MMM dd, yyyy}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"Total Invoices: {invoices.Count}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    header.Item().PaddingTop(10).BorderBottom(2).BorderColor(Colors.Blue.Darken2);
                });

                // Content
                page.Content().PaddingVertical(15).Column(content =>
                {
                    // Summary Cards
                    content.Item().Row(row =>
                    {
                        // Total
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Blue.Lighten5).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Amount").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"{currencySymbol}{totalAmount:N2}")
                                .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        row.ConstantItem(10);

                        // Paid
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background("#f0fdf4").Padding(10).Column(col =>
                        {
                            col.Item().Text("Paid").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"{currencySymbol}{paidAmount:N2}")
                                .FontSize(16).Bold().FontColor("#22c55e");
                        });

                        row.ConstantItem(10);

                        // Pending
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background("#fffbeb").Padding(10).Column(col =>
                        {
                            col.Item().Text("Pending").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"{currencySymbol}{pendingAmount:N2}")
                                .FontSize(16).Bold().FontColor("#f59e0b");
                        });
                    });

                    content.Item().PaddingTop(15);

                    // Invoices Table
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);   // #
                            columns.RelativeColumn(2);    // Invoice #
                            columns.RelativeColumn(2);    // Client
                            columns.RelativeColumn(1);    // Amount
                            columns.RelativeColumn(1);    // Status
                            columns.RelativeColumn(1.2f); // Due Date
                            columns.RelativeColumn(1.2f); // Paid Date
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("#").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("Invoice #").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("Client").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignRight()
                                .Text("Amount").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6).AlignCenter()
                                .Text("Status").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("Due Date").FontColor(Colors.White).SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text("Paid Date").FontColor(Colors.White).SemiBold().FontSize(8);
                        });

                        // Rows
                        for (int i = 0; i < invoices.Count; i++)
                        {
                            var inv = invoices[i];
                            var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            var statusColor = inv.Status switch
                            {
                                "Paid" => "#22c55e",
                                "Overdue" => "#ef4444",
                                _ => "#f59e0b"
                            };

                            table.Cell().Background(bgColor).Padding(6)
                                .Text((i + 1).ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(6)
                                .Text(inv.InvoiceNumber).FontSize(8);
                            table.Cell().Background(bgColor).Padding(6)
                                .Text(inv.ClientName).FontSize(8);
                            table.Cell().Background(bgColor).Padding(6).AlignRight()
                                .Text($"{currencySymbol}{inv.TotalAmount:N2}").FontSize(8);
                            table.Cell().Background(bgColor).Padding(6).AlignCenter()
                                .Text(inv.Status).FontSize(8).FontColor(statusColor).SemiBold();
                            table.Cell().Background(bgColor).Padding(6)
                                .Text(inv.DueDate?.ToString("MMM dd, yyyy") ?? "N/A").FontSize(8);
                            table.Cell().Background(bgColor).Padding(6)
                                .Text(inv.PaidDate?.ToString("MMM dd, yyyy") ?? "—").FontSize(8);
                        }

                        if (!invoices.Any())
                        {
                            table.Cell().ColumnSpan(7).Padding(15).AlignCenter()
                                .Text("No invoices found").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        }
                    });
                });

                // Footer
                page.Footer().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span(companyName).FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" — Invoice Report").FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string BuildAddress(string? address, string? city, string? state, string? country, string? postalCode)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(address))
            parts.Add(address);

        var cityState = new List<string>();
        if (!string.IsNullOrWhiteSpace(city))
            cityState.Add(city);
        if (!string.IsNullOrWhiteSpace(state))
            cityState.Add(state);
        if (cityState.Any())
            parts.Add(string.Join(", ", cityState));

        if (!string.IsNullOrWhiteSpace(postalCode))
            parts.Add(postalCode);

        if (!string.IsNullOrWhiteSpace(country))
            parts.Add(country);

        return string.Join(", ", parts);
    }

    private static string GetCurrencySymbol(string currency)
    {
        return currency.ToUpper() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "CAD" => "CA$",
            "AUD" => "A$",
            "PKR" => "Rs ",
            "INR" => "₹",
            "JPY" => "¥",
            _ => "$"
        };
    }
}
