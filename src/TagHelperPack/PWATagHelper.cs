using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace TagHelperPack;

/// <summary>
/// Generates Progressive Web App (PWA) installation prompts and capabilities.
/// </summary>
[HtmlTargetElement("pwa-install")]
[HtmlTargetElement("*", Attributes = "pwa-installable")]
public class PWATagHelper : TagHelper
{
    /// <summary>
    /// Whether to enable PWA installation prompt.
    /// </summary>
    [HtmlAttributeName("pwa-installable")]
    public bool IsInstallable { get; set; }

    /// <summary>
    /// Text to show in the install button.
    /// </summary>
    [HtmlAttributeName("install-text")]
    public string InstallText { get; set; } = "Install App";

    /// <summary>
    /// Text to show when app is already installed.
    /// </summary>
    [HtmlAttributeName("installed-text")]
    public string InstalledText { get; set; } = "App Installed";

    /// <summary>
    /// CSS class to apply when app can be installed.
    /// </summary>
    [HtmlAttributeName("installable-class")]
    public string? InstallableClass { get; set; }

    /// <summary>
    /// CSS class to apply when app is installed.
    /// </summary>
    [HtmlAttributeName("installed-class")]
    public string? InstalledClass { get; set; }

    /// <summary>
    /// Whether to show the install prompt automatically.
    /// </summary>
    [HtmlAttributeName("auto-prompt")]
    public bool AutoPrompt { get; set; } = false;

    /// <summary>
    /// Delay in milliseconds before showing auto prompt.
    /// </summary>
    [HtmlAttributeName("auto-prompt-delay")]
    public int AutoPromptDelay { get; set; } = 3000;

    /// <summary>
    /// Callback function when installation is successful.
    /// </summary>
    [HtmlAttributeName("on-install")]
    public string? OnInstall { get; set; }

    /// <summary>
    /// Callback function when installation is dismissed.
    /// </summary>
    [HtmlAttributeName("on-dismiss")]
    public string? OnDismiss { get; set; }

    /// <summary>
    /// Whether to hide the button if PWA is not supported.
    /// </summary>
    [HtmlAttributeName("hide-if-unsupported")]
    public bool HideIfUnsupported { get; set; } = true;

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

