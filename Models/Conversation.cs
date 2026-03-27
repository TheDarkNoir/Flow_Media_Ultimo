using System;

namespace FlowMediaWebMVC.Models
{
    [Serializable]
    public class Conversation
    {
        public string Id { get; set; }
        public string UserIdentifier { get; set; } // could be email or document
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}