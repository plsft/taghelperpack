# TagHelperPack

[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%209.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-6.0%2B-512BD4)](https://asp.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/plsft/taghelperpack)

A comprehensive collection of modern, performance-focused Tag Helpers for ASP.NET Core applications. Built with the latest .NET features and best practices to enhance your Razor pages and views.

## 🚀 Features

- **Multi-Framework Support**: Targets .NET 6.0, 8.0, and 9.0
- **Modern C# 12**: Leverages latest language features and patterns
- **Performance Optimized**: Built for modern web application requirements
- **SEO & Accessibility**: Includes helpers for better search engine optimization
- **Type Safe**: Full nullable reference types support
- **Well Tested**: Comprehensive unit test coverage

## 📦 Installation

### Package Manager Console
```powershell
Install-Package TagHelperPack
```

### .NET CLI
```bash
dotnet add package TagHelperPack
```

### PackageReference
```xml
<PackageReference Include="TagHelperPack" Version="1.0.0" />
```

## ⚙️ Setup

1. **Register Tag Helpers** in `_ViewImports.cshtml`:
```cshtml
@addTagHelper *, TagHelperPack
```

2. **Optional Optimizations** in `Program.cs` (recommended):
```csharp
builder.Services.AddTagHelperPack();
```

## 🏷️ Tag Helper Reference

### Core Conditional Helpers

#### `asp-if` - Conditional Rendering
Conditionally renders elements based on boolean expressions.

```cshtml
<div asp-if="Model.ShowAlert" class="alert alert-info">
    This will only render if ShowAlert is true
</div>

<button asp-if="User.Identity.IsAuthenticated">Logout</button>
```

#### `asp-authz` - Authorization-Based Rendering
Show/hide content based on user authentication and authorization policies.

```cshtml
<!-- Show only to authenticated users -->
<nav asp-authz="true">
    <a href="/dashboard">Dashboard</a>
</nav>

<!-- Show only to anonymous users -->
<div asp-authz="false">
    <a href="/login">Login</a>
</div>

<!-- Show only to users with specific policy -->
<admin-panel asp-authz-policy="AdminPolicy">
    Admin content here
</admin-panel>
```

### Modern Web Helpers

#### `lazy` - Image Lazy Loading
Adds native browser lazy loading with optional placeholder support.

```cshtml
<!-- Basic lazy loading -->
<img src="large-image.jpg" alt="Description" lazy="true" />

<!-- With placeholder -->
<img src="large-image.jpg" 
     alt="Description" 
     lazy="true" 
     lazy-placeholder="placeholder.jpg" />
```

**Generated Output:**
```html
<img src="large-image.jpg" alt="Description" loading="lazy" decoding="async" />
```

#### `seo` - SEO Meta Tags
Generates comprehensive SEO meta tags, Open Graph, and Twitter Card data.

```cshtml
<seo title="Amazing Product - My Store"
     description="Discover our amazing product with incredible features"
     canonical="https://mystore.com/products/amazing-product"
     og-title="Amazing Product"
     og-description="Discover our amazing product"
     og-image="https://mystore.com/images/product.jpg"
     og-type="product"
     twitter-card="summary_large_image" />
```

**Generated Output:**
```html
<title>Amazing Product - My Store</title>
<meta name="title" content="Amazing Product - My Store" />
<meta name="description" content="Discover our amazing product with incredible features" />
<link rel="canonical" href="https://mystore.com/products/amazing-product" />
<meta property="og:title" content="Amazing Product" />
<meta property="og:description" content="Discover our amazing product" />
<meta property="og:image" content="https://mystore.com/images/product.jpg" />
<meta property="og:type" content="product" />
<meta property="og:url" content="https://mystore.com/products/amazing-product" />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:title" content="Amazing Product" />
<meta name="twitter:description" content="Discover our amazing product" />
<meta name="twitter:image" content="https://mystore.com/images/product.jpg" />
```

#### `time-ago` - Relative Time Display
Displays human-friendly relative time with optional absolute time tooltips.

```cshtml
<!-- Using DateTime property -->
<time-ago time-ago="@Model.CreatedAt"></time-ago>

<!-- With custom tooltip format -->
<time-ago time-ago="@Model.UpdatedAt" 
          tooltip-format="F" 
          show-tooltip="true"></time-ago>

<!-- On any element -->
<span time-ago="@comment.PostedAt">Posted</span>
```

**Generated Output:**
```html
<time datetime="2024-01-15T10:30:00Z" title="Monday, January 15, 2024 10:30:00 AM">
    2 hours ago
</time>
```

#### `json-ld` - Structured Data
Generates Schema.org JSON-LD structured data for better SEO.

```cshtml
@{
    var organizationData = new {
        name = "My Company",
        url = "https://mycompany.com",
        logo = "https://mycompany.com/logo.png",
        contactPoint = new {
            contactType = "customer service",
            telephone = "+1-555-123-4567"
        }
    };
}

<json-ld type="Organization" data="organizationData" />
```

**Generated Output:**
```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Organization",
  "name": "My Company",
  "url": "https://mycompany.com",
  "logo": "https://mycompany.com/logo.png",
  "contactPoint": {
    "contactType": "customer service",
    "telephone": "+1-555-123-4567"
  }
}
</script>
```

#### `view-component` - Declarative View Components
Render view components declaratively without complex syntax.

```cshtml
<!-- Simple view component -->
<view-component name="NavigationMenu" />

<!-- With parameters -->
<view-component name="ProductList" params='new { Category = "Electronics", Count = 10 }' />
```

#### `class-if` / `class-unless` - Conditional CSS Classes
Conditionally apply CSS classes based on expressions.

```cshtml
<!-- Add classes when condition is true -->
<div class="card" 
     class-if="@Model.IsHighlighted:highlighted,featured">
    Content
</div>

<!-- Add classes when condition is false -->
<button class="btn btn-primary" 
        class-unless="@Model.IsProcessing:disabled,loading">
    Submit
</button>

<!-- Multiple conditions -->
<nav class="navbar" 
     class-if="@User.Identity.IsAuthenticated:user-nav;@Model.IsMobile:mobile-nav">
    Navigation
</nav>
```

### Content Helpers

#### `markdown` - Markdown Rendering
Renders Markdown content with HTML sanitization support.

```cshtml
<!-- Basic markdown -->
<markdown>
# Hello World
This is **bold** and this is *italic*.
</markdown>

<!-- Allow sanitized HTML -->
<markdown allow-html="true">
# Title
<div class="custom-class">Custom HTML content</div>
</markdown>

<!-- Preserve indentation -->
<markdown preserve-indentation="true">
@Model.MarkdownContent
</markdown>
```

#### `script[asp-inline]` - Script Inlining
Inline JavaScript files for better performance (eliminates HTTP requests).

```cshtml
<!-- Inline a script file -->
<script src="~/js/critical.js" asp-inline="true"></script>
```

**Generated Output:**
```html
<script>
// Contents of critical.js inlined here
console.log('Critical JavaScript loaded');
</script>
```

### Legacy Helpers (Maintained for Compatibility)

The following helpers are included for backward compatibility:

- `datalist` - Generate HTML5 datalist elements
- `description-for` - Display model property descriptions
- `display-for` - Enhanced display templates
- `display-name-for` - Display model property display names
- `editor-for` - Enhanced editor templates
- `enabled` - Enable/disable form elements conditionally
- `label-title` - Enhanced label elements with tooltips
- `render-partial` - Advanced partial view rendering

## 🎯 Best Practices

### Performance Optimization

1. **Use `AddTagHelperPack()`** in your service registration for optimal performance
2. **Lazy load images** with the `lazy` helper for better page load times
3. **Inline critical CSS/JS** using the script inlining helper
4. **Generate structured data** with `json-ld` for better SEO

### SEO Enhancement

```cshtml
@{
    ViewData["Title"] = "Product Details";
    var product = Model;
}

<!-- SEO Meta Tags -->
<seo title="@($"{product.Name} - {ViewData["Title"]}")"
     description="@product.Description"
     canonical="@Url.Action("Details", "Products", new { id = product.Id }, Request.Scheme)"
     og-title="@product.Name"
     og-description="@product.Description"
     og-image="@Url.Action("Image", "Products", new { id = product.Id }, Request.Scheme)"
     og-type="product" />

<!-- Structured Data -->
<json-ld type="Product" data="@(new {
    name = product.Name,
    description = product.Description,
    image = Url.Action("Image", "Products", new { id = product.Id }, Request.Scheme),
    offers = new {
        type = "Offer",
        price = product.Price,
        priceCurrency = "USD"
    }
})" />
```

### Authentication & Authorization

```cshtml
<!-- Show different content based on auth status -->
<div asp-authz="false" class="guest-welcome">
    <h2>Welcome, Guest!</h2>
    <a href="/register" class="btn btn-primary">Sign Up</a>
</div>

<nav asp-authz="true" class="user-navigation">
    <view-component name="UserMenu" />
    <a href="/logout">Logout</a>
</nav>

<!-- Admin-only sections -->
<admin-section asp-authz-policy="AdminPolicy">
    <view-component name="AdminPanel" />
</admin-section>
```

## 🔧 Configuration

### Service Registration Options

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Basic registration (recommended)
builder.Services.AddTagHelperPack();

// If you need custom configuration
builder.Services.Configure<TagHelperPackOptions>(options =>
{
    options.EnableCaching = true;
    options.CacheTimeout = TimeSpan.FromMinutes(30);
});
```

## 🌟 Examples

### E-commerce Product Page

```cshtml
@model ProductViewModel

<!-- SEO and Social Media -->
<seo title="@Model.Product.Name - Best Electronics Store"
     description="@Model.Product.Description"
     og-image="@Model.Product.MainImageUrl"
     og-type="product" />

<json-ld type="Product" data="@Model.StructuredData" />

<div class="product-container">
    <!-- Product Images with Lazy Loading -->
    <div class="product-images">
        <img src="@Model.Product.MainImageUrl" 
             alt="@Model.Product.Name"
             class="main-image" 
             lazy="true" 
             lazy-placeholder="~/images/placeholder.jpg" />
        
        @foreach(var image in Model.Product.Images)
        {
            <img src="@image.Url" 
                 alt="@image.Alt"
                 class="thumbnail"
                 lazy="true" />
        }
    </div>
    
    <!-- Product Info -->
    <div class="product-info">
        <h1 class="product-title" 
            class-if="@Model.Product.IsFeatured:featured-product">
            @Model.Product.Name
        </h1>
        
        <!-- Show different content for different user types -->
        <div asp-authz-policy="PremiumMember" class="premium-pricing">
            <span class="premium-price">$@Model.Product.PremiumPrice</span>
            <span class="savings">Save $@(Model.Product.Price - Model.Product.PremiumPrice)!</span>
        </div>
        
        <div asp-authz-policy="!PremiumMember" class="regular-pricing">
            <span class="regular-price">$@Model.Product.Price</span>
        </div>
        
        <!-- Product Description with Markdown -->
        <div class="description">
            <markdown>@Model.Product.MarkdownDescription</markdown>
        </div>
        
        <!-- Purchase Button -->
        <button class="btn btn-primary btn-lg" 
                class-unless="@Model.Product.InStock:btn-disabled,out-of-stock"
                asp-if="@Model.Product.InStock">
            Add to Cart - $@Model.Product.Price
        </button>
        
        <div asp-if="@(!Model.Product.InStock)" class="out-of-stock-message">
            <p>Currently out of stock</p>
            <button class="btn btn-secondary">Notify When Available</button>
        </div>
    </div>
</div>

<!-- Reviews Section -->
<div class="reviews-section">
    <h3>Customer Reviews</h3>
    @foreach(var review in Model.Reviews)
    {
        <div class="review">
            <div class="review-meta">
                <strong>@review.CustomerName</strong>
                <time-ago time-ago="@review.CreatedAt" 
                          show-tooltip="true">
                </time-ago>
            </div>
            <div class="review-content">
                <markdown>@review.Content</markdown>
            </div>
        </div>
    }
</div>

<!-- Related Products Component -->
<view-component name="RelatedProducts" 
                params='new { CategoryId = Model.Product.CategoryId, Exclude = Model.Product.Id }' />
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with ❤️ for the ASP.NET Core community. Special thanks to all contributors and the original TagHelperPack project that inspired this modern version.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/plsft/taghelperpack/issues)
- **Discussions**: [GitHub Discussions](https://github.com/plsft/taghelperpack/discussions)
- **Documentation**: [Wiki](https://github.com/plsft/taghelperpack/wiki)

---

**Made with 🚀 by the community, for the community**