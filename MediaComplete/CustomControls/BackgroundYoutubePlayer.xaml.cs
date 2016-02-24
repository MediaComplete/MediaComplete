using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using TinyWebServer;

namespace MediaComplete.CustomControls
{
    /// <summary>
    /// Interaction logic for BackgroundYoutubePlayer.xaml
    /// </summary>
    public partial class BackgroundYoutubePlayer
    {
        public const string Url = "http://localhost:{0}/BackgroundYoutubePlayer/";

        public BackgroundYoutubePlayer()
        {
            InitializeComponent();

            var url = String.Format(Url, FreeTcpPort());

            var embeddedServer = new WebServer(ServePage, url);
            embeddedServer.Run();
            YoutubeBrowser.Source = new Uri(url);
        }

        public static string ServePage(HttpListenerRequest request)
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

        /// <summary>
        /// Returns the next free TCP port on this machine. 
        /// 
        /// Code adapted from http://stackoverflow.com/questions/138043
        /// </summary>
        /// <returns>The next open TCP port</returns>
        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
