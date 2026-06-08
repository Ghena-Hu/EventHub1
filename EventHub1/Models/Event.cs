using System;
using System.Collections.Generic;

namespace EventHub1.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime Date { get; set; }
        public string Location { get; set; }

        public int MaxParticipants { get; set; }

        public string Category { get; set; }

        public string OwnerId { get; set; }

        public List<Participation> Participations { get; set; }
        public string ImageUrl { get; set; }
    }
}