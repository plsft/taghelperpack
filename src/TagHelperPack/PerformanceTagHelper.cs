using Microsoft.AspNetCore.Html;

namespace TagHelperPack;

/// <summary>
/// Adds performance monitoring and optimization features to elements.
/// </summary>
[HtmlTargetElement("*", Attributes = "perf-monitor")]
[HtmlTargetElement("*", Attributes = "perf-preload")]
[HtmlTargetElement("*", Attributes = "perf-critical")]
public class PerformanceTagHelper : TagHelper
{
    /// <summary>
    /// Whether to monitor performance metrics for this element.
    /// </summary>
    [HtmlAttributeName("perf-monitor")]
    public bool MonitorPerformance { get; set; }

    /// <summary>
    /// Resource to preload (dns, preconnect, prefetch, preload, modulepreload).
    /// </summary>
    [HtmlAttributeName("perf-preload")]
    public string? PreloadType { get; set; }

    /// <summary>
    /// URL to preload.
    /// </summary>
    [HtmlAttributeName("perf-preload-url")]
    public string? PreloadUrl { get; set; }

    /// <summary>
    /// Whether this is critical above-the-fold content.
    /// </summary>
    [HtmlAttributeName("perf-critical")]
    public bool IsCritical { get; set; }

    /// <summary>
    /// Resource hints to add (dns-prefetch, preconnect, prefetch, preload).
    /// </summary>
    [HtmlAttributeName("perf-hints")]
    public string? ResourceHints { get; set; }

    /// <summary>
    /// Web Vitals metrics to track (LCP, FID, CLS, FCP, TTFB).
    /// </summary>
    [HtmlAttributeName("perf-vitals")]
    public string? WebVitals { get; set; }

    /// <summary>
    /// Performance budget in milliseconds.
    /// </summary>
    [HtmlAttributeName("perf-budget")]
    public int? PerformanceBudget { get; set; }

    /// <summary>
    /// Callback function for performance data.
    /// </summary>
    [HtmlAttributeName("perf-callback")]
    public string? PerformanceCallback { get; set; }

    /// <summary>
    /// Whether to use Intersection Observer for performance tracking.
    /// </summary>
    [HtmlAttributeName("perf-observe")]
    public bool ObservePerformance { get; set; } = false;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        var performanceFeatures = new List<string>();

        // Add resource hints
        if (!string.IsNullOrEmpty(PreloadType) && !string.IsNullOrEmpty(PreloadUrl))
        {
            AddResourceHint(output, PreloadType, PreloadUrl);
        }

