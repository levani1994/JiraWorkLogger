using System.Net.Http.Json;
using ScrinToJiraSync.Models;

namespace ScrinToJiraSync.Services
{
    public class ScrinClient
    {
        private readonly HttpClient _client;

        public ScrinClient(string token)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("X-SSM-Token", token);
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<long> GetEmploymentId()
        {
            var url = "https://screenshotmonitor.com/api/v2/GetCommonData";
            var result = await _client.GetFromJsonAsync<CommonData>(url);
            return result.EmploymentId;
        }

        public async Task<List<Activity>> GetActivities(long employmentId, long fromUnix, long toUnix)
        {
            var url = "https://screenshotmonitor.com/api/v2/GetActivities";

            var body = new[]
            {
                new {
                    employmentId = employmentId.ToString(),
                    from = fromUnix,
                    to = toUnix
                }
            };

            var response = await _client.PostAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Activity>>();
        }
    }
}
