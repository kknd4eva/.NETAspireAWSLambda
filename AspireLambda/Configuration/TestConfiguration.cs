namespace AspireLambda.Configuration;

public class TestConfiguration
{
    public const string SectionName = "TestConfiguration";
    
    public string TestAttribute { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}