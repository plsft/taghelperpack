# Contributing to TagHelperPack

Thank you for considering contributing to TagHelperPack! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 (17.8+) or VS Code with C# extension
- Git

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/taghelperpack.git
   cd taghelperpack
   ```
3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```
4. **Build the solution**:
   ```bash
   dotnet build
   ```
5. **Run tests** to ensure everything works:
   ```bash
   dotnet test
   ```

## 🏗️ Project Structure

```
TagHelperPack/
├── src/
│   └── TagHelperPack/           # Main library
│       ├── *.cs                 # Tag Helper implementations
│       ├── GlobalUsings.cs      # Global using statements
│       └── DependencyInjection/ # Service registration
├── tests/
│   └── UnitTests/               # Unit tests
├── samples/
│   └── TagHelperPack.Sample/    # Sample application
├── .editorconfig                # Code style configuration
├── Directory.Build.props        # Common MSBuild properties
└── global.json                  # SDK version specification
```

## 🔧 Development Guidelines

### Code Style

We follow modern C# conventions and use EditorConfig for consistency:

- **C# 12** language features where appropriate
- **Nullable reference types** enabled
- **Implicit usings** for common namespaces
- **ArgumentNullException.ThrowIfNull()** for parameter validation
- **File-scoped namespaces**
- **Target typed expressions** where beneficial

### Tag Helper Guidelines

When creating new tag helpers:

1. **Inherit from `TagHelper`**
2. **Use appropriate attributes**:
   ```csharp
   [HtmlTargetElement("my-element", Attributes = "my-attribute")]
   public class MyTagHelper : TagHelper
   ```
3. **Support conditional rendering**:
   ```csharp
   if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
   {
       return;
   }
   ```
4. **Validate parameters**:
   ```csharp
   ArgumentNullException.ThrowIfNull(context);
   ArgumentNullException.ThrowIfNull(output);
   ```
5. **Use nullable annotations appropriately**
6. **Add comprehensive XML documentation**

### Testing

- **Write unit tests** for all new functionality
- **Use descriptive test names** that explain what is being tested
- **Follow AAA pattern** (Arrange, Act, Assert)
- **Test edge cases** and error conditions
- **Maintain high code coverage**

Example test structure:
```csharp
[Fact]
public void Process_WhenConditionIsTrue_RendersContent()
{
    // Arrange
    var context = CreateTagHelperContext();
    var output = CreateTagHelperOutput();
    var tagHelper = new MyTagHelper { Condition = true };

    // Act
    tagHelper.Process(context, output);

    // Assert
    Assert.False(output.IsContentModified);
}
```

## 📝 Contributing Process

### 1. Issue First

Before starting work on a significant change:
- **Check existing issues** to avoid duplication
- **Create an issue** to discuss your proposed changes
- **Wait for feedback** from maintainers

### 2. Branch Strategy

- Create a **feature branch** from `main`:
  ```bash
  git checkout -b feature/my-new-tag-helper
  ```
- Use descriptive branch names:
  - `feature/add-tooltip-helper`
  - `fix/markdown-escaping-issue`
  - `docs/update-installation-guide`

### 3. Commit Messages

Follow conventional commit format:
- `feat: add new tooltip tag helper`
- `fix: resolve markdown HTML escaping issue`
- `docs: update README with new examples`
- `test: add unit tests for SEO tag helper`
- `refactor: improve performance of lazy loading helper`

### 4. Pull Request Guidelines

When creating a pull request:

1. **Update documentation** if needed
2. **Add or update tests**
3. **Ensure all tests pass**
4. **Follow the PR template**
5. **Reference related issues**

#### PR Title Format
- Use clear, descriptive titles
- Follow conventional commit format
- Examples:
  - `feat: add responsive image tag helper`
  - `fix: resolve XSS vulnerability in markdown helper`

#### PR Description Template
```markdown
## Description
Brief description of the changes.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Added tests for new functionality
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Documentation updated (if needed)
- [ ] No breaking changes (or documented if necessary)
```

## 🐛 Bug Reports

When reporting bugs:

1. **Use the bug report template**
2. **Provide minimal reproduction steps**
3. **Include environment details**:
   - .NET version
   - ASP.NET Core version
   - Operating system
   - Browser (if applicable)
4. **Attach relevant logs or error messages**
5. **Search existing issues first**

## 💡 Feature Requests

For new feature requests:

1. **Use the feature request template**
2. **Explain the use case** and problem being solved
3. **Provide examples** of the desired API
4. **Consider backward compatibility**
5. **Discuss implementation approach**

## 🔍 Code Review Process

All contributions go through code review:

1. **Automated checks** must pass (build, tests, linting)
2. **At least one maintainer** must approve
3. **Address feedback** promptly and respectfully
4. **Squash commits** before merging (if requested)

### Review Criteria

Reviewers will check for:
- **Functionality**: Does it work as expected?
- **Code quality**: Is it readable and maintainable?
- **Performance**: Are there any performance implications?
- **Security**: Are there potential security issues?
- **Documentation**: Is it properly documented?
- **Tests**: Are there adequate tests?

## 🏷️ Creating New Tag Helpers

### Step-by-Step Guide

1. **Plan the API**:
   ```csharp
   // Define the HTML structure and attributes
   <my-helper attribute="value">Content</my-helper>
   ```

2. **Create the Tag Helper class**:
   ```csharp
   [HtmlTargetElement("my-helper")]
   public class MyTagHelper : TagHelper
   {
       [HtmlAttributeName("attribute")]
       public string? Attribute { get; set; }

       public override void Process(TagHelperContext context, TagHelperOutput output)
       {
           ArgumentNullException.ThrowIfNull(context);
           ArgumentNullException.ThrowIfNull(output);

           if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
           {
               return;
           }

           // Implementation here
       }
   }
   ```

3. **Add comprehensive tests**:
   ```csharp
   public class MyTagHelperTests
   {
       [Fact]
       public void Process_ValidInput_GeneratesExpectedOutput()
       {
           // Test implementation
       }
   }
   ```

4. **Update documentation**:
   - Add to README.md
   - Include usage examples
   - Document all attributes and behaviors

## 🎯 Areas for Contribution

We welcome contributions in these areas:

### High Priority
- **Performance improvements**
- **Security enhancements**
- **Bug fixes**
- **Documentation improvements**
- **Test coverage increases**

### New Features
- **Accessibility helpers** (ARIA attributes, screen reader support)
- **Performance monitoring helpers** (Web Vitals, timing)
- **Progressive Web App helpers** (service worker, manifest)
- **Internationalization helpers** (i18n support)
- **Analytics helpers** (Google Analytics, tracking)

### Infrastructure
- **GitHub Actions improvements**
- **Package publishing automation**
- **Benchmark testing**
- **Integration testing**

## 📞 Getting Help

If you need help:

1. **Check the documentation** first
2. **Search existing issues** and discussions
3. **Ask in GitHub Discussions**
4. **Join our community chat** (if available)

## 🙏 Recognition

Contributors will be:
- **Listed in CONTRIBUTORS.md**
- **Mentioned in release notes**
- **Credited in documentation** (for significant contributions)

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for making TagHelperPack better! 🚀**