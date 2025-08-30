using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace TagHelperPack;

/// <summary>
/// Implements virtual scrolling for large lists to improve performance, similar to React Virtual.
/// </summary>
[HtmlTargetElement("virtual-list")]
public class VirtualScrollTagHelper : TagHelper
{
    /// <summary>
    /// The height of each item in pixels.
    /// </summary>
    [HtmlAttributeName("item-height")]
    public int ItemHeight { get; set; } = 50;

    /// <summary>
    /// The height of the container in pixels.
    /// </summary>
    [HtmlAttributeName("container-height")]
    public int ContainerHeight { get; set; } = 400;

    /// <summary>
    /// Number of extra items to render outside the visible area for smoother scrolling.
    /// </summary>
    [HtmlAttributeName("overscan")]
    public int Overscan { get; set; } = 5;

    /// <summary>
    /// The total number of items in the list.
    /// </summary>
    [HtmlAttributeName("item-count")]
    public int ItemCount { get; set; }

    /// <summary>
    /// JavaScript function name that renders an item given an index.
    /// Function signature: (index: number) => string
    /// </summary>
    [HtmlAttributeName("item-renderer")]
    public string ItemRenderer { get; set; } = default!;

    /// <summary>
    /// CSS class for the container.
    /// </summary>
    [HtmlAttributeName("container-class")]
    public string? ContainerClass { get; set; }

    /// <summary>
    /// CSS class for individual items.
    /// </summary>
    [HtmlAttributeName("item-class")]
    public string? ItemClass { get; set; }

    /// <summary>
    /// Whether items have variable heights (experimental).
    /// </summary>
    [HtmlAttributeName("variable-height")]
    public bool VariableHeight { get; set; } = false;

    /// <summary>
    /// Scroll direction: vertical or horizontal.
    /// </summary>
    [HtmlAttributeName("direction")]
    public string Direction { get; set; } = "vertical";

    /// <summary>
    /// Loading placeholder HTML when items are being rendered.
    /// </summary>
    [HtmlAttributeName("loading-placeholder")]
    public string? LoadingPlaceholder { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        if (string.IsNullOrEmpty(ItemRenderer))
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";

        // Set up container styles and classes
        var containerClasses = new List<string> { "virtual-scroll-container" };
        if (!string.IsNullOrEmpty(ContainerClass))
        {
            containerClasses.Add(ContainerClass);
        }
        output.Attributes.SetAttribute("class", string.Join(" ", containerClasses));

        // Add data attributes for configuration
        var config = new
        {
            itemHeight = ItemHeight,
            containerHeight = ContainerHeight,
            overscan = Overscan,
            itemCount = ItemCount,
            itemRenderer = ItemRenderer,
            itemClass = ItemClass,
            variableHeight = VariableHeight,
            direction = Direction,
            loadingPlaceholder = LoadingPlaceholder
        };

        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        output.Attributes.SetAttribute("data-virtual-scroll", configJson);

        // Set initial styles
        var styleList = new List<string>();
        
        if (Direction.ToLower() == "vertical")
        {
            styleList.Add($"height: {ContainerHeight}px");
            styleList.Add("overflow-y: auto");
            styleList.Add("overflow-x: hidden");
        }
        else
        {
            styleList.Add($"width: {ContainerHeight}px");
            styleList.Add("overflow-x: auto");
            styleList.Add("overflow-y: hidden");
        }
        
        styleList.Add("position: relative");

        var existingStyle = output.Attributes["style"]?.Value?.ToString();
        if (!string.IsNullOrEmpty(existingStyle))
        {
            styleList.Insert(0, existingStyle);
        }

        output.Attributes.SetAttribute("style", string.Join("; ", styleList));

        // Add the virtual scrolling script
        output.PostContent.AppendHtml($"<script>{GenerateVirtualScrollScript()}</script>");
    }

