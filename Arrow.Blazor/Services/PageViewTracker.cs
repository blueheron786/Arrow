using Arrow.Blazor.Data;

namespace Arrow.Blazor.Services;

public class PageViewTracker : IPageViewTracker
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public PageViewTracker(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task TrackPageViewAsync(string pagePath)
    {
        try
        {
            await _analyticsRepository.TrackPageViewAsync(pagePath);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - we don't want tracking to break the app
            Console.WriteLine($"Error tracking page view: {ex.Message}");
        }
    }
}
