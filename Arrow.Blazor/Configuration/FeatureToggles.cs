using Microsoft.Extensions.Configuration;

namespace Arrow.Blazor.Configuration;

public static class FeatureToggles
{
    private const string FeaturesSectionName = "Features";
    private static IConfiguration? configuration;

    public static void Initialize(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        configuration = config;
    }

    private static IConfiguration Configuration => configuration ?? throw new InvalidOperationException(
        "Feature toggles have not been initialized. Call FeatureToggles.Initialize during startup.");

    public static bool IsEnabled(string featureName, bool fallback = false)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or whitespace.", nameof(featureName));
        }

        var section = Configuration.GetSection(FeaturesSectionName);
        return section.GetValue<bool?>(featureName) ?? fallback;
    }

    public static bool IsEmailEnabled => !string.IsNullOrWhiteSpace(Configuration["MailerSend:ApiKey"]);

    public static bool IsUserRegistrationEnabled => IsEnabled("UserRegistration");

    public static bool IsFeedbackEnabled => IsEnabled("SubmitFeedback");

}
