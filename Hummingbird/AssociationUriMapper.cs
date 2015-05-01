using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Hummingbird
{
    class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;
        private string path = "";//"/Pages";
        public override Uri MapUri(Uri uri)
        {
            tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());
            Debug.WriteLine(tempUri);
            // URI association launch for my app detected
            //if (tempUri.Contains("myappuri:MainPage?Category="))
            //{
            //    // Get the category (after "Category=").
            //    int categoryIndex = tempUri.IndexOf("Category=") + 9;
            //    string category = tempUri.Substring(categoryIndex);
            //    // Redirect to the MainPage.xaml with the proper category to be displayed
            //    return new Uri("/MainPage.xaml?Category=" + category, UriKind.Relative);
            //}

            #region Custom Mappings
            // URI association launch for app detected
            if (tempUri.Contains("WallpaperSettings"))
            {
                Debug.WriteLine("WallpaperSettings UriAssociation Found");

                string sendData = "fromLockscreen=" + true;
                return new Uri(path + "/Settings.xaml?" + sendData, UriKind.Relative);
            }
            
            if (tempUri.Contains("killerrin-anitro:type=") || tempUri.Contains("anitro:type="))
            {
                Debug.WriteLine("killerrin-anitro: or anitro: UriAssociation Found");

                int categoryIndex = tempUri.IndexOf("type=");
                string uriAssociation = tempUri.Substring(0, categoryIndex);
                string category = tempUri.Substring(categoryIndex);
                return GenerateURI(categoryIndex, uriAssociation, category);
            }
            else if (tempUri.Contains("anime:slug=") || tempUri.Contains("manga:slug="))
            {
                Debug.WriteLine("anime:slug= or manga:slug= UriAssociation Found");

                int categoryIndex = tempUri.IndexOf("slug=");
                string uriAssociation = tempUri.Substring(0, categoryIndex);
                string category = tempUri.Substring(categoryIndex);
                return GenerateURI(categoryIndex, uriAssociation, category);
            }
            else if (tempUri.Contains("anime:url=") || tempUri.Contains("manga:url="))
            {
                Debug.WriteLine("anime:url= or manga:url= UriAssociation Found");

                int categoryIndex = tempUri.IndexOf("url=");
                string uriAssociation = tempUri.Substring(0, categoryIndex);
                string category = tempUri.Substring(categoryIndex);
                return GenerateURI(categoryIndex, uriAssociation, category);
            }
            #endregion
            #region Launch Specific Page
            // If the above gives nothing, ensure mainpage atleast opens
            if (tempUri.Contains("anitro:") || tempUri.Contains("killerrin-anitro") || tempUri.Contains("anime") || tempUri.Contains("manga"))
            {
                Debug.WriteLine("UriAssociation Found. Forcing Open through URI Accociation");
                int categoryIndex = tempUri.IndexOf(':');
                string page = tempUri.Substring(categoryIndex + 1);
                if (string.IsNullOrEmpty(page))
                {
                    Debug.WriteLine("Uri Association invalid: Loading /MainPage.xaml");
                    return new Uri(path + "/MainPage.xaml", UriKind.Relative);
                }
                else
                {
                    if (page[0] != '/') page = "/" + page;

                    Debug.WriteLine(page);
                    return new Uri(path + page, UriKind.Relative);
                }
            }
            #endregion

            // Otherwise perform normal launch.
            Debug.WriteLine("No Uri Association Found: Loading: " + uri.OriginalString);
            Uri mappedUri = new Uri(path + uri.OriginalString, UriKind.Relative);
            return mappedUri; //uri;
        }

        private Uri GenerateURI(int categoryIndex, string _uriAssosiation, string _queryString)
        {
            Debug.WriteLine("GenerateURI(): Entering");
            Debug.WriteLine(categoryIndex);
            Debug.WriteLine(_uriAssosiation);
            Debug.WriteLine(_queryString);


            if (_uriAssosiation.Contains("killerrin-anitro:") || _uriAssosiation.Contains("anitro:")) //Uri uri = new Uri("killerrin-anitro:type=anime&slug=steins-gate", UriKind.RelativeOrAbsolute);
            { //Uri uri = new Uri("anitro:type=anime&slug=steins-gate", UriKind.RelativeOrAbsolute);
                string type = _queryString.Substring(5, 5);
                if (type.Contains("anime"))
                {
                    string sendData = _queryString.Substring(5 + 6) +"&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);
                }
                else if (type.Contains("manga"))
                {
                    string sendData = _queryString.Substring(5 + 6) + "&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);
                }
                else
                {
                    if (String.IsNullOrEmpty(_queryString))
                    {
                        return new Uri(path + "/MainPage.xaml", UriKind.Relative);
                    }
                    else
                    {
                        Debug.WriteLine(path + _queryString);
                        return new Uri(path + _queryString, UriKind.Relative);
                    }
                }

            }
            else if (_uriAssosiation.Contains("anime:")) 
            {
                Debug.WriteLine("Entering anime:");
                if (_queryString.Contains("url=")) //Uri uri = new Uri("anime:url=http://www.hummingbird.me/anime/steins-gate", UriKind.RelativeOrAbsolute);
                {
                    string slug = tempUri.Substring(categoryIndex + 32);
                    Debug.WriteLine(slug);

                    string sendData = "slug=" + slug + "&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);
                }
                else if (_queryString.Contains("slug=")) //Uri uri = new Uri("anime:slug=steins-gate", UriKind.RelativeOrAbsolute);
                {
                    string sendData = _queryString + "&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);   
                }
            }
            else if (_uriAssosiation.Contains("manga:")) 
            {
                if (_queryString.Contains("url=")) //Uri uri = new Uri("manga:url=http://www.hummingbird.me/manga/steins-gate", UriKind.RelativeOrAbsolute);
                {
                    string slug = tempUri.Substring(categoryIndex + 32);
                    string sendData = "slug=" + slug + "&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);
                }
                else if (_queryString.Contains("slug=")) //Uri uri = new Uri("manga:slug=steins-gate", UriKind.RelativeOrAbsolute);
                {
                    string sendData = _queryString + "&status=uriAssociation";
                    return new Uri(path + "/LibraryPage.xaml?" + sendData, UriKind.Relative);
                }
            }

            return new Uri(path + "/MainPage.xaml", UriKind.Relative);
        }
    }
}
