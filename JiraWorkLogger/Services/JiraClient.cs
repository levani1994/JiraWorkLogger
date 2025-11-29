using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class JiraClient
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly int _tzOffset;
    private readonly string _email;
    private string _accountId; // Remove readonly
    private Action<string, string>? _logCallback; // Callback: (message, type)

    public JiraClient(string email, string token, string baseUrl, int timeZoneOffsetMinutes, Action<string, string>? logCallback = null)
    {
        _baseUrl = baseUrl;
        _tzOffset = timeZoneOffsetMinutes;
        _email = email;
        _logCallback = logCallback;
        _client = new HttpClient();
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private void Log(string message, string type = "info")
    {
        if (_logCallback != null)
        {
            _logCallback(message, type);
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    public async Task AddWorklog(string issueKey, long seconds, DateTime logDate)
    {
        var url = $"{_baseUrl}/rest/api/3/issue/{issueKey}/worklog";

        // Check if issue exists
        if (!await IssueExists(issueKey))
        {
            Log($"Issue {issueKey} not found. Skipping worklog.", "error");
            return;
        }

        // Get current user's accountId if not already cached
        if (string.IsNullOrEmpty(_accountId))
        {
            _accountId = await GetCurrentUserAccountId();
        }

        // Check if worklog already exists for this date by current user
        if (await HasMyWorklogForDate(issueKey, logDate))
        {
            Log($"You already have a worklog for {issueKey} on {logDate:yyyy-MM-dd}. Skipping.", "info");
            return;
        }

        // Create the start time at 9 AM in the specified timezone
        var offset = TimeSpan.FromMinutes(_tzOffset);
        var startTime = new DateTimeOffset(logDate.Year, logDate.Month, logDate.Day, 9, 0, 0, offset);

        // Format without colon in timezone offset
        var offsetString = $"{(offset.TotalMinutes >= 0 ? "+" : "-")}{Math.Abs(offset.Hours):D2}{Math.Abs(offset.Minutes):D2}";
        var startedString = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fff") + offsetString;

        var payload = new
        {
            timeSpentSeconds = seconds,
            started = startedString
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Log($"Failed to add worklog for {issueKey}: {response.StatusCode} - {error}", "error");
            return;
        }

        Log($"Worklog added successfully for {issueKey} on {logDate:yyyy-MM-dd}", "success");
    }

    private async Task<string> GetCurrentUserAccountId()
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/3/myself";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(json);

            if (userData.TryGetProperty("accountId", out var accountId))
            {
                return accountId.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            Log($"Error getting current user accountId: {ex.Message}", "error");
            return null;
        }
    }

    private async Task<bool> IssueExists(string issueKey)
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/3/issue/{issueKey}";
            var response = await _client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasMyWorklogForDate(string issueKey, DateTime logDate)
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/3/issue/{issueKey}/worklog";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var worklogData = JsonSerializer.Deserialize<JsonElement>(json);

            if (!worklogData.TryGetProperty("worklogs", out var worklogs))
                return false;

            foreach (var worklog in worklogs.EnumerateArray())
            {
                // Check if this worklog belongs to current user
                if (worklog.TryGetProperty("author", out var author))
                {
                    if (author.TryGetProperty("accountId", out var authorAccountId))
                    {
                        var authorId = authorAccountId.GetString();

                        // Only check worklogs by current user
                        if (authorId == _accountId)
                        {
                            if (worklog.TryGetProperty("started", out var started))
                            {
                                var startedStr = started.GetString();
                                if (DateTime.TryParse(startedStr, out var worklogDate))
                                {
                                    // Check if the worklog is on the same date
                                    if (worklogDate.Date == logDate.Date)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Log($"Error checking existing worklogs for {issueKey}: {ex.Message}", "error");
            return false; // If we can't check, assume it doesn't exist and try to add
        }
    }
}