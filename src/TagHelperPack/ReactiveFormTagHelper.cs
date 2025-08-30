using Microsoft.AspNetCore.Html;
using System.Text.Json;

namespace TagHelperPack;

/// <summary>
/// Creates reactive forms with real-time validation and state management, similar to React Hook Form.
/// </summary>
[HtmlTargetElement("reactive-form")]
public class ReactiveFormTagHelper : TagHelper
{
    /// <summary>
    /// The form name for state management.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = "form";

    /// <summary>
    /// Whether to validate on blur (default: true).
    /// </summary>
    [HtmlAttributeName("validate-on-blur")]
    public bool ValidateOnBlur { get; set; } = true;

    /// <summary>
    /// Whether to validate on change (default: false).
    /// </summary>
    [HtmlAttributeName("validate-on-change")]
    public bool ValidateOnChange { get; set; } = false;

    /// <summary>
    /// Whether to validate on submit (default: true).
    /// </summary>
    [HtmlAttributeName("validate-on-submit")]
    public bool ValidateOnSubmit { get; set; } = true;

    /// <summary>
    /// Default values as JSON.
    /// </summary>
    [HtmlAttributeName("default-values")]
    public object? DefaultValues { get; set; }

    /// <summary>
    /// Validation schema as JSON.
    /// </summary>
    [HtmlAttributeName("validation-schema")]
    public string? ValidationSchema { get; set; }

    /// <summary>
    /// Custom error messages.
    /// </summary>
    [HtmlAttributeName("error-messages")]
    public object? ErrorMessages { get; set; }

    /// <summary>
    /// Whether to persist form state to localStorage.
    /// </summary>
    [HtmlAttributeName("persist-state")]
    public bool PersistState { get; set; } = false;

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

        output.TagName = "form";
        output.Attributes.SetAttribute("data-reactive-form", Name);
        output.Attributes.SetAttribute("novalidate", "");

        // Add configuration as data attributes
        output.Attributes.SetAttribute("data-validate-on-blur", ValidateOnBlur.ToString().ToLower());
        output.Attributes.SetAttribute("data-validate-on-change", ValidateOnChange.ToString().ToLower());
        output.Attributes.SetAttribute("data-validate-on-submit", ValidateOnSubmit.ToString().ToLower());
        output.Attributes.SetAttribute("data-persist-state", PersistState.ToString().ToLower());