    private static string GenerateVirtualScrollScript()
    {
        return @"
(function() {
    class VirtualScroller {
        constructor(container) {
            this.container = container;
            const config = JSON.parse(container.dataset.virtualScroll);
            
            this.itemHeight = config.itemHeight;
            this.containerHeight = config.containerHeight;
            this.overscan = config.overscan;
            this.itemCount = config.itemCount;
            this.itemRenderer = config.itemRenderer;
            this.itemClass = config.itemClass || '';
            this.variableHeight = config.variableHeight;
            this.direction = config.direction || 'vertical';
            this.loadingPlaceholder = config.loadingPlaceholder || '';
            
            this.scrollTop = 0;
            this.visibleStart = 0;
            this.visibleEnd = 0;
            this.renderedItems = new Map();
            this.itemHeights = new Map();
            
            this.init();
        }
        
        init() {
            this.createScrollArea();
            this.createViewport();
            this.bindEvents();
            this.updateVisibleItems();
        }
        
        createScrollArea() {
            this.scrollArea = document.createElement('div');
            this.scrollArea.style.position = 'absolute';
            this.scrollArea.style.top = '0';
            this.scrollArea.style.left = '0';
            this.scrollArea.style.right = '0';
            
            if (this.direction === 'vertical') {
                this.scrollArea.style.height = `${this.itemCount * this.itemHeight}px`;
            } else {
                this.scrollArea.style.width = `${this.itemCount * this.itemHeight}px`;
            }
            
            this.container.appendChild(this.scrollArea);
        }
        
        createViewport() {
            this.viewport = document.createElement('div');
            this.viewport.style.position = 'relative';
            this.viewport.style.overflow = 'hidden';
            this.viewport.className = 'virtual-scroll-viewport';
            
            this.container.appendChild(this.viewport);
        }
        
        bindEvents() {
            this.container.addEventListener('scroll', () => {
                this.handleScroll();
            });
            
            // Handle resize
            window.addEventListener('resize', () => {
                this.updateVisibleItems();
            });
        }
        
        handleScroll() {
            if (this.direction === 'vertical') {
                this.scrollTop = this.container.scrollTop;
            } else {
                this.scrollTop = this.container.scrollLeft;
            }
            
            this.updateVisibleItems();
        }
        
        updateVisibleItems() {
            const containerSize = this.direction === 'vertical' ? 
                this.containerHeight : this.container.offsetWidth;
            
            let start = Math.floor(this.scrollTop / this.itemHeight);
            let end = Math.min(
                start + Math.ceil(containerSize / this.itemHeight),
                this.itemCount - 1
            );
            
            // Add overscan
            start = Math.max(0, start - this.overscan);
            end = Math.min(this.itemCount - 1, end + this.overscan);
            
            this.visibleStart = start;
            this.visibleEnd = end;
            
            this.renderItems();
        }
        
        renderItems() {
            // Remove items that are no longer visible
            this.renderedItems.forEach((element, index) => {
                if (index < this.visibleStart || index > this.visibleEnd) {
                    element.remove();
                    this.renderedItems.delete(index);
                }
            });
            
            // Add new visible items
            for (let i = this.visibleStart; i <= this.visibleEnd; i++) {
                if (!this.renderedItems.has(i)) {
                    this.renderItem(i);
                }
            }
        }
        
        renderItem(index) {
            if (!window[this.itemRenderer]) {
                console.error(`Item renderer function '${this.itemRenderer}' not found`);
                return;
            }
            
            const itemElement = document.createElement('div');
            itemElement.className = `virtual-scroll-item ${this.itemClass}`.trim();
            itemElement.style.position = 'absolute';
            itemElement.style.top = this.direction === 'vertical' ? 
                `${index * this.itemHeight}px` : '0';
            itemElement.style.left = this.direction === 'horizontal' ? 
                `${index * this.itemHeight}px` : '0';
            itemElement.style.height = this.direction === 'vertical' ? 
                `${this.itemHeight}px` : '100%';
            itemElement.style.width = this.direction === 'horizontal' ? 
                `${this.itemHeight}px` : '100%';
            
            // Show loading placeholder initially
            if (this.loadingPlaceholder) {
                itemElement.innerHTML = this.loadingPlaceholder;
            }
            
            // Render item content asynchronously
            Promise.resolve().then(() => {
                const content = window[this.itemRenderer](index);
                itemElement.innerHTML = content;
                
                // Handle variable height
                if (this.variableHeight && this.direction === 'vertical') {
                    const actualHeight = itemElement.offsetHeight;
                    this.itemHeights.set(index, actualHeight);
                    this.updateScrollArea();
                }
            });
            
            this.viewport.appendChild(itemElement);
            this.renderedItems.set(index, itemElement);
        }
        
        updateScrollArea() {
            if (this.variableHeight) {
                let totalHeight = 0;
                for (let i = 0; i < this.itemCount; i++) {
                    totalHeight += this.itemHeights.get(i) || this.itemHeight;
                }
                
                if (this.direction === 'vertical') {
                    this.scrollArea.style.height = `${totalHeight}px`;
                } else {
                    this.scrollArea.style.width = `${totalHeight}px`;
                }
            }
        }
        
        scrollToIndex(index) {
            const position = index * this.itemHeight;
            
            if (this.direction === 'vertical') {
                this.container.scrollTop = position;
            } else {
                this.container.scrollLeft = position;
            }
        }
        
        getVisibleRange() {
            return {
                start: this.visibleStart,
                end: this.visibleEnd
            };
        }
        
        refresh() {
            this.renderedItems.clear();
            this.viewport.innerHTML = '';
            this.updateVisibleItems();
        }
    }
    
    // Initialize all virtual scroll containers
    function initVirtualScrollers() {
        document.querySelectorAll('[data-virtual-scroll]').forEach(container => {
            if (!container.virtualScroller) {
                container.virtualScroller = new VirtualScroller(container);
            }
        });
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initVirtualScrollers);
    } else {
        initVirtualScrollers();
    }
    
    // Export for external use
    window.VirtualScroller = VirtualScroller;
})();";
    }
}