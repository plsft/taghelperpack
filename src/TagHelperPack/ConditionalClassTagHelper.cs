namespace TagHelperPack;

/// <summary>
/// Conditionally adds CSS classes based on boolean expressions.
/// </summary>
[HtmlTargetElement("*", Attributes = "class-if")]
[HtmlTargetElement("*", Attributes = "class-unless")]
public class ConditionalClassTagHelper : TagHelper
{
    /// <summary>
    /// CSS classes to add if the condition is true, in the format "condition:class1,class2".
    /// Multiple conditions can be separated by semicolons.
    /// </summary>
    [HtmlAttributeName("class-if")]
    public string? ClassIf { get; set; }

    /// <summary>
    /// CSS classes to add if the condition is false, in the format "condition:class1,class2".
    /// Multiple conditions can be separated by semicolons.
    /// </summary>
    [HtmlAttributeName("class-unless")]
    public string? ClassUnless { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        var classesToAdd = new List<string>();

        // Process class-if conditions
        if (!string.IsNullOrEmpty(ClassIf))
        {
            classesToAdd.AddRange(ProcessConditions(ClassIf, true));
        }

        // Process class-unless conditions  
        if (!string.IsNullOrEmpty(ClassUnless))
        {
            classesToAdd.AddRange(ProcessConditions(ClassUnless, false));
        }

        // Add the classes to the output
        if (classesToAdd.Count > 0)
        {
            var existingClass = output.Attributes["class"]?.Value?.ToString() ?? "";
            var newClasses = string.Join(" ", classesToAdd);
            var combinedClasses = string.IsNullOrEmpty(existingClass) 
                ? newClasses 
                : $"{existingClass} {newClasses}";
            
            output.Attributes.SetAttribute("class", combinedClasses);
        }

        // Remove the conditional attributes
        output.Attributes.RemoveAll("class-if");
        output.Attributes.RemoveAll("class-unless");
    }

    private List<string> ProcessConditions(string conditions, bool shouldMatch)
    {
        var result = new List<string>();
        
        // For simplicity, this assumes boolean properties are passed directly
        // In a real implementation, you might want to support more complex expressions
        var parts = conditions.Split(';', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex > 0)
            {
                var conditionStr = part[..colonIndex].Trim();
                var classesStr = part[(colonIndex + 1)..].Trim();
                
                // Simple boolean parsing - in practice you might want more sophisticated parsing
                if (bool.TryParse(conditionStr, out var condition))
                {
                    if (condition == shouldMatch)
                    {
                        var classes = classesStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim());
                        result.AddRange(classes);
                    }
                }
            }
        }

        return result;
    }
}