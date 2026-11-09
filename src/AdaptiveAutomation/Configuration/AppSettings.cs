namespace AdaptiveAutomation.Configuration;

public class AppSettings
{
    public BrowserSettings Browser { get; set; } = new();
    public ApplicationSettings Application { get; set; } = new();
    public TestDataSettings TestData { get; set; } = new();
    public MatchingSettings Matching { get; set; } = new();
    public ExecutionSettings Execution { get; set; } = new();
}

public class BrowserSettings
{
    public string Name { get; set; } = "Chrome";
    public bool Headless { get; set; } = false;
}

public class ApplicationSettings
{
    public string BaseUrl { get; set; } = "https://app.cloudqa.io/home/AutomationPracticeForm";
}

public class TestDataSettings
{
    public string FirstName { get; set; } = "Shiva";
    public string Gender { get; set; } = "Male";
    public string State { get; set; } = "India";
}

public class MatchingSettings
{
    public double MinimumConfidenceScore { get; set; } = 60.0;
    public double AmbiguityMargin { get; set; } = 5.0;
}

public class ExecutionSettings
{
    public int TimeoutSeconds { get; set; } = 15;
    public bool CaptureScreenshotOnFailure { get; set; } = true;
}

