using Microsoft.AspNetCore.Mvc;

namespace TagHelperPack;

/// <summary>
/// Renders a view component with optional parameters.
/// </summary>
[HtmlTargetElement("view-component", Attributes = "name")]
public class ViewComponentTagHelper : TagHelper
{
    /// <summary>
    /// The name of the view component to render.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional parameters to pass to the view component as JSON.
    /// </summary>
    [HtmlAttributeName("params")]
    public object? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/>.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            return;
        }

        var viewComponentHelper = ViewContext.HttpContext.RequestServices
            .GetService(typeof(IViewComponentHelper)) as IViewComponentHelper;
        
        if (viewComponentHelper is IViewContextAware contextAware)
        {
            contextAware.Contextualize(ViewContext);
            
            var result = Parameters != null 
                ? await viewComponentHelper.InvokeAsync(Name, Parameters)
                : await viewComponentHelper.InvokeAsync(Name);

            output.Content.SetHtmlContent(result);
        }

        output.TagName = null;
    }
}