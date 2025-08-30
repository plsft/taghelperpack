namespace TagHelperPack;

/// <summary>
/// Adds lazy loading attributes to img elements for better performance.
/// </summary>
[HtmlTargetElement("img", Attributes = "lazy")]
public class LazyLoadingTagHelper : TagHelper
{
    /// <summary>
    /// When true, adds native browser lazy loading support.
    /// </summary>
    [HtmlAttributeName("lazy")]
    public bool LazyLoad { get; set; }

    /// <summary>
    /// Optional placeholder image to show while loading.
    /// </summary>
    [HtmlAttributeName("lazy-placeholder")]
    public string? PlaceholderSrc { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (!LazyLoad)
        {
            return;
        }

        // Add native lazy loading
        output.Attributes.SetAttribute("loading", "lazy");

        // Add decoding hint for better performance
        if (!output.Attributes.ContainsName("decoding"))
        {
            output.Attributes.SetAttribute("decoding", "async");
        }

        // Add placeholder if provided
        if (!string.IsNullOrEmpty(PlaceholderSrc))
        {
            var originalSrc = output.Attributes["src"]?.Value?.ToString();
            if (!string.IsNullOrEmpty(originalSrc))
            {
                output.Attributes.SetAttribute("data-src", originalSrc);
                output.Attributes.SetAttribute("src", PlaceholderSrc);
            }
        }

        // Remove the lazy attribute from output
        output.Attributes.RemoveAll("lazy");
        output.Attributes.RemoveAll("lazy-placeholder");
    }
}