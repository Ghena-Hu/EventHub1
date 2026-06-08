using System.Collections.Generic;

namespace EventHub1.Models
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<Event> Events { get; set; }
        public List<Comment> Comments { get; set; }
    }
}