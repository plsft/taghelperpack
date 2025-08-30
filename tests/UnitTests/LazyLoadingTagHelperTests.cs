using Microsoft.AspNetCore.Razor.TagHelpers;
using TagHelperPack;
using Xunit;

namespace UnitTests;

public class LazyLoadingTagHelperTests
{
    [Fact]
    public void Process_WithLazyTrue_AddsLoadingAttribute()
    {
        // Arrange
        var context = new TagHelperContext(
            "img",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "img",
            new TagHelperAttributeList(),
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new LazyLoadingTagHelper
        {
            LazyLoad = true
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.True(output.Attributes.ContainsName("loading"));
        Assert.Equal("lazy", output.Attributes["loading"].Value);
        Assert.True(output.Attributes.ContainsName("decoding"));
        Assert.Equal("async", output.Attributes["decoding"].Value);
        Assert.False(output.Attributes.ContainsName("lazy"));
    }
    
    [Fact]
    public void Process_WithPlaceholder_SetsDataSrcAndPlaceholder()
    {
        // Arrange
        var context = new TagHelperContext(
            "img",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test"
        );
        
        var output = new TagHelperOutput(
            "img",
            new TagHelperAttributeList
            {
                { "src", "original.jpg" }
            },
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            }
        );
        
        var tagHelper = new LazyLoadingTagHelper
        {
            LazyLoad = true,
            PlaceholderSrc = "placeholder.jpg"
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.True(output.Attributes.ContainsName("data-src"));
        Assert.Equal("original.jpg", output.Attributes["data-src"].Value);
        Assert.Equal("placeholder.jpg", output.Attributes["src"].Value);
    }
}