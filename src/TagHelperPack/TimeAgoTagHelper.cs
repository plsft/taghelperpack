using System.Globalization;
using Microsoft.AspNetCore.Html;

namespace TagHelperPack;

/// <summary>
/// Displays a relative time (e.g., "2 hours ago") with an optional tooltip showing the absolute time.
/// </summary>
[HtmlTargetElement("time-ago")]
[HtmlTargetElement("*", Attributes = "time-ago")]
public class TimeAgoTagHelper : TagHelper
{
    /// <summary>
    /// The DateTime value to display as relative time.
    /// </summary>
    [HtmlAttributeName("time-ago")]
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Whether to include a tooltip with the absolute time.
    /// </summary>
    [HtmlAttributeName("show-tooltip")]
    public bool ShowTooltip { get; set; } = true;

    /// <summary>
    /// Custom format for the tooltip. Defaults to "F" (full date/time pattern).
    /// </summary>
    [HtmlAttributeName("tooltip-format")]
    public string TooltipFormat { get; set; } = "F";

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (!DateTime.HasValue)
        {
            return;
        }

        var timeSpan = System.DateTime.UtcNow - DateTime.Value.ToUniversalTime();
        var relativeTime = GetRelativeTime(timeSpan);

        // If this is a dedicated time-ago element, convert to time element
        if (output.TagName == "time-ago")
        {
            output.TagName = "time";
        }

        // Add datetime attribute for semantic HTML
        output.Attributes.SetAttribute("datetime", DateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        // Add title tooltip if enabled
        if (ShowTooltip)
        {
            var tooltipText = DateTime.Value.ToString(TooltipFormat, CultureInfo.CurrentCulture);
            output.Attributes.SetAttribute("title", tooltipText);
        }

        // Set the content to the relative time
        output.Content.SetContent(relativeTime);

        // Remove the time-ago attribute from output
        output.Attributes.RemoveAll("time-ago");
        output.Attributes.RemoveAll("show-tooltip");
        output.Attributes.RemoveAll("tooltip-format");
    }

    private static string GetRelativeTime(TimeSpan timeSpan)
    {
        var totalMinutes = Math.Abs(timeSpan.TotalMinutes);
        var future = timeSpan.TotalSeconds < 0;

        return totalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)totalMinutes} minute{((int)totalMinutes != 1 ? "s" : "")} {(future ? "from now" : "ago")}",
            < 1440 => $"{(int)(totalMinutes / 60)} hour{((int)(totalMinutes / 60) != 1 ? "s" : "")} {(future ? "from now" : "ago")}",
            < 10080 => $"{(int)(totalMinutes / 1440)} day{((int)(totalMinutes / 1440) != 1 ? "s" : "")} {(future ? "from now" : "ago")}",
            < 43200 => $"{(int)(totalMinutes / 10080)} week{((int)(totalMinutes / 10080) != 1 ? "s" : "")} {(future ? "from now" : "ago")}",
            < 525600 => $"{(int)(totalMinutes / 43200)} month{((int)(totalMinutes / 43200) != 1 ? "s" : "")} {(future ? "from now" : "ago")}",
            _ => $"{(int)(totalMinutes / 525600)} year{((int)(totalMinutes / 525600) != 1 ? "s" : "")} {(future ? "from now" : "ago")}"
        };
    }
}