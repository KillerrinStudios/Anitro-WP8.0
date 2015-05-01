using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// HTTP Client
using System.Net.Http;
using System.Net.Http.Headers;

// Json.net
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Bson;
using System.Net.NetworkInformation;

namespace AnitroAPI
{
    public static class Hummingbird
    {
        public static bool isPostingLibraryUpdateComplete = true;

        #region API Methods
        public static async Task<List<Anime>> SearchAnime(string searchTerm)
        {
            Debug.WriteLine("Entering");
            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message https://hummingbirdv1.p.mashape.com/search/anime?query=search&auth_token=token
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hummingbirdv1.p.mashape.com/search/anime?query=" + searchTerm + "&auth_token=" + Storage.Settings.User.authToken.Value);//Consts.settings.auth_token);

            // Add our custom headers
            //requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // Just as an example I'm turning the response into a string here
                string responseAsString = await response.Content.ReadAsStringAsync();

                //Debug.WriteLine(responseAsString + "\n\n");
                
                List<Anime> anime = ParseSearchResult(responseAsString);
                foreach (Anime a in anime)
                {
                    Debug.WriteLine(a.title);
                }


                return anime;
            }
            return new List<Anime>();
        }
        
        public static async Task<Anime> GetAnime(string _anime)
        {
            /// ------------------------------------------------ ///
            /// Double Check if _anime string is API Compliant.  ///
            /// ------------------------------------------------ ///
            string anime = ConvertToAPIConpliantString(_anime, '-');

            /// --------------------------------------------------- ///
            /// Once _anime string is API Compliant, begin the GET  ///
            /// --------------------------------------------------- ///
            Debug.WriteLine("Entering: " + anime, "Get Anime()");

            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hummingbirdv1.p.mashape.com/anime/" + anime); //"http://hummingbird.me/search?query="+uri);//

            // Add our custom headers
            //requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // Just as an example I'm turning the response into a string here
                string responseAsString = await response.Content.ReadAsStringAsync();

                //Debug.WriteLine(responseAsString);
                //Console.WriteLine(responseAsString);


                JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                Anime animeObject = JsonConvert.DeserializeObject<Anime>(o.ToString()); ;

                Debug.WriteLine("Exiting", "GetAnime()");
                //byte[] data = Encoding.UTF8.GetBytes(responseAsString);
                //return Encoding.UTF8.GetString(data, 0, data.Length);
                //return responseAsString;
                return animeObject;
            }
            return new Anime();
            
            #region Old Code
            //getAnimeString = string.Empty;
            ///// ------------------------------------------------ ///
            ///// Double Check if _anime string is API Compliant.  ///
            ///// ------------------------------------------------ ///
            //string anime = ConvertToAPIConpliantString(_anime);

            ///// --------------------------------------------------- ///
            ///// Once _anime string is API Compliant, begin the GET  ///
            ///// --------------------------------------------------- ///
            //Debug.WriteLine("Entering GetAnime");

            //// Create a client
            //HttpClient httpClient = new HttpClient();

            //// Add a new Request Message
            //HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hummingbirdv1.p.mashape.com/anime/"+anime);

            //// Add our custom headers
            ////requestMessage.Headers.Add("Content-Type", "application/json");
            //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            //Debug.WriteLine("FFFF");
            //// Send the request to the server
            ////HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            //HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            //Debug.WriteLine("Response sent");

            //if (response.IsSuccessStatusCode)
            //{
            //    // Just as an example I'm turning the response into a string here
            //    string responseAsString = await response.Content.ReadAsStringAsync();

            //    Debug.WriteLine(responseAsString);
            //    //Console.WriteLine(responseAsString);

            //    Debug.WriteLine("Exiting GetAnime \n\n");
            //    getAnimeString = responseAsString;
            //    //return (responseAsString);
            //}
            //else
            //{
            //    // Do Stuff
            //    return;
            //}

