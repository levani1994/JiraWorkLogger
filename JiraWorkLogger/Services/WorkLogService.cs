using ScrinToJiraSync.Config;
using ScrinToJiraSync.Services;
using ScrinToJiraSync.Models;

namespace JiraWorkLogger.Services
{
    public class WorkLogService
    {
        public class PreviewItem
        {
            public DateTime Date { get; set; }
            public string Ticket { get; set; }
            public long Seconds { get; set; }
        }

        public class LogMessage
        {
            public string Message { get; set; }
            public string Type { get; set; } // "success", "error", "info"
        }

        public event Action<LogMessage>? OnLogMessage;

        private void Log(string message, string type = "info")
        {
            var handler = OnLogMessage;
            if (handler != null)
            {
                try
                {
                    handler.Invoke(new LogMessage { Message = message, Type = type });
                }
                catch
                {
                    // Ignore errors from event handlers to prevent crashes
                }
            }
        }

        public async Task<List<PreviewItem>> GetPreview(
            string scrinToken,
            string jiraEmail,
            string jiraApiToken,
            string jiraBaseUrl,
            int timeZoneOffsetMinutes,
            DateTime startDate,
            DateTime endDate)
        {
            var preview = new List<PreviewItem>();

            try
            {
                var scrin = new ScrinClient(scrinToken);
                long employmentId = await scrin.GetEmploymentId();
                Log($"Using employmentId = {employmentId}", "info");

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    long fromUnix = DateToUnixStart(date);
                    long toUnix = DateToUnixEnd(date);

                    var activities = await scrin.GetActivities(employmentId, fromUnix, toUnix);

                    var dayGroups = activities
                        .Where(a => !string.IsNullOrWhiteSpace(a.Note))
                        .GroupBy(a => a.Note)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Sum(a => a.To - a.From)
                        );

                    foreach (var entry in dayGroups)
                    {
                        preview.Add(new PreviewItem
                        {
                            Date = date,
                            Ticket = entry.Key,
                            Seconds = entry.Value
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error getting preview: {ex.Message}", "error");
                throw;
            }

            return preview;
        }

        public async Task LogToJira(
            List<PreviewItem> preview,
            string jiraEmail,
            string jiraApiToken,
            string jiraBaseUrl,
            int timeZoneOffsetMinutes)
        {
            var jira = new JiraClient(
                jiraEmail,
                jiraApiToken,
                jiraBaseUrl,
                timeZoneOffsetMinutes,
                (message, type) => Log(message, type)
            );

            Log("Logging to Jira...", "info");

            foreach (var item in preview)
            {
                Log($"Logging {item.Seconds / 60} minutes to {item.Ticket} on {item.Date:yyyy-MM-dd}", "info");
                await jira.AddWorklog(item.Ticket, item.Seconds, item.Date);
            }

            Log("Done.", "success");
        }

        private static long DateToUnixStart(DateTime date)
        {
            var utc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            return ((DateTimeOffset)utc).ToUnixTimeSeconds();
        }

        private static long DateToUnixEnd(DateTime date)
        {
            var utc = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);
            return ((DateTimeOffset)utc).ToUnixTimeSeconds();
        }
    }
}

