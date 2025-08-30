using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace TagHelperPack;

/// <summary>
/// Creates custom web components with Shadow DOM support and modern JavaScript integration.
/// </summary>
[HtmlTargetElement("web-component", Attributes = "name")]
public class WebComponentTagHelper : TagHelper
{
    /// <summary>
    /// The name of the custom web component.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Whether to use Shadow DOM for style encapsulation.
    /// </summary>
    [HtmlAttributeName("shadow-dom")]
    public bool UseShadowDom { get; set; } = true;

    /// <summary>
    /// CSS styles to inject into the component.
    /// </summary>
    [HtmlAttributeName("styles")]
    public string? Styles { get; set; }

    /// <summary>
    /// JavaScript code for component behavior.
    /// </summary>
    [HtmlAttributeName("script")]
    public string? Script { get; set; }

    /// <summary>
    /// Component properties as JSON.
    /// </summary>
    [HtmlAttributeName("props")]
    public object? Props { get; set; }

    /// <summary>
    /// Whether to register the component automatically.
    /// </summary>
    [HtmlAttributeName("auto-register")]
    public bool AutoRegister { get; set; } = true;

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        var childContent = await output.GetChildContentAsync();
        var template = childContent.GetContent();

        var content = new HtmlContentBuilder();

        // Generate the custom element
        output.TagName = Name;

        // Add properties as attributes if provided
        if (Props != null)
        {
            var properties = Props.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(Props);
                if (value != null)
                {
                    var attrName = prop.Name.ToKebabCase();
                    output.Attributes.SetAttribute(attrName, value.ToString());
                }
            }
        }

        // Set the template content
        output.Content.SetContent(template);

        // Generate the web component definition script
        if (AutoRegister)
        {
            var scriptContent = GenerateWebComponentScript(Name, template, Styles, Script, UseShadowDom);
            content.AppendHtml($"<script>{scriptContent}</script>");
            output.PostContent.SetHtmlContent(content);
        }
    }

    private static string GenerateWebComponentScript(string name, string template, string? styles, string? script, bool useShadowDom)
    {
        var className = name.ToPascalCase() + "Element";
        var encoder = JavaScriptEncoder.Default;

        return $@"
(function() {{
    if (customElements.get('{name}')) return;
    
    class {className} extends HTMLElement {{
        constructor() {{
            super();
            {(useShadowDom ? "this.attachShadow({ mode: 'open' });" : "")}
        }}
        
        connectedCallback() {{
            this.render();
            {(string.IsNullOrEmpty(script) ? "" : script)}
        }}
        
        render() {{
            const template = `{encoder.Encode(template)}`;
            {(styles != null ? $"const styles = `<style>{encoder.Encode(styles)}</style>`;" : "const styles = '';")}
            
            {(useShadowDom
                ? "this.shadowRoot.innerHTML = styles + template;"
                : "this.innerHTML = template;")}
        }}
        
        static get observedAttributes() {{
            return []; // Add attributes to observe
        }}
        
        attributeChangedCallback(name, oldValue, newValue) {{
            this.render();
        }}
    }}
    
    customElements.define('{name}', {className});
}})();";
    }
}

/// <summary>
/// Extension methods for string manipulation in web components.
/// </summary>
public static class StringExtensions
{
    public static string ToKebabCase(this string input)
    {
        return string.Concat(input.Select((c, i) => 
            char.IsUpper(c) && i > 0 ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
    }

    public static string ToPascalCase(this string input)
    {
        return string.Concat(input.Split('-').Select(s => 
            char.ToUpper(s[0]) + s[1..]));
    }
}