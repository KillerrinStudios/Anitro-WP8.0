using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnitroAPI
{
    public class User
    {
        public string name { get; set; }
        public string url { get; set; }
        public string avatar { get; set; }
        public string avatar_small { get; set; }
        public bool nb { get; set; }
    }

    public class Permissions
    {
        public bool destroy { get; set; }
    }

    public class Substory
    {
        public int id { get; set; }
        public string substory_type { get; set; }
        public string created_at { get; set; }
        public string comment { get; set; }
        public Permissions permissions { get; set; }
        public string new_status { get; set; }
        public string episode_number { get; set; }
        public object service { get; set; }
    }

    public class Poster
    {
        public string name { get; set; }
        public string url { get; set; }
        public string avatar { get; set; }
        public string avatar_small { get; set; }
        public bool nb { get; set; }
    }

    public class Media
    {
        public string slug { get; set; }
        public string status { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string alternate_title { get; set; }
        public int episode_count { get; set; }
        public string cover_image { get; set; }
        public string synopsis { get; set; }
        public string show_type { get; set; }
        public List<Genre> genres { get; set; }
    }

    public class StatusFeedObject
    {
        public int id { get; set; }
        public string story_type { get; set; }
        public User user { get; set; }
        public List<Substory> substories { get; set; }
        public string updated_at { get; set; }
        public Poster poster { get; set; }
        public bool self_post { get; set; }
        public int substories_count { get; set; }
        public Media media { get; set; }
    }

    public class ActivityFeed
    {
        public List<StatusFeedObject> status_feed { get; set; }
    }

    public class ActivityFeedObject
    {
        public string slug { get; set; }
        public string storyImage { get; set; }
        public string header { get; set; }
        public string content { get; set; }
        public string timeStamp { get; set; }
    }
}
