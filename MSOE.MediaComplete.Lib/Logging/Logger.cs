using System;
using log4net;
using log4net.Core;

namespace MSOE.MediaComplete.Lib.Logging
{
    /// <summary>
    /// All logs are written to both the console and rolling file appender defined in <see>MediaCompleteLog4Net.config</see> file.
    /// </summary>
    public static class Logger
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// This logs exceptions that occur in the application.  These exceptions include caught exceptions.
        /// These messages need to be acceptable for the user to see if researched, they can take reasonable amounts of space
        /// but should be available to always be logged if the user desires without significant performance hits
        /// e.g. "Exception with null music file", NullPointerException
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

        public static void SetLogLevel(int logLevel)
        {
            var level = Level.Error;
            switch (logLevel)
            {
                case (int)LoggingLevel.Info:
                    level = Level.Info;
                    break;
                case (int)LoggingLevel.Debug:
                    level = Level.Debug;
                    break;
            }
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = level;
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }

        /// <summary>
        /// 0 -- Info Logging is off
        /// 1 -- Info Logging is on
        /// 314 -- Debug Logging is on (developer only)
        /// </summary>
        public enum LoggingLevel
        {
            Error=0,
            Info=1,
            Debug=314
        }
    }
}
