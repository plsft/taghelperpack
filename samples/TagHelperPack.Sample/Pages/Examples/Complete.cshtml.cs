using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagHelperPack.Sample.Pages.Examples;

public class CompleteModel : PageModel
{
    public ProductViewModel Product { get; set; } = default!;
    public List<ProductViewModel> RelatedProducts { get; set; } = default!;
    public bool IsFeatured { get; set; } = true;

    public void OnGet()
    {
        // Initialize demo data
        Product = new ProductViewModel
        {
            Name = "Premium Wireless Headphones",
            ShortDescription = "Experience crystal-clear audio with advanced noise cancellation technology.",
            Price = 299.99m,
            OriginalPrice = 399.99m,
            HasDiscount = true,
            DiscountPercent = 25,
            InStock = true,
            Rating = 4.8m,
            ReviewCount = 152,
            MainImageUrl = "https://picsum.photos/600/400?random=101",
            MarkdownDescription = @"
# Premium Audio Experience

Our **Premium Wireless Headphones** deliver exceptional sound quality with:

- **Active Noise Cancellation** - Block out distractions
- **40-hour Battery Life** - All-day listening
- **Quick Charge** - 5 minutes = 2 hours playback
- **Premium Materials** - Comfortable for extended wear

## Technical Specifications

The headphones feature advanced 40mm drivers with a frequency response of 20Hz-20kHz, ensuring you hear every detail in your music.

### Connectivity
- Bluetooth 5.2 with multipoint connection
- USB-C charging
- 3.5mm wired backup

*Perfect for music lovers, professionals, and travelers.*",
            Images = new List<ImageViewModel>
            {
                new() { Url = "https://picsum.photos/200/150?random=102", Alt = "Side view" },
                new() { Url = "https://picsum.photos/200/150?random=103", Alt = "Detail view" },
                new() { Url = "https://picsum.photos/200/150?random=104", Alt = "Packaging" },
                new() { Url = "https://picsum.photos/200/150?random=105", Alt = "Accessories" }
            },
            Specifications = new Dictionary<string, string>
            {
                { "Driver Size", "40mm Dynamic" },
                { "Frequency Response", "20Hz - 20kHz" },
                { "Battery Life", "40 hours" },
                { "Charging Time", "2 hours" },
                { "Weight", "250g" },
                { "Connectivity", "Bluetooth 5.2" },
                { "Noise Cancellation", "Active ANC" },
                { "Warranty", "2 years" }
            },
            Reviews = new List<ReviewViewModel>
            {
                new() 
                { 
                    CustomerName = "John D.", 
                    Rating = 5, 
                    Content = "**Absolutely amazing!** The sound quality is incredible and the noise cancellation works perfectly.",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new() 
                { 
                    CustomerName = "Sarah M.", 
                    Rating = 4, 
                    Content = "Great headphones overall. Battery life is as advertised. Only minor issue is they're a bit heavy for long sessions.",
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new() 
                { 
                    CustomerName = "Mike R.", 
                    Rating = 5, 
                    Content = "Perfect for my daily commute. The ANC blocks out all the subway noise. *Highly recommended!*",
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new() 
                { 
                    CustomerName = "Lisa K.", 
                    Rating = 4, 
                    Content = "Excellent build quality and the quick charge feature is a game changer.",
                    CreatedAt = DateTime.Now.AddHours(-12)
                },
                new() 
                { 
                    CustomerName = "David W.", 
                    Rating = 5, 
                    Content = "Best headphones I've ever owned. The multipoint connection lets me switch between devices seamlessly.",
                    CreatedAt = DateTime.Now.AddHours(-6)
                }
            }
        };

        RelatedProducts = new List<ProductViewModel>
        {
            new() { Name = "Wireless Earbuds", Price = 149.99m, ImageUrl = "https://picsum.photos/200/200?random=106" },
            new() { Name = "Portable Speaker", Price = 79.99m, ImageUrl = "https://picsum.photos/200/200?random=107" },
            new() { Name = "Audio Cable", Price = 19.99m, ImageUrl = "https://picsum.photos/200/200?random=108" },
            new() { Name = "Carrying Case", Price = 29.99m, ImageUrl = "https://picsum.photos/200/200?random=109" }
        };
    }
}

public class ProductViewModel
{
    public string Name { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public bool HasDiscount { get; set; }
    public int DiscountPercent { get; set; }
    public bool InStock { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string MainImageUrl { get; set; } = default!;
    public string MarkdownDescription { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public List<ImageViewModel> Images { get; set; } = new();
    public Dictionary<string, string> Specifications { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
}

public class ImageViewModel
{
    public string Url { get; set; } = default!;
    public string Alt { get; set; } = default!;
}

public class ReviewViewModel
{
    public string CustomerName { get; set; } = default!;
    public int Rating { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}