using Hummingbird.Resources;
using System;
using System.Diagnostics;
namespace Hummingbird
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }

        // Merging Test
    }
}