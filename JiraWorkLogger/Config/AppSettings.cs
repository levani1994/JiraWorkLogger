namespace ScrinToJiraSync.Config
{
    public class AppSettings
    {
        public ScrinConfig Scrin { get; set; }
        public JiraConfig Jira { get; set; }
        public GeneralSettings Settings { get; set; }
    }

    public class ScrinConfig
    {
        public string Token { get; set; }
    }

    public class JiraConfig
    {
        public string Email { get; set; }
        public string ApiToken { get; set; }
        public string BaseUrl { get; set; }
    }

    public class GeneralSettings
    {
        public int TimeZoneOffsetMinutes { get; set; }
    }
}
