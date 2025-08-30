using Microsoft.AspNetCore.Html;

namespace TagHelperPack;

/// <summary>
/// Uses Intersection Observer API for scroll-triggered animations and lazy loading.
/// </summary>
[HtmlTargetElement("*", Attributes = "intersection-observe")]
public class IntersectionObserverTagHelper : TagHelper
{
    /// <summary>
    /// Whether to enable intersection observation.
    /// </summary>
    [HtmlAttributeName("intersection-observe")]
    public bool ObserveIntersection { get; set; }

    /// <summary>
    /// The root margin for the intersection observer (default: "0px").
    /// </summary>
    [HtmlAttributeName("observe-margin")]
    public string RootMargin { get; set; } = "0px";

    /// <summary>
    /// The threshold for triggering intersection (default: 0.1).
    /// </summary>
    [HtmlAttributeName("observe-threshold")]
    public double Threshold { get; set; } = 0.1;

    /// <summary>
    /// CSS class to add when element enters viewport.
    /// </summary>
    [HtmlAttributeName("observe-enter-class")]
    public string? EnterClass { get; set; }

    /// <summary>
    /// CSS class to remove when element enters viewport.
    /// </summary>
    [HtmlAttributeName("observe-exit-class")]
    public string? ExitClass { get; set; }

    /// <summary>
    /// Animation to trigger when element enters viewport.
    /// </summary>
    [HtmlAttributeName("observe-animation")]
    public string? Animation { get; set; }

    /// <summary>
    /// Whether to observe only once (unobserve after first intersection).
    /// </summary>
    [HtmlAttributeName("observe-once")]
    public bool ObserveOnce { get; set; } = false;

    /// <summary>
    /// Callback function name to call when element enters viewport.
    /// </summary>
    [HtmlAttributeName("observe-callback")]
    public string? Callback { get; set; }

    /// <summary>
    /// Custom data to pass to the callback.
    /// </summary>
    [HtmlAttributeName("observe-data")]
    public string? CallbackData { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (!ObserveIntersection)
        {
            return;
        }

        // Add data attributes for the observer
        output.Attributes.SetAttribute("data-intersection-observe", "true");
        output.Attributes.SetAttribute("data-observe-margin", RootMargin);
        output.Attributes.SetAttribute("data-observe-threshold", Threshold.ToString());
        output.Attributes.SetAttribute("data-observe-once", ObserveOnce.ToString().ToLower());

        if (!string.IsNullOrEmpty(EnterClass))
        {
            output.Attributes.SetAttribute("data-observe-enter-class", EnterClass);
        }

        if (!string.IsNullOrEmpty(ExitClass))
        {
            output.Attributes.SetAttribute("data-observe-exit-class", ExitClass);
        }

        if (!string.IsNullOrEmpty(Animation))
        {
            output.Attributes.SetAttribute("data-observe-animation", Animation);
        }

        if (!string.IsNullOrEmpty(Callback))
        {
            output.Attributes.SetAttribute("data-observe-callback", Callback);
        }

        if (!string.IsNullOrEmpty(CallbackData))
        {
            output.Attributes.SetAttribute("data-observe-data", CallbackData);
        }

        // Add the intersection observer script
        output.PostContent.AppendHtml($"<script>{GenerateIntersectionObserverScript()}</script>");

        // Remove the helper attributes from output
        output.Attributes.RemoveAll("intersection-observe");
        output.Attributes.RemoveAll("observe-margin");
        output.Attributes.RemoveAll("observe-threshold");
        output.Attributes.RemoveAll("observe-enter-class");
        output.Attributes.RemoveAll("observe-exit-class");
        output.Attributes.RemoveAll("observe-animation");
        output.Attributes.RemoveAll("observe-once");
        output.Attributes.RemoveAll("observe-callback");
        output.Attributes.RemoveAll("observe-data");
    }

    private static string GenerateIntersectionObserverScript()
    {
        return @"
(function() {
    if (!window.IntersectionObserver) {
        console.warn('Intersection Observer not supported');
        return;
    }
    
    const observerMap = new Map();
    
    function createObserver(rootMargin, threshold) {
        const key = `${rootMargin}-${threshold}`;
        
        if (observerMap.has(key)) {
            return observerMap.get(key);
        }
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                const element = entry.target;
                const isIntersecting = entry.isIntersecting;
                
                if (isIntersecting) {
                    handleIntersection(element);
                    
                    if (element.dataset.observeOnce === 'true') {
                        observer.unobserve(element);
                    }
                } else {
                    handleExit(element);
                }
            });
        }, {
            rootMargin: rootMargin,
            threshold: parseFloat(threshold)
        });
        
        observerMap.set(key, observer);
        return observer;
    }
    
    function handleIntersection(element) {
        const enterClass = element.dataset.observeEnterClass;
        const exitClass = element.dataset.observeExitClass;
        const animation = element.dataset.observeAnimation;
        const callback = element.dataset.observeCallback;
        const callbackData = element.dataset.observeData;
        
        if (enterClass) {
            element.classList.add(enterClass);
        }
        
        if (exitClass) {
            element.classList.remove(exitClass);
        }
        
        if (animation) {
            element.style.animation = animation;
        }
        
        if (callback && typeof window[callback] === 'function') {
            const data = callbackData ? JSON.parse(callbackData) : {};
            window[callback](element, data);
        }
        
        // Dispatch custom event
        element.dispatchEvent(new CustomEvent('intersection-enter', {
            detail: { element, isIntersecting: true }
        }));
    }
    
    function handleExit(element) {
        const enterClass = element.dataset.observeEnterClass;
        const exitClass = element.dataset.observeExitClass;
        
        if (enterClass) {
            element.classList.remove(enterClass);
        }
        
        if (exitClass) {
            element.classList.add(exitClass);
        }
        
        // Dispatch custom event
        element.dispatchEvent(new CustomEvent('intersection-exit', {
            detail: { element, isIntersecting: false }
        }));
    }
    
    // Initialize intersection observers
    function initIntersectionObservers() {
        document.querySelectorAll('[data-intersection-observe]').forEach(element => {
            const rootMargin = element.dataset.observeMargin || '0px';
            const threshold = element.dataset.observeThreshold || '0.1';
            
            const observer = createObserver(rootMargin, threshold);
            observer.observe(element);
        });
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initIntersectionObservers);
    } else {
        initIntersectionObservers();
    }
    
    // Re-initialize when new elements are added
    window.reinitIntersectionObservers = initIntersectionObservers;
})();";
    }
}