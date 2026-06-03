namespace EventHub1.Models
{
    public class Participation
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int EventId { get; set; }

        public string Status { get; set; }
        // "Ich komme", "Vielleicht", "Absage"
    }
}
