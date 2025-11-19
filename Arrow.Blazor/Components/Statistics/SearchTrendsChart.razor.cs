using Arrow.Blazor.Data;
using Arrow.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Arrow.Blazor.Components.Statistics;

public partial class SearchTrendsChart : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IAnalyticsRepository AnalyticsRepository { get; set; } = default!;

    private string ComponentId = Guid.NewGuid().ToString("N")[..8];
    private DateTime StartDate = DateTime.Now.AddDays(-30);
    private DateTime EndDate = DateTime.Now;
    private bool IsLoading = false;
    private List<DailyPageViewStats> PageViews = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeChart();
            await LoadPageViews();
        }
    }

    private async Task InitializeChart()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("initializePageViewsChart", $"pageViewsChart-{ComponentId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing chart: {ex.Message}");
        }
    }

    private async Task SetLast30Days()
    {
        EndDate = DateTime.Now.Date;
        StartDate = DateTime.Now.AddDays(-30).Date;
        await LoadPageViews();
    }

    private async Task LoadPageViews()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            PageViews = await AnalyticsRepository.GetDailyPageViewsAsync("/", StartDate, EndDate);
            
            // Fill missing dates with 0 counts
            PageViews = FillMissingDates(PageViews, StartDate, EndDate);
            
            // Update the chart
            var chartData = new
            {
                labels = PageViews.Select(x => x.Date.ToString("MMM dd")).ToArray(),
                data = PageViews.Select(x => x.ViewCount).ToArray()
            };

            await JSRuntime.InvokeVoidAsync("updatePageViewsChart", $"pageViewsChart-{ComponentId}", chartData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading page views: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private List<DailyPageViewStats> FillMissingDates(List<DailyPageViewStats> views, DateTime startDate, DateTime endDate)
    {
        var viewDict = views.ToDictionary(t => t.Date.Date, t => t.ViewCount);
        var filledViews = new List<DailyPageViewStats>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            filledViews.Add(new DailyPageViewStats
            {
                Date = date,
                ViewCount = viewDict.TryGetValue(date, out var count) ? count : 0
            });
        }

        return filledViews;
    }

    private string GetDateRangeSummary()
    {
        var days = (EndDate - StartDate).Days + 1;
        var totalViews = PageViews.Sum(x => x.ViewCount);
        return $"{days} days • {totalViews:N0} total views";
    }
}
