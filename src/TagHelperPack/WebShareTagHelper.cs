using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace TagHelperPack;

/// <summary>
/// Implements the Web Share API for native sharing on supported devices.
/// </summary>
[HtmlTargetElement("share-button")]
[HtmlTargetElement("*", Attributes = "web-share")]
public class WebShareTagHelper : TagHelper
{
    /// <summary>
    /// Whether to enable web sharing.
    /// </summary>
    [HtmlAttributeName("web-share")]
    public bool EnableWebShare { get; set; }

    /// <summary>
    /// The title to share.
    /// </summary>
    [HtmlAttributeName("share-title")]
    public string? Title { get; set; }

    /// <summary>
    /// The text to share.
    /// </summary>
    [HtmlAttributeName("share-text")]
    public string? Text { get; set; }

    /// <summary>
    /// The URL to share.
    /// </summary>
    [HtmlAttributeName("share-url")]
    public string? Url { get; set; }

    /// <summary>
    /// Files to share (for supported content types).
    /// </summary>
    [HtmlAttributeName("share-files")]
    public string[]? Files { get; set; }

    /// <summary>
    /// Fallback sharing method when Web Share API is not supported.
    /// Options: "copy", "email", "twitter", "facebook", "linkedin", "none"
    /// </summary>
    [HtmlAttributeName("fallback")]
    public string Fallback { get; set; } = "copy";

    /// <summary>
    /// Success callback function name.
    /// </summary>
    [HtmlAttributeName("on-success")]
    public string? OnSuccess { get; set; }

    /// <summary>
    /// Error callback function name.
    /// </summary>
    [HtmlAttributeName("on-error")]
    public string? OnError { get; set; }

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

        // Get child content
        var childContent = await output.GetChildContentAsync();

        // If this is a share-button element, convert to button
        if (output.TagName == "share-button")
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "button");
        }

        // Set default URL if not provided
        if (string.IsNullOrEmpty(Url))
        {
            var request = ViewContext.HttpContext.Request;
            Url = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
        }

        // Create share data
        var shareData = new
        {
            title = Title,
            text = Text,
            url = Url,
            files = Files
        };

        var shareDataJson = JsonSerializer.Serialize(shareData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        // Add data attributes
        output.Attributes.SetAttribute("data-web-share", "true");
        output.Attributes.SetAttribute("data-share-data", shareDataJson);
        output.Attributes.SetAttribute("data-fallback", Fallback);

        if (!string.IsNullOrEmpty(OnSuccess))
        {
            output.Attributes.SetAttribute("data-on-success", OnSuccess);
        }

        if (!string.IsNullOrEmpty(OnError))
        {
            output.Attributes.SetAttribute("data-on-error", OnError);
        }

        // Set content if empty
        if (childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.SetContent("Share");
        }

        // Add the web share script
        output.PostContent.AppendHtml($"<script>{GenerateWebShareScript()}</script>");

        // Remove helper attributes
        output.Attributes.RemoveAll("web-share");
        output.Attributes.RemoveAll("share-title");
        output.Attributes.RemoveAll("share-text");
        output.Attributes.RemoveAll("share-url");
        output.Attributes.RemoveAll("share-files");
        output.Attributes.RemoveAll("fallback");
        output.Attributes.RemoveAll("on-success");
        output.Attributes.RemoveAll("on-error");
    }

    private static string GenerateWebShareScript()
    {
        return @"
(function() {
    function handleWebShare(element) {
        const shareData = JSON.parse(element.dataset.shareData);
        const fallback = element.dataset.fallback;
        const onSuccess = element.dataset.onSuccess;
        const onError = element.dataset.onError;
        
        async function share() {
            if (navigator.share && navigator.canShare && navigator.canShare(shareData)) {
                try {
                    await navigator.share(shareData);
                    
                    if (onSuccess && typeof window[onSuccess] === 'function') {
                        window[onSuccess](shareData);
                    }
                    
                    element.dispatchEvent(new CustomEvent('web-share-success', {
                        detail: shareData
                    }));
                } catch (error) {
                    console.error('Error sharing:', error);
                    
                    if (onError && typeof window[onError] === 'function') {
                        window[onError](error, shareData);
                    }
                    
                    element.dispatchEvent(new CustomEvent('web-share-error', {
                        detail: { error, shareData }
                    }));
                    
                    if (error.name !== 'AbortError') {
                        handleFallback(shareData, fallback);
                    }
                }
            } else {
                handleFallback(shareData, fallback);
            }
        }
        
        function handleFallback(data, method) {
            const url = encodeURIComponent(data.url || '');
            const text = encodeURIComponent(data.text || '');
            const title = encodeURIComponent(data.title || '');
            
            switch (method) {
                case 'copy':
                    copyToClipboard(`${data.title || ''}\n${data.text || ''}\n${data.url || ''}`);
                    break;
                    
                case 'email':
                    window.open(`mailto:?subject=${title}&body=${text}%0A${url}`);
                    break;
                    
                case 'twitter':
                    window.open(`https://twitter.com/intent/tweet?text=${text}&url=${url}`, '_blank');
                    break;
                    
                case 'facebook':
                    window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}`, '_blank');
                    break;
                    
                case 'linkedin':
                    window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, '_blank');
                    break;
                    
                case 'none':
                    // Do nothing
                    break;
                    
                default:
                    copyToClipboard(`${data.title || ''}\n${data.text || ''}\n${data.url || ''}`);
            }
        }
        
        async function copyToClipboard(text) {
            try {
                await navigator.clipboard.writeText(text);
                
                // Show a temporary success message
                const originalText = element.textContent;
                element.textContent = 'Copied!';
                setTimeout(() => {
                    element.textContent = originalText;
                }, 2000);
                
            } catch (error) {
                console.error('Failed to copy to clipboard:', error);
                
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = text;
                document.body.appendChild(textArea);
                textArea.select();
                
                try {
                    document.execCommand('copy');
                    element.textContent = 'Copied!';
                    setTimeout(() => {
                        element.textContent = originalText;
                    }, 2000);
                } catch (fallbackError) {
                    console.error('Fallback copy failed:', fallbackError);
                } finally {
                    document.body.removeChild(textArea);
                }
            }
        }
        
        element.addEventListener('click', share);
    }
    
    // Initialize all web share elements
    function initWebShare() {
        document.querySelectorAll('[data-web-share]').forEach(handleWebShare);
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWebShare);
    } else {
        initWebShare();
    }
})();";
    }
}