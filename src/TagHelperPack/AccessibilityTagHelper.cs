using Microsoft.AspNetCore.Html;

namespace TagHelperPack;

/// <summary>
/// Enhances elements with comprehensive accessibility features and ARIA attributes.
/// </summary>
[HtmlTargetElement("*", Attributes = "a11y-role")]
[HtmlTargetElement("*", Attributes = "a11y-label")]
[HtmlTargetElement("*", Attributes = "a11y-live")]
public class AccessibilityTagHelper : TagHelper
{
    /// <summary>
    /// ARIA role for the element.
    /// </summary>
    [HtmlAttributeName("a11y-role")]
    public string? Role { get; set; }

    /// <summary>
    /// Accessible label for screen readers.
    /// </summary>
    [HtmlAttributeName("a11y-label")]
    public string? Label { get; set; }

    /// <summary>
    /// Accessible description for the element.
    /// </summary>
    [HtmlAttributeName("a11y-description")]
    public string? Description { get; set; }

    /// <summary>
    /// Live region politeness setting (off, polite, assertive).
    /// </summary>
    [HtmlAttributeName("a11y-live")]
    public string? Live { get; set; }

    /// <summary>
    /// Whether the element is expanded (for collapsible content).
    /// </summary>
    [HtmlAttributeName("a11y-expanded")]
    public bool? Expanded { get; set; }

    /// <summary>
    /// Whether the element has a popup.
    /// </summary>
    [HtmlAttributeName("a11y-haspopup")]
    public string? HasPopup { get; set; }

    /// <summary>
    /// ID of element(s) that control this element.
    /// </summary>
    [HtmlAttributeName("a11y-controls")]
    public string? Controls { get; set; }

    /// <summary>
    /// ID of element(s) that describe this element.
    /// </summary>
    [HtmlAttributeName("a11y-describedby")]
    public string? DescribedBy { get; set; }

    /// <summary>
    /// ID of element(s) that label this element.
    /// </summary>
    [HtmlAttributeName("a11y-labelledby")]
    public string? LabelledBy { get; set; }

    /// <summary>
    /// Whether the element is hidden from screen readers.
    /// </summary>
    [HtmlAttributeName("a11y-hidden")]
    public bool? Hidden { get; set; }

    /// <summary>
    /// Current value for range widgets.
    /// </summary>
    [HtmlAttributeName("a11y-valuenow")]
    public double? ValueNow { get; set; }

    /// <summary>
    /// Minimum value for range widgets.
    /// </summary>
    [HtmlAttributeName("a11y-valuemin")]
    public double? ValueMin { get; set; }

    /// <summary>
    /// Maximum value for range widgets.
    /// </summary>
    [HtmlAttributeName("a11y-valuemax")]
    public double? ValueMax { get; set; }

    /// <summary>
    /// Text description of current value.
    /// </summary>
    [HtmlAttributeName("a11y-valuetext")]
    public string? ValueText { get; set; }

    /// <summary>
    /// Level in hierarchical structure (1-6).
    /// </summary>
    [HtmlAttributeName("a11y-level")]
    public int? Level { get; set; }

    /// <summary>
    /// Position in set.
    /// </summary>
    [HtmlAttributeName("a11y-posinset")]
    public int? PosInSet { get; set; }

    /// <summary>
    /// Size of the set.
    /// </summary>
    [HtmlAttributeName("a11y-setsize")]
    public int? SetSize { get; set; }

    /// <summary>
    /// Whether to add skip links for keyboard navigation.
    /// </summary>
    [HtmlAttributeName("a11y-skip-link")]
    public string? SkipLink { get; set; }

    /// <summary>
    /// Focus management mode (auto, manual, none).
    /// </summary>
    [HtmlAttributeName("a11y-focus")]
    public string? FocusMode { get; set; } = "auto";

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        // Add ARIA attributes
        if (!string.IsNullOrEmpty(Role))
        {
            output.Attributes.SetAttribute("role", Role);
        }

        if (!string.IsNullOrEmpty(Label))
        {
            output.Attributes.SetAttribute("aria-label", Label);
        }

        if (!string.IsNullOrEmpty(Description))
        {
            var descId = $"desc-{Guid.NewGuid():N}";
            output.Attributes.SetAttribute("aria-describedby", descId);
            output.PostContent.AppendHtml($"<span id=\"{descId}\" class=\"sr-only\">{Description}</span>");
        }

        if (!string.IsNullOrEmpty(Live))
        {
            output.Attributes.SetAttribute("aria-live", Live);
        }

        if (Expanded.HasValue)
        {
            output.Attributes.SetAttribute("aria-expanded", Expanded.Value.ToString().ToLower());
        }

        if (!string.IsNullOrEmpty(HasPopup))
        {
            output.Attributes.SetAttribute("aria-haspopup", HasPopup);
        }

        if (!string.IsNullOrEmpty(Controls))
        {
            output.Attributes.SetAttribute("aria-controls", Controls);
        }

        if (!string.IsNullOrEmpty(DescribedBy))
        {
            output.Attributes.SetAttribute("aria-describedby", DescribedBy);
        }

        if (!string.IsNullOrEmpty(LabelledBy))
        {
            output.Attributes.SetAttribute("aria-labelledby", LabelledBy);
        }

