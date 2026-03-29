using RazorConsole.Components;

namespace Wdiw.Tui.Components.UI;

public class WButton : TextButton
{
    private string? _appliedContent;

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(Content) && Content != _appliedContent)
        {
            _appliedContent = $"[{Content}]";
            Content = _appliedContent;
        }

        base.OnParametersSet();
    }
}