namespace ScrinToJiraSync.Models
{
    public class Activity
    {
        public string ActivityId { get; set; }
        public string Note { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
}