        if (!string.IsNullOrEmpty(ResourceHints))
        {
            var hints = ResourceHints.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var hint in hints)
            {
                var parts = hint.Trim().Split(':', 2);
                if (parts.Length == 2)
                {
                    AddResourceHint(output, parts[0].Trim(), parts[1].Trim());
                }
            }
        }

        // Add performance monitoring attributes
        if (MonitorPerformance)
        {
            output.Attributes.SetAttribute("data-perf-monitor", "true");
            performanceFeatures.Add("monitor");
        }

        if (IsCritical)
        {
            output.Attributes.SetAttribute("data-perf-critical", "true");
            performanceFeatures.Add("critical");
        }

        if (!string.IsNullOrEmpty(WebVitals))
        {
            output.Attributes.SetAttribute("data-perf-vitals", WebVitals);
            performanceFeatures.Add("vitals");
        }

        if (PerformanceBudget.HasValue)
        {
            output.Attributes.SetAttribute("data-perf-budget", PerformanceBudget.Value.ToString());
            performanceFeatures.Add("budget");
        }

        if (!string.IsNullOrEmpty(PerformanceCallback))
        {
            output.Attributes.SetAttribute("data-perf-callback", PerformanceCallback);
        }

        if (ObservePerformance)
        {
            output.Attributes.SetAttribute("data-perf-observe", "true");
            performanceFeatures.Add("observe");
        }

        // Add performance script if any features are enabled
        if (performanceFeatures.Count > 0)
        {
            output.PostContent.AppendHtml($"<script>{GeneratePerformanceScript()}</script>");
        }

        // Remove helper attributes
        output.Attributes.RemoveAll("perf-monitor");
        output.Attributes.RemoveAll("perf-preload");
        output.Attributes.RemoveAll("perf-preload-url");
        output.Attributes.RemoveAll("perf-critical");
        output.Attributes.RemoveAll("perf-hints");
        output.Attributes.RemoveAll("perf-vitals");
        output.Attributes.RemoveAll("perf-budget");
        output.Attributes.RemoveAll("perf-callback");
        output.Attributes.RemoveAll("perf-observe");
    }

    private void AddResourceHint(TagHelperOutput output, string type, string url)
    {
        var linkTag = type.ToLower() switch
        {
            "dns" or "dns-prefetch" => $"<link rel=\"dns-prefetch\" href=\"{url}\">",
            "preconnect" => $"<link rel=\"preconnect\" href=\"{url}\">",
            "prefetch" => $"<link rel=\"prefetch\" href=\"{url}\">",
            "preload" => $"<link rel=\"preload\" href=\"{url}\" as=\"{GetResourceType(url)}\">",
            "modulepreload" => $"<link rel=\"modulepreload\" href=\"{url}\">",
            _ => null
        };

        if (linkTag != null)
        {
            output.PreContent.AppendHtml(linkTag);
        }
    }

    private static string GetResourceType(string url)
    {
        var extension = Path.GetExtension(url).ToLower();
        return extension switch
        {
            ".js" => "script",
            ".css" => "style",
            ".woff" or ".woff2" => "font",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => "image",
            ".mp4" or ".webm" or ".ogg" => "video",
            ".mp3" or ".wav" or ".ogg" => "audio",
            _ => "fetch"
        };
    }

    private static string GeneratePerformanceScript()
    {
        return @"
(function() {
    class PerformanceMonitor {
        constructor() {
            this.metrics = {};
            this.observers = [];
            this.init();
        }
        
        init() {
            this.initWebVitals();
            this.initElementMonitoring();
            this.initCriticalResourceTiming();
        }
        
        initWebVitals() {
            // Core Web Vitals monitoring
            if ('PerformanceObserver' in window) {
                // Largest Contentful Paint (LCP)
                new PerformanceObserver((entryList) => {
                    const entries = entryList.getEntries();
                    const lastEntry = entries[entries.length - 1];
                    this.reportMetric('LCP', lastEntry.startTime);
                }).observe({ type: 'largest-contentful-paint', buffered: true });
                
                // First Input Delay (FID)
                new PerformanceObserver((entryList) => {
                    for (const entry of entryList.getEntries()) {
                        this.reportMetric('FID', entry.processingStart - entry.startTime);
                    }
                }).observe({ type: 'first-input', buffered: true });
                
                // Cumulative Layout Shift (CLS)
                let clsValue = 0;
                new PerformanceObserver((entryList) => {
                    for (const entry of entryList.getEntries()) {
                        if (!entry.hadRecentInput) {
                            clsValue += entry.value;
                        }
                    }
                    this.reportMetric('CLS', clsValue);
                }).observe({ type: 'layout-shift', buffered: true });
            }
        }
        
        initElementMonitoring() {
            document.querySelectorAll('[data-perf-monitor]').forEach(element => {
                this.monitorElement(element);
            });
            
            // Observe critical elements
            if ('IntersectionObserver' in window) {
                const criticalObserver = new IntersectionObserver((entries) => {
                    entries.forEach(entry => {
                        if (entry.isIntersecting) {
                            this.measureElementPerformance(entry.target);
                        }
                    });
                });
                
                document.querySelectorAll('[data-perf-critical], [data-perf-observe]').forEach(element => {
                    criticalObserver.observe(element);
                });
            }
        }
        
        monitorElement(element) {
            const startTime = performance.now();
            const budget = parseInt(element.dataset.perfBudget) || null;
            const vitals = element.dataset.perfVitals ? element.dataset.perfVitals.split(',') : [];
            const callback = element.dataset.perfCallback;
            
            // Use ResizeObserver to detect layout changes
            if ('ResizeObserver' in window) {
                new ResizeObserver(() => {
                    const endTime = performance.now();
                    const duration = endTime - startTime;
                    
                    const metrics = {
                        element: element,
                        duration: duration,
                        exceedsBudget: budget ? duration > budget : false,
                        timestamp: endTime
                    };
                    
                    if (callback && typeof window[callback] === 'function') {
                        window[callback](metrics);
                    }
                    
                    this.reportElementMetrics(element, metrics);
                }).observe(element);
            }
        }
        
        measureElementPerformance(element) {
            const rect = element.getBoundingClientRect();
            const isVisible = rect.top >= 0 && rect.left >= 0 && 
                             rect.bottom <= window.innerHeight && 
                             rect.right <= window.innerWidth;
            
            if (isVisible && element.dataset.perfCritical) {
                // Measure time to visibility for critical elements
                const navigationStart = performance.timing.navigationStart;
                const timeToVisible = performance.now() - navigationStart;
                
                this.reportMetric('TimeToVisible', timeToVisible, element);
            }
        }
        
        initCriticalResourceTiming() {
            window.addEventListener('load', () => {
                const navigation = performance.getEntriesByType('navigation')[0];
                const resources = performance.getEntriesByType('resource');
                
                // Report critical resource timing
                resources.forEach(resource => {
                    if (resource.duration > 1000) { // Resources taking > 1s
                        this.reportMetric('SlowResource', resource.duration, null, resource.name);
                    }
                });
                
                // Overall page load metrics
                this.reportMetric('DOMContentLoaded', navigation.domContentLoadedEventEnd - navigation.navigationStart);
                this.reportMetric('LoadComplete', navigation.loadEventEnd - navigation.navigationStart);
            });
        }
        
        reportMetric(name, value, element = null, resource = null) {
            const metric = {
                name,
                value,
                timestamp: performance.now(),
                element: element ? this.getElementSelector(element) : null,
                resource: resource
            };
            
            this.metrics[name] = metric;
            
            // Send to analytics or monitoring service
            this.sendToAnalytics(metric);
            
            // Log to console in development
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                console.log(`Performance Metric: ${name} = ${value}ms`, metric);
            }
        }
        
        reportElementMetrics(element, metrics) {
            const elementSelector = this.getElementSelector(element);
            
            // Dispatch custom event
            element.dispatchEvent(new CustomEvent('performance-measured', {
                detail: metrics
            }));
            
            // Check performance budget
            if (metrics.exceedsBudget) {
                console.warn(`Performance budget exceeded for element: ${elementSelector}`, metrics);
                
                element.dispatchEvent(new CustomEvent('performance-budget-exceeded', {
                    detail: metrics
                }));
            }
        }
        
        getElementSelector(element) {
            if (element.id) return `#${element.id}`;
            if (element.className) return `.${element.className.split(' ')[0]}`;
            return element.tagName.toLowerCase();
        }
        
        sendToAnalytics(metric) {
            // Send to Google Analytics 4
            if (typeof gtag !== 'undefined') {
                gtag('event', 'performance_metric', {
                    metric_name: metric.name,
                    metric_value: metric.value,
                    element_selector: metric.element,
                    resource_name: metric.resource
                });
            }
            
            // Send to custom analytics endpoint
            if (window.performanceEndpoint) {
                fetch(window.performanceEndpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(metric)
                }).catch(err => console.error('Failed to send performance data:', err));
            }
        }
        
        getMetrics() {
            return { ...this.metrics };
        }
        
        exportMetrics() {
            const data = JSON.stringify(this.metrics, null, 2);
            const blob = new Blob([data], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            
            const a = document.createElement('a');
            a.href = url;
            a.download = `performance-metrics-${Date.now()}.json`;
            a.click();
            
            URL.revokeObjectURL(url);
        }
    }
    
    // Initialize performance monitoring
    let performanceMonitor;
    
    function initPerformanceMonitoring() {
        performanceMonitor = new PerformanceMonitor();
        
        // Export to window for external access
        window.performanceMonitor = performanceMonitor;
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPerformanceMonitoring);
    } else {
        initPerformanceMonitoring();
    }
})();";
    }
}