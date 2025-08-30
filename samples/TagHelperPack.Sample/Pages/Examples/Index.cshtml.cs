using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagHelperPack.Sample.Pages.Examples;

public class IndexModel : PageModel
{
    public bool ShowSuccess { get; set; }

    public void OnGet(bool showSuccess = false)
    {
        ShowSuccess = showSuccess;
    }
}