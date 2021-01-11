using System;
using System.Collections.Generic;

using CustomFloorPlugin.Extensions;

using UnityEngine;

using Level = IPA.Logging.Logger.Level;


namespace CustomFloorPlugin.Utilities {


    /// <summary>
    /// This is the logger class of CustomFloorPlugin<br/>
    /// It is designed to be included via <see langword="using static"/> and provides standard overloads for many types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is a Logger, shut up Microsoft")]
    internal static class Logging {


        /// <summary>
        /// Standard string logging, exactly what it says on the tin.
        /// </summary>
        /// <param name="message">The string to print</param>
        /// <param name="level">At what level the message will be printed</param>
        internal static void Log(string message = "<3", Level level = Level.Info) {
            Logger.logger.Log(level, message);
        }


        /// <summary>
        /// Logs an exception, automatically logs inner exceptions
        /// </summary>
        /// <param name="e">The exception to print</param>
        /// <param name="level">At what level the exception will be printed</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        internal static void Log(Exception e, Level level = Level.Notice) {
            Log("An error has been caught:\n" + e.GetType().Name + "\nAt:\n" + e.StackTrace + "\nWith message:\n" + e.Message, level);
            if (e.InnerException != null) {
                Log("---Inner Exception:---", level);
                Log(e, level);
            }
        }


        /// <summary>
        /// Logs all list entries, <see cref="GameObject"/>s and <see cref="Component"/>s will be logged via their respective overloads
        /// </summary>
        /// <param name="messages">The list to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log<T>(List<T> messages, Level level = Level.Info) {
            foreach (T message in messages) {
                try {
                    if (message is GameObject) {
                        Log(message as GameObject, level);
                    }
                    else if (message is Component) {
                        Log(message as Component, level);
                    }
                    else {
                        Log(message, level);
                    }
                }
                catch (Exception e) {
                    Log(e, Level.Error);
                }
            }
        }


        /// <summary>
        /// Logs all array entries, <see cref="GameObject"/>s and <see cref="Component"/>s will be logged via their respective overloads
        /// </summary>
        /// <param name="messages">The array to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log<T>(T[] messages, Level level = Level.Info) {
            foreach (T message in messages) {
                try {
                    if (message is GameObject) {
                        Log(message as GameObject, level);
                    }
                    else if (message is Component) {
                        Log(message as Component, level);
                    }
                    else {
                        Log(message, level);
                    }
                }
                catch (Exception e) {
                    Log(e, Level.Error);
                }
            }
        }


        /// <summary>
        /// Logs all set entries, <see cref="GameObject"/>s and <see cref="Component"/>s will be logged via their respective overloads
        /// </summary>
        /// <param name="messages">The set to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log<T>(HashSet<T> messages, Level level = Level.Info) {
            foreach (T message in messages) {
                try {
                    if (message is GameObject) {
                        Log(message as GameObject, level);
                    }
                    else if (message is Component) {
                        Log(message as Component, level);
                    }
                    else {
                        Log(message, level);
                    }
                }
                catch (Exception e) {
                    Log(e, Level.Error);
                }
            }
        }


        /// <summary>
        /// Logs a <see cref="Component"/> by printing it's full path
        /// </summary>
        /// <param name="message">The <see cref="Component"/> to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log(Component message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            }
            catch (Exception e) {
                Log(e, Level.Error);
            }
        }


        /// <summary>
        /// Logs a <see cref="GameObject"/> by printing it's full path
        /// </summary>
        /// <param name="message">The <see cref="GameObject"/> to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log(GameObject message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            }
            catch (Exception e) {
                Log(e, Level.Error);
            }
        }


        /// <summary>
        /// Logs any <see langword="object"/> via <see cref="object.ToString"/>
        /// </summary>
        /// <param name="message">The <see langword="object"/> to log</param>
        /// <param name="level">At what level the final message will be printed</param>
        internal static void Log(object message, Level level = Level.Info) {
            try {
                Log(message.ToString(), level);
            }
            catch (Exception e) {
                Log(e, Level.Error);
            }
        }
    }


    /// <summary>
    /// This has been seperated out from the main <see cref="Logging"/> class to hide <see cref="logger"/> when using "<see langword="using static " cref="Logging"/>;"
    /// </summary>
    internal static class Logger {


        /// <summary>
        /// The logger for this plugin
        /// </summary>
        internal static IPA.Logging.Logger logger;
    }
}