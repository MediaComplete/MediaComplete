using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MSOE.MediaComplete.Lib.Logging
{
    public static class Logger
    {
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This logs informational messages about the current state of the application and code
        /// These messages need to be acceptable for the user to see if researched, they can take reasonable amounts of space
        /// but should be available to always be logged if the user desires without significant performance hits
        /// e.g. "Playing 'We Will Rock You' by 'Queen'"
        /// </summary>
        /// <param name="message">the message to output to the log</param>
        public static void LogInformation(string message)
        {
            if (Log.IsInfoEnabled)
                Log.Info(message);
        }

        /// <summary>
        /// This logs warning messages about the current state of the application and code, not actual errors but possible points issue
        /// These messages need to be acceptable for the user to see if researched, they can take reasonable amounts of space
        /// but should be available to always be logged if the user desires without significant performance hits
        /// e.g. "A corrupted file 'MyFavoriteCorruption.cat' was attempted to be played"
        /// </summary>
        /// <param name="message">the message to output to the log</param>
        public static void LogWarning(string message)
        {
            if (Log.IsWarnEnabled)
                Log.Debug(message);
        }

        /// <summary>
        /// This logs debug messages about the application.  This can log ANYTHING that a developer wants it to log.
        /// These messages will NEVER be enabled in production code.  
        /// This can take excessive amounts of space does not need to support high performance (although that would still be ideal)
        /// e.g. "Print bytes read:  [[bytes]]"
        /// </summary>
        /// <param name="message">the message to output to the log</param>
        public static void LogDebug(string message)
        {
            if(Log.IsDebugEnabled)
                Log.Debug(message);
        }

        /// <summary>
        /// This logs exceptions that occurr in the application.  These exceptions include caught exceptions.
        /// These messages need to be acceptable for the user to see if researched, they can take reasonable amounts of space
        /// but should be available to always be logged if the user desires without significant performance hits
        /// e.g. "Exception with null music file", NullPointException
        /// </summary>
        /// <param name="message">the message to output to the log</param>
        /// <param name="exception">the exception to be output to the log</param>
        public static void LogException(string message, Exception exception)
        {
            if (Log.IsErrorEnabled)
                Log.Error(message,exception);
        }

        /// <summary>
        /// This logs fatal application issues that may be unrecoverable.  Often times exceptions will lead to fatal errors.
        /// These messages need to be acceptable for the user to see if researched, they can take reasonable amounts of space
        /// but should be available to always be logged if the user desires without significant performance hits
        /// e.g. "User forced shutdown of application"
        /// </summary>
        /// <param name="message">the message to output to the log</param>
        public static void LogFatalError(string message)
        {
            if (Log.IsFatalEnabled)
                Log.Fatal(message);
        }
    }
}