        if (Hidden.HasValue)
        {
            output.Attributes.SetAttribute("aria-hidden", Hidden.Value.ToString().ToLower());
        }

        if (ValueNow.HasValue)
        {
            output.Attributes.SetAttribute("aria-valuenow", ValueNow.Value.ToString());
        }

        if (ValueMin.HasValue)
        {
            output.Attributes.SetAttribute("aria-valuemin", ValueMin.Value.ToString());
        }

        if (ValueMax.HasValue)
        {
            output.Attributes.SetAttribute("aria-valuemax", ValueMax.Value.ToString());
        }

        if (!string.IsNullOrEmpty(ValueText))
        {
            output.Attributes.SetAttribute("aria-valuetext", ValueText);
        }

        if (Level.HasValue)
        {
            output.Attributes.SetAttribute("aria-level", Level.Value.ToString());
        }

        if (PosInSet.HasValue)
        {
            output.Attributes.SetAttribute("aria-posinset", PosInSet.Value.ToString());
        }

        if (SetSize.HasValue)
        {
            output.Attributes.SetAttribute("aria-setsize", SetSize.Value.ToString());
        }

        // Add skip link
        if (!string.IsNullOrEmpty(SkipLink))
        {
            var skipId = output.Attributes["id"]?.Value?.ToString() ?? $"skip-{Guid.NewGuid():N}";
            if (!output.Attributes.ContainsName("id"))
            {
                output.Attributes.SetAttribute("id", skipId);
            }
            
            output.PreContent.AppendHtml($"<a href=\"#{skipId}\" class=\"skip-link sr-only-focusable\">{SkipLink}</a>");
        }

        // Add focus management
        if (!string.IsNullOrEmpty(FocusMode) && FocusMode != "none")
        {
            output.Attributes.SetAttribute("data-a11y-focus", FocusMode);
            output.PostContent.AppendHtml($"<script>{GenerateA11yFocusScript()}</script>");
        }

        // Remove helper attributes
        output.Attributes.RemoveAll("a11y-role");
        output.Attributes.RemoveAll("a11y-label");
        output.Attributes.RemoveAll("a11y-description");
        output.Attributes.RemoveAll("a11y-live");
        output.Attributes.RemoveAll("a11y-expanded");
        output.Attributes.RemoveAll("a11y-haspopup");
        output.Attributes.RemoveAll("a11y-controls");
        output.Attributes.RemoveAll("a11y-describedby");
        output.Attributes.RemoveAll("a11y-labelledby");
        output.Attributes.RemoveAll("a11y-hidden");
        output.Attributes.RemoveAll("a11y-valuenow");
        output.Attributes.RemoveAll("a11y-valuemin");
        output.Attributes.RemoveAll("a11y-valuemax");
        output.Attributes.RemoveAll("a11y-valuetext");
        output.Attributes.RemoveAll("a11y-level");
        output.Attributes.RemoveAll("a11y-posinset");
        output.Attributes.RemoveAll("a11y-setsize");
        output.Attributes.RemoveAll("a11y-skip-link");
        output.Attributes.RemoveAll("a11y-focus");
    }

    private static string GenerateA11yFocusScript()
    {
        return @"
(function() {
    function initA11yFocus() {
        document.querySelectorAll('[data-a11y-focus]').forEach(element => {
            const mode = element.dataset.a11yFocus;
            
            if (mode === 'auto') {
                // Auto-manage focus for interactive elements
                if (element.matches('button, [role=""button""], a, [tabindex]')) {
                    element.addEventListener('click', (e) => {
                        // Announce action to screen readers
                        const announcement = element.getAttribute('aria-label') || 
                                           element.textContent || 
                                           'Action completed';
                        announceToScreenReader(announcement);
                    });
                }
                
                // Focus management for modals/dialogs
                if (element.matches('[role=""dialog""], [role=""alertdialog""]')) {
                    manageFocusForDialog(element);
                }
            }
            
            // Keyboard navigation enhancement
            element.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && element.hasAttribute('aria-expanded')) {
                    element.setAttribute('aria-expanded', 'false');
                    element.focus();
                }
            });
        });
    }
    
    function announceToScreenReader(message) {
        const announcement = document.createElement('div');
        announcement.setAttribute('aria-live', 'polite');
        announcement.setAttribute('aria-atomic', 'true');
        announcement.className = 'sr-only';
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    }
    
    function manageFocusForDialog(dialog) {
        const focusableElements = dialog.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex=""-1""])'
        );
        
        if (focusableElements.length === 0) return;
        
        const firstFocusable = focusableElements[0];
        const lastFocusable = focusableElements[focusableElements.length - 1];
        
        // Focus first element when dialog opens
        if (dialog.getAttribute('aria-expanded') === 'true' || 
            !dialog.hasAttribute('aria-expanded')) {
            firstFocusable.focus();
        }
        
        // Trap focus within dialog
        dialog.addEventListener('keydown', (e) => {
            if (e.key === 'Tab') {
                if (e.shiftKey) {
                    if (document.activeElement === firstFocusable) {
                        lastFocusable.focus();
                        e.preventDefault();
                    }
                } else {
                    if (document.activeElement === lastFocusable) {
                        firstFocusable.focus();
                        e.preventDefault();
                    }
                }
            }
        });
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initA11yFocus);
    } else {
        initA11yFocus();
    }
})();";
    }
}