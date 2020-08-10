using System.Collections.Generic;

namespace SetSubscriptionTags
{
    public class Subscription
    {
        public string Id { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }
}