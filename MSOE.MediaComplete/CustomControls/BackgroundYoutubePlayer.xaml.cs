using System;
using System.IO;
using System.Net;
using System.Windows;
using TinyWebServer;

namespace MSOE.MediaComplete.CustomControls
{
    /// <summary>
    /// Interaction logic for BackgroundYoutubePlayer.xaml
    /// </summary>
    public partial class BackgroundYoutubePlayer
    {
        public const string Url = "http://localhost:65465/BackgroundYoutubePlayer/";

        public BackgroundYoutubePlayer()
        {
            InitializeComponent();
            var embeddedServer = new WebServer(ServePage, new[] { Url });
            embeddedServer.Run();

            YoutubeBrowser.Source = new Uri(Url);
        }

        public string ServePage(HttpListenerRequest request)
        {
            var html = "";
            var uri = new Uri("/CustomControls/BackgroundYoutubePlayer.html", UriKind.Relative);
            var streamResourceInfo = Application.GetContentStream(uri);
            if (streamResourceInfo != null)
            {
                var stream = streamResourceInfo.Stream;
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }

            return html;
        }
    }
}