        if (DefaultValues != null)
        {
            var defaultValuesJson = JsonSerializer.Serialize(DefaultValues, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            output.Attributes.SetAttribute("data-default-values", defaultValuesJson);
        }

        if (!string.IsNullOrEmpty(ValidationSchema))
        {
            output.Attributes.SetAttribute("data-validation-schema", ValidationSchema);
        }

        if (ErrorMessages != null)
        {
            var errorMessagesJson = JsonSerializer.Serialize(ErrorMessages, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            output.Attributes.SetAttribute("data-error-messages", errorMessagesJson);
        }

        output.Content.SetHtmlContent(childContent);

        // Add the reactive form script
        var script = GenerateReactiveFormScript();
        output.PostContent.AppendHtml($"<script>{script}</script>");
    }

    private static string GenerateReactiveFormScript()
    {
        return @"
(function() {
    class ReactiveForm {
        constructor(form) {
            this.form = form;
            this.name = form.dataset.reactiveForm;
            this.validateOnBlur = form.dataset.validateOnBlur === 'true';
            this.validateOnChange = form.dataset.validateOnChange === 'true';
            this.validateOnSubmit = form.dataset.validateOnSubmit === 'true';
            this.persistState = form.dataset.persistState === 'true';
            
            this.defaultValues = form.dataset.defaultValues ? 
                JSON.parse(form.dataset.defaultValues) : {};
            this.validationSchema = form.dataset.validationSchema ? 
                JSON.parse(form.dataset.validationSchema) : {};
            this.errorMessages = form.dataset.errorMessages ? 
                JSON.parse(form.dataset.errorMessages) : {};
            
            this.state = { ...this.defaultValues };
            this.errors = {};
            this.touched = {};
            
            this.init();
        }
        
        init() {
            // Load persisted state
            if (this.persistState) {
                const saved = localStorage.getItem(`form-${this.name}`);
                if (saved) {
                    this.state = { ...this.state, ...JSON.parse(saved) };
                }
            }
            
            // Populate form with state
            this.populateForm();
            
            // Add event listeners
            this.form.addEventListener('input', (e) => this.handleInput(e));
            this.form.addEventListener('blur', (e) => this.handleBlur(e), true);
            this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        }
        
        handleInput(e) {
            const { name, value, type, checked } = e.target;
            if (!name) return;
            
            this.state[name] = type === 'checkbox' ? checked : value;
            
            if (this.validateOnChange) {
                this.validateField(name);
            }
            
            if (this.persistState) {
                localStorage.setItem(`form-${this.name}`, JSON.stringify(this.state));
            }
            
            this.updateFieldState(name);
        }
        
        handleBlur(e) {
            const { name } = e.target;
            if (!name) return;
            
            this.touched[name] = true;
            
            if (this.validateOnBlur) {
                this.validateField(name);
            }
        }
        
        handleSubmit(e) {
            if (this.validateOnSubmit) {
                const isValid = this.validateForm();
                if (!isValid) {
                    e.preventDefault();
                    return false;
                }
            }
        }
        
        validateField(name) {
            const value = this.state[name];
            const rules = this.validationSchema[name];
            
            if (!rules) return true;
            
            let error = null;
            
            if (rules.required && (!value || value.toString().trim() === '')) {
                error = this.errorMessages[name]?.required || 'This field is required';
            } else if (rules.pattern && value && !new RegExp(rules.pattern).test(value)) {
                error = this.errorMessages[name]?.pattern || 'Invalid format';
            } else if (rules.minLength && value && value.length < rules.minLength) {
                error = this.errorMessages[name]?.minLength || `Minimum ${rules.minLength} characters`;
            } else if (rules.maxLength && value && value.length > rules.maxLength) {
                error = this.errorMessages[name]?.maxLength || `Maximum ${rules.maxLength} characters`;
            }
            
            this.errors[name] = error;
            this.updateFieldState(name);
            
            return !error;
        }
        
        validateForm() {
            let isValid = true;
            Object.keys(this.validationSchema).forEach(name => {
                if (!this.validateField(name)) {
                    isValid = false;
                }
            });
            return isValid;
        }
        
        updateFieldState(name) {
            const field = this.form.querySelector(`[name='${name}']`);
            const errorElement = this.form.querySelector(`[data-error-for='${name}']`);
            
            if (field) {
                field.classList.toggle('is-invalid', !!this.errors[name]);
                field.classList.toggle('is-valid', !this.errors[name] && this.touched[name]);
            }
            
            if (errorElement) {
                errorElement.textContent = this.errors[name] || '';
                errorElement.style.display = this.errors[name] ? 'block' : 'none';
            }
        }
        
        populateForm() {
            Object.keys(this.state).forEach(name => {
                const field = this.form.querySelector(`[name='${name}']`);
                if (field) {
                    if (field.type === 'checkbox' || field.type === 'radio') {
                        field.checked = this.state[name];
                    } else {
                        field.value = this.state[name];
                    }
                }
            });
        }
        
        reset() {
            this.state = { ...this.defaultValues };
            this.errors = {};
            this.touched = {};
            this.populateForm();
            if (this.persistState) {
                localStorage.removeItem(`form-${this.name}`);
            }
        }
        
        getState() {
            return { ...this.state };
        }
        
        setState(newState) {
            this.state = { ...this.state, ...newState };
            this.populateForm();
        }
    }
    
    // Initialize all reactive forms
    document.querySelectorAll('[data-reactive-form]').forEach(form => {
        new ReactiveForm(form);
    });
})();";
    }
}