using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace TagHelperPack;

/// <summary>
/// Generates SEO meta tags based on provided properties.
/// </summary>
[HtmlTargetElement("seo", TagStructure = TagStructure.WithoutEndTag)]
public class SeoTagHelper : TagHelper
{
    /// <summary>
    /// The page title.
    /// </summary>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The page description.
    /// </summary>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The canonical URL for the page.
    /// </summary>
    [HtmlAttributeName("canonical")]
    public string? CanonicalUrl { get; set; }

    /// <summary>
    /// The Open Graph title.
    /// </summary>
    [HtmlAttributeName("og-title")]
    public string? OgTitle { get; set; }

    /// <summary>
    /// The Open Graph description.
    /// </summary>
    [HtmlAttributeName("og-description")]
    public string? OgDescription { get; set; }

    /// <summary>
    /// The Open Graph image URL.
    /// </summary>
    [HtmlAttributeName("og-image")]
    public string? OgImage { get; set; }

    /// <summary>
    /// The Open Graph type (e.g., website, article).
    /// </summary>
    [HtmlAttributeName("og-type")]
    public string? OgType { get; set; } = "website";

    /// <summary>
    /// Twitter card type.
    /// </summary>
    [HtmlAttributeName("twitter-card")]
    public string? TwitterCard { get; set; } = "summary_large_image";

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/>.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        var content = new HtmlContentBuilder();

        // Basic SEO tags
        if (!string.IsNullOrEmpty(Title))
        {
            content.AppendHtml($"<title>{HtmlEncoder.Default.Encode(Title)}</title>\n");
            content.AppendHtml($"<meta name=\"title\" content=\"{HtmlEncoder.Default.Encode(Title)}\" />\n");
        }

        if (!string.IsNullOrEmpty(Description))
        {
            content.AppendHtml($"<meta name=\"description\" content=\"{HtmlEncoder.Default.Encode(Description)}\" />\n");
        }

        if (!string.IsNullOrEmpty(CanonicalUrl))
        {
            content.AppendHtml($"<link rel=\"canonical\" href=\"{HtmlEncoder.Default.Encode(CanonicalUrl)}\" />\n");
        }

        // Open Graph tags
        var ogTitleValue = OgTitle ?? Title;
        if (!string.IsNullOrEmpty(ogTitleValue))
        {
            content.AppendHtml($"<meta property=\"og:title\" content=\"{HtmlEncoder.Default.Encode(ogTitleValue)}\" />\n");
        }

        var ogDescriptionValue = OgDescription ?? Description;
        if (!string.IsNullOrEmpty(ogDescriptionValue))
        {
            content.AppendHtml($"<meta property=\"og:description\" content=\"{HtmlEncoder.Default.Encode(ogDescriptionValue)}\" />\n");
        }

        if (!string.IsNullOrEmpty(OgImage))
        {
            content.AppendHtml($"<meta property=\"og:image\" content=\"{HtmlEncoder.Default.Encode(OgImage)}\" />\n");
        }

        if (!string.IsNullOrEmpty(OgType))
        {
            content.AppendHtml($"<meta property=\"og:type\" content=\"{HtmlEncoder.Default.Encode(OgType)}\" />\n");
        }

        // Get current URL for og:url
        var currentUrl = $"{ViewContext.HttpContext.Request.Scheme}://{ViewContext.HttpContext.Request.Host}{ViewContext.HttpContext.Request.PathBase}{ViewContext.HttpContext.Request.Path}";
        content.AppendHtml($"<meta property=\"og:url\" content=\"{HtmlEncoder.Default.Encode(currentUrl)}\" />\n");

        // Twitter Card tags
        if (!string.IsNullOrEmpty(TwitterCard))
        {
            content.AppendHtml($"<meta name=\"twitter:card\" content=\"{HtmlEncoder.Default.Encode(TwitterCard)}\" />\n");
        }

        var twitterTitleValue = OgTitle ?? Title;
        if (!string.IsNullOrEmpty(twitterTitleValue))
        {
            content.AppendHtml($"<meta name=\"twitter:title\" content=\"{HtmlEncoder.Default.Encode(twitterTitleValue)}\" />\n");
        }

        var twitterDescriptionValue = OgDescription ?? Description;
        if (!string.IsNullOrEmpty(twitterDescriptionValue))
        {
            content.AppendHtml($"<meta name=\"twitter:description\" content=\"{HtmlEncoder.Default.Encode(twitterDescriptionValue)}\" />\n");
        }

        if (!string.IsNullOrEmpty(OgImage))
        {
            content.AppendHtml($"<meta name=\"twitter:image\" content=\"{HtmlEncoder.Default.Encode(OgImage)}\" />\n");
        }

        output.Content.SetHtmlContent(content);
        output.TagName = null;
    }
}