            //while (getAnimeString == string.Empty) { }
            #endregion

        }
        public static async Task<bool> GetStatusFeed()
        {
            Debug.WriteLine("GetStatusFeed(): Entering");
            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://hummingbird.me/api/v1/users/" + Storage.Settings.User.userName.Value + "/feed?page=1"); // http://hummingbird.me/api/v1/users/killerrin/feed?page=1
            Debug.WriteLine("GetStatusFeed(): Getting: "+requestMessage.RequestUri.OriginalString);

            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("GetStatusFeed(): Response successful");
                // Just as an example I'm turning the response into a string here
                string responseAsString = await response.Content.ReadAsStringAsync();

                //Debug.WriteLine(responseAsString);
                //Console.WriteLine(responseAsString);

                responseAsString = "{\"status_feed\":" + responseAsString + "}";

                Debug.WriteLine("GetStatusFeed(): Parsing Library To List");
                //Debug.WriteLine(responseAsString);
                JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                ActivityFeed activityFeed = JsonConvert.DeserializeObject<ActivityFeed>(o.ToString());


                bool b = ParseActivityFeed(activityFeed);
                //Consts.activityFeed = aFO;

                //Debug.WriteLine("GetActivityFeed(): Exiting Successful");
                //byte[] data = Encoding.UTF8.GetBytes(responseAsString);
                //return Encoding.UTF8.GetString(data, 0, data.Length);
                //return responseAsString;
                return b;
            }

            Debug.WriteLine("GetActivityFeed(): Exiting Failed");
            return false;
        }
        public static async Task<string> GetLibrary(string status)
        {
            //string url = "https://hummingbirdv1.p.mashape.com/users/killerrindev/library?status=currently-watching&auth_token=swmRwwrimshWG8EtjKZK";//%3Cauth_token%3E";
            string url = "https://hummingbirdv1.p.mashape.com/users/" + Storage.Settings.User.userName.Value + "/library?status=" + status + "&auth_token=" + Storage.Settings.User.authToken.Value;// Consts.settings.auth_token;

            Debug.WriteLine(url);
            Debug.WriteLine("GetLibrary(): Entering");

            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            // Add our custom headers
            //requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            //Debug.WriteLine("Test");
            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            //string responseAsString
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("GetLibrary(): Response Successful");

                // Turn the response into a string for parsing later
                string responseAsString = await response.Content.ReadAsStringAsync();
                //Debug.WriteLine(responseAsString);

                // Due to json randomness, chop off the last two characters
                //responseAsString = responseAsString.Substring(0, responseAsString.Length - 2);

                switch (status)
                {
                    case "currently-watching":
                        Consts.cwLoaded = false;
                        break;
                    case "plan-to-watch":
                        Consts.pTWLoaded = false;
                        break;
                    case "completed":
                        Consts.cLoaded = false;
                        break;
                    case "on-hold":
                        Consts.oHLoaded = false;
                        break;
                    case "dropped":
                        Consts.dLoaded = false;
                        break;
                    case "":
                        Consts.cwLoaded = false;
                        Consts.pTWLoaded = false;
                        Consts.cLoaded = false;
                        Consts.oHLoaded = false;
                        Consts.dLoaded = false;
                        break;
                }

                Task<bool> libraryParse = ParseLibrary(responseAsString, status);
                await libraryParse;

                Debug.WriteLine("GetLibrary(): Exiting");

                return responseAsString;
            }
            else
            {
                // Do Stuff
                Debug.WriteLine("GetLibrary(): Response Failed");

                return "";
            }
        }
        public static async Task<Consts.LoginError> PostLogin(string username, string password)
        {
            Debug.WriteLine("Entering", "PostLogin()");

            //Debug.WriteLine("Creating Client");
            // Create a client
            HttpClient httpClient = new HttpClient();

            //Debug.WriteLine("Making Custom Message");
            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://hummingbirdv1.p.mashape.com/users/authenticate"); //"http://httpbin.org/post");

            //Debug.WriteLine("Setting Headers");
            // Add our custom headers
            requestMessage.Headers.Add("accept", "application/json"); //"accept"
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            //Debug.WriteLine("Setting Content");
            // Add our Content
            requestMessage.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("username", username),
                    //new KeyValuePair<string,string>("email", emailBox),
                    new KeyValuePair<string,string>("password", password)
                });

            //Debug.WriteLine("Sending Message");
            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            //Debug.WriteLine("Parsing the Response");
            if (response.IsSuccessStatusCode)
            {
                // Set the Username for future usage
                //Consts.settings.userName = username;
                Storage.Settings.User.userName.Value = username;

                //Debug.WriteLine("Reading the String");
                // Grab the string and grab the content
                string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                //Debug.WriteLine("Done reading String");
                //Parse the responseAsString to remove ""'s
                char[] txtarr = responseAsString.ToCharArray();
                //Consts.settings.auth_token = "";
                string _authToken = "";
                foreach (char c in txtarr)
                {
                    switch (c)
                    {
                        case '"':
                            break;
                        default:
                            //Consts.settings.auth_token += c;
                            _authToken += c;
                            break;
                    }
                }

                Storage.Settings.User.authToken.Value = _authToken;

                Debug.WriteLine(responseAsString + " " + Storage.Settings.User.authToken.Value, "PostLogin()");
                Debug.WriteLine("Exiting", "PostLogin()");

                return Consts.LoginError.None;
            }
            else
            {
                // Do Stuff
                if (!Consts.IsConnectedToInternet())
                {
                    Debug.WriteLine("Network Error", "PostLogin()");
                    return Consts.LoginError.NetworkError;
                }
                else if (await response.Content.ReadAsStringAsync() == "{\"error\":\"Invalid credentials\"}")
                {
                    Debug.WriteLine("Invalid Login Credidentials","PostLogin()");
                    return Consts.LoginError.InvalidLogin;
                }
                else
                {
                    Debug.WriteLine("Error connecting to server","PostLogin()");
                    return Consts.LoginError.ServerError;
                }
            }
            //await GetLibrary();
            //await PostLibraryUpdate();
        }
        public static async Task<bool> PostStatusUpdate(string _text)
        {
            Debug.WriteLine("PostStatusUpdate(): Entering");

            /// ------------------------------------------------ ///
            /// Double Check if _anime string is API Compliant.  ///
            /// ------------------------------------------------ ///
            string text = _text; // ConvertToAPIConpliantString(_text, '+');

            /// --------------------------------------------------- ///
            /// Once _text string is API Compliant, begin the POST  ///
            /// --------------------------------------------------- ///

            //Debug.WriteLine("Creating Client");
            // Create a client
            HttpClient httpClient = new HttpClient();

            //Debug.WriteLine("Making Custom Message");
            // Add a new Request Message                                                                               Consts.settings.userName
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://hummingbird.me/users/" + Storage.Settings.User.userName.Value + "/comment.json"); //"http://httpbin.org/post");

            //Debug.WriteLine("Setting Headers");
            // Add our custom headers
            requestMessage.Headers.Add("accept", "application/json"); //"accept"
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            //Debug.WriteLine("Setting Content");
            // Add our Content
            requestMessage.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("auth_token", Storage.Settings.User.authToken.Value),//Consts.settings.auth_token),
                    new KeyValuePair<string,string>("comment", text),
                });

            //Debug.WriteLine("Sending Message");
            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            //Debug.WriteLine("Parsing the Response");
            if (response.IsSuccessStatusCode)
            {
                // Grab the string and grab the content
                string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                DateTime dT = DateTime.Now;
                ActivityFeedObject temp = new ActivityFeedObject
                {
                    storyImage = Storage.Settings.User.userAvatar.Value,//Consts.settings.userAvatar,
                    header = Storage.Settings.User.userName.Value,//Consts.settings.userName,
                    content = _text,
                    timeStamp = dT.Date.Year + "-" + dT.Date.Month + "-" + dT.Date.Day + " at " +
                                dT.TimeOfDay.Hours + ":" + dT.TimeOfDay.Minutes + ":" + dT.TimeOfDay.Seconds
                };

                Consts.activityFeed.Insert(0, temp);

                return true;
            }

            return false;
        }
        public static async Task<bool> PostLibraryUpdate(LibraryObject libraryObject)
        {
            Debug.WriteLine("PostLibraryUpdate(LibraryObject libraryObject): Entering");
            isPostingLibraryUpdateComplete = false;

            if (libraryObject.notes == null) { libraryObject.notes = ""; }

            Debug.WriteLine("Posting: ");
            Debug.WriteLine(libraryObject.anime.slug);
            Debug.WriteLine(libraryObject.status);
            Debug.WriteLine(libraryObject.@private.ToString());
            Debug.WriteLine(libraryObject.rating.value);
            Debug.WriteLine(Convert.ToInt32(libraryObject.rewatched_times));
            Debug.WriteLine(libraryObject.notes.ToString());
            Debug.WriteLine(Convert.ToInt32(libraryObject.episodes_watched));
            Debug.WriteLine(false);

            Task<bool> bTask = PostLibraryUpdate(libraryObject.anime.slug,
                                                    libraryObject.status,
                                                    libraryObject.@private.ToString(),
                                                    libraryObject.rating.value,
                                                    Convert.ToInt32(libraryObject.rewatched_times),
                                                    libraryObject.notes.ToString(),
                                                    Convert.ToInt32(libraryObject.episodes_watched),
                                                    false);
            await bTask;

            isPostingLibraryUpdateComplete = true;
            return bTask.Result;
        }
        public static async Task<bool> PostLibraryUpdate(string _slug, string status, string privacy, string rating, int rewatchedTimes, string notes, int episodesWatched, bool incrimentEpisodes)
        {
            Debug.WriteLine("PostLibraryUpdate(): Entering");
            /// ------------------------------------------------ ///
            /// Double Check if _anime string is API Compliant.  ///
            /// ------------------------------------------------ ///
            string anime = ConvertToAPIConpliantString(_slug, '-');

            /// --------------------------------------------------- ///
            /// Once _anime string is API Compliant, begin the GET  ///
            /// --------------------------------------------------- ///
            /// 

            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://hummingbirdv1.p.mashape.com/libraries/"+anime);

            // Add our custom headers
            requestMessage.Headers.Add("accept", "application/json"); //"accept"
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            // Add our Content
            requestMessage.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("auth_token", Storage.Settings.User.authToken.Value),//Consts.settings.auth_token),
                    new KeyValuePair<string,string>("status", status),
                    new KeyValuePair<string,string>("privacy", privacy),
                    new KeyValuePair<string,string>("rating", rating), // none = None Selected, 0-2 = Unhappy, 3 = Neutral, 4-5 = Happy
                    new KeyValuePair<string,string>("rewatched_times", rewatchedTimes.ToString()),
                    new KeyValuePair<string,string>("notes", notes),
                    new KeyValuePair<string,string>("episodes_watched", episodesWatched.ToString()),
                    new KeyValuePair<string,string>("increment_episodes", (incrimentEpisodes.ToString()).ToLower())
                });

            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // Just as an example I'm turning the response into a string here
                string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                //Debug.WriteLine(responseAsString);
                //Console.WriteLine(responseAsString);

                Debug.WriteLine("PostLibraryUpdate(): Exiting Succeeded");
                return true;
            }

            Debug.WriteLine("PostLibraryUpdate(): Exiting Failed");
            return false;
        }
        public static async Task PostLibraryRemove(string _anime)
        {
            /// ------------------------------------------------ ///
            /// Double Check if _anime string is API Compliant.  ///
            /// ------------------------------------------------ ///
            string anime = ConvertToAPIConpliantString(_anime, '-');

            /// --------------------------------------------------- ///
            /// Once _anime string is API Compliant, begin the GET  ///
            /// --------------------------------------------------- ///

            Debug.WriteLine("Entering", "PostLibraryUpdate()");

            // Create a client
            HttpClient httpClient = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://hummingbirdv1.p.mashape.com/libraries/" + anime + "/remove");

            // Add our custom headers
            requestMessage.Headers.Add("accept", "application/json"); //"accept"
            requestMessage.Headers.Add("X-Mashape-Authorization", Consts.MASHAPE_KEY);

            // Add our Content
            requestMessage.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("auth_token", Storage.Settings.User.authToken.Value),//Consts.settings.auth_token),
                });

            // Send the request to the server
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                // Just as an example I'm turning the response into a string here
                string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                Debug.WriteLine(responseAsString);
                Console.WriteLine(responseAsString);
            }
            else
            {
                // Do Stuff
            }

            Debug.WriteLine("Exiting", "PostLibraryUpdate()");
        }
        #endregion

        #region Streamlining Methods
        public static async Task<bool> GetAllLibraries()  
        {
            Debug.WriteLine("GetAllLibraries(): Entering");
            bool testingLoading = true;

            #region Regenerate Library
            bool regenerateLibrary;
            if (Consts.IsConnectedToInternet())
            {
                Debug.WriteLine(DateTime.Now.ToString());
                if (Debugger.IsAttached)
                {
                    double time = 1.0;
                    Debug.WriteLine(Storage.Settings.libraryLastPulled.Value.AddMinutes(time).ToString());

                    if (DateTime.Now >= Storage.Settings.libraryLastPulled.Value.AddMinutes(time))//Consts.settings.libraryLastPulled <= Consts.settings.libraryLastPulled.AddMinutes(1.0))
                    {
                        if (!testingLoading)
                        {
                            Debug.WriteLine("Regenerating Library");
                            await GenerateAllLibraries(Consts.LibrarySelection.All);
                            regenerateLibrary = true;
                        }
                        else { Debug.WriteLine("Testing Library Load: Do Not Regen"); regenerateLibrary = false; }
                    }
                    else { Debug.WriteLine("Not enough time passed to auto regen library"); regenerateLibrary = false; }
                }
                else
                {
                    Debug.WriteLine(Storage.Settings.libraryLastPulled.Value.AddDays(Storage.Settings.autoGenerateLibraryAfterXDays.Value).ToString());
                    if (DateTime.Now >= Storage.Settings.libraryLastPulled.Value.AddDays(Storage.Settings.autoGenerateLibraryAfterXDays.Value))//Consts.settings.libraryLastPulled <= Consts.settings.libraryLastPulled.AddDays(Consts.settings.autoGenerateLibraryAfterXDays))
                    {
                        Debug.WriteLine("Regenerating Library");
                        await GenerateAllLibraries(Consts.LibrarySelection.All);
                        regenerateLibrary = true;
                    }
                    else { Debug.WriteLine("Not enough time passed to auto regen library"); regenerateLibrary = false; }
                }
            }
            else
            {
                Debug.WriteLine("No network Connected. Can not attempt to regenerate library");
                regenerateLibrary = false;
            }
            #endregion

            if (!regenerateLibrary)
            {
                Debug.WriteLine("LoadLibrary(): Loading Library: all");
                Task<bool> allLibraries = Storage.LoadAnimeLibrary("");
                if (!await allLibraries)
                {
                    GenerateAllLibraries(Consts.LibrarySelection.All);
                }
                Debug.WriteLine("Library Loaded", "LoadLibrary()");
            }

            #region favourites
            try
            {
                Debug.WriteLine("Loading Library: favourites", "LoadLibrary()");


                //GenerateLibrary();
                Task<bool> b = Storage.LoadAnimeLibrary("favourites");
                if (!await b)
                {
                    GenerateAllLibraries(Consts.LibrarySelection.Favourites);
                }

                //while (!Consts.IsLibraryLoaded()) { Debug.WriteLine("Awaiting Library Load", "LoadLibrary()"); }
                Debug.WriteLine("Library Loaded", "LoadLibrary()");
            }
            catch (Exception)
            {
                Debug.WriteLine("Generate Library: favourites");
                GenerateAllLibraries(Consts.LibrarySelection.Favourites);
            }
            #endregion

            Debug.WriteLine("GetAllLibraries(): Exiting");
            return true;
        }

        public static async Task<bool> GenerateAllLibraries(Consts.LibrarySelection library)
        {
            if (!Consts.IsConnectedToInternet())
            {
                return false;
            }

            switch (library)
            {
                case Consts.LibrarySelection.CurrentlyWatching:
                    Debug.WriteLine("Generating Library: currently-watching");
                    Task<string> cw = GetLibrary("currently-watching");
                    await cw;
                    break;
                case Consts.LibrarySelection.PlanToWatch:
                    Debug.WriteLine("Generating Library: plan-to-watch");
                    Task<string> ptw = GetLibrary("plan-to-watch");
                    await ptw;
                    break;
                case Consts.LibrarySelection.Completed:
                    Debug.WriteLine("Generating Library: completed");
                    Task<string> c = GetLibrary("completed");
                    await c;
                    break;
                case Consts.LibrarySelection.OnHold:
                    Debug.WriteLine("Generating Library: on-hold");
                    Task<string> oh = Hummingbird.GetLibrary("on-hold");
                    await oh;
                    break;
                case Consts.LibrarySelection.Dropped:
                    Debug.WriteLine("Generating Library: dropped");
                    Task<string> d = Hummingbird.GetLibrary("dropped");
                    await d;
                    break;
                case Consts.LibrarySelection.Favourites:
                    Debug.WriteLine("Generating Library: favourites");
                    //Consts.favourites = new System.Collections.ObjectModel.ObservableCollection<LibraryObject>();
                    Consts.fLoaded = true;
                    break;
                default:
                    break;
            }

            if (library == Consts.LibrarySelection.All)
            {
                if (Storage.isApplicationClosing) { return false; }

                if (Consts.IsConnectedToInternet())
                {
                    Debug.WriteLine("Generating Library: all");
                    Task<string> aniLibrary = GetLibrary("");
                    await aniLibrary;

                    Task<bool> aL = Storage.SaveAnimeLibrary("");
                    await aL;
                }

                #region oldcode
                //if (Consts.IsConnectedToInternet())
                //{
                //    Debug.WriteLine("Generating Library: currently-watching");
                //    Task<string> cw = GetLibrary("currently-watching");
                //    await cw;
                //    Task<bool> sW = Storage.SaveAnimeLibrary("currently-watching");
                //    await sW;
                //}

                //if (Storage.isApplicationClosing) { return false; }

                //if (Consts.IsConnectedToInternet())
                //{
                //    Debug.WriteLine("Generating Library: plan-to-watch");
                //    Task<string> ptw = GetLibrary("plan-to-watch");
                //    await ptw;
                //    Task<bool> sPTW = Storage.SaveAnimeLibrary("plan-to-watch");
                //    await sPTW;
                //}

                //if (Storage.isApplicationClosing) { return false; }

                //if (Consts.IsConnectedToInternet())
                //{
                //    Debug.WriteLine("Generating Library: completed");
                //    Task<string> c = GetLibrary("completed");
                //    await c;
                //    Task<bool> sC = Storage.SaveAnimeLibrary("completed");
                //    await sC;
                //}

                //if (Storage.isApplicationClosing) { return false; }

                //if (Consts.IsConnectedToInternet())
                //{
                //    Debug.WriteLine("Generating Library: on-hold");
                //    Task<string> oh = GetLibrary("on-hold");
                //    await oh;
                //    Task<bool> sOH = Storage.SaveAnimeLibrary("on-hold");
                //    await sOH;
                //}
                

                //if (Storage.isApplicationClosing) { return false; }

                //if (Consts.IsConnectedToInternet())
                //{
                //    Debug.WriteLine("Generating Library: dropped");
                //    Task<string> d = GetLibrary("dropped");
                //    await d;
                //    Task<bool> sD = Storage.SaveAnimeLibrary("dropped");
                //    await sD;
                //}
                #endregion

                // Finally, Save Favorites
                Debug.WriteLine("Generating Library: favourites");
                Consts.fLoaded = true;
                //Task<bool> sF = Storage.SaveAnimeLibrary("favourites");
                //await sF;
                
                Storage.Settings.libraryLastPulled.Value = DateTime.Now;

                return true;
            }
            else
            {
                string libraryString = string.Empty;
                switch (library)
                {
                    case Consts.LibrarySelection.CurrentlyWatching:
                        libraryString = "currently-watching";
                        break;
                    case Consts.LibrarySelection.PlanToWatch:
                        libraryString = "plan-to-watch";
                        break;
                    case Consts.LibrarySelection.Completed:
                        libraryString = "completed";
                        break;
                    case Consts.LibrarySelection.OnHold:
                        libraryString = "on-hold";
                        break;
                    case Consts.LibrarySelection.Dropped:
                        libraryString = "dropped";
                        break;
                    case Consts.LibrarySelection.Favourites:
                        libraryString = "favourites";
                        break;
                }

                Task<bool> saving = Storage.SaveAnimeLibrary(libraryString);
                await saving;

                Storage.Settings.libraryLastPulled.Value = DateTime.Now;

                return saving.Result;
            }
        }
        #endregion

        #region Helper Methods
        private static string ConvertToAPIConpliantString(string _text, char parseValue)
        {
            string text = _text;
            text.ToLower();
            char[] txtarr = text.ToCharArray();
            text = "";
            foreach (char c in txtarr)
            {
                switch (c)
                {
                    case ' ':
                        text += parseValue;
                        break;
                    default:
                        text += c;
                        break;
                }
            }

            return text;
        }
        #endregion

        #region Parsings
        private static async Task<bool> ParseLibrary(string responseAsString, string status) //async Task<List<LibraryObject>>
        {
            Debug.WriteLine("ParseLibrary("+status+"): Entering");

            //Debug.WriteLine(responseAsString);

            if (String.IsNullOrWhiteSpace(responseAsString)) 
            { 
                Debug.WriteLine("ParseLibrary(): Exiting");

                switch (status)
                {
                    case "":
                        Consts.cwLoaded = true;
                        Consts.pTWLoaded = true;
                        Consts.cLoaded = true;
                        Consts.oHLoaded = true;
                        Consts.dLoaded = true;
                        break;
                    case "currently-watching":
                        Consts.cwLoaded = true;
                        break;
                    case "plan-to-watch":
                        Consts.pTWLoaded = true;
                        break;
                    case "completed":
                        Consts.cLoaded = true;
                        break;
                    case "on-hold":
                        Consts.oHLoaded = true;
                        break;
                    case "dropped":
                        Consts.dLoaded = true;
                        break;
                }

                return false; 
            }
            else
            {
                //Debug.WriteLine("Parsing Library");
                string response = "{\"library\":" + responseAsString + "}";

                //Debug.WriteLine(response);

                JObject o = JObject.Parse(response); // This would be the string you defined above
                //Debug.WriteLine("Parsed");
                LibraryList lib = JsonConvert.DeserializeObject<LibraryList>(o.ToString()); ;
                //Debug.WriteLine("Parsing Lib parsed");

                foreach (LibraryObject lO in lib.library)
                {
                    Debug.WriteLine("Parsed: " + lO.anime.title + " | " + lO.status);
                    LibraryObject tempAnimeObject = lO;

                    //Set Genres and URI
                    //-- Set the URIs
                    tempAnimeObject.anime.cover_image_uri = new Uri(tempAnimeObject.anime.cover_image, UriKind.Absolute);

                    //-- Get Genres
                    //Task<Anime> temp = GetAnime(tempAnimeObject.anime.slug);
                    //await temp;
                    //tempAnimeObject.anime.genres = temp.Result.genres;
                    tempAnimeObject.anime.genres = new List<Genre> { new Genre { name = "" } };

                    if (!String.IsNullOrEmpty(tempAnimeObject.rating.value)) { tempAnimeObject.rating.valueAsDouble = System.Convert.ToDouble(tempAnimeObject.rating.value); }
                    else { tempAnimeObject.rating.valueAsDouble = 0.0; }

                    //library.Add(tempAnimeObject);


                    switch (status)
                    {
                        case "":
                            Consts.AddToLibrary(Consts.GetLibrarySelectionFromStatus(tempAnimeObject.status),
                                                tempAnimeObject,
                                                false,
                                                false);
                            break;
                        case "currently-watching":
                            Consts.AddToLibrary(Consts.LibrarySelection.CurrentlyWatching, tempAnimeObject, false, false);
                            //Consts.currentlyWatching.Add(tempAnimeObject);
                            break;
                        case "plan-to-watch":
                            Consts.AddToLibrary(Consts.LibrarySelection.PlanToWatch, tempAnimeObject, false, false);
                            //Consts.planToWatch.Add(tempAnimeObject);
                            break;
                        case "completed":
                            Consts.AddToLibrary(Consts.LibrarySelection.Completed, tempAnimeObject, false, false);
                            //Consts.completed.Add(tempAnimeObject);
                            break;
                        case "on-hold":
                            Consts.AddToLibrary(Consts.LibrarySelection.OnHold, tempAnimeObject, false, false);
                            //Consts.onHold.Add(tempAnimeObject);
                            break;
                        case "dropped":
                            Consts.AddToLibrary(Consts.LibrarySelection.Dropped, tempAnimeObject, false, false);
                            //Consts.dropped.Add(tempAnimeObject);
                            break;
                    }
                }

                switch (status)
                {
                    case "":
                        Consts.cwLoaded = true;
                        Consts.pTWLoaded = true;
                        Consts.cLoaded = true;
                        Consts.oHLoaded = true;
                        Consts.dLoaded = true;
                        break;
                    case "currently-watching":
                        Consts.cwLoaded = true;
                        break;
                    case "plan-to-watch":
                        Consts.pTWLoaded = true;
                        break;
                    case "completed":
                        Consts.cLoaded = true;
                        break;
                    case "on-hold":
                        Consts.oHLoaded = true;
                        break;
                    case "dropped":
                        Consts.dLoaded = true;
                        break;
                }

                Debug.WriteLine("ParseLibrary("+status+"): Exiting");

                return true;
            }
        }
        
        private static List<Anime> ParseSearchResult(string searchResponseAsString)
        {
            ///
            /// If Errors arrise, convert back to an async which returns a Task<List<Anime>>
            /// 

            Debug.WriteLine("ParseSearchResult(): Entering");
            //Debug.WriteLine(searchResponseAsString);

            if (String.IsNullOrWhiteSpace(searchResponseAsString)) { Debug.WriteLine("ParseSearchResult(): String is null or empty"); return new List<Anime>(); }
            else
            {
                Debug.WriteLine("ParseSearchResult(): String is Populated");
                string response = "{\"anime\":" + searchResponseAsString + "}";

                JObject o = JObject.Parse(response); // This would be the string you defined above
                //Debug.WriteLine("Parsed");
                AnimeList ani = JsonConvert.DeserializeObject<AnimeList>(o.ToString());
                //Debug.WriteLine("Parsing Lib parsed");

                foreach (Anime a in ani.anime)
                {
                    a.cover_image_uri = new Uri(a.cover_image, UriKind.Absolute);
                }

                Debug.WriteLine("ParseSearchResult(): Exiting");
                return ani.anime;
            }

            #region oldCode
            //string responseAsString = searchResponseAsString.Substring(1, searchResponseAsString.Length - 2);
            //string[] responseSplit = responseAsString.Split(']');
            //List<string> jsonSplit = new List<string> { };
            //List<Anime> search = new List<Anime> { };

            //Debug.WriteLine(responseSplit[0]);

            //Debug.WriteLine("ParseSearchResult(): First Loop");
            //for (int i = 0; i < responseSplit.Length - 1; i++)
            //{
            //    Debug.WriteLine(responseSplit[i]);
            //    if (string.IsNullOrWhiteSpace(responseSplit[i])) { break; }

            //    if (i == 0)
            //    {
            //        jsonSplit.Add(responseSplit[i] + "]}");
            //    }
            //    else
            //    {
            //        string str = responseSplit[i];
            //        str = str.Substring(2);
            //        jsonSplit.Add(str + "]}");
            //    }
            //}

            //Debug.WriteLine("ParseSearchResult(): Second Loop");
            //for (int i = 0; i < jsonSplit.Count; i++)
            //{
            //    Debug.WriteLine("Parsing Json", "ParseSearchResult()");
            //    JObject o = JObject.Parse(jsonSplit[i]);

            //    Anime tempAnimeObject = JsonConvert.DeserializeObject<Anime>(o.ToString());
            //    Debug.WriteLine("Deserialized", "ParseLibrary()");


            //    // Set URI
            //    //-- Set the URIs
            //    //tempAnimeObject.cover_image_uri = new Uri(tempAnimeObject.cover_image, UriKind.Absolute);

            //    search.Add(tempAnimeObject);
            //}
            #endregion


        }

        public static bool ParseActivityFeed(ActivityFeed aF)
        {
            Debug.WriteLine("ParseActivityFeed(): Entering");
            try
            {
                //ObservableCollection<ActivityFeedObject> activityFeedObject = new ObservableCollection<ActivityFeedObject>();

                //if (!Storage.DoesFileExist("avatar.jpg")) { }
                Storage.SaveFileFromServer(new Uri(aF.status_feed[0].user.avatar, UriKind.Absolute), Storage.AVATARIMAGE);

                foreach (StatusFeedObject sFO in aF.status_feed)
                {
                    ActivityFeedObject temp = new ActivityFeedObject { };
                    string contentString;

                    if (string.IsNullOrEmpty(Storage.Settings.User.userAvatar.Value))//Consts.settings.userAvatar))
                    {
                        Debug.WriteLine("Saving avatar");
                        //Storage.SaveFileFromServer(new Uri(sFO.user.avatar, UriKind.Absolute), "avatar.jpg");
                        Debug.WriteLine("Avatar done saving");

                        Storage.Settings.User.userAvatar.Value = sFO.user.avatar;
                        //Consts.settings.userAvatar = sFO.user.avatar;
                        //Storage.SaveSettingsInfo();
                    }

                    switch (sFO.story_type)
                    {
                        case "media_story":
                            string tts = sFO.updated_at.Substring(0, sFO.updated_at.Length - 1);
                            string[] tS = tts.Split('T');

                            if (sFO.substories[0].substory_type == "watchlist_status_update")
                            {
                                switch (sFO.substories[0].new_status)
                                {
                                    case "currently_watching":
                                        contentString = sFO.user.name + " is currently watching";
                                        break;
                                    case "plan_to_watch":
                                        contentString = sFO.user.name + " plans to watch";
                                        break;
                                    case "completed":
                                        contentString = sFO.user.name + " has completed";
                                        break;
                                    case "on_hold":
                                        contentString = sFO.user.name + " has placed on hold";
                                        break;
                                    case "dropped":
                                        contentString = sFO.user.name + " has dropped";
                                        break;
                                    default:
                                        contentString = "";
                                        break;
                                }
                            }
                            else if (sFO.substories[0].substory_type == "watched_episode")
                            {
                                contentString = sFO.user.name + " watched episode " + sFO.substories[0].episode_number;
                            }
                            else { contentString = ""; }

                            string storyImageString = "";

                            temp = new ActivityFeedObject
                            {
                                slug = sFO.media.slug,
                                storyImage = sFO.media.cover_image,
                                header = sFO.media.title,
                                content = contentString,
                                timeStamp = tS[0] + " at " + tS[1],
                            };
                            break;
                        case "comment":
                            string tts2 = sFO.updated_at.Substring(0, sFO.updated_at.Length - 1);
                            string[] tS2 = tts2.Split('T');

                            string commentCut = sFO.substories[0].comment;

                            var schema = "ms-appdata:///Local/";
                            string storyimage = sFO.user.avatar; //schema + Storage.AVATARIMAGE;


                            temp = new ActivityFeedObject
                            {
                                storyImage = storyimage, //sFO.user.avatar,
                                header = sFO.user.name,
                                content = sFO.substories[0].comment,
                                timeStamp = tS2[0] + " at " + tS2[1],
                            };
                            break;
                    }

                    //activityFeedObject.Add(temp);
                    Consts.activityFeed.Add(temp);
                }

                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

    }
}
