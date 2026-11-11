using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

internal static class InvoicePdf
{
    public static byte[] Create(Order order)
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;
        var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
        var headingFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
        var smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
        var pageSize = page.GetClientSize();
        var y = 24f;

        graphics.DrawString("TURBO TRACK MODELS - INVOICE", titleFont,
            PdfBrushes.Black, new PointF(0, y));
        y += 34;
        graphics.DrawString($"Order ID: {order.OrderId}", bodyFont,
            PdfBrushes.Black, new PointF(0, y));
        DrawRight($"Date: {order.OrderDate:dd MMMM yyyy}", bodyFont, pageSize.Width - 12, y);
        y += 24;

        var customer = order.Customer;
        graphics.DrawString("Bill To", headingFont, PdfBrushes.Black, new PointF(0, y));
        y += 16;
        DrawLine($"{customer.User.FirstName} {customer.User.LastName}");
        DrawLine(customer.User.Email ?? string.Empty);
        DrawLine(customer.Street);
        DrawLine($"{customer.Town}, {customer.PostCode}");
        y += 14;

        graphics.DrawRectangle(PdfBrushes.LightGray, 0, y, pageSize.Width, 20);
        graphics.DrawString("Product", headingFont, PdfBrushes.Black, new PointF(5, y + 3));
        var quantityRight = pageSize.Width * 0.67f;
        var priceRight = pageSize.Width * 0.83f;
        var totalRight = pageSize.Width - 12;
        DrawRight("Qty", headingFont, quantityRight, y + 3);
        DrawRight("Price", headingFont, priceRight, y + 3);
        DrawRight("Total", headingFont, totalRight, y + 3);
        y += 28;

        foreach (var item in order.OrderItems)
        {
            if (y > pageSize.Height - 70)
            {
                page = document.Pages.Add();
                graphics = page.Graphics;
                pageSize = page.GetClientSize();
                y = 24;
            }

            var name = item.Product.ProductName;
            if (name.Length > 48)
            {
                name = $"{name[..45]}...";
            }

            graphics.DrawString(name, bodyFont, PdfBrushes.Black, new PointF(5, y));
            DrawRight(item.Quantity.ToString(), bodyFont, quantityRight, y);
            DrawRight($"£{item.UnitPrice:F2}", bodyFont, priceRight, y);
            DrawRight($"£{item.Quantity * item.UnitPrice:F2}", bodyFont, totalRight, y);
            y += 18;
        }

        if (order.ShippingCost != 0)
        {
            if (y > pageSize.Height - 70)
            {
                page = document.Pages.Add();
                graphics = page.Graphics;
                pageSize = page.GetClientSize();
                y = 24;
            }

            graphics.DrawString("Shipping", bodyFont, PdfBrushes.Black, new PointF(5, y));
            DrawRight($"£{order.ShippingCost:F2}", bodyFont, totalRight, y);
            y += 18;
        }

        if (y > pageSize.Height - 80)
        {
            page = document.Pages.Add();
            graphics = page.Graphics;
            pageSize = page.GetClientSize();
            y = 24;
        }

        y += 10;
        graphics.DrawLine(PdfPens.Black, new PointF(pageSize.Width * 0.58f, y),
            new PointF(pageSize.Width - 12, y));
        y += 18;
        DrawRight($"Total: £{order.TotalCost:F2}", headingFont, pageSize.Width - 12, y);
        y += 42;
        graphics.DrawString("Thank you for your business!", bodyFont,
            PdfBrushes.Black, new PointF(0, y));
        graphics.DrawString($"Generated {DateTime.Now:dd MMMM yyyy HH:mm}",
            smallFont, PdfBrushes.Gray, new PointF(0, y + 16));

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();

        void DrawLine(string text)
        {
            graphics.DrawString(text, bodyFont, PdfBrushes.Black, new PointF(0, y));
            y += 14;
        }

        void DrawRight(string text, PdfFont font, float rightEdge, float top)
        {
            var textWidth = font.MeasureString(text).Width;
            graphics.DrawString(text, font, PdfBrushes.Black,
                new PointF(Math.Max(0, rightEdge - textWidth), top));
        }
    }
}
