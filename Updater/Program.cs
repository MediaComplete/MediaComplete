using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        private enum DownloadBehavior { Ignore, Prompt, Download, Install }

        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Console.WriteLine("Current application version: {0}", version);

            var url = ConfigurationManager.AppSettings["Url"];
            Console.WriteLine("Check web for current version at {0}", url);
            string newVersion = null;
            try
            {
                newVersion = new WebClient().DownloadString(url);
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occurred while attempting to retrieve the most up-to-date version number: {0}", e.Message);
                Console.WriteLine("Full stack trace for developers: ");
                Console.WriteLine(e.StackTrace);
                return;
            }
            

            Console.WriteLine("Most up-to-date version is: {0}", newVersion);

            if (!(newVersion.CompareTo(version) > 0))
            {
                Console.WriteLine("Application is up to date.");
                return;
            }

            Console.WriteLine("Update Required!");
            var behaviorString = ConfigurationManager.AppSettings["DownloadBehavior"];
            DownloadBehavior behavior;

            var valid = Enum.TryParse(behaviorString, out behavior);
            if (!valid)
            {
                Console.WriteLine("Detected invalid configuration! \"DownloadBehavior\" was {0}, but it must be one of: {1}", 
                    behaviorString, Enum.GetNames(typeof(DownloadBehavior)).Aggregate((e1, e2) => e1 + ", " + e2));
            }

            switch(behavior)
            {
                case DownloadBehavior.Ignore:
                    Console.WriteLine("Behavior is \"Ignore\", taking no action.");
                    break;
                case DownloadBehavior.Prompt:
                    Console.WriteLine("Behavior is \"Prompt\", informing user.");
                    // TODO pop-up with Url
                    break;
                case DownloadBehavior.Download:
                    Console.WriteLine("Behavior is \"Download\", downloading file and informing user.");
                    // TODO download and pop-up when ready
                    break;
                case DownloadBehavior.Install:
                    Console.WriteLine("Behavior is \"Install\", downloading file and triggering update.");
                    // TODO download and run EXE
                    break;
            }
        }
    }
}
