using Arrow.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace Arrow.Blazor.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private IPageViewTracker PageViewTracker { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await PageViewTracker.TrackPageViewAsync("/");
    }
}
