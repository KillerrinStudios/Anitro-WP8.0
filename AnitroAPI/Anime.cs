using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnitroAPI
{
    public class Anime
    {
        public string slug { get; set; }
        public string status { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string alternate_title { get; set; } //string
        public string episode_count { get; set; } // int
        public string cover_image { get; set; }
        public Uri cover_image_uri { get; set; }
        public string synopsis { get; set; }
        public string show_type { get; set; }
        public List<Genre> genres { get; set; }
    }

    public class Genre
    {
        public string name { get; set; }
    }

    public class Rating
    {
        public string type { get; set; }
        public string value { get; set; }
        public double valueAsDouble { get; set; }
    }

    public class LibraryObject
    {
        public string episodes_watched { get; set; } // int
        public string last_watched { get; set; }
        public string rewatched_times { get; set; } // int
        public object notes { get; set; } //string
        public object notes_present { get; set; } //bool
        public string status { get; set; }
        public string id { get; set; }
        public bool @private { get; set; }
        public object rewatching { get; set; } //bool
        public Anime anime { get; set; }
        public Rating rating { get; set; }
    }

    public class AnimeList
    {
        public List<Anime> anime { get; set; }
    }

    public class LibraryList
    {
        public List<LibraryObject> library { get; set; }
    }

    #region oldClass
    //public class Anime
    //{
    //    // Library Information        
    //    public string episodeWatched = "";
    //    public string lastWatched = "";
    //    public string rewatchedTimes = "";
    //    public string notes = "";
    //    public string notesPresent = "";
    //    public string libraryStatus = "";
    //    public string id = "";
    //    public string privacy = "";
    //    public string rewatching = "";
    //    public string rating = "";

    //    // Anime Information
    //    public string slug = "";
    //    public string animeStatus = "";
    //    public string url = "";
    //    public string title = "";
    //    public string alternateTitle = "";
    //    public string episodeCount = "";
    //    public string coverImage = "";
    //    public string synopsis = "";
    //    public string showType = "";
    //    public List<string> genres = new List<string> { };

    //    public Anime()
    //    {
            
    //    }

    //    public string ConvertToParsableString()
    //    {
    //        string animeDeparsed = episodeWatched + "|" + lastWatched + "|" + rewatchedTimes + "|" + notes + "|" + notesPresent + "|" + libraryStatus + "|" + id + "|" +
    //                                    privacy + "|" + rewatching + "|" + rating + "|" + slug + "|" + animeStatus + "|" + url + "|" + title + "|" + alternateTitle + "|" +
    //                                        episodeCount + "|" + coverImage + "|" + synopsis + "|" + showType;
    //        for (int i = 0; i < genres.Count; i++)
    //        {
    //            animeDeparsed += ("※" + genres[i]);
    //        }
    //        return animeDeparsed;
    //    }
    //}
    #endregion
}