        // If this is a pwa-install element, convert to button
        if (output.TagName == "pwa-install")
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "button");
        }

        // Add PWA data attributes
        output.Attributes.SetAttribute("data-pwa-install", "true");
        output.Attributes.SetAttribute("data-install-text", InstallText);
        output.Attributes.SetAttribute("data-installed-text", InstalledText);
        output.Attributes.SetAttribute("data-auto-prompt", AutoPrompt.ToString().ToLower());
        output.Attributes.SetAttribute("data-auto-prompt-delay", AutoPromptDelay.ToString());
        output.Attributes.SetAttribute("data-hide-if-unsupported", HideIfUnsupported.ToString().ToLower());

        if (!string.IsNullOrEmpty(InstallableClass))
        {
            output.Attributes.SetAttribute("data-installable-class", InstallableClass);
        }

        if (!string.IsNullOrEmpty(InstalledClass))
        {
            output.Attributes.SetAttribute("data-installed-class", InstalledClass);
        }

        if (!string.IsNullOrEmpty(OnInstall))
        {
            output.Attributes.SetAttribute("data-on-install", OnInstall);
        }

        if (!string.IsNullOrEmpty(OnDismiss))
        {
            output.Attributes.SetAttribute("data-on-dismiss", OnDismiss);
        }

        // Set default content if empty
        if (childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.SetContent(InstallText);
        }

        // Add the PWA script
        output.PostContent.AppendHtml($"<script>{GeneratePWAScript()}</script>");

        // Remove helper attributes
        output.Attributes.RemoveAll("pwa-installable");
        output.Attributes.RemoveAll("install-text");
        output.Attributes.RemoveAll("installed-text");
        output.Attributes.RemoveAll("installable-class");
        output.Attributes.RemoveAll("installed-class");
        output.Attributes.RemoveAll("auto-prompt");
        output.Attributes.RemoveAll("auto-prompt-delay");
        output.Attributes.RemoveAll("on-install");
        output.Attributes.RemoveAll("on-dismiss");
        output.Attributes.RemoveAll("hide-if-unsupported");
    }

    private static string GeneratePWAScript()
    {
        return @"
(function() {
    let deferredPrompt;
    let isInstalled = false;
    
    class PWAInstaller {
        constructor(element) {
            this.element = element;
            this.installText = element.dataset.installText || 'Install App';
            this.installedText = element.dataset.installedText || 'App Installed';
            this.installableClass = element.dataset.installableClass;
            this.installedClass = element.dataset.installedClass;
            this.autoPrompt = element.dataset.autoPrompt === 'true';
            this.autoPromptDelay = parseInt(element.dataset.autoPromptDelay) || 3000;
            this.onInstall = element.dataset.onInstall;
            this.onDismiss = element.dataset.onDismiss;
            this.hideIfUnsupported = element.dataset.hideIfUnsupported === 'true';
            
            this.init();
        }
        
        init() {
            // Check if already installed
            if (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) {
                this.handleInstalled();
                return;
            }
            
            // Check for iOS Safari standalone
            if (window.navigator.standalone) {
                this.handleInstalled();
                return;
            }
            
            // Initially hide if unsupported
            if (this.hideIfUnsupported) {
                this.element.style.display = 'none';
            }
            
            this.element.addEventListener('click', () => this.install());
            
            // Auto prompt if enabled
            if (this.autoPrompt) {
                setTimeout(() => {
                    if (deferredPrompt && !isInstalled) {
                        this.install();
                    }
                }, this.autoPromptDelay);
            }
        }
        
        handleInstallable() {
            if (this.hideIfUnsupported) {
                this.element.style.display = '';
            }
            
            this.element.textContent = this.installText;
            
            if (this.installableClass) {
                this.element.classList.add(this.installableClass);
            }
            
            if (this.installedClass) {
                this.element.classList.remove(this.installedClass);
            }
            
            this.element.dispatchEvent(new CustomEvent('pwa-installable'));
        }
        
        handleInstalled() {
            isInstalled = true;
            this.element.textContent = this.installedText;
            this.element.disabled = true;
            
            if (this.installedClass) {
                this.element.classList.add(this.installedClass);
            }
            
            if (this.installableClass) {
                this.element.classList.remove(this.installableClass);
            }
            
            this.element.dispatchEvent(new CustomEvent('pwa-installed'));
            
            if (this.onInstall && typeof window[this.onInstall] === 'function') {
                window[this.onInstall]();
            }
        }
        
        async install() {
            if (!deferredPrompt) {
                return;
            }
            
            try {
                deferredPrompt.prompt();
                const { outcome } = await deferredPrompt.userChoice;
                
                if (outcome === 'accepted') {
                    this.handleInstalled();
                } else {
                    this.element.dispatchEvent(new CustomEvent('pwa-dismissed'));
                    
                    if (this.onDismiss && typeof window[this.onDismiss] === 'function') {
                        window[this.onDismiss]();
                    }
                }
                
                deferredPrompt = null;
            } catch (error) {
                console.error('PWA installation error:', error);
            }
        }
    }
    
    // Global PWA event listeners
    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredPrompt = e;
        
        // Update all PWA install buttons
        document.querySelectorAll('[data-pwa-install]').forEach(element => {
            const installer = element.pwaInstaller || new PWAInstaller(element);
            element.pwaInstaller = installer;
            installer.handleInstallable();
        });
    });
    
    window.addEventListener('appinstalled', () => {
        deferredPrompt = null;
        
        // Update all PWA install buttons
        document.querySelectorAll('[data-pwa-install]').forEach(element => {
            if (element.pwaInstaller) {
                element.pwaInstaller.handleInstalled();
            }
        });
    });
    
    // Service Worker registration helper
    function registerServiceWorker() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => {
                    console.log('Service Worker registered:', registration);
                })
                .catch(error => {
                    console.error('Service Worker registration failed:', error);
                });
        }
    }
    
    // Initialize PWA installers
    function initPWAInstallers() {
        document.querySelectorAll('[data-pwa-install]').forEach(element => {
            if (!element.pwaInstaller) {
                element.pwaInstaller = new PWAInstaller(element);
            }
        });
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            initPWAInstallers();
            registerServiceWorker();
        });
    } else {
        initPWAInstallers();
        registerServiceWorker();
    }
})();";
    }
}