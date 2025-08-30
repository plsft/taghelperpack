using System.Text.Encodings.Web;
using System.Text.Json;

namespace TagHelperPack;

/// <summary>
/// Generates JSON-LD structured data for better SEO.
/// </summary>
[HtmlTargetElement("json-ld")]
public class JsonLdTagHelper : TagHelper
{
    /// <summary>
    /// The type of schema.org structured data (e.g., "Organization", "Person", "Article").
    /// </summary>
    [HtmlAttributeName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The object to serialize as JSON-LD. Should be an anonymous object or POCO.
    /// </summary>
    [HtmlAttributeName("data")]
    public object? Data { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (Data == null)
        {
            output.SuppressOutput();
            return;
        }

        // Create the JSON-LD structure
        var jsonLdObject = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org"
        };

        if (!string.IsNullOrEmpty(Type))
        {
            jsonLdObject["@type"] = Type;
        }

        // Merge the provided data
        if (Data is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                jsonLdObject[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            // Use reflection to get properties from the object
            var properties = Data.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(Data);
                if (value != null)
                {
                    jsonLdObject[prop.Name] = value;
                }
            }
        }

        // Serialize to JSON
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(jsonLdObject, options);

        // Output as script tag
        output.TagName = "script";
        output.Attributes.SetAttribute("type", "application/ld+json");
        output.Content.SetContent(json);
    }
}